extends Node

## Interaction system - handles player proximity detection and workstation interactions

signal interaction_triggered(station_name: String, station_type: String)

var interaction_key: String = "interact"
var contracts_board_panel: Control = null
var main_scene: Node = null

func _ready() -> void:
	_ensure_interaction_action()
	_find_ui_panels()

func _ensure_interaction_action() -> void:
	"""Ensure the interact action is registered"""
	if not InputMap.has_action(interaction_key):
		InputMap.add_action(interaction_key)
		var event = InputEventKey.new()
		event.physical_keycode = KEY_E
		InputMap.action_add_event(interaction_key, event)

func _find_ui_panels() -> void:
	"""Find UI panels needed for interactions"""
	main_scene = get_parent()
	if main_scene == null:
		main_scene = get_tree().current_scene
	if main_scene == null:
		main_scene = get_tree().root.get_node_or_null("Main")
	if main_scene != null:
		contracts_board_panel = main_scene.get_node_or_null("UI/ContractsBoardPanel")
	if not contracts_board_panel:
		push_warning("ContractsBoardPanel not found in UI")

func trigger_interaction(workstation: Node) -> void:
	"""Trigger interaction with a workstation"""
	var station_name = workstation.station_name if "station_name" in workstation else workstation.name
	var station_type = workstation.station_type if "station_type" in workstation else "unknown"
	
	print("Interaction triggered: %s (type: %s)" % [station_name, station_type])
	interaction_triggered.emit(station_name, station_type)
	
	if station_type == "contracts":
		show_contracts_board()

func show_contracts_board() -> void:
	"""Show the contracts board UI"""
	if contracts_board_panel == null:
		_find_ui_panels()
	if main_scene != null and main_scene.has_method("show_contracts_board_panel"):
		main_scene.show_contracts_board_panel()
		return
	if contracts_board_panel:
		contracts_board_panel.show_board()
	else:
		push_error("Contracts board panel not found")
