# WebGL preflight static review

Static review performed from the repository before opening/running the Unity project.

## What can be checked from the repo

This review can inspect committed configuration and source files. It cannot actually build or run WebGL, and it cannot reproduce iOS Safari runtime behavior. Those still require Unity + real browser/device testing.

## Confirmed project baseline

- Unity version: `2021.3.4f1`.
- Active build scene: `Assets/Scenes/MONGLICLIENT.unity`.
- WebGL build settings exist in `ProjectSettings/ProjectSettings.asset`.
- Mirror scripting define symbols are present for both Standalone and WebGL.
- Mirror is embedded under `Assets/Mirror`.
- Mirror `SimpleWebTransport` exists in the repository.

## WebGL settings observed

From `ProjectSettings/ProjectSettings.asset`:

```text
webGLMemorySize: 32
webGLExceptionSupport: 1
webGLNameFilesAsHashes: 0
webGLDataCaching: 1
webGLTemplate: APPLICATION:Default
webGLCompressionFormat: 1
webGLThreadsSupport: 0
webGLDecompressionFallback: 1
```

### Assessment

#### Good

- `webGLDataCaching: 1` is useful for repeated browser loads.
- `webGLDecompressionFallback: 1` makes hosting more forgiving if compressed content headers are not perfect.
- `webGLThreadsSupport: 0` avoids requiring cross-origin isolation headers, which simplifies browser/mobile deployment.
- Mirror symbols are enabled for WebGL.

#### Risk / suspicious

- `webGLMemorySize: 32` is very low for a Unity 3D game. This may be especially problematic on iOS Safari if the initial scene/assets are not tiny.
- `webGLTemplate: APPLICATION:Default` means there is no custom WebGL template currently visible in settings. This makes it harder to add diagnostics, mobile loading UX, user-agent checks, audio unlock handling and friendly error overlays.
- `webGLNameFilesAsHashes: 0` is fine for development, but production cache behavior should be reviewed.
- Compression is enabled. Final hosting must serve correct MIME and `Content-Encoding` headers, especially for `.wasm`, `.data`, `.js`, `.gz` or `.br` files.

## Networking findings

`Assets/Mirror/Transports/SimpleWeb/SimpleWebTransport.cs` exists and defines:

```csharp
public const string NormalScheme = "ws";
public const string SecureScheme = "wss";
```

This is good for browser compatibility because WebGL clients should use WebSocket/WSS, not raw UDP/TCP.

### Still not confirmed

The repository contains SimpleWebTransport, but this static review has **not confirmed** that the active scene/network manager is actually using it.

Need to verify in Unity:

- Which `Transport` component is attached to the active NetworkManager.
- Whether the active transport is `SimpleWebTransport`.
- Whether WebGL builds use `clientUseWss = true` or SSL/reverse-proxy setup.
- Which port and hostname are used.
- Whether native/headless server and WebGL browser client use compatible transport settings.

## iOS/browser risk areas

### 1. Memory

`webGLMemorySize: 32` should be treated as a red flag until tested.

Recommended next test values:

```text
Development smoke test: 128 MB
Gameplay test: 256 MB or more, depending on actual build size/assets
Production: smallest stable value after device testing
```

Do not blindly increase memory too much for iOS because mobile Safari may reject large allocations. The right value must be found by testing.

### 2. No custom WebGL shell

Because the project uses the default template, there is no committed project-level place for:

- browser diagnostics;
- `Tap to start` overlay;
- audio unlock;
- WebSocket/WSS endpoint display;
- build version/commit display;
- friendly unsupported-browser messages.

Recommendation:

Create a custom WebGL template or a separate diagnostics page before serious mobile testing.

### 3. Fullscreen

There is an old WebGL fullscreen plugin in the project history, but this static review has not confirmed current active usage.

On iOS, true fullscreen behavior can be unreliable. Gameplay should not depend on fullscreen being available.

### 4. Touch input

The project uses Unity Input System and has an old Input System on-screen controls sample in build settings, disabled.

Need to verify:

- Current active control path for mobile.
- Whether touch controls are in the active scene.
- Whether page scroll/zoom is prevented in the WebGL shell.

## Recommended immediate checks in Unity

### Check 1 - Confirm active transport

Open `MONGLICLIENT.unity` and find the NetworkManager object.

Confirm:

```text
Transport component = SimpleWebTransport
WebGL client connects through ws/wss
Production uses wss:// when page is served over https://
```

### Check 2 - Increase WebGL memory for smoke test

For testing only, try:

```text
WebGL Memory Size: 128 MB
```

Then test iOS Safari early. If it fails, test both lower and higher values with a tiny scene.

### Check 3 - Create a WebGL smoke scene

Make a tiny scene that tests only:

```text
Unity loads
Tap input works
Audio unlock works
WSS connects
One local object moves
One server echo or round-trip works
```

### Check 4 - Add diagnostics before game load

Add a diagnostics page or custom template that checks:

```text
WebGL 1
WebGL 2
WebAssembly
WebSocket
AudioContext
Touch support
Pointer events
Fullscreen API
IndexedDB/localStorage
DevicePixelRatio
Viewport size
User Agent
```

## Proposed safe repo changes before manual testing

### Option A - Docs only

Keep current PR documentation and manually test in Unity.

### Option B - Add standalone diagnostics page

Add a file outside Unity runtime, for example:

```text
Tools/WebGLDiagnostics/diagnostics.html
```

This can be opened from any browser/device before running the Unity build.

### Option C - Add custom WebGL template

Create:

```text
Assets/WebGLTemplates/MongliDiagnostics/
```

This is more integrated but should be done carefully because it changes build output behavior.

## Current recommendation

Before changing networking or Unity versions:

1. Confirm active Mirror transport in Unity.
2. Validate `SimpleWebTransport` with WebGL.
3. Test iOS Safari with a tiny smoke scene.
4. Increase WebGL memory from `32 MB` for testing.
5. Add a diagnostics shell/page.

If Mirror + SimpleWebTransport works through `wss://` on iOS Safari, keep it for now and clean the architecture before considering NGO/FishNet/Photon.
