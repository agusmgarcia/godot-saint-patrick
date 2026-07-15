extends CharacterBody3D

## Base class for all characters. Provides a state machine
## with animation integration.
class_name Character

enum Gender {FEMALE, MALE}

enum State {IDLE}

const IDLE_ANIMATIONS: Dictionary = {
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

## The gender of the character. Determines which animation set to use.
@export var gender: Gender = Gender.FEMALE

var _state: State = State.IDLE
var _animation_player: AnimationPlayer

func _ready() -> void:
    _animation_player = $AnimationPlayer
    _animation_player.animation_finished.connect(_on_animation_finished)
    idle()

## Instructs the character to enter the idle state.
func idle() -> void:
   _on_state_set(State.IDLE)

func _on_state_set(new_state: State) -> void:
    _state = new_state
    match _state:
        State.IDLE:
            _play_random_animation(IDLE_ANIMATIONS[gender])

func _on_animation_finished(_anim_name: StringName) -> void:
    match _state:
        State.IDLE:
            _play_random_animation(IDLE_ANIMATIONS[gender])
            
func _play_random_animation(animations: Array) -> void:
    var anim_name: String = animations.pick_random()
    _animation_player.play(anim_name, 2)
