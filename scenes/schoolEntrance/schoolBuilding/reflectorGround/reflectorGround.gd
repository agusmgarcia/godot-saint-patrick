extends Node3D

## A tiny ground-mounted light reflector emitter that casts a focused
## colored beam upward. Designed for decorative accent lighting, such as
## Saint Patrick's green illumination on building facades.
class_name ReflectorGround

var _light: SpotLight3D

func _ready() -> void:
	_light = $Light

## Turns the reflector on.
func turn_on() -> void:
	_light.visible = true

## Turns the reflector off.
func turn_off() -> void:
	_light.visible = false
