tool
extends Node2D

export var color = Color.white

func _draw() -> void:
	if Engine.editor_hint:
		draw_circle(Vector2(0, 0), 10, color)
