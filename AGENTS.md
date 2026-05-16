# AGENTS.md — Project instructions for Codex

## Project identity

This repository belongs to **ElTesoroDeMongli**, a technical/creative project developed by Alejandro / Alejarkor.

The project is expected to evolve iteratively and may combine some of these areas:

- Unity / C# development.
- VR / immersive interaction.
- Game systems, prototypes and real-time simulation.
- AI-assisted tooling, agents or procedural systems.
- Clean architecture that allows the project to grow without becoming a mess.

When the current repository contradicts this file, trust the repository first and propose updates to this file.

---

## Default working style

Act as a senior technical teammate, not as an autocomplete tool.

Before making changes:

1. Inspect the existing structure.
2. Identify the actual framework, engine version, package manager and build system.
3. Check nearby files before creating new patterns.
4. Prefer small, reviewable changes.
5. Explain important architectural decisions briefly.

Do not rewrite large areas unless the task explicitly asks for it or the current implementation is clearly blocking progress.

If the user asks something vague, make a reasonable technical assumption and state it briefly before acting.

---

## Communication

Use clear Spanish when replying to Alejandro.

Be direct, practical and technically honest.

Avoid excessive enthusiasm, generic motivational text or long theoretical explanations.

When something is uncertain, say so.

When a change has risks, mention them.

---

## Repository exploration rules

At the start of a task, look for:

- `README.md`
- `AGENTS.md`
- `CLAUDE.md`
- `package.json`
- `pnpm-lock.yaml`
- `package-lock.json`
- `yarn.lock`
- `Cargo.toml`
- `pyproject.toml`
- `requirements.txt`
- `*.sln`
- `*.csproj`
- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `Assets/`
- `Source/`
- `src/`
- `docs/`

For Unity projects, always inspect:

- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `Assets/`
- existing assembly definitions
- existing scene/prefab/script organization

---

## Coding standards

General rules:

- Prefer clarity over cleverness.
- Keep functions focused.
- Avoid hidden global state.
- Avoid magic numbers; expose meaningful constants/configuration.
- Do not introduce heavy dependencies without a clear reason.
- Preserve existing naming conventions unless they are clearly inconsistent.
- Add comments only where the intent is not obvious.
- Do not add speculative features.

C# / Unity rules:

- Use `PascalCase` for public types, methods and properties.
- Use `camelCase` for local variables and private fields unless the project uses `_camelCase`.
- Prefer serialized private fields over public mutable fields:
  - `[SerializeField] private SomeType value;`
- Avoid expensive work in `Update()` unless justified.
- Cache component references when appropriate.
- Avoid `FindObjectOfType`, `GameObject.Find` and reflection-based lookup in runtime paths unless there is no better option.
- Prefer explicit dependencies and inspector references for Unity behaviours.
- Separate data, runtime logic and presentation where practical.
- Keep MonoBehaviours thin when systems start growing.
- For async/network/realtime systems, make lifecycle and cancellation explicit.

---

## Architecture preferences

Favor architectures that are:

- Modular.
- Testable where practical.
- Easy to debug in Unity.
- Friendly to iterative prototyping.
- Not overengineered for the current stage.

Preferred patterns:

- Small services/components with clear responsibility.
- Config via ScriptableObjects, JSON or serialized settings where useful.
- Interfaces only when there are multiple implementations or clear test/extension value.
- Events/signals for decoupled runtime communication, but avoid event spaghetti.
- State machines for character, creature, interaction or gameplay flow.
- Data-driven definitions for entities, creatures, items, abilities or procedural content.

Avoid:

- Giant manager classes.
- Hardcoded scene object names.
- Systems that only work in one scene without a reason.
- Premature ECS/DOTS unless the repo already uses it or performance requires it.
- Abstractions that make debugging harder.

---

## AI / procedural systems

When implementing AI, procedural content or agent-like logic:

- Keep deterministic seeds where useful.
- Separate generation data from generation execution.
- Store generated outputs only when needed.
- Make debug visualization available if the system is spatial, procedural or perception-based.
- Prefer inspectable intermediate data over black-box logic.
- Add simple test scenes or debug tools when they help validate behaviour quickly.

For creatures/NPCs/game agents:

- Separate perception, decision and action.
- Avoid hardcoding behaviour directly into animation or visual scripts.
- Make personality/stats/tuning data editable.
- Use clear state names and transitions.

---

## VR / immersive interaction

When working on VR features:

- Prioritize comfort, performance and clear interaction feedback.
- Avoid assumptions tied to only one headset unless required.
- Keep input abstraction separate from gameplay logic.
- Avoid high-frequency allocations.
- Be careful with camera transforms, world scale and tracking origins.
- Prefer diegetic UI or spatial interaction where it improves the experience, but do not force it.

For Meta Quest / standalone VR:

- Be conservative with shaders, post-processing and realtime effects.
- Consider mobile GPU limitations.
- Avoid unnecessary CPU/GPU sync points.
- Use object pooling for repeated runtime spawning.
- Keep frame budget in mind.

---

## Performance rules

Before optimizing, identify the likely bottleneck.

For Unity:

- Avoid garbage allocations in per-frame paths.
- Avoid repeated LINQ in hot paths.
- Avoid repeated `GetComponent` in hot paths.
- Pool frequently spawned objects.
- Prefer simple data structures unless complexity is justified.
- Use the Profiler when performance claims matter.

For realtime networking/audio/video/VR:

- Treat latency as a core requirement.
- Avoid blocking calls on main thread.
- Make buffer sizes explicit.
- Separate capture, processing and transport concerns.

---

## Testing and validation

When making code changes:

1. Run the most relevant available tests/checks.
2. If tests do not exist, perform the smallest meaningful validation.
3. Explain what was and was not validated.

Look for commands in:

- `README.md`
- package scripts
- CI files
- Unity test runner setup
- solution/project files

If commands are unknown, do not invent them. Report what should be run.

---

## Git and safety

Do not:

- Commit unless explicitly asked.
- Force-push.
- Delete user work.
- Rewrite history.
- Remove large sections without explaining why.
- Modify generated/vendor files unless necessary.

Before risky changes:

- Summarize the plan.
- Prefer patch-sized edits.
- Keep fallback paths simple.

---

## Documentation expectations

When adding a non-trivial system, update or create concise docs that explain:

- What the system does.
- Where the main files live.
- How to configure it.
- How to test or debug it.
- Known limitations.

Good documentation is practical, not ceremonial.

---

## Useful task approach

For feature work:

1. Understand current architecture.
2. Identify minimal implementation path.
3. Add/modify code.
4. Validate.
5. Summarize changed files and next steps.

For bug fixing:

1. Reproduce or infer the failure path.
2. Find the smallest root cause.
3. Patch with minimal blast radius.
4. Add guard/test if useful.
5. Explain the failure in plain language.

For refactoring:

1. Preserve behaviour.
2. Make small mechanical changes first.
3. Only introduce abstractions when they remove real duplication or risk.
4. Keep public API changes explicit.

---

## Project-specific assumptions to verify

These are likely, but must be verified from the repo:

- Main engine may be Unity.
- Main language may be C#.
- Target platform may include PC and/or Meta Quest standalone VR.
- The project may include gameplay systems, procedural generation, creature logic or AI-assisted tooling.

Do not assume these blindly. Confirm from the files before acting.
