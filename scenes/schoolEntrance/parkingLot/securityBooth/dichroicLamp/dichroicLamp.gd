extends Node3D

## A ceiling-mounted dichroic (recessed) spotlight lamp that casts
## a focused warm beam downward. Common in security booths and offices.
class_name DichroicLamp

var _light: SpotLight3D

func _ready() -> void:
	_light = $Light

## Turns the lamp on.
func turn_on() -> void:
	_light.visible = true

## Turns the lamp off.
func turn_off() -> void:
	_light.visible = false
