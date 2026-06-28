extends Node3D

## The main school building with an entrance light bar and ground
## reflectors that can be toggled on and off.
class_name SchoolBuilding

var _light: SpotLight3D
var _reflectors: Array[ReflectorGround] = []

func _ready() -> void:
	_light = $SecondFloor/EntranceOverhang/LightBar/Light
	_reflectors.append($GrassSection/RightFront/Reflector1)
	_reflectors.append($GrassSection/RightFront/Reflector2)
	_reflectors.append($GrassSection/LeftFront/Reflector1)
	_reflectors.append($GrassSection/LeftFront/Reflector2)

## Turns the entrance light and reflectors on.
func turn_on_light() -> void:
	_light.visible = true
	for reflector in _reflectors:
		reflector.turn_on()

## Turns the entrance light and reflectors off.
func turn_off_light() -> void:
	_light.visible = false
	for reflector in _reflectors:
		reflector.turn_off()
