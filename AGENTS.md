# AGENTS.md

## Project Overview

This is a **Godot 4.6** 3D game project using the **GL Compatibility** renderer and **Jolt Physics** engine. The project follows a component-based architecture where reusable game elements are self-contained units.

---

## Project Structure

```txt
components/       → Reusable game components (scenes + scripts)
materials/        → Shared PBR materials (texture maps + .tres definitions)
```

---

## Conventions

### Naming

- **camelCase** is used for all file and folder names (e.g., `securityBooth.gd`, `exposedBrickWall.tres`).
- Component folders are named after the component itself (e.g., `components/securityBooth/`).
- Material folders are named after the material (e.g., `materials/concrete/`).
- Texture files follow the pattern: `<materialName>.<mapType>.jpg` (e.g., `concrete.color.jpg`, `concrete.normalGL.jpg`, `concrete.roughness.jpg`).

### File Encoding & Formatting

- UTF-8 charset for all files (`.editorconfig`).
- LF line endings enforced via `.gitattributes`.

---

## Components

Each component lives in its own folder under `components/` and consists of:

1. **A `.tscn` scene file** — the visual/physical structure of the component.
2. **A `.gd` script file** — the behavior/logic attached to the root node.
3. **A `.gd.uid` file** — Godot's internal UID tracking (auto-generated, do not edit).

### Component Design Rules

- The **root node** of a component must be of type `Node3D` and should have a `class_name` declared in its script for easy referencing. The class name should be the component name in **PascalCase**.
- **All direct child nodes within a component must be positioned relative to the root, with the root itself expected to sit at the origin `(0, 0, 0)`.** The component should be built as if its center or anchor point is at the origin. Any world-space positioning (where the component actually goes in a level or scene) must be handled by the **parent/consumer** of the component — not inside the component itself. This keeps components reusable and composable: they don't carry assumptions about where they'll be placed.
- **Keep each node's local origin (pivot point) at `(0, 0, 0)` whenever possible.** Position the node in the world by adjusting its transform from the parent level, not by offsetting geometry away from the node's own origin. This ensures rotations, scaling, and animations behave predictably around a clean center point. When a node absolutely needs a non-zero pivot (e.g., a door hinge offset to one edge), that's acceptable — but it should be a deliberate choice, not an accident.
- **Never change a node's local rotation center (pivot point) when modifying a component.** The rotation center of each node is an intentional design decision that affects how the component animates and behaves. If a change genuinely requires moving the pivot/rotation center of a node, you must ask the user for explicit approval before proceeding.

### Script Patterns

- Scripts use `extends Node3D` at the top, followed by a `class_name` declaration.
- Doc comments (`##`) are used for public API (class description, public methods, public properties).
- Constants are declared at the top using `UPPER_SNAKE_CASE`.
- Enums are declared after constants.
- Signals are declared after enums.
- Public properties (with getters) come before private variables.
- Private variables use the `_` prefix convention (e.g., `_state`, `_arm`).
- Node references are resolved in `_ready()` using `$Path/To/Node` syntax.
- State machines are implemented with enums and checked in `_process()`.

### Scene Structure Patterns

- Logical grouping nodes (e.g., `Walls`, `Roof`, `Floor`, `Barrier`) organize child meshes and colliders.
- Each physical surface has a `StaticBody3D` > `CollisionShape3D` hierarchy for collision.
- `MeshInstance3D` nodes are used for visual representation, often with `surface_material_override` to apply materials.
- Colliders are named `Collider` and their shapes are named `Shape` for consistency.

---

## Materials

Materials live under `materials/<materialName>/` and consist of:

1. **A `.tres` resource file** — the `StandardMaterial3D` definition referencing the texture maps.
2. **Texture map files** (`.jpg`) — PBR maps following this naming:
   - `<name>.color.jpg` — Albedo/diffuse
   - `<name>.normalGL.jpg` — Normal map (OpenGL format)
   - `<name>.roughness.jpg` — Roughness
   - `<name>.ambientOcclusion.jpg` — AO
   - `<name>.displacement.jpg` — Height/displacement
   - `<name>.metalness.jpg` — Metallic (only when applicable)

### Material Sourcing

Textures are sourced from [ambientCG](https://ambientcg.com) using their public API. When a new component needs a material that doesn't already exist in `materials/`:

1. Search ambientCG for the best matching texture based on the component's description.
2. Present the chosen texture to the user for **yes/no approval** before downloading.
3. Download the **1K JPG** variant.
4. Extract and rename the texture maps to match the project naming convention (`<name>.color.jpg`, `<name>.normalGL.jpg`, etc.).
5. Create the `.tres` material resource file wiring up all available maps.

If a suitable material already exists in `materials/`, reuse it instead of downloading a new one.

### Material Resource Patterns

- External resource IDs in `.tres` files use descriptive names (e.g., `"Color"`, `"NormalGL"`, `"Roughness"`, `"AmbientOcclusion"`, `"Displacement"`, `"Metalness"`).

---

## Physics

- The project uses **Jolt Physics** as the 3D physics engine.
- Collision shapes are simplified approximations (e.g., full-wall `BoxShape3D`) even when the visual mesh has openings (like windows), to keep physics performant.

---

## Characters

Characters live under `characters/` and each has its own folder (e.g., `characters/helena/`).

### Base Script

A shared base script exists at `characters/character/character.gd` with `class_name Character`. It provides:

- A state machine (currently with `IDLE` state).
- A `Gender` enum (`FEMALE`, `MALE`) exposed as an `@export` property to determine which animation set to use.
- Random animation selection within each state (e.g., picks one of 3 idle animations randomly, looping indefinitely).
- A public `idle()` method to command the character from outside.

All character scripts must `extends Character`.

### Character Scene Structure

Each character scene (`.tscn`) is **self-contained** — it does NOT inherit from a base scene. The structure is:

```bash
<CharacterName> (instance of <characterName>.fbx)
  ├── Skeleton3D (comes from FBX)
  └── AnimationPlayer
        └── libraries:
              femaleIdle1 = res://animations/femaleIdle1.fbx
              femaleIdle2 = res://animations/femaleIdle2.fbx
              femaleIdle3 = res://animations/femaleIdle3.fbx
              (+ all other shared animations)
```

### Animation Rules

- **Every character must have an `AnimationPlayer` node with ALL shared animations loaded as libraries.** When a new animation is added to the project, it must be added to every character's `AnimationPlayer`.
- Animation libraries are stored in `animations/` and follow the naming pattern: `<gender><ActionName><number>.fbx` (e.g., `femaleIdle1.fbx`, `maleIdle2.fbx`).
- Animation track names follow Mixamo convention: `<libraryName>/mixamo_com` (e.g., `femaleIdle1/mixamo_com`).
- The `AnimationPlayer` must be a direct child of the character root node so the base script can resolve it via `$AnimationPlayer`.

### Adding a New Character

1. Create a folder under `characters/<characterName>/`.
2. Place the character's FBX model as `<characterName>.fbx`.
3. Create a `.tscn` scene that instances the FBX as the root node.
4. Add an `AnimationPlayer` as a child with **all** shared animation libraries loaded (copy from an existing character).
5. Create a `.gd` script that `extends Character` with `class_name <CharacterName>`.
6. Set the `gender` export in the scene's inspector to match the character.

---

## Key Principles

1. **Components are origin-anchored** — build everything around `(0, 0, 0)` and let the consumer place it in the world.
2. **Separation of visual and physical** — meshes are for rendering, colliders are for physics; they're siblings under a common parent node.
3. **Shared materials are external resources** — materials are defined once in `materials/` and referenced across scenes, promoting reuse and consistency.
4. **Self-documenting scripts** — public API uses `##` doc comments; internal implementation uses regular comments or TODOs.
