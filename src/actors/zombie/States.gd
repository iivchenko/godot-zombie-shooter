tool
extends Node2D

export var fakeViewRadius = 100;

func _draw() -> void:
	if Engine.editor_hint:
		draw_circle(Vector2.ZERO, fakeViewRadius, Color(1, 1, 1, 0.5))
