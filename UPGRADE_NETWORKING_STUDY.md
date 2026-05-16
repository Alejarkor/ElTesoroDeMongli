# Upgrade and networking study - ElTesoroDeMongli

Study focused on updating Unity/packages and evaluating whether to keep Mirror or migrate to a newer networking stack while preserving current functionality.

## Current project baseline

Repository inspection shows:

- Unity Editor: `2021.3.4f1`.
- Active build scene: `Assets/Scenes/MONGLICLIENT.unity`.
- Disabled/prototype scenes still present in build settings: `GameScene`, `CharacterControllerTest`, `MONGLISERVER`, `Empty`, Input System sample.
- Package versions:
  - Addressables `1.19.19`
  - Cinemachine `2.9.5`
  - Input System `1.4.2`
  - TextMeshPro `3.0.6`
  - Timeline `1.6.4`
- Networking appears to use Mirror embedded directly under `Assets/Mirror`, not as a clean package dependency.
- Some Mirror core files have project-local modifications in history/current comparison.

## Main conclusion

Do **not** upgrade Unity, packages and networking stack in the same step.

Recommended approach:

1. Stabilize the current project.
2. Upgrade Unity/packages in a dedicated compatibility branch.
3. Build a small isolated networking prototype with the candidate stack.
4. Migrate gameplay networking only after validating feature parity.

The highest risk is not the Unity version itself. The highest risk is that gameplay, animation, ownership and network synchronization are currently coupled and partially undocumented.

## Unity upgrade options

### Option 1 - Conservative patch upgrade inside 2021 LTS

Goal: lowest-risk modernization.

Actions:

- Open the project with a newer Unity 2021.3 LTS patch.
- Let Unity reimport.
- Fix compile issues.
- Update only minor package versions compatible with Unity 2021.
- Keep Mirror as-is initially.

Pros:

- Lowest risk.
- Best first step to clean warnings and package drift.
- Lets us validate the current game before bigger changes.

Cons:

- Does not unlock the latest Unity networking ecosystem.
- Still leaves old architecture/networking issues.

Recommended as first technical step.

### Option 2 - Upgrade to Unity 6 LTS

Goal: modern platform base.

Actions:

- Duplicate project / branch.
- Open with Unity 6 LTS.
- Update packages.
- Validate shaders, Input System, Addressables, UI, Mirror and build targets.

Pros:

- Long-term modern base.
- Better future compatibility with current Unity tooling and multiplayer services.

Cons:

- Higher migration risk.
- Mirror compatibility must be validated.
- Asset/shader/package issues likely.
- Possible API breaks in old third-party assets.

Recommended after the conservative baseline passes.

## Networking options

### Option A - Keep Mirror, but clean integration

Current situation:

- Mirror is embedded in `Assets/Mirror`.
- Some Mirror core files appear modified directly.

Recommended if keeping Mirror:

- Move Mirror to a managed package or clearly isolate it under `_ThirdParty`.
- Avoid modifying Mirror core files directly.
- Add `PATCHES.md` documenting any unavoidable changes.
- Wrap Mirror-specific logic in project-level adapters.

Pros:

- Lowest migration cost.
- Existing player/spawn/sync logic probably already depends on it.
- Good short-term path to preserve current functionality.

Cons:

- Current integration is messy.
- Local vendor modifications make upgrades risky.
- Future Mirror upgrades may be painful.

Best use case:

- Keep the current game working while cleaning architecture.

### Option B - Migrate to Unity Netcode for GameObjects

What it gives:

- Official Unity networking stack.
- Integrated with Unity Transport and Unity Multiplayer Services.
- Current Netcode for GameObjects 2.x targets modern Unity versions.
- Supports GameObject/world-state synchronization, host/client-server workflows and newer distributed authority concepts.

Pros:

- Official Unity path.
- Better alignment with Unity Relay, Lobby, Matchmaker and future Unity tooling.
- Good for a project we want to maintain long-term inside Unity ecosystem.

Cons:

- Not a drop-in Mirror replacement.
- Requires rewriting `NetworkBehaviour`, RPCs, SyncVars/NetworkVariables, spawning, authority checks and transform synchronization.
- Latest NGO direction is coupled to newer Unity versions.

Best use case:

- Strategic modernization if the project is going to continue evolving seriously.

### Option C - Migrate to Fish-Networking

What it gives:

- Free Unity networking solution.
- Server-authoritative by design but allows host mode.
- No CCU cap/paywall according to its positioning.
- Broad topology support through transports.

Pros:

- More feature-rich than many free options.
- Good if we want serious networking without Unity service lock-in.
- Potentially better for authoritative gameplay than ad-hoc Mirror usage.

Cons:

- Still requires a real migration.
- Smaller ecosystem than Unity official tooling.
- Needs a dedicated prototype before committing.

Best use case:

- Robust server-authoritative architecture without depending heavily on Unity Gaming Services.

### Option D - Photon Fusion

Worth evaluating separately for production multiplayer, especially if we want hosted/session services, prediction-oriented gameplay and commercial tooling.

However, this is a product/platform decision because it introduces vendor pricing, cloud dependency and a different architecture mindset.

## Recommended decision

My recommendation for this project:

1. **Short term:** keep Mirror, clean current architecture and document authority/spawning.
2. **Medium term:** upgrade Unity in stages.
3. **Strategic branch:** prototype Unity Netcode for GameObjects and Fish-Networking in parallel using the same minimal gameplay scenario.
4. Choose the final networking system based on prototype results, not theory.

I would not migrate networking before we have a clean `PlayerRuntimeMap`.

## Minimum functionality to preserve

Before changing networking, document and test:

- Login/API flow.
- Start client/server/host flow.
- Player spawn.
- Local player ownership.
- Remote player visibility.
- Movement synchronization.
- Animation synchronization.
- Jump/land/attack animation events.
- Audio behavior local vs remote.
- Scene loading.
- Disconnect/reconnect behavior.
- WebGL fullscreen/build behavior if still relevant.

## Prototype plan

Create a separate small scene, not touching the current gameplay scene:

```text
Assets/_Project/Scenes/NetworkingPrototype.unity
```

Prototype requirements:

- Spawn 2 players.
- Move local player.
- Sync remote transform smoothly.
- Sync velocity/state for animation.
- Spawn/despawn a simple network object.
- Test host/client and dedicated server if relevant.
- Measure code complexity and migration friction.

Implement the same prototype twice:

```text
Prototype_NGO
Prototype_FishNet
```

Optionally a third baseline:

```text
Prototype_MirrorClean
```

Decision criteria:

- Ease of migration from current code.
- Authority model clarity.
- WebGL/desktop compatibility.
- Server hosting options.
- Debuggability.
- Amount of glue code required.
- Long-term maintainability.

## Proposed branch roadmap

### Branch 1 - `upgrade/current-baseline-validation`

Purpose:

- Open with current Unity.
- Confirm compile status.
- Document runtime map.
- No package/network migration yet.

Deliverable:

```text
PLAYER_RUNTIME_MAP.md
```

### Branch 2 - `upgrade/unity-2021-latest-lts`

Purpose:

- Upgrade only within Unity 2021 LTS patch line.
- Keep Mirror.
- Fix compile/import warnings.

### Branch 3 - `upgrade/unity-6-compatibility`

Purpose:

- Test Unity 6 compatibility.
- Do not migrate networking yet.
- Identify package/shader/API breaks.

### Branch 4 - `prototype/networking-ngo`

Purpose:

- Small isolated prototype using Netcode for GameObjects.
- Validate spawn/authority/sync.

### Branch 5 - `prototype/networking-fishnet`

Purpose:

- Same isolated prototype using Fish-Networking.
- Compare complexity and stability.

### Branch 6 - `refactor/network-abstraction`

Purpose:

- Introduce project-level networking interfaces before migrating gameplay.

Possible interfaces:

```csharp
public interface INetworkPlayerIdentity
{
    bool IsLocalPlayer { get; }
    bool IsOwner { get; }
    bool IsServer { get; }
    ulong NetworkId { get; }
}

public interface INetworkSpawnService
{
    void SpawnPlayer(object connectionContext);
    void DespawnPlayer(ulong networkId);
}
```

## Refactor needed before migration

Before changing networking stack, clean these layers:

```text
Input -> Movement -> Gameplay State -> Animation/Audio Presentation
                    -> Network Sync
```

Avoid:

```text
Movement <-> Network <-> Animation <-> UI <-> Input
```

The networking stack should become an implementation detail, not something spread across every gameplay script.

## Final recommendation

Best practical route:

1. Keep Mirror for now.
2. Fix compile/runtime safety.
3. Document active player prefab and authority model.
4. Upgrade Unity conservatively.
5. Test Unity 6 separately.
6. Prototype NGO and FishNet separately.
7. Migrate only if one prototype clearly beats cleaned Mirror.

Current preference:

- Small/experimental multiplayer game: **clean Mirror first**.
- Serious long-term Unity project: **Unity 6 + Netcode for GameObjects** is worth prototyping.
- Strong authoritative networking without Unity service dependency: **Fish-Networking** is the strongest alternative to prototype.
