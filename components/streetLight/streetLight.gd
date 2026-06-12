extends Node3D

## A simple street light with a tall pole and a lamp head.
## Provides the ability to turn the light on or off.
class_name StreetLight

var _light: SpotLight3D

func _ready() -> void:
	_light = $LampHead/Light

## Turns the light on.
func turn_on() -> void:
	_light.visible = true

## Turns the light off.
func turn_off() -> void:
	_light.visible = false
