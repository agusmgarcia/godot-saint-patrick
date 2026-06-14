extends Node3D

## A tall reflector post for illuminating large areas like parking lots.
## Similar to a street light but with significantly higher light intensity.
class_name ReflectorPost

var _light: SpotLight3D

func _ready() -> void:
	_light = $LampHead/Light

## Turns the light on.
func turn_on() -> void:
	_light.visible = true

## Turns the light off.
func turn_off() -> void:
	_light.visible = false
