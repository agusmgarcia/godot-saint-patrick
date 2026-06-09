extends Node3D

## A wall-mounted exterior light fixture that casts warm light
## both upward and downward, similar to a modern rectangular sconce.
class_name WallLight

var _light_up: SpotLight3D
var _light_down: SpotLight3D


func _ready() -> void:
	_light_up = $LightUp
	_light_down = $LightDown


## Turns the light on.
func turn_on() -> void:
	_light_up.visible = true
	_light_down.visible = true


## Turns the light off.
func turn_off() -> void:
	_light_up.visible = false
	_light_down.visible = false
