extends Node3D

## The main school building with an entrance light bar that can be
## toggled on and off.
class_name SchoolBuilding

var _light: SpotLight3D

func _ready() -> void:
	_light = $SecondFloor/EntranceOverhang/LightBar/Light

## Turns the entrance light on.
func turn_on_light() -> void:
	_light.visible = true

## Turns the entrance light off.
func turn_off_light() -> void:
	_light.visible = false
