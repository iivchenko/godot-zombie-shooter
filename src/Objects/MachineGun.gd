extends KinematicBody2D

export var ammo = 30

var _player = null
var _life = true

onready var _interact_state = $States/Interact

func _on_InteractionArea_player_entered(body: Node) -> void:
	_player = body
	_player.connect("InteractedSignal", self, "_interact")
	_interact_state.set_process(true)
	_interact_state.visible = true
	

func _on_InteractionArea_player_exited(_body: Node) -> void:
	_player.disconnect("InteractedSignal", self, "_interact")
	_player = null
	_interact_state.set_process(false)
	_interact_state.visible = false
	

func _interact() -> void:
	if _life:
		_life = false
		_player.AddMachineGun(ammo)
		queue_free()
