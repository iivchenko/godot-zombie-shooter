extends KinematicBody2D

signal interacted
signal exited
signal state_changed
signal life_changed
signal player_killed

enum { STAND, SIMPLE_GUN, GOOD_GUN, MACHINE_GUN }

onready var _target_ray = $TargetRay
onready var _target = $TargetRay/Target

onready var _step = $Audio/Step
onready var _gun_shot = $Audio/GunShot
onready var _no_ammo = $Audio/NoAmmo
onready var _hit_sound = $Audio/Hit
onready var _blood_effect = $Blood

onready var _stand_node = $State/Stand
onready var _simple_gun_node = $State/SimpleGun
onready var _good_gun_node = $State/GoodGun
onready var _machine_gun_node = $State/MachineGun

var _state = STAND
var _states = null

export var life = 100 setget life_set, life_get
var _audio = false
export var _max_speed = 100.0
var _velocity = Vector2.ZERO
var _shoot_delay = 0.0

var _simple_gun_ammo = -1
var _good_gun_ammo = -1
var _machine_gun_ammo = -1

var _simple_gun_damage = 25
var _good_gun_damage = 40
var _machine_gun_damage = 75

var _simple_gun_delay = 0.6
var _good_gun_delay = 0.2
var _machine_gun_delay = 0.01

onready var _hit_factory = preload("res://src/effects/hit.tscn")

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_HIDDEN)


func _process(delta) -> void:
	_shoot_delay = max(0.0, _shoot_delay - delta)
	
	if _velocity != Vector2.ZERO and _audio == false:
		_audio = true
		_step.play()

	_target.global_position = get_global_mouse_position()
	_target_ray.cast_to = _target.position

	if Input.is_action_just_pressed("player_shoot") and _is_shoot_state():
		if _has_ammo(_state) and _no_shoot_delay():
			var hit = _hit_factory.instance()
			hit.damage = _get_damage(_state)
			hit.global_position = _target_ray.get_collision_point() if _target_ray.is_colliding() else _target.global_position
			get_tree().current_scene.add_child(hit)
			hit.hit()
			_gun_shot.play()
			_update_ammo(-1)
			_shoot_delay = _get_shoot_delay(_state)
		elif not _has_ammo(_state):
			_no_ammo.play()
	
	if Input.is_action_just_pressed("player_select_simple_gun") and _has_ammo(SIMPLE_GUN):
		_switch(SIMPLE_GUN)
	
	if Input.is_action_just_pressed("player_select_good_gun") and _has_ammo(GOOD_GUN):
		_switch(GOOD_GUN)
		
	if Input.is_action_just_pressed("player_select_machine_gun") and _has_ammo(MACHINE_GUN):
		_switch(MACHINE_GUN)


func _physics_process (_delay) -> void:
	look_at(get_global_mouse_position())
	
	var direction = Vector2(
		Input.get_action_strength("player_move_right") - Input.get_action_strength("player_move_left"), 
		Input.get_action_strength("player_move_down") - Input.get_action_strength("player_move_up")) 
		
	_velocity = direction * _max_speed	
	_velocity = move_and_slide(_velocity)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey:
		if event.is_action_pressed("interact"):
			emit_signal("interacted")
		elif event.is_action_pressed("player_exit"):
			emit_signal("exited")


func life_set(value: int) -> void:
	life = value
	emit_signal("life_changed", life)


func life_get() -> int:
	return life


func _is_shoot_state() -> bool:
	match _state:
		SIMPLE_GUN, GOOD_GUN, MACHINE_GUN:
			return true
		_:
			return false


func _has_ammo (state) -> bool:
	match state:
		SIMPLE_GUN:
			return true if _simple_gun_ammo > 0 else false
		GOOD_GUN:
			return true if _good_gun_ammo > 0 else false
		MACHINE_GUN:
			return true if _machine_gun_ammo > 0 else false
		_:
			return false


func _no_shoot_delay() -> bool:
	return _shoot_delay <= 0.0


func _get_damage(state) -> int: 
	match state:
		SIMPLE_GUN:
			return _simple_gun_damage
		GOOD_GUN:
			return _good_gun_damage
		MACHINE_GUN:
			return _machine_gun_damage
		_:
			return 0


func _get_ammo(state) -> int: 
	match state:
		SIMPLE_GUN:
			return _simple_gun_ammo
		GOOD_GUN:
			return _good_gun_ammo
		MACHINE_GUN:
			return _machine_gun_ammo
		_:
			return 0


func _get_shoot_delay(state) -> float:
	match state:
		SIMPLE_GUN:
			return _simple_gun_delay
		GOOD_GUN:
			return _good_gun_delay
		MACHINE_GUN:
			return _machine_gun_delay
		_:
			return 0.0


func add_simple_gun(ammo: int) -> void:
	
	_switch(SIMPLE_GUN)
	_update_ammo(ammo)


func add_good_gun(ammo: int) -> void:
	_switch(GOOD_GUN)
	_update_ammo(ammo)


func add_machine_gun(ammo: int) -> void:
	_switch(MACHINE_GUN)
	_update_ammo(ammo)


func _disable_state(state) -> void:
	match state:
		STAND:
			_stand_node.set_process(false)
			_stand_node.visible = false
		SIMPLE_GUN:
			_simple_gun_node.set_process(false)
			_simple_gun_node.visible = false
		GOOD_GUN:
			_good_gun_node.set_process(false)
			_good_gun_node.visible = false
		MACHINE_GUN:
			_machine_gun_node.set_process(false)
			_machine_gun_node.visible = false


func _enable_state(state) -> void:
	match state:
		STAND:
			_stand_node.set_process(true)
			_stand_node.visible = true
		SIMPLE_GUN:
			_simple_gun_node.set_process(true)
			_simple_gun_node.visible = true
		GOOD_GUN:
			_good_gun_node.set_process(true)
			_good_gun_node.visible = true
		MACHINE_GUN:
			_machine_gun_node.set_process(true)
			_machine_gun_node.visible = true


func _switch(state) -> void:
	_disable_state(_state)
	_enable_state(state)
	_state = state
	
	emit_signal("state_changed", _state, _get_ammo(_state))


func _update_ammo(ammo: int) -> void:
	match _state:
		SIMPLE_GUN:
			_simple_gun_ammo += ammo
		GOOD_GUN:
			_good_gun_ammo += ammo
		MACHINE_GUN:
			_machine_gun_ammo += ammo

	emit_signal("state_changed", _state, _get_ammo(_state))


func _on_step_audio_finished() -> void:
	_audio = false


func hit(damage: int) -> void:
	life = max(life - damage, 0)

	emit_signal("life_changed", life)
	_hit_sound.play()
	_blood_effect.emitting = true
	
	if life <= 0:
		emit_signal("player_killed")
