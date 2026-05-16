# Technical audit - ElTesoroDeMongli

Initial technical audit focused on safe cleanup, bug discovery and refactoring opportunities.

## Project snapshot

- Unity version: `2021.3.4f1`.
- Default branch: `main`.
- Active build scene: `Assets/Scenes/MONGLICLIENT.unity`.
- Main packages detected:
  - Addressables `1.19.19`
  - Cinemachine `2.9.5`
  - Input System `1.4.2`
  - TextMeshPro `3.0.6`
- Major third-party/vendor folders detected:
  - `Assets/Mirror`
  - `Assets/Character Movement Fundamentals`
  - `Assets/Plugins/IngameDebugConsole`
  - `Assets/Samples/Input System`

## Safe cleanup already applied

- Cleaned `.gitignore`:
  - Removed duplicated `UserSettings/EditorUserSettings.asset` entry.
  - Ignored the whole `UserSettings/` folder.
  - Added common local/editor ignores: `.vscode/`, `.idea/`, `.DS_Store`, `Thumbs.db`.
  - Added common Unity build outputs such as `*.aab` and `*.app`.

## Main risks found

### 1. Mixed project/vendor structure

The project currently mixes first-party gameplay code/assets with full third-party packages under `Assets/`.

Recommended future structure:

```text
Assets/_Project
Assets/_ThirdParty
Assets/_Art
Assets/_Scenes
Assets/_Tests
```

This should be done carefully because Unity GUID references are sensitive. Prefer moving assets inside Unity Editor rather than file-system-only moves.

### 2. Large duplicated animation assets

There are several animation files that look duplicated or experimental:

```text
RUN.anim
RUN - Copy.anim
Walk_N.anim
Walk_N - Copy.anim
Jump.anim
Jump 1.anim
Idle2 - Copy.anim
InAir.anim
InAir - Copy.anim
Slide.anim
Slide - Copy.anim
```

Action recommended:

1. Check which clips are referenced by the active animator controller.
2. Check which clips are referenced by prefabs/scenes.
3. Move unused candidates to a temporary quarantine folder before deleting.
4. Delete only after opening the main scenes without missing references.

### 3. Build settings contain inactive/prototype scenes

Detected scenes in build settings:

```text
Assets/Scenes/GameScene.unity               disabled
Assets/Samples/Input System/...Sample.unity disabled
Assets/Scenes/CharacterControllerTest.unity disabled
Assets/Scenes/MONGLICLIENT.unity            enabled
Assets/Scenes/MONGLISERVER.unity            disabled
Assets/Scenes/Empty.unity                   disabled
```

This is not breaking anything, but the build config should eventually be intentional and documented.

### 4. Runtime debug console included in scene/assets

`Assets/Plugins/IngameDebugConsole` is included. This is useful during development but should be reviewed for release builds.

Recommended options:

- Keep it in development builds only.
- Add compile symbols or runtime guards.
- Remove from production scene if not needed.

### 5. External movement code has fragile getter assumptions

In `Assets/Character Movement Fundamentals/Source/Scripts/Core scripts/Sensor.cs`, getters such as `GetCollider()` and `GetTransform()` assume an internal hit list contains at least one element.

Potential issue:

```csharp
return hitColliders[0];
```

If called without a previous successful hit, this can throw an index exception.

Recommended safe refactor:

```csharp
public Collider GetCollider()
{
    return hitColliders.Count > 0 ? hitColliders[0] : null;
}

public Transform GetTransform()
{
    return hitTransforms.Count > 0 ? hitTransforms[0] : null;
}
```

This change should be tested because existing code may implicitly assume non-null values.

### 6. Sensor temporarily changes object layers

`Sensor.Cast()` temporarily moves ignored colliders to the `Ignore Raycast` layer, performs the cast, and restores layers.

This works, but it is risky if other systems read layers in the same frame or if an exception happens during cast logic.

Recommended future refactor:

- Avoid changing runtime layers if possible.
- Use explicit `LayerMask` filtering, `Physics.IgnoreCollision`, or a non-alloc cast and manually filter ignored colliders.
- If layer swapping is kept, wrap restoration in a `try/finally` block.

### 7. Unity/package versions are old but stable

The project is pinned to Unity `2021.3.4f1` with older packages. Do not upgrade all packages in this cleanup branch.

Recommended approach:

1. Keep this branch limited to safe cleanup and documentation.
2. Create a separate branch for package upgrades.
3. Upgrade Unity LTS and packages only after a successful baseline build.

## Suggested next phase

Create a second branch:

```text
refactor/runtime-safety-phase-2
```

Suggested changes:

1. Add null-safe getters in `Sensor.cs`.
2. Wrap layer restore logic in `try/finally`.
3. Review active scene references to debug console and prototype scenes.
4. Identify scripts that belong to project code vs third-party code.
5. Create `_Project` namespace/folder structure for new code going forward.

## Do not do yet

- Do not delete duplicated animations until scene/prefab references are checked in Unity.
- Do not upgrade Unity or packages in this branch.
- Do not move large asset folders outside Unity Editor unless GUID references are verified.
- Do not modify Mirror source directly unless there is a very specific reason.
