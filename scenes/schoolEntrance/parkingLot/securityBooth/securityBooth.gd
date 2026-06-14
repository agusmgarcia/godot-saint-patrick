extends Node3D

## Controls the security booth barrier arm, allowing it to open and close
## smoothly. The barrier collider is automatically disabled when the arm
## is open, letting the player pass through.
class_name SecurityBooth

## The rotation angle (in degrees on the Z-axis) when the barrier arm is fully closed.
const CLOSED_ANGLE: float = 0.0

## The speed (in degrees per second) at which the barrier arm moves when closing.
const CLOSED_SPEED: float = 100.0

## The rotation angle (in degrees on the Z-axis) when the barrier arm is fully open.
const OPEN_ANGLE: float = -50.0

## The speed (in degrees per second) at which the barrier arm moves when opening.
const OPEN_SPEED: float = 100.0

## Represents the possible states of the barrier arm.
enum BarrierState {CLOSED, OPEN, OPENING, CLOSING}

## Emitted when the barrier state changes. The [param state] parameter contains
## the new [enum BarrierState] value.
signal barrier_state_changed(state: BarrierState)

## The current state of the barrier.
var state: BarrierState:
	get:
		return _state

var _arm: Node3D
var _barrier_collider_shape: CollisionShape3D
var _dichroic_lamp: DichroicLamp
var _target_angle: float
var _state: BarrierState

func _ready() -> void:
	_arm = $Barrier/Arm
	_barrier_collider_shape = $Barrier/Collider/Shape
	_dichroic_lamp = $Roof/DichroicLamp
	_barrier_collider_shape.disabled = false
	_target_angle = CLOSED_ANGLE
	_state = BarrierState.CLOSED

func _process(delta: float) -> void:
	if _state != BarrierState.OPENING and _state != BarrierState.CLOSING:
		return

	var current_angle: float = _arm.rotation_degrees.z
	var current_speed: float = OPEN_SPEED if _state == BarrierState.OPENING else CLOSED_SPEED
	var new_angle: float = move_toward(current_angle, _target_angle, current_speed * delta)

	_arm.rotation_degrees.z = new_angle
	
	if not is_equal_approx(new_angle, _target_angle):
		return

	_arm.rotation_degrees.z = _target_angle

	if _state == BarrierState.OPENING:
		_barrier_collider_shape.disabled = true
		_state = BarrierState.OPEN
		barrier_state_changed.emit(_state)
	elif _state == BarrierState.CLOSING:
		_barrier_collider_shape.disabled = false
		_state = BarrierState.CLOSED
		barrier_state_changed.emit(_state)

## Smoothly opens the barrier arm. Does nothing if the barrier is already
## open or in the process of opening.
func open() -> void:
	if _state == BarrierState.OPEN or _state == BarrierState.OPENING:
		return

	_barrier_collider_shape.disabled = false
	_state = BarrierState.OPENING
	_target_angle = OPEN_ANGLE
	barrier_state_changed.emit(_state)

## Smoothly closes the barrier arm. Does nothing if the barrier is already
## closed or in the process of closing.
func close() -> void:
	if _state == BarrierState.CLOSED or _state == BarrierState.CLOSING:
		return

	_barrier_collider_shape.disabled = false
	_state = BarrierState.CLOSING
	_target_angle = CLOSED_ANGLE
	barrier_state_changed.emit(_state)

## Turns on the interior dichroic lamp.
func turn_on_light() -> void:
	_dichroic_lamp.turn_on()

## Turns off the interior dichroic lamp.
func turn_off_light() -> void:
	_dichroic_lamp.turn_off()
