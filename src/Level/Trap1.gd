extends Node2D

onready var _spawner1 = $Spawner1
onready var _spawner2 = $Spawner2
onready var zombie = preload("res://src/Actors/zombie.tscn")

var _trapped = false

var max_count = 10

func _process(_delta: float) -> void:
	if _trapped and max_count > 0:
		var parent = get_parent()

		var z1 = zombie.instance()
		z1.view_radius = 1000
		z1.global_position = _spawner1.global_position
		parent.add_child(z1)
		
		var z2 = zombie.instance()
		z2.view_radius = 1000
		z2.global_position = _spawner2.global_position
		parent.add_child(z2)
		
		max_count -= 2

func OnPlayerTrapped(_body: Node) -> void:
	_trapped = true
	
func OnPlayerReleased(_body: Node) -> void:
	_trapped = false
