# Gameplay architecture review - ElTesoroDeMongli

Review focused on gameplay logic, instantiation, synchronization, controls, runtime behavior, refactoring opportunities, optimization and project organization.

> Scope note: this review was done from repository inspection through GitHub. It was not validated by opening the project in Unity. Items marked as **needs Unity validation** should be checked in Editor before changing code or deleting assets.

## Executive summary

The project looks like a prototype that has gone through several gameplay/control/networking experiments. The biggest technical risk is architectural drift:

- gameplay, networking, control and animation responsibilities are mixed;
- external packages were modified or partially adapted inside `Assets/`;
- old systems appear to have been removed, moved or replaced across commits;
- current animation code references custom types that were not found through the inspected current paths;
- scene/build setup keeps prototype scenes disabled but still configured;
- network authority and animation ownership need a clearer single model.

The right move is to refactor by phases, not by one big rewrite.

## High-priority findings

### 1. Possible missing/custom controller references

`Assets/Character Movement Fundamentals/Source/Scripts/Animation & Audio/AnimationControl.cs` references these custom types:

```csharp
MongliWallkerController
MongliAnimatorNetworkController
MongliAnimatorLocalController
```

The referenced files were not found in the expected current paths inspected from `main`.

Why this matters:

- If the classes are actually missing, the project will not compile.
- If they exist in non-obvious locations, the project structure is hiding core gameplay code inside vendor/package folders.
- The name `Wallker` looks like a typo that may have propagated into class/file names.

Recommended action:

1. Open Unity and confirm whether there are compile errors.
2. Locate these classes from the IDE with Find Symbol.
3. Move project-owned gameplay code out of vendor folders into a project namespace/folder.
4. Rename `Wallker` to `Walker` only through a controlled Unity/IDE refactor.

Suggested target:

```text
Assets/_Project/Scripts/Gameplay/Movement/MongliWalkerController.cs
Assets/_Project/Scripts/Gameplay/Animation/MongliAnimatorNetworkController.cs
Assets/_Project/Scripts/Gameplay/Animation/MongliAnimatorLocalController.cs
```

### 2. Animation synchronization is too coupled

`AnimationControl` directly decides whether to update a local animator controller or a network animator controller using an `isLocal` boolean.

Current smell:

```csharp
if (!isLocal)
{
    animNetworkController.SetFloat(...);
}
else
{
    animLocalController.SetFloat(...);
}
```

Problems:

- duplicated branches for each parameter;
- manual `isLocal` can desync from Mirror authority/local player state;
- animation code knows too much about networking;
- hardcoded string parameter names are used every frame;
- missing null checks can cause runtime exceptions if references are not wired.

Recommended refactor:

Introduce a small animation driver abstraction:

```csharp
public interface IAnimatorDriver
{
    void SetFloat(int parameterHash, float value);
    void SetBool(int parameterHash, bool value);
    void SetTrigger(int parameterHash, bool resetFirst = false);
}
```

Then provide:

```text
LocalAnimatorDriver
NetworkAnimatorDriver
```

`AnimationControl` should only talk to `IAnimatorDriver`, not care whether the player is local or remote.

Also replace per-frame strings with cached hashes:

```csharp
static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
```

### 3. Audio runtime safety issues

`AudioControl.cs` assumes several fields/components always exist:

```csharp
controller = GetComponent<MongliWallkerController>();
animator = GetComponentInChildren<Animator>();
mover = GetComponent<Mover>();
...
int _footStepClipIndex = Random.Range(0, footStepClips.Length);
audioSource.PlayOneShot(footStepClips[_footStepClipIndex], ...);
```

Potential bugs:

- `controller`, `mover` or `audioSource` can be null.
- `footStepClips` can be null or empty.
- `jumpClip` / `landClip` can be null.
- Events are subscribed in `Start()` but never unsubscribed.
- In networked mode, server/host/client may play duplicate or unintended audio if ownership is not filtered.

Recommended fix:

- Add `[RequireComponent]` where possible.
- Subscribe in `OnEnable()` and unsubscribe in `OnDisable()`.
- Guard all audio clip calls.
- Decide whether footsteps are local-only, remote spatial audio, or server-disabled.

Target behavior:

```text
Local player: play local movement audio normally.
Remote players: play spatialized footsteps/land sounds if close enough.
Server-only instance: do not play audio.
```

### 4. Vendor code has been modified directly

Mirror files under `Assets/Mirror/Core/` show project modifications in history/current diff.

Risk:

- future Mirror upgrades become painful;
- local fixes are hidden inside vendor code;
- behavior diverges from upstream Mirror docs/examples;
- other developers will not know what has been customized.

Recommended approach:

- Treat `Assets/Mirror` as read-only vendor code.
- Move project behavior into subclasses, components or wrappers.
- Document any unavoidable Mirror patch in a `PATCHES.md` file.
- Ideally install Mirror through Package Manager or a pinned Git URL, not as loose modified files.

### 5. Network authority model needs to be explicit

Historical commit messages suggest a mixed model:

```text
local player handles its own movement/animation;
server handles remote animations;
server lerps local player;
authoritative for others.
```

This is a risky middle ground unless it is very deliberate.

Recommended decision:

Choose one clear model:

#### Option A - Server authoritative movement

Client sends input to server. Server simulates movement. Server syncs transform/state.

Pros:

- harder to cheat;
- one source of truth;
- simpler remote consistency.

Cons:

- needs prediction/reconciliation to feel responsive.

#### Option B - Client authoritative movement

Local client simulates movement and sends transform/state. Server accepts or validates.

Pros:

- simpler and responsive;
- good for prototype or casual game.

Cons:

- easier to cheat;
- needs anti-jitter/sanity checks.

For this project, I would start with **client authoritative movement with server sanity validation**, then evolve later if needed.

### 6. Instantiation/spawning should be centralized

There are signs of old `NetworkPlayer` prefab usage and scene-based setup. Current exact prefab path should be validated in Unity because `Assets/Prefabs/NetworkPlayer.prefab` was not found in `main` through direct path fetch.

Recommended architecture:

```text
GameBootstrap
  └── GameSessionManager
        ├── PlayerSpawnService
        ├── NetworkSessionService
        ├── SceneLoadService
        └── GameplayStateService
```

Mirror-specific spawning should live in one place:

```text
MongliNetworkManager : NetworkManager
PlayerSpawnService
```

Avoid putting spawn logic inside player movement, UI or animation scripts.

### 7. Control/input systems should be unified

The project history shows several control/input experiments:

```text
GamepadControls
PointerControls
PlayerControls.inputactions
TouchManager
MobileInput
DesktopInput
InputSwitcher
```

Current architecture should expose one clean input API:

```csharp
public interface IPlayerInputSource
{
    Vector2 Move { get; }
    Vector2 Look { get; }
    bool JumpPressed { get; }
    bool SprintHeld { get; }
    bool AttackPressed { get; }
}
```

Implementations:

```text
UnityInputSystemPlayerInputSource
MobileTouchInputSource
BotInputSource / ReplayInputSource later if needed
```

Movement should consume `IPlayerInputSource`, not know about keyboard, touch or gamepad directly.

## Medium-priority findings

### 8. Runtime object creation should be minimized

Older controller code and UI snippets show runtime-created UI and auto-generated objects. Runtime creation is not always bad, but for gameplay prefabs it becomes hard to debug.

Recommendation:

- Prefer prefabs wired in the Editor.
- Use runtime factories only for dynamic gameplay objects.
- Keep UI creation out of movement/network scripts.

### 9. Stringly-typed animator parameters

Animator parameter names are passed as strings every frame. This is fragile and slower than hashes.

Recommendation:

```csharp
private static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
```

### 10. Event lifecycle should be consistent

Some scripts connect events in `OnEnable/OnDisable`, others in `Start()` only.

Recommended rule:

```text
Subscribe in OnEnable.
Unsubscribe in OnDisable.
Resolve required references in Awake.
Validate optional references before use.
```

### 11. Scene/build setup should be explicit

Current build settings include one enabled scene and several disabled/prototype scenes.

Recommended cleanup:

```text
Assets/_Project/Scenes/Boot.unity
Assets/_Project/Scenes/MainMenu.unity
Assets/_Project/Scenes/GameClient.unity
Assets/_Project/Scenes/GameServer.unity
Assets/_Project/Scenes/Test/...
```

### 12. Third-party and project code should be separated

Recommended structure:

```text
Assets/_Project/Scripts
Assets/_Project/Prefabs
Assets/_Project/Scenes
Assets/_Project/ScriptableObjects
Assets/_Project/Art
Assets/_ThirdParty/Mirror
Assets/_ThirdParty/CharacterMovementFundamentals
Assets/_ThirdParty/IngameDebugConsole
```

Important: move assets through Unity Editor when possible, not by raw filesystem moves.

## Proposed refactor roadmap

### Phase 1 - Compile and ownership audit

Goal: know exactly what code is active.

Tasks:

1. Open project in Unity.
2. Fix/confirm compile status.
3. Locate all custom gameplay scripts.
4. Identify scripts attached to the active player prefab.
5. Document player prefab component stack.
6. Confirm network authority model.

Deliverable:

```text
PLAYER_RUNTIME_MAP.md
```

### Phase 2 - Animation/audio safety

Goal: remove fragile runtime assumptions.

Tasks:

1. Add null guards and `[RequireComponent]` where appropriate.
2. Move event subscriptions to `OnEnable/OnDisable`.
3. Replace animator parameter strings with hashes.
4. Introduce `IAnimatorDriver`.
5. Split local/network animation drivers.

### Phase 3 - Input decoupling

Goal: one control abstraction.

Tasks:

1. Define `IPlayerInputSource`.
2. Implement Input System source.
3. Keep touch/gamepad/keyboard behind the same interface.
4. Make movement depend on the interface, not devices.

### Phase 4 - Network authority cleanup

Goal: one source of truth for sync.

Tasks:

1. Decide authority model.
2. Centralize spawn logic in `MongliNetworkManager` / `PlayerSpawnService`.
3. Remove animation sync from movement code.
4. Use velocity/state to drive remote animations.
5. Add debug overlay for local/remote/server authority.

### Phase 5 - Asset and scene cleanup

Goal: reduce repo/build noise.

Tasks:

1. Identify referenced animation clips.
2. Quarantine unused animation copies.
3. Clean disabled scenes from build settings or move to test folders.
4. Separate third-party folders.
5. Document vendor patches.

## Concrete PR candidates

### PR A - `refactor/audio-control-safety`

Low-medium risk.

- Add null guards in `AudioControl`.
- Avoid empty footstep clip errors.
- Move event subscription lifecycle to `OnEnable/OnDisable`.

### PR B - `refactor/animation-parameter-hashes`

Low-medium risk.

- Cache animator parameter hashes.
- Keep behavior unchanged.

### PR C - `refactor/animation-driver-abstraction`

Medium risk.

- Introduce `IAnimatorDriver`.
- Reduce local/network branching.

### PR D - `docs/player-runtime-map`

Low risk.

- Document player prefab components and authority model after Unity inspection.

### PR E - `chore/vendor-patches-doc`

Low risk.

- Add `PATCHES.md` documenting Mirror/local package modifications.

## Opinionated target architecture

I would aim for this dependency direction:

```text
Input -> Movement -> Gameplay State -> Animation/Audio Presentation
                    -> Network Sync
```

Not this:

```text
Movement <-> Animation <-> Network <-> Input <-> UI
```

The player should have clear layers:

```text
PlayerRoot
  PlayerIdentity / NetworkIdentity
  PlayerInputAdapter
  PlayerMovementController
  PlayerState
  PlayerAnimationPresenter
  PlayerAudioPresenter
  PlayerNetworkSync
```

Rules:

- Movement does not know about UI.
- Animation does not know about input devices.
- Audio does not know about network authority except whether it should play.
- Network sync does not directly drive animator parameters unless through a presenter/driver.
- Spawn logic is not inside the player controller.

## Immediate next recommendation

Do not start by moving folders or deleting assets.

Start with:

1. Validate compile status in Unity.
2. Confirm whether `MongliWallkerController` and animator controller classes exist.
3. Create `PLAYER_RUNTIME_MAP.md`.
4. Then apply PR A and PR B.
