extends Node2D

var triggered = false

func _on_player_trapped(_body: Node) -> void:
	if not triggered:
		triggered = true
		$ZomTV.start()
		$ZomTV2.start()
		$ZomTV3.start()
