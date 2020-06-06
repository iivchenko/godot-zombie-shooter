extends Node2D

func _ready() -> void:
	var zombies = get_children()
	
	for i in zombies.size():
		zombies[i].connect("HitedSignal", self, "_on_alarm")

func _on_alarm() -> void:
	var zombies = get_children()
	
	for i in zombies.size():
		zombies[i].ViewRadius = 1000
