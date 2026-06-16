extends Control

signal contract_selected(contract: Dictionary)

var main_scene: Node
var player: CharacterBody2D
var contracts: Array = []

var contract_list: ItemList
var title_label: Label
var subtitle_label: Label
var details_label: Label
var close_button: Button
var accept_button: Button
var scroll_container: ScrollContainer

const MIN_PANEL_SIZE := Vector2(320.0, 280.0)
const MAX_PANEL_SIZE := Vector2(460.0, 420.0)

func _ready() -> void:
	contract_list = get_node_or_null("Margin/Content/ContractListSection/Scroll/ContractList")
	title_label = get_node_or_null("Margin/Content/HeaderRow/TitleLabel")
	subtitle_label = get_node_or_null("Margin/Content/Subtitle")
	details_label = get_node_or_null("Margin/Content/DetailCard/Margin/Details")
	close_button = get_node_or_null("Margin/Content/HeaderRow/CloseButton")
	accept_button = get_node_or_null("Margin/Content/ActionRow/AcceptButton")
	scroll_container = get_node_or_null("Margin/Content/ContractListSection/Scroll")

	if contract_list == null or close_button == null or details_label == null or accept_button == null:
		push_error("Contracts board panel nodes not found")
		return

	main_scene = get_tree().root.get_node_or_null("Main")
	if main_scene == null:
		push_error("Main scene not found; contracts board UI will not work")
		return
	_resolve_player()
	refresh_responsive_layout()

	contract_list.item_selected.connect(_on_contract_list_item_selected)
	refresh_board()

func refresh_board() -> void:
	if main_scene == null or contract_list == null:
		return

	if main_scene.has_method("get_available_contracts"):
		contracts = main_scene.get_available_contracts()
	else:
		contracts.clear()

	_populate_contract_list()

	var selected_contract_id := ""
	if main_scene.has_method("get_board_selected_contract_id"):
		selected_contract_id = str(main_scene.get_board_selected_contract_id())

	if selected_contract_id != "":
		var selected_index := _find_contract_index(selected_contract_id)
		if selected_index >= 0:
			contract_list.select(selected_index)
			_update_details(contracts[selected_index])
			_update_accept_button_state(true)
			return
	if not contracts.is_empty():
		contract_list.select(0)
		_on_contract_list_item_selected(0)
		return

	_update_details({})
	_update_accept_button_state(false)

func select_contract_at_index(index: int) -> void:
	if index < 0 or index >= contracts.size():
		return
	contract_list.select(index)
	_on_contract_list_item_selected(index)

func show_board() -> void:
	refresh_responsive_layout()
	refresh_board()
	_resolve_player()
	if player != null and player.has_method("begin_interaction"):
		player.begin_interaction()
	show()

func hide_board() -> void:
	_resolve_player()
	if player != null and player.has_method("end_interaction"):
		player.end_interaction()
	visible = false

func _populate_contract_list() -> void:
	contract_list.clear()

	if contracts.is_empty():
		contract_list.add_item("No contracts available")
		if subtitle_label != null:
			subtitle_label.text = "No live leads are open right now. Accepted work, completed jobs, or locked follow-on leads can thin the board for a while."
		return

	if subtitle_label != null:
		var locked_count := 0
		if main_scene != null and main_scene.has_method("get_locked_contracts"):
			locked_count = (main_scene.get_locked_contracts() as Array).size()
		subtitle_label.text = "Select a lead, accept it here, then return to the workstation to activate hosting. Locked follow-on leads: %s." % str(locked_count)

	for contract in contracts:
		if not contract is Dictionary:
			continue

		var display_text := "%s  |  $%s/mo  |  %s mo  |  Support: %s" % [
			str(contract.get("name", "Unknown")),
			str(contract.get("monthly_revenue_value", 0)),
			str(contract.get("duration_months", 0)),
			str(contract.get("support_tag", "Unknown")),
		]
		contract_list.add_item(display_text)

func _on_contract_list_item_selected(index: int) -> void:
	if index < 0 or index >= contracts.size():
		_update_accept_button_state(false)
		return

	var selected_contract: Dictionary = contracts[index]
	if main_scene != null and main_scene.has_method("select_board_contract"):
		main_scene.select_board_contract(str(selected_contract.get("id", "")))

	_update_details(selected_contract)
	_update_accept_button_state(true)
	contract_selected.emit(selected_contract)

func _on_close_button_pressed() -> void:
	hide_board()

func _update_details(contract: Dictionary) -> void:
	if details_label == null:
		return

	if contract.is_empty():
		details_label.text = "No contract selected yet."
		return

	var resources: Dictionary = contract.get("resources", {})
	var cpu := int(resources.get("cpu", 0))
	var memory := int(resources.get("ram_gb", 0))
	var storage := int(resources.get("storage_gb", 0))

	details_label.text = "\n".join([
		"Selected lead: %s" % str(contract.get("name", "Unknown")),
		"Monthly revenue: $%s" % str(contract.get("monthly_revenue_value", 0)),
		"Duration: %s months" % str(contract.get("duration_months", 0)),
		"Risk: %s" % str(contract.get("risk_tag", "Standard")),
		"Support requirement: %s" % str(contract.get("support_tag", "Standard")),
		"Hosting load: CPU %s, RAM %s GB, Storage %s GB" % [str(cpu), str(memory), str(storage)],
	])

func _on_accept_button_pressed() -> void:
	if main_scene == null or not main_scene.has_method("accept_selected_contract"):
		return

	var accept_result: Dictionary = main_scene.accept_selected_contract()
	if not bool(accept_result.get("ok", false)):
		if subtitle_label != null:
			subtitle_label.text = str(accept_result.get("reason", "Unable to accept that lead right now."))
		return

	var accepted_contract := accept_result.get("contract", {}) as Dictionary
	if subtitle_label != null:
		subtitle_label.text = "%s accepted. Head back to the workstation to activate hosting." % str(accepted_contract.get("name", "Lead"))
	refresh_board()

func _update_accept_button_state(enabled: bool) -> void:
	if accept_button == null:
		return
	accept_button.disabled = not enabled or contracts.is_empty()

func _find_contract_index(contract_id: String) -> int:
	for index in range(contracts.size()):
		var contract: Dictionary = contracts[index]
		if str(contract.get("id", "")) == contract_id:
			return index
	return -1

func _resolve_player() -> void:
	if player != null:
		return
	if main_scene == null:
		var ui_root := get_parent()
		if ui_root != null:
			main_scene = ui_root.get_parent()
	if main_scene != null:
		player = main_scene.get_node_or_null("Player")

func refresh_responsive_layout(viewport_size: Vector2 = Vector2.ZERO) -> void:
	var target_viewport: Vector2 = viewport_size
	if target_viewport == Vector2.ZERO:
		var viewport: Viewport = get_viewport()
		if viewport == null:
			return
		target_viewport = viewport.get_visible_rect().size

	if target_viewport.x <= 0.0 or target_viewport.y <= 0.0:
		return

	var panel_width: float = clamp(target_viewport.x * 0.34, MIN_PANEL_SIZE.x, MAX_PANEL_SIZE.x)
	var panel_height: float = clamp(target_viewport.y * 0.58, MIN_PANEL_SIZE.y, MAX_PANEL_SIZE.y)
	custom_minimum_size = Vector2(panel_width, panel_height)
	offset_left = -panel_width * 0.5
	offset_right = panel_width * 0.5
	offset_top = -panel_height * 0.5
	offset_bottom = panel_height * 0.5

	if scroll_container != null:
		var scroll_height: float = clamp(panel_height * 0.42, 120.0, 220.0)
		scroll_container.custom_minimum_size = Vector2(panel_width - 44.0, scroll_height)
