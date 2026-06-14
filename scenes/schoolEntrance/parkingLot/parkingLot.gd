extends Node3D

## The school parking lot area with reflector post lighting.
class_name ParkingLot

var _reflector_post_lights: Array[ReflectorPost] = []

func _ready() -> void:
	var lights_node = $Surface/ReflectorPosts
	for child in lights_node.get_children():
		if child is ReflectorPost:
			_reflector_post_lights.append(child)

## Turns all reflector post lights on.
func turn_on_lights() -> void:
	for light in _reflector_post_lights:
		light.turn_on()

## Turns all reflector post lights off.
func turn_off_lights() -> void:
	for light in _reflector_post_lights:
		light.turn_off()
