extends CharacterBody2D

signal state_changed(previous_state: String, next_state: String)

const GAMEPLAY_BASE_VIEWPORT := Vector2(1280.0, 720.0)
const GAMEPLAY_MIN_ZOOM := 0.85
const GAMEPLAY_MAX_ZOOM := 1.45

enum PlayerState {
	IDLE,
	MOVING,
	INTERACTING,
}

@export var move_speed: float = 160.0

var current_state: PlayerState = PlayerState.IDLE
var last_direction: Vector2 = Vector2(0, 1)

@onready var gameplay_camera: Camera2D = $Camera2D

func _ready() -> void:
	_ensure_input_actions()
	_connect_viewport_resize()
	refresh_gameplay_camera_zoom()
	_set_state(PlayerState.IDLE)
	_set_idle_animation()

func _physics_process(_delta: float) -> void:
	var input_vector := get_input_vector()
	if current_state == PlayerState.INTERACTING:
		velocity = Vector2.ZERO
		move_and_slide()
		_set_interaction_animation()
		return

	apply_movement(input_vector)
	_update_state_from_input(input_vector)
	_update_animation(input_vector)

func get_input_vector() -> Vector2:
	return Input.get_vector("move_left", "move_right", "move_up", "move_down")

func apply_movement(input_vector: Vector2) -> void:
	if current_state == PlayerState.INTERACTING:
		velocity = Vector2.ZERO
		return

	velocity = input_vector * move_speed
	if is_inside_tree() and get_world_2d() != null:
		move_and_slide()

func begin_interaction() -> void:
	_set_state(PlayerState.INTERACTING)
	velocity = Vector2.ZERO
	_set_interaction_animation()

func end_interaction() -> void:
	_set_state(PlayerState.IDLE)
	velocity = Vector2.ZERO
	_set_idle_animation()

func get_state_name() -> String:
	return _state_to_string(current_state)

func is_interacting() -> bool:
	return current_state == PlayerState.INTERACTING

func refresh_gameplay_camera_zoom(viewport_size: Vector2 = Vector2.ZERO) -> void:
	if gameplay_camera == null:
		gameplay_camera = get_node_or_null("Camera2D")
	if gameplay_camera == null:
		return

	var target_viewport: Vector2 = viewport_size
	if target_viewport == Vector2.ZERO:
		var viewport: Viewport = get_viewport()
		if viewport == null:
			return
		target_viewport = viewport.get_visible_rect().size

	if target_viewport.x <= 0.0 or target_viewport.y <= 0.0:
		return

	var width_zoom: float = target_viewport.x / GAMEPLAY_BASE_VIEWPORT.x
	var height_zoom: float = target_viewport.y / GAMEPLAY_BASE_VIEWPORT.y
	var zoom_scalar: float = clamp(min(width_zoom, height_zoom), GAMEPLAY_MIN_ZOOM, GAMEPLAY_MAX_ZOOM)
	gameplay_camera.zoom = Vector2.ONE * zoom_scalar

func _update_state_from_input(input_vector: Vector2) -> void:
	if input_vector.length() > 0.0:
		last_direction = input_vector.normalized()
		_set_state(PlayerState.MOVING)
		return

	_set_state(PlayerState.IDLE)

func _update_animation(input_vector: Vector2) -> void:
	var sprite: AnimatedSprite2D = get_node_or_null("PlayerSprite")
	if input_vector.length() > 0.0:
		_set_animation_for_direction(last_direction)
		return

	if sprite and sprite.animation != "idle":
		sprite.animation = "idle"
		sprite.play()

func _set_animation_for_direction(direction: Vector2) -> void:
	var sprite: AnimatedSprite2D = get_node_or_null("PlayerSprite")
	if not sprite:
		return

	var abs_x: float = abs(direction.x)
	var abs_y: float = abs(direction.y)
	var target_animation: String = "walk_down"

	if abs_y > abs_x:
		target_animation = "walk_up" if direction.y < 0 else "walk_down"
	else:
		target_animation = "walk_left" if direction.x < 0 else "walk_right"

	if sprite.animation != target_animation:
		sprite.animation = target_animation
		sprite.play()

func _set_idle_animation() -> void:
	var sprite: AnimatedSprite2D = get_node_or_null("PlayerSprite")
	if sprite:
		sprite.animation = "idle"
		sprite.play()

func _set_interaction_animation() -> void:
	var sprite: AnimatedSprite2D = get_node_or_null("PlayerSprite")
	if sprite and sprite.animation != "interact":
		sprite.animation = "interact"
		sprite.play()

func _set_state(next_state: PlayerState) -> void:
	if current_state == next_state:
		return

	var previous_state_name := _state_to_string(current_state)
	current_state = next_state
	var next_state_name := _state_to_string(current_state)
	print("Player state changed: %s -> %s" % [previous_state_name, next_state_name])
	state_changed.emit(previous_state_name, next_state_name)

func _state_to_string(state: PlayerState) -> String:
	match state:
		PlayerState.IDLE:
			return "idle"
		PlayerState.MOVING:
			return "moving"
		PlayerState.INTERACTING:
			return "interacting"
	return "unknown"

func _ensure_input_actions() -> void:
	_ensure_action("move_up", KEY_W)
	_ensure_action("move_down", KEY_S)
	_ensure_action("move_left", KEY_A)
	_ensure_action("move_right", KEY_D)

func _connect_viewport_resize() -> void:
	var viewport := get_viewport()
	if viewport == null:
		return
	var resize_callable := Callable(self, "_on_viewport_size_changed")
	if not viewport.is_connected("size_changed", resize_callable):
		viewport.connect("size_changed", resize_callable)

func _on_viewport_size_changed() -> void:
	refresh_gameplay_camera_zoom()

func _ensure_action(action_name: String, keycode: Key) -> void:
	if not InputMap.has_action(action_name):
		InputMap.add_action(action_name)

	for event in InputMap.action_get_events(action_name):
		if event is InputEventKey and event.physical_keycode == keycode:
			return

	var input_event := InputEventKey.new()
	input_event.physical_keycode = keycode
	InputMap.action_add_event(action_name, input_event)
