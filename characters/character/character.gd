extends CharacterBody3D

## Base class for all characters. Provides a state machine
## with animation integration. The AnimationPlayer is created
## programmatically at runtime — subclass scenes do NOT need
## to include one manually.
class_name Character


# <================================ PUBLIC =================================> #

## The gender of the character. Determines which animation set to use.
@export var gender: Gender = Gender.FEMALE

## When enabled, idle and walk use drunk animations.
@export var drunk := false


## Instructs the character to enter the idle state.
func idle() -> void:
	if drunk:
		_state = State.IDLE_DRUNK
		velocity = Vector3.ZERO
		_timer.start(10.0)
		_play_random_animation(_animation_player, ANIMATIONS[State.IDLE_DRUNK][gender])
	else:
		_state = State.IDLE
		velocity = Vector3.ZERO
		_timer.start(10.0)
		_play_random_animation(_animation_player, ANIMATIONS[State.IDLE][gender])


## Instructs the character to enter the talk state. The character
## will loop through random talking animations indefinitely.
func talk() -> void:
	_state = State.TALK
	velocity = Vector3.ZERO
	_timer.stop()
	_play_random_animation(_animation_player, ANIMATIONS[State.TALK][gender])


## Instructs the character to walk to a random point on the navigation mesh.
func walk(target_position: Vector3) -> void:
	if drunk:
		_state = State.WALK_DRUNK
		_timer.stop()
		_navigation_agent.target_position = target_position
		_play_random_animation(_animation_player, ANIMATIONS[State.WALK_DRUNK][gender])
	else:
		_state = State.WALK
		_timer.stop()
		_navigation_agent.target_position = target_position
		_play_random_animation(_animation_player, ANIMATIONS[State.WALK][gender])
	

# <================================= ENUMS ==================================> #

## The character's gender, used to select the correct animation set.
enum Gender {FEMALE, MALE}

## All possible states in the character's state machine.
enum State {IDLE, IDLE_DRUNK, FLY_REMOVAL, WALK, WALK_DRUNK, TALK}

## Walking speed in meters per second.
const WALK_SPEED := 1.4

## Drunk walking speed in meters per second.
const DRUNK_WALK_SPEED := 0.9

## A dictionary with all the animations, discriminated by state and gender.
const ANIMATIONS: Dictionary = {
	State.IDLE: {
		Gender.FEMALE: [
			"femaleIdle1/mixamo_com",
			"femaleIdle2/mixamo_com",
			"femaleIdle3/mixamo_com",
		],
		Gender.MALE: [],
	},
	State.IDLE_DRUNK: {
		Gender.FEMALE: [
			"femaleDrunkIdle1/mixamo_com",
		],
		Gender.MALE: [],
	},
	State.FLY_REMOVAL: {
		Gender.FEMALE: [
			"femaleFlyRemoval1/mixamo_com"
		],
		Gender.MALE: []
	},
	State.WALK: {
		Gender.FEMALE: [
			"femaleWalk1/mixamo_com"
		],
		Gender.MALE: []
	},
	State.WALK_DRUNK: {
		Gender.FEMALE: [
			"femaleDrunkWalk1/mixamo_com"
		],
		Gender.MALE: []
	},
	State.TALK: {
		Gender.FEMALE: [
			"femaleTalking1/mixamo_com",
			"femaleTalking2/mixamo_com",
			"femaleTalking3/mixamo_com",
		],
		Gender.MALE: [],
	}
}


# <================================ PRIVATE =================================> #

## The current state of the character's state machine.
var _state: State

## The AnimationPlayer instance created at runtime.
var _animation_player: AnimationPlayer

## One-shot timer used to trigger random behaviors (e.g., fly removal).
var _timer: Timer

## The navigation agent used for pathfinding during walk states.
var _navigation_agent: NavigationAgent3D

## Initializes the animation player, timer, navigation agent, and
## enters the idle state.
func _ready() -> void:
	_animation_player = _create_animation_player(gender)
	_animation_player.animation_finished.connect(_on_animation_finished)
	add_child(_animation_player)

	_timer = Timer.new()
	_timer.one_shot = true
	_timer.timeout.connect(_on_timer_timeout)
	add_child(_timer)

	_navigation_agent = NavigationAgent3D.new()
	_navigation_agent.avoidance_enabled = false
	add_child(_navigation_agent)

	talk()


## Handles per-frame movement logic based on the current state.
func _physics_process(delta: float) -> void:
	match _state:
		State.IDLE:
			return

		State.IDLE_DRUNK:
			return

		State.FLY_REMOVAL:
			return

		State.TALK:
			return

		State.WALK, State.WALK_DRUNK:
			if _navigation_agent.is_navigation_finished():
				idle()
				return

			var direction := (_navigation_agent.get_next_path_position() - global_position).normalized()
			if direction.length() > 0.01:
				var target_rotation := atan2(direction.x, direction.z)
				rotation.y = lerp_angle(rotation.y, target_rotation, delta * 8.0)

			velocity = direction * (WALK_SPEED if !drunk else DRUNK_WALK_SPEED)
			move_and_slide()
			return


## Called when the current animation finishes. Picks the next
## animation based on the current state.
func _on_animation_finished(_animation_name: StringName) -> void:
	match _state:
		State.IDLE:
			_play_random_animation(_animation_player, ANIMATIONS[State.IDLE][gender], 2)
			return

		State.IDLE_DRUNK:
			_play_random_animation(_animation_player, ANIMATIONS[State.IDLE_DRUNK][gender], 2)
			return

		State.FLY_REMOVAL:
			idle()
			return
		
		State.TALK:
			_play_random_animation(_animation_player, ANIMATIONS[State.TALK][gender])
			return

		State.WALK:
			_play_random_animation(_animation_player, ANIMATIONS[State.WALK][gender])
			return

		State.WALK_DRUNK:
			_play_random_animation(_animation_player, ANIMATIONS[State.WALK_DRUNK][gender])
			return
		

## Called when the idle timer expires. May trigger a random
## behavior like fly removal.
func _on_timer_timeout() -> void:
	match _state:
		State.IDLE:
			if randf() < 0.15:
				_state = State.FLY_REMOVAL
				_timer.stop()
				_play_random_animation(_animation_player, ANIMATIONS[State.FLY_REMOVAL][gender])
			else:
				_timer.start(10.0)
			return

		State.IDLE_DRUNK:
			_timer.start(10.0)
			return

		State.FLY_REMOVAL:
			return

		State.TALK:
			return
			
		State.WALK:
			return

		State.WALK_DRUNK:
			return


# <================================ STATIC =================================> #

## Plays a random animation using the animation_player.
## Picks a random animation from the animations array.
static func _play_random_animation(animation_player: AnimationPlayer, animations: Array, custom_blend: float = 0.5) -> void:
	var animation_name: String = animations.pick_random()
	animation_player.play(animation_name, custom_blend)


## Creates an AnimationPlayer with all animation libraries matching
## the character's gender. Libraries are loaded once and cached in a
## static variable so that subsequent instances skip the filesystem scan.
static func _create_animation_player(p_gender: Gender) -> AnimationPlayer:
	var animation_player := AnimationPlayer.new()
	animation_player.name = "AnimationPlayer"
	animation_player.root_node = NodePath("../Model")

	var libraries := _get_animation_libraries(p_gender)
	for library_name: String in libraries:
		animation_player.add_animation_library(library_name, libraries[library_name].duplicate())

	return animation_player


## Holds animation libraries in a dictionary, so they are loaded
## once per gender.
static var _animation_libraries_cache: Dictionary = {}


## Loads and caches animation libraries for the given gender.
## Uses the ANIMATIONS dictionary to determine exactly which
## libraries to load. On the first call the libraries are loaded
## from disk; subsequent calls return the cached result instantly.
static func _get_animation_libraries(p_gender: Gender) -> Dictionary:
	if _animation_libraries_cache.has(p_gender):
		return _animation_libraries_cache[p_gender]

	const ANIMATIONS_DIR := "res://animations/"
	var libraries: Dictionary = {}

	for state: State in ANIMATIONS:
		var animation_names: Array = ANIMATIONS[state][p_gender]
		for animation_name: String in animation_names:
			var library_name := animation_name.split("/")[0]
			if not libraries.has(library_name):
				var library = load(ANIMATIONS_DIR + library_name + ".fbx")
				assert(library != null, "Animation library '%s' not found at '%s'" % [library_name, ANIMATIONS_DIR + library_name + ".fbx"])
				libraries[library_name] = library

	_animation_libraries_cache[p_gender] = libraries
	return libraries
