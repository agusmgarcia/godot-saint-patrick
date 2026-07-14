extends Node3D # Or CharacterBody3D if you changed the root type

@onready var animation_player: AnimationPlayer = $AnimationPlayer

func _ready() -> void:
	# Replace "idle" with the exact name of your animation track
	animation_player.play("idle/mixamo_com")