extends Node2D

onready var zombie = preload("res://src/actors/zombie.tscn")

var _trapped = false

var max_count = 1

func _process(_delta: float) -> void:
	if _trapped and max_count > 0:
		var parent = get_parent()

		var z1 = zombie.instance()
		z1.position = position
		parent.add_child(z1)
		z1.view_radius = 1000
		
		max_count -= 1

func OnPlayerTrapped(_body: Node) -> void:
	_trapped = true
	
func OnPlayerReleased(_body: Node) -> void:
	_trapped = false
