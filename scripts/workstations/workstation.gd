extends StaticBody2D

@export var station_name: String = "Starter Workstation"
@export var station_type: String = "generic"
@export_multiline var station_summary: String = "A starter workstation."
@export var hardware_specs: Dictionary = {}
@export_file("*.json") var hardware_data_path: String = ""
@export_file("*.json") var contracts_data_path: String = ""
@export var interact_radius: float = 56.0
@export var interact_box_size: Vector2 = Vector2.ZERO
@export var accent_color: Color = Color(0.3, 0.65, 0.95, 1.0)

@onready var visual: Polygon2D = $Visual
@onready var prompt: Label = $Prompt

func _ready() -> void:
	_load_hardware_specs()
	_update_visual(false)
	prompt.text = "[E] %s" % station_name
	prompt.visible = false

func build_panel_data() -> Dictionary:
	var panel_data := {
		"title": station_name,
		"type": station_type.capitalize(),
		"summary": station_summary,
		"hardware": get_hardware_specs(),
		"contracts_data_path": contracts_data_path,
	}

	return panel_data

func set_highlighted(is_highlighted: bool) -> void:
	prompt.visible = is_highlighted
	_update_visual(is_highlighted)

func can_interact(player_position: Vector2) -> bool:
	if interact_box_size != Vector2.ZERO:
		var local_position := to_local(player_position)
		var half_size := interact_box_size * 0.5
		return abs(local_position.x) <= half_size.x and abs(local_position.y) <= half_size.y
	return global_position.distance_to(player_position) <= interact_radius

func _update_visual(is_highlighted: bool) -> void:
	visual.color = accent_color.lightened(0.2) if is_highlighted else accent_color

func get_hardware_specs() -> Dictionary:
	if hardware_specs.is_empty():
		_load_hardware_specs()
	return hardware_specs.duplicate(true)

func _load_hardware_specs() -> void:
	if hardware_data_path == "" or not FileAccess.file_exists(hardware_data_path):
		return

	var source := FileAccess.get_file_as_string(hardware_data_path)
	var parsed = JSON.parse_string(source)
	if typeof(parsed) != TYPE_DICTIONARY:
		return

	hardware_specs = (parsed as Dictionary).duplicate(true)
