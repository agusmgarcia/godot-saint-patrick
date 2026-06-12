extends Node3D

## Manages the street block area including sidewalks, streets, and street lights.
## Provides the ability to turn all street lights on or off.
class_name StreetBlock

var _street_lights: Array[StreetLight] = []

func _ready() -> void:
	var lights_node = $StreetLights
	for child in lights_node.get_children():
		if child is StreetLight:
			_street_lights.append(child)

## Turns all street lights on.
func turn_on_lights() -> void:
	for light in _street_lights:
		light.turn_on()

## Turns all street lights off.
func turn_off_lights() -> void:
	for light in _street_lights:
		light.turn_off()
