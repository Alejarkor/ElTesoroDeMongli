# Upgrade and networking study - ElTesoroDeMongli

Study focused on updating Unity/packages and evaluating whether to keep Mirror or migrate to a newer networking stack while preserving current functionality.

## Current project baseline

Repository inspection shows:

- Unity Editor: `2021.3.4f1`.
- Target product requirement: browser/WebGL build.
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

## Critical WebGL/browser constraint

Because the game targets browser/WebGL, the networking stack must support browser-safe transports.

In practice:

- Do not assume normal UDP sockets are available from the browser client.
- Do not assume raw TCP sockets are available from the browser client.
- Prefer `WSS` / secure WebSocket for production browser builds.
- HTTP/REST is fine for login, inventory, matchmaking and persistence, but not ideal for high-frequency gameplay state.
- WebRTC DataChannels can be interesting for low-latency browser networking, but they add signaling, NAT traversal and server/relay complexity.

This means every networking candidate must be evaluated by transport, not only by API.

## Main conclusion

Do **not** upgrade Unity, packages and networking stack in the same step.

Also: do **not** choose a new networking library unless its WebGL transport story is proven with a running prototype.

Recommended approach:

1. Stabilize the current project.
2. Upgrade Unity/packages in a dedicated compatibility branch.
3. Build a small isolated WebGL networking prototype with the candidate stack.
4. Migrate gameplay networking only after validating feature parity in browser.

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
- Validate a WebGL build early.

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
- Validate shaders, Input System, Addressables, UI, Mirror and WebGL build targets.

Pros:

- Long-term modern base.
- Better future compatibility with current Unity tooling and multiplayer services.

Cons:

- Higher migration risk.
- Mirror compatibility must be validated.
- Asset/shader/package issues likely.
- Possible API breaks in old third-party assets.
- WebGL build/runtime behavior must be retested, not assumed.

Recommended after the conservative baseline passes.

## Networking options for browser/WebGL

### Option A - Keep Mirror, but clean integration

Current situation:

- Mirror is embedded in `Assets/Mirror`.
- Some Mirror core files appear modified directly.

WebGL compatibility assessment:

- Mirror can be viable for browser builds if using a WebSocket-compatible transport.
- Do not use UDP/KCP transport for the WebGL client.
- Native server can run outside the browser, but the browser client should connect through WebSocket/WSS.

Recommended if keeping Mirror:

- Confirm which transport is currently configured in the active scene/network manager.
- Switch/standardize browser builds around WebSocket/WSS transport.
- Move Mirror to a managed package or clearly isolate it under `_ThirdParty`.
- Avoid modifying Mirror core files directly.
- Add `PATCHES.md` documenting any unavoidable changes.
- Wrap Mirror-specific logic in project-level adapters.

Pros:

- Lowest migration cost.
- Existing player/spawn/sync logic probably already depends on it.
- Good short-term path to preserve current functionality.
- Most likely path to keep browser functionality with minimal rewrite.

Cons:

- Current integration is messy.
- Local vendor modifications make upgrades risky.
- Future Mirror upgrades may be painful.
- WebSocket may add more overhead/latency than UDP-based transports.

Best use case:

- Keep the current browser game working while cleaning architecture.

### Option B - Migrate to Unity Netcode for GameObjects

What it gives:

- Official Unity networking stack.
- Integrated with Unity Transport and Unity Multiplayer Services.
- Current Netcode for GameObjects 2.x targets modern Unity versions.
- Supports GameObject/world-state synchronization, host/client-server workflows and newer distributed authority concepts.

WebGL compatibility assessment:

- Potentially viable for browser only if the chosen Unity Transport configuration supports WebSocket for WebGL.
- Should not be selected until a Unity WebGL build can connect to a real server through WebSocket/WSS.
- Treat this as a Unity 6 modernization prototype, not as a direct replacement in the current project.

Pros:

- Official Unity path.
- Better alignment with Unity Relay, Lobby, Matchmaker and future Unity tooling.
- Good for a project we want to maintain long-term inside Unity ecosystem.

Cons:

- Not a drop-in Mirror replacement.
- Requires rewriting `NetworkBehaviour`, RPCs, SyncVars/NetworkVariables, spawning, authority checks and transform synchronization.
- Latest NGO direction is coupled to newer Unity versions.
- Browser transport compatibility must be validated very early.

Best use case:

- Strategic modernization if the project is going to continue evolving seriously and Unity ecosystem alignment matters.

### Option C - Migrate to Fish-Networking

What it gives:

- Free Unity networking solution.
- Server-authoritative by design but allows host mode.
- Broad topology support through transports.

WebGL compatibility assessment:

- Do not assume browser compatibility from the high-level framework.
- It is only viable if the selected Fish-Networking transport supports WebSocket/WebGL properly.
- Needs a dedicated browser prototype before committing.

Pros:

- More feature-rich than many free options.
- Good if we want serious networking without Unity service lock-in.
- Potentially better for authoritative gameplay than ad-hoc Mirror usage.

Cons:

- Still requires a real migration.
- Smaller ecosystem than Unity official tooling.
- Transport choice is critical for WebGL.
- Needs a dedicated prototype before committing.

Best use case:

- Robust server-authoritative architecture without depending heavily on Unity Gaming Services, if WebGL transport validation passes.

### Option D - Photon Fusion

Worth evaluating separately for production multiplayer, especially if we want hosted/session services, prediction-oriented gameplay and commercial tooling.

WebGL compatibility assessment:

- Must be checked against current Photon Fusion WebGL support, pricing and deployment requirements.
- If browser support is first-class, it can be attractive, but it becomes a vendor/platform decision.

Cons:

- Pricing/vendor dependency.
- Different architecture mindset.
- Migration effort is still high.

## Recommended decision after considering WebGL

The browser requirement changes the recommendation:

1. **Short term:** keep Mirror if it can run with a WebSocket/WSS transport in the current project.
2. **Medium term:** clean Mirror integration and document the exact browser transport setup.
3. **Strategic branch:** prototype Netcode for GameObjects and Fish-Networking only if they can prove WebGL/WSS connectivity.
4. Do not prioritize any UDP-first stack for the WebGL client.

Current preference with browser target:

```text
1. Mirror + WebSocket/WSS transport, cleaned and documented.
2. Unity NGO only after WebGL WebSocket prototype succeeds.
3. Fish-Networking only after WebGL transport prototype succeeds.
4. Photon Fusion only if vendor dependency/pricing is acceptable and WebGL support is confirmed.
```

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
- WebGL fullscreen/build behavior.
- Browser connection through `wss://`.
- Server hosting behind HTTPS/WSS reverse proxy if needed.

## WebGL networking prototype plan

Create a separate small scene, not touching the current gameplay scene:

```text
Assets/_Project/Scenes/NetworkingPrototype.unity
```

Prototype requirements:

- Build as WebGL.
- Host server as native standalone/headless, not inside browser.
- Browser client connects through `wss://`.
- Spawn 2 players.
- Move local player.
- Sync remote transform smoothly.
- Sync velocity/state for animation.
- Spawn/despawn a simple network object.
- Test reconnect/disconnect.
- Measure latency and jitter from browser.
- Confirm it works behind the intended deployment environment.

Implement the same prototype with:

```text
Prototype_MirrorWebSocket
Prototype_NGO_WebSocket
Prototype_FishNet_WebSocket
```

Decision criteria:

- Does WebGL build compile?
- Does browser connect through WSS?
- Is server hosting simple?
- Does reconnect work?
- Is latency acceptable?
- Can it handle current player movement/animation sync?
- How much glue code is needed?
- How painful is migration from current code?

## Proposed branch roadmap

### Branch 1 - `upgrade/current-baseline-validation`

Purpose:

- Open with current Unity.
- Confirm compile status.
- Document runtime map.
- Confirm current WebGL build status.
- No package/network migration yet.

Deliverable:

```text
PLAYER_RUNTIME_MAP.md
WEBGL_BASELINE_NOTES.md
```

### Branch 2 - `networking/mirror-websocket-baseline`

Purpose:

- Identify and standardize Mirror WebSocket/WSS transport.
- Confirm browser client connection.
- Document server deployment notes.

### Branch 3 - `upgrade/unity-2021-latest-lts`

Purpose:

- Upgrade only within Unity 2021 LTS patch line.
- Keep Mirror WebSocket.
- Fix compile/import warnings.
- Validate WebGL build.

### Branch 4 - `upgrade/unity-6-compatibility`

Purpose:

- Test Unity 6 compatibility.
- Do not migrate networking yet.
- Identify package/shader/API/WebGL breaks.

### Branch 5 - `prototype/networking-ngo-webgl`

Purpose:

- Isolated WebGL prototype using Netcode for GameObjects.
- Validate browser WSS connectivity, spawn, authority and sync.

### Branch 6 - `prototype/networking-fishnet-webgl`

Purpose:

- Isolated WebGL prototype using Fish-Networking.
- Validate browser WSS connectivity, spawn, authority and sync.

### Branch 7 - `refactor/network-abstraction`

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

Best practical route for a browser game:

1. Keep Mirror for now.
2. Confirm current WebGL build and current transport.
3. Move toward Mirror + WebSocket/WSS as baseline.
4. Fix compile/runtime safety.
5. Document active player prefab and authority model.
6. Upgrade Unity conservatively.
7. Test Unity 6 separately.
8. Prototype NGO/FishNet only through real WebGL/WSS builds.
9. Migrate only if one prototype clearly beats cleaned Mirror.

Current preference:

- Browser-first + minimal risk: **clean Mirror + WebSocket/WSS**.
- Browser-first + long-term Unity ecosystem: **Unity 6 + NGO**, but only after WebGL prototype.
- Browser-first + stronger authoritative networking: **Fish-Networking**, but only if WebGL transport is proven.
