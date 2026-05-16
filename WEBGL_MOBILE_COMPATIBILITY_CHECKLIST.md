# WebGL mobile/browser compatibility checklist

Checklist before manual testing of the browser/WebGL version.

## Why this matters

The project targets browser/WebGL, including desktop, Android and iOS. Browser support is not only about rendering WebGL; the real risks are:

- WebAssembly memory limits on mobile browsers.
- iOS Safari/WebKit behavior.
- Touch input and fullscreen restrictions.
- Audio unlock restrictions.
- WebSocket/WSS networking requirements.
- HTTPS/CORS/compression/server headers.
- Asset size and loading time.

## Important iOS note

On iOS/iPadOS, do not treat Safari, Chrome and Firefox as fully different browser engines. In practice, iOS browsers are WebKit-based. Testing Chrome iOS and Firefox iOS is still useful, but the main rendering/runtime behavior will be close to Safari/WebKit.

This means that if something breaks in Safari iOS, there is a significant chance it also breaks in Chrome iOS and Firefox iOS.

## Minimum test matrix

### Desktop

- Windows + Chrome
- Windows + Edge
- Windows + Firefox
- macOS + Safari
- macOS + Chrome

### Android

- Android + Chrome
- Android + Firefox
- Android + Samsung Internet, if available

### iOS / iPadOS

- iPhone + Safari
- iPhone + Chrome
- iPad + Safari
- iPad + Chrome

## Device classes

Try to cover at least:

- Low/mid Android phone.
- Recent Android phone.
- Older iPhone still in expected user range.
- Recent iPhone.
- iPad if tablet support matters.

## Browser feature checks

Before launching the Unity content, add or use a simple HTML/JS diagnostics panel that reports:

```text
User Agent
Platform
Device pixel ratio
Screen size
WebGL 1 support
WebGL 2 support
WebAssembly support
WebSocket support
AudioContext support
Pointer Events support
Touch Events support
Fullscreen API support
IndexedDB/localStorage support
```

Recommended outcome:

- If WebGL is missing, show a friendly unsupported browser message.
- If WebAssembly is missing, show a friendly unsupported browser message.
- If WebGL 2 is missing, either fallback to WebGL 1 or show a clear warning.

## Unity build settings to review

### Player settings

Check in Unity WebGL Player Settings:

- Compression format.
- Decompression fallback.
- Memory size / memory growth behavior.
- WebGL 1/2 graphics API support depending on Unity version.
- Data caching.
- Exceptions setting.
- Managed stripping level.
- Code optimization mode.
- Development build for diagnostics.

### Recommended test setup

For early compatibility testing:

- Create a Development Build.
- Enable visible logging to browser console.
- Keep compression simple until server headers are verified.
- Test without aggressive stripping first.
- Add a version/build label visible on screen.

For production:

- Use Brotli or gzip only if the server is correctly configured.
- Serve via HTTPS.
- Use WSS for networking if the page is HTTPS.
- Validate cache headers.

## Server/deployment checks

For Unity WebGL hosting, verify:

- Correct MIME types for `.wasm`, `.data`, `.js`, `.symbols`, `.br`, `.gz`.
- Correct `Content-Encoding` headers for compressed builds.
- HTTPS enabled.
- WSS endpoint enabled if multiplayer is used.
- CORS configured if API/game server uses a different domain.
- Reverse proxy timeout settings do not kill WebSocket connections.
- Mobile network access tested, not only local Wi-Fi.

## Networking checks

Because browser clients cannot use normal raw TCP/UDP sockets like native apps, the WebGL client should use browser-safe transport:

- Prefer WebSocket/WSS for Mirror or equivalent high-level networking.
- Do not use KCP/UDP for the WebGL client.
- If using HTTPS, use `wss://`, not `ws://`.
- Confirm reconnect behavior after mobile sleep/background.
- Confirm disconnect behavior when tab is closed or refreshed.
- Confirm server cleans ghost players.

## iOS-specific risks

### 1. Memory pressure

Unity WebGL builds can fail on iOS Safari if memory usage or initial asset load is too high.

Mitigation:

- Keep initial scene tiny.
- Avoid loading all assets at startup.
- Use Addressables/asset streaming carefully.
- Reduce texture sizes.
- Prefer compressed textures suitable for WebGL/mobile.
- Avoid huge animation clips in initial load.
- Avoid unnecessary duplicated assets.

### 2. Audio restrictions

Mobile browsers often require a user gesture before audio can start.

Mitigation:

- Start audio only after a first tap/click.
- Add a clear “Tap to start” screen.
- Initialize AudioContext after user interaction.

### 3. Fullscreen restrictions

Fullscreen behavior is inconsistent on iOS, especially iPhone Safari.

Mitigation:

- Do not make gameplay depend on true fullscreen.
- Provide responsive canvas layout.
- Hide browser UI as much as possible, but assume it may remain visible.
- Test orientation changes.

### 4. Touch input

Mouse assumptions break on mobile.

Mitigation:

- Use pointer/touch abstraction.
- Avoid right-click/middle-click requirements.
- Ensure virtual joystick/buttons scale correctly.
- Test multi-touch.
- Prevent unwanted page scroll/zoom during gameplay.

### 5. Background/sleep

Mobile browsers pause or throttle tabs aggressively.

Mitigation:

- Handle visibility change from JS if needed.
- Pause gameplay/network heartbeat cleanly.
- Reconnect or resync after returning.

## Android-specific risks

- Different GPU/driver behavior between devices.
- Some browsers/devices may blacklist WebGL features.
- Memory can still be tight on low/mid devices.
- Chrome Android and Samsung Internet may behave differently.
- Address bar resizing can alter viewport height.

## Suggested pre-test tasks

### Task 1 - Add browser diagnostics page/panel

Add a lightweight preloader panel before Unity starts or as a separate `diagnostics.html`.

It should report browser features and show clear pass/warn/fail results.

### Task 2 - Add visible build info

Show on screen:

```text
Build version
Commit hash
Unity version
Environment: local/staging/production
Server URL
Transport: ws/wss
```

### Task 3 - Add mobile-safe start screen

Before loading/starting gameplay:

```text
Tap to start
```

This unlocks audio and avoids autoplay issues.

### Task 4 - Add WebGL error overlay

Catch and display:

- WebGL unsupported.
- WebAssembly unsupported.
- WebSocket connection failed.
- API login failed.
- Asset load failed.

### Task 5 - Create a tiny WebGL smoke test scene

A minimal scene should test only:

- Unity starts.
- Input works.
- Audio unlock works.
- WebSocket connects.
- One local object moves.
- One network echo/roundtrip works.

This scene should load faster than the real game and help isolate browser problems.

## Manual test script

For each device/browser:

1. Open page.
2. Confirm diagnostics pass.
3. Tap start.
4. Confirm Unity loads.
5. Confirm no browser console fatal errors.
6. Login/API call works.
7. Connect to server through WSS.
8. Spawn player.
9. Move player.
10. Confirm remote player sync from another browser.
11. Trigger jump/animation/audio.
12. Rotate device.
13. Background app/tab for 30 seconds.
14. Return to game.
15. Confirm reconnect/resync or graceful disconnect.
16. Refresh page.
17. Confirm old player is cleaned from server.

## Pass/fail criteria

### Minimum acceptable

- Game loads on desktop Chrome/Edge/Firefox and Android Chrome.
- Game loads on iOS Safari on at least one recent iPhone.
- Login works.
- WebSocket/WSS connects.
- Player spawns and moves.
- Remote player visible.
- No fatal memory crash during first 5 minutes.

### Ideal

- Works on iOS Safari, Chrome iOS and iPad Safari.
- Handles tab background/return.
- Handles refresh/disconnect cleanly.
- Initial load below acceptable size/time target.
- No duplicated audio/network players.

## Recommendation before deeper networking migration

Do this before testing NGO/FishNet/Photon:

1. Create diagnostics page/panel.
2. Confirm current Mirror transport for WebGL.
3. Validate WSS deployment.
4. Create WebGL smoke test scene.
5. Test iOS Safari early.

If current Mirror + WebSocket works on iOS Safari, do not migrate networking yet. Clean it first.
