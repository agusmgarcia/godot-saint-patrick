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

enum State {IDLE}


# <================================ PRIVATE =================================> #

var _state: State
var _animation_player: AnimationPlayer

func _ready() -> void:
	_animation_player = _create_animation_player(gender)
	_animation_player.animation_finished.connect(_on_animation_finished)
	add_child(_animation_player)

	idle()


func _set_state(new_state: State) -> void:
	_state = new_state
	match _state:
		State.IDLE:
			_play_random_animation(_animation_player, _ANIMATIONS[State.IDLE][gender], 2)


func _on_animation_finished(_animation_name: StringName) -> void:
	match _state:
		State.IDLE:
			_play_random_animation(_animation_player, _ANIMATIONS[State.IDLE][gender], 2)


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


## Holds animation libraries in a dictionary, so the animations directory
## is scanned once per gender.
static var _animation_libraries_cache: Dictionary = {}


## Loads and caches animation libraries for the given gender.
## On the first call the animations directory is scanned; subsequent
## calls return the cached result instantly.
static func _get_animation_libraries(p_gender: Gender) -> Dictionary:
	if _animation_libraries_cache.has(p_gender):
		return _animation_libraries_cache[p_gender]

	const ANIMATIONS_DIR := "res://animations/"
	var libraries: Dictionary = {}

	var animation_directory := DirAccess.open(ANIMATIONS_DIR)
	if not animation_directory:
		_animation_libraries_cache[p_gender] = libraries
		return libraries

	animation_directory.list_dir_begin()

	var gender_prefix: String = "female" if p_gender == Gender.FEMALE else "male"
	var file_name := animation_directory.get_next()

	while file_name != "":
		if not animation_directory.current_is_dir() and file_name.begins_with(gender_prefix) and file_name.ends_with(".fbx"):
			var library_name := file_name.get_basename()
			var library := load(ANIMATIONS_DIR + file_name)
			if library:
				libraries[library_name] = library

		file_name = animation_directory.get_next()

	animation_directory.list_dir_end()

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
		Gender.MALE: [
			"maleIdle1/mixamo_com",
			"maleIdle2/mixamo_com",
			"maleIdle3/mixamo_com",
		],
	}
}