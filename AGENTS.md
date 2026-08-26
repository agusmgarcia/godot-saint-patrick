# AGENTS.md

## Engine & Stack

- Godot 4.7.1, C# (.NET 10, LangVersion 14), Jolt Physics
- Root namespace: `SaintPatrick`
- Nullable enabled; nullable warnings treated as errors

## Project Structure

```txt
components/   Reusable Node components (generic/abstract ones may live here too)
entities/     Game entities and their sub-nodes (animation, movement, states, controller)
systems/      Global/singleton-style nodes (selectors, input)
utils/        Pure C# utilities and abstract base types
scenes/       Scene files (.tscn)
materials/    Material resources
```

Each folder contains a subfolder per class, matching the class name in camelCase.

## Namespaces

- `SaintPatrick.Components` — components
- `SaintPatrick.Entities` — entities
- `SaintPatrick.Systems` — systems
- `SaintPatrick.Utils` — utilities

## Naming Conventions

- Classes: PascalCase
- Private fields: `_camelCase`
- Properties/methods: PascalCase
- Enums: PascalCase type, PascalCase members, prefixed with `E` (e.g. `EGender`)
- `[GlobalClass]` on every concrete Godot node class
- Export hints always include range, unit suffix, and `hide_control` where appropriate

## Code Style

- `this.` always used for member access
- `base.` always used for inherited member access
- `in` parameter modifier used for Vector3 and other structs passed by reference
- Expression-bodied members (`=>`) preferred for single-expression methods
- Block body used when method has more than one statement
- Partial classes used for all Godot node types (required by Godot C#)
- No IDE simplification diagnostics enforced (see `.editorconfig`)

## Architecture Patterns

### Component ownership

- Components are child nodes; they access their owning entity via `GetOwner<T>()`.
- Components are self-contained — they should not reach outside their owner's subtree.

### NodesTracker

- Utility for watching nodes entering/exiting a subtree by type and optionally by name.
- Used to loosely couple parent entities with their child components.
- Owners call `Track(root)` in `_EnterTree` and `Untrack()` in `_ExitTree`.
- Exposes `Node` (single), `Nodes` (set), `NodeTracked` event, `NodeUntracked` event.

### ObservableProperty

- Wraps a value and fires a `Changed` event only when the value actually changes.
- Used to expose reactive properties on entities (e.g. flags, enums).

### StatesMachine

- Generic abstract Node; concrete machines inherit and call `SetState<TState, TParams>`.
- States are plain classes (not Nodes), pooled via `ElementsPool`.
- Each state implements `OnInit`, `OnUpdate(delta)`, `OnDispose`, `ReadyToTransition`.
- Transitions are queued and processed in `_PhysicsProcess`; `force:true` bypasses `ReadyToTransition`.
- Same-type re-entry updates `StateParams` in-place without calling `OnDispose`/`OnInit`.

### ElementsPool

- Static generic object pool; used internally by `StatesMachine` to reuse state instances.

### Movement component

- Maintains `_direction` (normalized Vector3) and `_speed` (scalar float accumulator).
- `_pendingSpeedDelta` is set each frame by `Accelerate` (positive) or `Decelerate` (negative), reset to 0 at end of frame.
- `_PhysicsProcess` order: apply pending delta → clamp speed [0, MaxSpeed] → apply air friction when airborne → build velocity → apply gravity Y → MoveAndSlide.
- `MaxSpeed` is `protected set` so subclasses can override it per call.
- `Gravity` and `AirFriction` default from `ProjectSettings` (`physics/3d/default_gravity`, `physics/3d/default_linear_damp`).
- Subclasses extend by adding acceleration presets and setting `MaxSpeed` before calling `Accelerate`.

### CorrectedAnimationPlayer

- Abstract AnimationPlayer that lerps a child `Model` node's position based on the active animation, to correct root motion offsets.
- Subclasses implement `GetTargetPosition(animationName)`.

## Physics Processing

- All game logic runs in `_PhysicsProcess`, not `_Process`.
- `_EnterTree`/`_ExitTree` used for setup/teardown; `_Ready` used sparingly.

## Export Properties

- All exported properties use `{ get; private set; }` (or `protected set` when subclasses need to write).
- Default values sourced from `ProjectSettings` where a Godot engine default exists.
