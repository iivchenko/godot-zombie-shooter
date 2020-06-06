tool
extends Node2D

func _process(_delta: float) -> void:
	update()
	
func _draw() -> void:	
	zombie_draw(self.get_parent())
	
func zombie_draw(node):
	for child in node.get_children():
		if child.name.begins_with("Zombie"):
			draw_circle(child.position, child.ViewRadius, Color(1, 1, 1, 0.25))
		if child.get_child_count() > 0:
			zombie_draw(child)
