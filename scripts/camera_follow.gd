extends Camera3D

@export var smooth_speed: float = 0.1
@export var offset: Vector3
@export var target = Node3D

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if target:
		var target_pos = target.global_position + offset
		global_position = global_position.lerp(target_pos, smooth_speed)
