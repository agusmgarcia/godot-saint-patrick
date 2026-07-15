extends CharacterBody3D

## Base class for all characters. Provides a state machine
## with animation integration. The AnimationPlayer is created
## programmatically at runtime — subclass scenes do NOT need
## to include one manually.
class_name Character


# <================================ PUBLIC =================================> #

## The gender of the character. Determines which animation set to use.
@export var gender: Gender = Gender.FEMALE


## Instructs the character to enter the idle state.
func idle() -> void:
	_set_state(State.IDLE)


# <================================= ENUMS ==================================> #

enum Gender {FEMALE, MALE}

enum State {IDLE, FLY_REMOVAL}


# <================================ PRIVATE =================================> #

var _state: State
var _animation_player: AnimationPlayer
var _timer: Timer

func _ready() -> void:
	_animation_player = _create_animation_player(gender)
	_animation_player.animation_finished.connect(_on_animation_finished)
	add_child(_animation_player)

	_timer = Timer.new()
	_timer.one_shot = true
	_timer.timeout.connect(_on_timer_timeout)
	add_child(_timer)

	idle()


func _set_state(new_state: State) -> void:
	_state = new_state

	match _state:
		State.IDLE:
			_timer.start(10.0)
			_play_random_animation(_animation_player, _ANIMATIONS[State.IDLE][gender], 2)

		State.FLY_REMOVAL:
			_timer.stop()
			_play_random_animation(_animation_player, _ANIMATIONS[State.FLY_REMOVAL][gender], 0.5)


func _on_animation_finished(_animation_name: StringName) -> void:
	match _state:
		State.IDLE:
			_play_random_animation(_animation_player, _ANIMATIONS[State.IDLE][gender], 2)

		State.FLY_REMOVAL:
			idle()


func _on_timer_timeout() -> void:
	match _state:
		State.IDLE:
			if randf() < 0.15:
				_set_state(State.FLY_REMOVAL)
			else:
				_timer.start(10.0)

		State.FLY_REMOVAL:
			pass


# <================================ STATIC =================================> #

## Plays a random animation using the animation_player.
## Picks a random animation from the animations array.
static func _play_random_animation(animation_player: AnimationPlayer, animations: Array, custom_blend: float = -1) -> void:
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
## Uses the _ANIMATIONS dictionary to determine exactly which
## libraries to load. On the first call the libraries are loaded
## from disk; subsequent calls return the cached result instantly.
static func _get_animation_libraries(p_gender: Gender) -> Dictionary:
	if _animation_libraries_cache.has(p_gender):
		return _animation_libraries_cache[p_gender]

	const ANIMATIONS_DIR := "res://animations/"
	var libraries: Dictionary = {}

	for state: State in _ANIMATIONS:
		var animation_names: Array = _ANIMATIONS[state][p_gender]
		for animation_name: String in animation_names:
			var library_name := animation_name.split("/")[0]
			if not libraries.has(library_name):
				var library = load(ANIMATIONS_DIR + library_name + ".fbx")
				assert(library != null, "Animation library '%s' not found at '%s'" % [library_name, ANIMATIONS_DIR + library_name + ".fbx"])
				libraries[library_name] = library

	_animation_libraries_cache[p_gender] = libraries
	return libraries


## A dictionary with all the animations, discriminated by state and gender.
const _ANIMATIONS: Dictionary = {
	State.IDLE: {
		Gender.FEMALE: [
			"femaleIdle1/mixamo_com",
			"femaleIdle2/mixamo_com",
			"femaleIdle3/mixamo_com",
		],
		Gender.MALE: [],
	},
	State.FLY_REMOVAL: {
		Gender.FEMALE: [
			"femaleFlyRemoval1/mixamo_com"
		],
		Gender.MALE: []
	}
}