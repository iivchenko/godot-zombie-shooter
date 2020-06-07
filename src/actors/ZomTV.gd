extends StaticBody2D

export var _life = 150
export var _time = 1

var _template = preload("res://src/Actors/Zombie/Zombie.tscn")
var _processed = false

func _process(_delta: float) -> void:
	if not _processed and _life <= 0 and $ExplosionEffect.emitting == false:
		_processed = true
		queue_free()

func start() -> void:
	$Timer.wait_time = _time
	$Timer.start(-1)
	
func hit(damage) -> void:
	_life -= damage[0]
	
	if _life <= 0:
		$ExplosionEffect.emitting = true
		$ExplosionSound.play()
		$Sprite.visible = false
	
func SpawnZombie() -> void:
	var zombie = _template.instance()
	zombie.ViewRadius = 2000
	zombie.position = $SpawnPoint.position
	add_child(zombie)
