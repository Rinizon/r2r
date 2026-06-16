extends Node2D

const SAVE_PATH := "user://mvp_save.json"
const EXPANSION_CATALOG_PATH := "res://data/hardware/upgrade_ladder.json"
const UI_BASE_VIEWPORT := Vector2(1280.0, 720.0)
const MAIN_PANEL_MIN_WIDTH := 380.0
const MAIN_PANEL_MAX_WIDTH := 520.0
const LOG_PANEL_MIN_WIDTH := 180.0
const LOG_PANEL_MAX_WIDTH := 220.0
const RIGHT_STACK_MIN_WIDTH := 220.0
const RIGHT_STACK_MAX_WIDTH := 250.0
const STATION_PANEL_MIN_WIDTH := 240.0
const STATION_PANEL_MAX_WIDTH := 320.0

@onready var player: CharacterBody2D = $Player
@onready var workstations: Node = $Workstations
@onready var station_panel: PanelContainer = $UI/StationPanel
@onready var main_panel: PanelContainer = $UI/LayoutRoot/Margin/TopRow/MainPanel
@onready var log_panel: PanelContainer = $UI/LayoutRoot/Margin/TopRow/LogPanel
@onready var economy_hud: PanelContainer = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances
@onready var time_controls: PanelContainer = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls
@onready var monthly_summary: PanelContainer = $UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary
@onready var warning_alert: PanelContainer = $UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert
@onready var right_stack: VBoxContainer = $UI/LayoutRoot/Margin/TopRow/RightStack
@onready var panel_title: Label = $UI/StationPanel/Margin/Content/HeaderRow/Title
@onready var panel_close_button: Button = $UI/StationPanel/Margin/Content/HeaderRow/Close
@onready var panel_subtitle: Label = $UI/StationPanel/Margin/Content/Subtitle
@onready var panel_details: RichTextLabel = $UI/StationPanel/Margin/Content/Details
@onready var panel_actions: Container = $UI/StationPanel/Margin/Content/Actions
@onready var panel_action_status: Label = $UI/StationPanel/Margin/Content/ActionStatus
@onready var panel_hint: Label = $UI/StationPanel/Margin/Content/Hint
@onready var economy_cash_value: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/CashValue
@onready var economy_income_value: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/IncomeValue
@onready var economy_expenses_value: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/ExpensesValue
@onready var onboarding_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/OnboardingPanel/Margin/Content/Body
@onready var economy_next_cost_value: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab/NextCostPanel/Margin/Content/Value
@onready var time_date_label: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/DateLabel
@onready var time_clock_label: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/TimeLabel
@onready var time_speed_label: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedLabel
@onready var time_status_label: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/StatusLabel
@onready var time_speed_1_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed1
@onready var time_speed_2_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed2
@onready var time_speed_3_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed3
@onready var continue_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Continue
@onready var reset_progress_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Reset
@onready var save_status_label: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Status
@onready var operations_tab_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Operations
@onready var contracts_tab_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Contracts
@onready var progress_tab_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Progress
@onready var operations_tab: Control = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab
@onready var contracts_tab: Control = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab
@onready var progress_tab: Control = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab
@onready var contracts_available_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/AvailableSection/Margin/Content/Body
@onready var contracts_queue_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/QueueSection/Margin/Content/Body
@onready var contracts_active_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ActiveSection/Margin/Content/Body
@onready var contracts_outcomes_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/OutcomesSection/Margin/Content/Body
@onready var contracts_response_enable_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/EnableHold
@onready var contracts_response_clear_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/ClearHold
@onready var contracts_response_backup_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/BackupPass
@onready var contracts_response_late_shift_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/LateShift
@onready var contracts_response_stabilize_button: Button = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/StabilizeLine
@onready var contracts_response_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Body
@onready var contracts_board_panel: Control = $UI/ContractsBoardPanel
@onready var progress_summary_body: Label = $UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab/ProgressSummary/Margin/Content/Body
@onready var log_panel_content: VBoxContainer = $UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content
@onready var log_panel_collapse_button: Button = $UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/HeaderRow/Collapse
@onready var log_panel_collapsed_strip: Button = $UI/LayoutRoot/Margin/TopRow/LogPanel/CollapsedStrip
@onready var log_contracts_body: Label = $UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/ContractsSection/ContractsBody
@onready var log_progress_body: Label = $UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/ProgressSection/ProgressBody
@onready var monthly_summary_close_button: Button = $UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary/Margin/Content/Close
@onready var monthly_summary_text: Label = $UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary/Margin/Content/SummaryText
@onready var warning_alert_close_button: Button = $UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert/Margin/Content/Close
@onready var warning_alert_text: Label = $UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert/Margin/Content/AlertText

var focused_station: StaticBody2D
var opened_station: StaticBody2D
var all_contracts: Array = []
var available_contracts: Array = []
var pending_contracts: Array = []
var selected_contract_id: String = ""
var board_selected_contract_id: String = ""
var active_contracts: Array = []
var completed_contracts: Array = []
var cancelled_contracts: Array = []
var active_incidents: Array = []
var warning_log: Array = []
var starter_hardware: Dictionary = {}
var expansion_catalog: Array = []
var selected_expansion_id: String = ""
var selected_contract_index: int = -1
var purchased_expansions := {}
var hosting_hold_enabled := false
var gameplay_state_initialized := false
var month_names := ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
var active_main_tab := "operations"
var log_panel_collapsed := false
var clock_state := {
	"month": 1,
	"day": 1,
	"hour": 8,
	"minute": 0,
	"speed": 1,
}
var last_monthly_report: Dictionary = {}
var clock_running := true
var pending_time_minutes := 0.0
var economy_state := {
	"cash": 1200,
	"monthly_income": 0,
	"monthly_expenses": 0,
	"power_cost": 0,
	"internet_cost": 0,
	"hardware_upkeep_cost": 0,
	"next_cost": "External Server Unit purchase: $450",
}

func _ready() -> void:
	_resolve_nodes()
	_initialize_gameplay_state()
	_connect_main_tabs()
	_connect_log_panel()
	_connect_station_panel()
	_connect_contracts_board_panel()
	_connect_contract_response_controls()
	_connect_save_controls()
	_connect_viewport_resize()
	_select_main_tab(active_main_tab)
	_connect_time_controls()
	refresh_responsive_ui_layout()
	_refresh_economy_hud()
	_refresh_time_controls()
	_refresh_main_tab_summaries()
	_refresh_log_panel()
	_update_save_controls()
	_sync_station_panel_height()
	hide_station_panel()

func _process(_delta: float) -> void:
	_update_station_focus()
	if not clock_running:
		return

	pending_time_minutes += _delta * 10.0 * float(clock_state["speed"])
	var whole_minutes := int(pending_time_minutes)
	if whole_minutes <= 0:
		return

	pending_time_minutes -= whole_minutes
	advance_time(whole_minutes)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("interact"):
		if warning_alert.visible:
			hide_warning_alert()
		elif monthly_summary.visible:
			hide_monthly_summary()
		elif contracts_board_panel != null and contracts_board_panel.visible:
			_hide_contracts_board_panel()
		elif station_panel.visible:
			hide_station_panel()
		elif focused_station != null:
			_open_station_interaction(focused_station)
	elif event.is_action_pressed("ui_cancel"):
		if warning_alert.visible:
			hide_warning_alert()
		elif monthly_summary.visible:
			hide_monthly_summary()
		elif contracts_board_panel != null and contracts_board_panel.visible:
			_hide_contracts_board_panel()
		elif station_panel.visible:
			hide_station_panel()

func find_interactable_stations() -> Array:
	_resolve_nodes()
	var stations: Array = []
	for child in workstations.get_children():
		if child.has_method("build_panel_data"):
			stations.append(child)
	return stations

func show_station_panel(station: StaticBody2D) -> void:
	_resolve_nodes()
	_hide_contracts_board_panel()
	if player != null and player.has_method("begin_interaction"):
		player.begin_interaction()
	_sync_station_panel_height()
	var data: Dictionary = station.build_panel_data()
	panel_title.text = str(data.get("title", "Workstation"))
	panel_subtitle.text = str(data.get("summary", ""))
	panel_details.text = _build_station_details(station, data)
	_render_station_actions(station)
	panel_action_status.text = ""
	panel_hint.text = _build_station_hint(station)
	station_panel.visible = true
	opened_station = station

func show_contracts_board_panel() -> void:
	_resolve_nodes()
	hide_station_panel()
	if contracts_board_panel == null:
		return
	if contracts_board_panel.has_method("show_board"):
		contracts_board_panel.show_board()
	else:
		contracts_board_panel.visible = true

func hide_station_panel() -> void:
	_resolve_nodes()
	station_panel.visible = false
	panel_title.text = ""
	panel_subtitle.text = ""
	panel_details.text = ""
	_clear_station_actions()
	panel_action_status.text = ""
	panel_hint.text = ""
	opened_station = null
	if player != null and player.has_method("end_interaction"):
		player.end_interaction()

func get_board_selected_contract_id() -> String:
	_initialize_gameplay_state()
	return board_selected_contract_id

func refresh_responsive_ui_layout(viewport_size: Vector2 = Vector2.ZERO) -> void:
	_resolve_nodes()
	var target_viewport: Vector2 = viewport_size
	if target_viewport == Vector2.ZERO:
		var viewport: Viewport = get_viewport()
		if viewport == null:
			return
		target_viewport = viewport.get_visible_rect().size

	if target_viewport.x <= 0.0 or target_viewport.y <= 0.0:
		return

	main_panel.custom_minimum_size.x = clamp(target_viewport.x * 0.34, MAIN_PANEL_MIN_WIDTH, MAIN_PANEL_MAX_WIDTH)
	log_panel.custom_minimum_size.x = clamp(target_viewport.x * 0.16, LOG_PANEL_MIN_WIDTH, LOG_PANEL_MAX_WIDTH)
	if right_stack != null:
		right_stack.custom_minimum_size.x = clamp(target_viewport.x * 0.18, RIGHT_STACK_MIN_WIDTH, RIGHT_STACK_MAX_WIDTH)

	var station_width: float = clamp(target_viewport.x * 0.23, STATION_PANEL_MIN_WIDTH, STATION_PANEL_MAX_WIDTH)
	station_panel.offset_left = -(station_width + 16.0)
	station_panel.offset_right = -16.0
	station_panel.offset_top = 16.0
	if target_viewport.x <= 1040.0:
		station_panel.offset_top = 12.0
	if contracts_board_panel != null and contracts_board_panel.has_method("refresh_responsive_layout"):
		contracts_board_panel.call("refresh_responsive_layout", target_viewport)
	_sync_station_panel_height(target_viewport)

func _sync_station_panel_height(viewport_size: Vector2 = Vector2.ZERO) -> void:
	_resolve_nodes()
	if station_panel == null or main_panel == null:
		return
	var minimum_height := main_panel.get_combined_minimum_size().y
	var target_height := main_panel.size.y
	if target_height <= 0.0:
		target_height = minimum_height
	if target_height <= 0.0:
		return
	var available_height: float = 0.0
	if viewport_size != Vector2.ZERO:
		available_height = max(viewport_size.y - (station_panel.offset_top + 16.0), minimum_height)
	elif get_viewport() != null:
		available_height = max(get_viewport().get_visible_rect().size.y - (station_panel.offset_top + 16.0), minimum_height)
	if available_height > 0.0:
		target_height = min(target_height, available_height)
	station_panel.custom_minimum_size.y = minimum_height
	station_panel.offset_bottom = station_panel.offset_top + target_height

func _update_station_focus() -> void:
	_resolve_nodes()
	var nearest_station := _find_nearest_station()
	if nearest_station == focused_station:
		return

	if focused_station != null:
		focused_station.set_highlighted(false)

	focused_station = nearest_station

	if focused_station != null:
		focused_station.set_highlighted(true)

func _find_nearest_station() -> StaticBody2D:
	_resolve_nodes()
	var nearest_station: StaticBody2D
	var nearest_distance := INF

	for station in find_interactable_stations():
		if not station.can_interact(player.global_position):
			continue

		var distance: float = station.global_position.distance_to(player.global_position)
		if distance < nearest_distance:
			nearest_station = station
			nearest_distance = distance

	return nearest_station

func _open_station_interaction(station: StaticBody2D) -> void:
	if station == null:
		return

	if station.station_type == "contracts":
		var interaction_controller := get_node_or_null("InteractionController")
		if interaction_controller != null and interaction_controller.has_method("trigger_interaction"):
			interaction_controller.trigger_interaction(station)
			return
		show_contracts_board_panel()
		return

	show_station_panel(station)

func _resolve_nodes() -> void:
	if player == null:
		player = get_node_or_null("Player")
	if workstations == null:
		workstations = get_node_or_null("Workstations")
	if station_panel == null:
		station_panel = get_node_or_null("UI/StationPanel")
	if main_panel == null:
		main_panel = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel")
	if log_panel == null:
		log_panel = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel")
	if right_stack == null:
		right_stack = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack")
	if economy_hud == null:
		economy_hud = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances")
	if time_controls == null:
		time_controls = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls")
	if monthly_summary == null:
		monthly_summary = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary")
	if warning_alert == null:
		warning_alert = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert")
	if panel_title == null:
		panel_title = get_node_or_null("UI/StationPanel/Margin/Content/HeaderRow/Title")
	if panel_close_button == null:
		panel_close_button = get_node_or_null("UI/StationPanel/Margin/Content/HeaderRow/Close")
	if panel_subtitle == null:
		panel_subtitle = get_node_or_null("UI/StationPanel/Margin/Content/Subtitle")
	if panel_details == null:
		panel_details = get_node_or_null("UI/StationPanel/Margin/Content/Details")
	if panel_actions == null:
		panel_actions = get_node_or_null("UI/StationPanel/Margin/Content/Actions")
	if panel_action_status == null:
		panel_action_status = get_node_or_null("UI/StationPanel/Margin/Content/ActionStatus")
	if panel_hint == null:
		panel_hint = get_node_or_null("UI/StationPanel/Margin/Content/Hint")
	if economy_cash_value == null:
		economy_cash_value = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/CashValue")
	if economy_income_value == null:
		economy_income_value = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/IncomeValue")
	if economy_expenses_value == null:
		economy_expenses_value = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/ExpensesValue")
	if onboarding_body == null:
		onboarding_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/OnboardingPanel/Margin/Content/Body")
	if economy_next_cost_value == null:
		economy_next_cost_value = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab/NextCostPanel/Margin/Content/Value")
	if time_date_label == null:
		time_date_label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/DateLabel")
	if time_clock_label == null:
		time_clock_label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/TimeLabel")
	if time_speed_label == null:
		time_speed_label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedLabel")
	if time_status_label == null:
		time_status_label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/StatusLabel")
	if time_speed_1_button == null:
		time_speed_1_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed1")
	if time_speed_2_button == null:
		time_speed_2_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed2")
	if time_speed_3_button == null:
		time_speed_3_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed3")
	if continue_button == null:
		continue_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Continue")
	if reset_progress_button == null:
		reset_progress_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Reset")
	if save_status_label == null:
		save_status_label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Status")
	if operations_tab_button == null:
		operations_tab_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Operations")
	if contracts_tab_button == null:
		contracts_tab_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Contracts")
	if progress_tab_button == null:
		progress_tab_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons/Progress")
	if operations_tab == null:
		operations_tab = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab")
	if contracts_tab == null:
		contracts_tab = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab")
	if progress_tab == null:
		progress_tab = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab")
	if contracts_available_body == null:
		contracts_available_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/AvailableSection/Margin/Content/Body")
	if contracts_queue_body == null:
		contracts_queue_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/QueueSection/Margin/Content/Body")
	if contracts_active_body == null:
		contracts_active_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ActiveSection/Margin/Content/Body")
	if contracts_outcomes_body == null:
		contracts_outcomes_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/OutcomesSection/Margin/Content/Body")
	if contracts_response_enable_button == null:
		contracts_response_enable_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/EnableHold")
	if contracts_response_clear_button == null:
		contracts_response_clear_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/ClearHold")
	if contracts_response_backup_button == null:
		contracts_response_backup_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/BackupPass")
	if contracts_response_late_shift_button == null:
		contracts_response_late_shift_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/LateShift")
	if contracts_response_stabilize_button == null:
		contracts_response_stabilize_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/StabilizeLine")
	if contracts_response_body == null:
		contracts_response_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Body")
	if contracts_board_panel == null:
		contracts_board_panel = get_node_or_null("UI/ContractsBoardPanel")
	if progress_summary_body == null:
		progress_summary_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab/ProgressSummary/Margin/Content/Body")
	if log_panel_content == null:
		log_panel_content = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content")
	if log_panel_collapse_button == null:
		log_panel_collapse_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/HeaderRow/Collapse")
	if log_panel_collapsed_strip == null:
		log_panel_collapsed_strip = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/CollapsedStrip")
	if log_contracts_body == null:
		log_contracts_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/ContractsSection/ContractsBody")
	if log_progress_body == null:
		log_progress_body = get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/ProgressSection/ProgressBody")
	if monthly_summary_close_button == null:
		monthly_summary_close_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary/Margin/Content/Close")
	if monthly_summary_text == null:
		monthly_summary_text = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary/Margin/Content/SummaryText")
	if warning_alert_close_button == null:
		warning_alert_close_button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert/Margin/Content/Close")
	if warning_alert_text == null:
		warning_alert_text = get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert/Margin/Content/AlertText")

func _connect_viewport_resize() -> void:
	var viewport := get_viewport()
	if viewport == null:
		return
	var resize_callable := Callable(self, "_on_viewport_size_changed")
	if not viewport.is_connected("size_changed", resize_callable):
		viewport.connect("size_changed", resize_callable)

func _on_viewport_size_changed() -> void:
	refresh_responsive_ui_layout()

func get_economy_snapshot() -> Dictionary:
	_refresh_economy_state()
	return economy_state.duplicate(true)

func get_time_snapshot() -> Dictionary:
	return {
		"month_name": month_names[int(clock_state["month"]) - 1],
		"day": int(clock_state["day"]),
		"hour": int(clock_state["hour"]),
		"minute": int(clock_state["minute"]),
		"speed": int(clock_state["speed"]),
	}

func get_last_monthly_report() -> Dictionary:
	return last_monthly_report.duplicate(true)

func has_save_data() -> bool:
	return FileAccess.file_exists(SAVE_PATH)

func save_game() -> bool:
	_initialize_gameplay_state()
	var file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if file == null:
		_update_save_controls("Save unavailable.")
		return false
	file.store_string(JSON.stringify(_build_save_payload()))
	_update_save_controls("Progress saved locally.")
	return true

func load_game() -> bool:
	if not has_save_data():
		_update_save_controls("No save available.")
		return false

	var source := FileAccess.get_file_as_string(SAVE_PATH)
	var parsed = JSON.parse_string(source)
	if typeof(parsed) != TYPE_DICTIONARY:
		_update_save_controls("Save data is invalid.")
		return false

	_apply_save_payload(parsed as Dictionary)
	_update_save_controls("Local save loaded.")
	return true

func reset_progress() -> void:
	if has_save_data():
		DirAccess.remove_absolute(ProjectSettings.globalize_path(SAVE_PATH))
	_reset_runtime_state()
	_update_save_controls("Progress reset to startup.")

func enable_hosting_hold() -> Dictionary:
	if hosting_hold_enabled:
		return {"ok": false, "reason": "Hosting hold is already enabled."}
	hosting_hold_enabled = true
	_refresh_live_service_warning_state()
	save_game()
	return {"ok": true, "status": "New hosting hold enabled. Throttle mode will protect service quality while overload is active."}

func clear_hosting_hold() -> Dictionary:
	if not hosting_hold_enabled:
		return {"ok": false, "reason": "Hosting hold is already cleared."}
	hosting_hold_enabled = false
	_refresh_live_service_warning_state()
	save_game()
	return {"ok": true, "status": "Hosting hold cleared. New activation can resume."}

func run_backup_pass() -> Dictionary:
	return _resolve_support_incident_by_type(["backup_check"], 8, "Ran a backup verification pass and cleared the compliance-heavy support request.")

func work_late_shift() -> Dictionary:
	return _resolve_support_incident_by_type(["after_hours", "forum_cleanup"], 6, "Stayed late and cleared the urgent support queue before it spilled into next month.")

func stabilize_line() -> Dictionary:
	return _resolve_support_incident_by_type(["internet_slowdown"], 10, "Spent the evening stabilizing the little office internet line and calming the traffic spike issue.")

func hide_monthly_summary() -> void:
	_resolve_nodes()
	if monthly_summary != null:
		monthly_summary.visible = false

func hide_warning_alert() -> void:
	_resolve_nodes()
	if warning_alert != null:
		warning_alert.visible = false

func toggle_log_panel() -> void:
	log_panel_collapsed = not log_panel_collapsed
	_apply_log_panel_state()

func set_time_speed(speed: int) -> void:
	if speed in [2, 3]:
		clock_state["speed"] = speed
	elif speed == 1:
		clock_state["speed"] = 1
	_refresh_time_controls()

func start_time() -> void:
	clock_running = true
	_refresh_time_controls()

func pause_time() -> void:
	clock_running = false
	pending_time_minutes = 0.0
	_refresh_time_controls()

func increase_time_speed() -> void:
	var speed_levels := [1, 2, 3]
	var current_speed := int(clock_state["speed"])
	var current_index := speed_levels.find(current_speed)
	if current_index < 0:
		current_index = 0
	var next_index := posmod(current_index + 1, speed_levels.size())
	clock_state["speed"] = speed_levels[next_index]
	clock_running = true
	_refresh_time_controls()

func reset_time_speed() -> void:
	clock_state["speed"] = 1
	clock_running = true
	_refresh_time_controls()

func advance_to_next_month() -> void:
	var minutes_until_next_month := ((30 - int(clock_state["day"])) * 24 * 60) + ((24 - int(clock_state["hour"]) - 1) * 60) + (60 - int(clock_state["minute"]))
	advance_time(minutes_until_next_month)

func advance_time(minutes: int) -> void:
	var total_minutes := (int(clock_state["hour"]) * 60) + int(clock_state["minute"]) + minutes
	while total_minutes >= 24 * 60:
		total_minutes -= 24 * 60
		clock_state["day"] = int(clock_state["day"]) + 1
		if int(clock_state["day"]) > 30:
			clock_state["day"] = 1
			clock_state["month"] = int(clock_state["month"]) + 1
			if int(clock_state["month"]) > month_names.size():
				clock_state["month"] = 1
			_run_monthly_rollover()
	clock_state["hour"] = total_minutes / 60
	clock_state["minute"] = total_minutes % 60
	_refresh_time_controls()

func get_station_actions(station: StaticBody2D) -> Array:
	var actions: Array = []

	if station.station_type == "computer":
		if _has_available_expansion_targets():
			var selected_upgrade := _get_selected_expansion_target()
			var selected_label := str(selected_upgrade.get("label", "Upgrade"))
			var selected_cost := int(selected_upgrade.get("cost", 0))
			var selected_upkeep := int(selected_upgrade.get("monthly_expense", 0))
			actions.append({
				"id": "previous_upgrade",
				"label": "Previous Upgrade",
				"status": "Review the previous available hardware upgrade.",
			})
			actions.append({
				"id": "next_upgrade",
				"label": "Next Upgrade",
				"status": "Review the next available hardware upgrade.",
			})
			actions.append({
				"id": "purchase_selected_upgrade",
				"label": "Buy %s ($%s, +$%s/mo)" % [selected_label, str(selected_cost), str(selected_upkeep)],
				"status": "Buy the %s for $%s and add $%s/mo in upkeep." % [selected_label, str(selected_cost), str(selected_upkeep)],
			})
		actions.append({
			"id": "activate_hosting",
			"label": "Activate Hosting",
			"status": "Activate the currently selected contract on this workstation.",
		})
		actions.append({
			"id": "clear_selection",
			"label": "Clear Selection",
			"status": "Clear the current board selection without activating hosting.",
		})
	elif station.station_type == "contracts":
		actions.append({
			"id": "previous_contract",
			"label": "Previous Contract",
			"status": "Move to the previous starter contract.",
		})
		actions.append({
			"id": "next_contract",
			"label": "Next Contract",
			"status": "Move to the next starter contract.",
		})
		actions.append({
			"id": "accept_contract",
			"label": "Accept Contract",
			"status": "Accept this contract and move it into your hosting queue.",
		})
	elif station.station_type == "expansion":
		actions.append({
			"id": "review_expansion",
			"label": "Review Expansion",
			"status": "This external server unlocks the first real step beyond the desk setup and hints at more specialized support work ahead.",
		})

	return actions

func press_station_action(action_id: String) -> Dictionary:
	_resolve_nodes()
	if opened_station == null:
		return {"ok": false, "status": "Open a workstation panel before using its controls."}

	var result := _handle_station_action(opened_station, action_id)
	panel_action_status.text = str(result.get("status", ""))
	if bool(result.get("ok", false)):
		var data: Dictionary = opened_station.build_panel_data()
		panel_title.text = str(data.get("title", "Workstation"))
		panel_subtitle.text = str(data.get("summary", ""))
		panel_details.text = _build_station_details(opened_station, data)
		_render_station_actions(opened_station)
		panel_hint.text = _build_station_hint(opened_station)
	_refresh_log_panel()
	return result

func get_available_contracts() -> Array:
	_initialize_gameplay_state()
	return available_contracts.duplicate(true)

func get_locked_contracts() -> Array:
	_initialize_gameplay_state()
	return _get_locked_contracts()

func get_pending_contracts() -> Array:
	_initialize_gameplay_state()
	return pending_contracts.duplicate(true)

func select_contract(contract_id: String) -> void:
	_initialize_gameplay_state()
	selected_contract_id = contract_id

func select_board_contract(contract_id: String) -> Dictionary:
	_initialize_gameplay_state()
	var contract_index := _find_available_contract_index_by_id(contract_id)
	if contract_index < 0:
		return {"ok": false, "reason": "Selected board contract is unavailable."}

	selected_contract_index = contract_index
	board_selected_contract_id = contract_id
	var contract: Dictionary = available_contracts[contract_index].duplicate(true)
	return {"ok": true, "contract": contract}

func accept_selected_contract() -> Dictionary:
	_initialize_gameplay_state()
	if board_selected_contract_id == "":
		return {"ok": false, "reason": "No board contract selected."}

	var contract_index := _find_available_contract_index_by_id(board_selected_contract_id)
	if contract_index < 0:
		return {"ok": false, "reason": "Selected board contract is unavailable."}

	var accepted_contract: Dictionary = available_contracts[contract_index].duplicate(true)
	pending_contracts.append(accepted_contract)

	if selected_contract_id == "":
		selected_contract_id = String(accepted_contract.get("id", ""))

	_refresh_available_contracts()
	_refresh_main_tab_summaries()
	save_game()
	return {"ok": true, "contract": accepted_contract}

func activate_selected_contract() -> Dictionary:
	_initialize_gameplay_state()
	if hosting_hold_enabled:
		return {"ok": false, "reason": "New hosting is on hold while service issues are being managed."}
	if selected_contract_id == "":
		return {"ok": false, "reason": "No contract selected."}

	var pending_index := _find_pending_contract_index_by_id(selected_contract_id)
	if pending_index < 0:
		return {"ok": false, "reason": "Contract has not been accepted from the board yet."}

	var contract: Dictionary = pending_contracts[pending_index].duplicate(true)

	var active_contract := contract.duplicate(true)
	active_contract["status"] = "active"
	active_contract["months_remaining"] = int(contract.get("duration_months", 1))
	active_contract["satisfaction"] = 100
	active_contract["warning_streak"] = 0
	active_contract["penalty_total"] = 0
	pending_contracts.remove_at(pending_index)
	active_contracts.append(active_contract)
	_queue_support_incident_for_contract(active_contract)
	if not pending_contracts.is_empty():
		selected_contract_id = String(pending_contracts[0].get("id", ""))
	else:
		clear_selected_contract()
	_refresh_economy_state()
	_refresh_main_tab_summaries()
	_refresh_log_panel()
	_refresh_live_service_warning_state()
	save_game()
	return {"ok": true, "contract": contract}

func tick_month() -> Dictionary:
	_initialize_gameplay_state()
	var payout_total := 0
	var completed_ids: Array = []
	var cancelled_ids: Array = []
	var monthly_costs := get_monthly_cost_breakdown()
	var expense_total := int(monthly_costs.get("power_cost", 0)) + int(monthly_costs.get("internet_cost", 0))
	var warning_count := 0
	var penalty_total := 0
	warning_log.clear()
	var overloaded := _is_service_overloaded()
	var incident_report := _process_support_incidents_for_month()
	warning_count += int(incident_report.get("warning_count", 0))
	penalty_total += int(incident_report.get("penalty_total", 0))

	for index in range(active_contracts.size() - 1, -1, -1):
		var contract: Dictionary = active_contracts[index]
		var contract_payout := int(contract.get("monthly_revenue_value", 0))
		contract["months_remaining"] = int(contract.get("months_remaining", 0)) - 1
		if overloaded:
			warning_count += 1
			if hosting_hold_enabled:
				contract_payout = maxi(1, int(round(float(contract_payout) * 0.75)))
				contract["warning_streak"] = 0
				warning_log.append("%s is being throttled under a hosting hold to protect service quality." % str(contract.get("name", "Contract")))
			else:
				contract["warning_streak"] = int(contract.get("warning_streak", 0)) + 1
				contract["satisfaction"] = max(0, int(contract.get("satisfaction", 100)) - 12)
				var warning_message := "%s is experiencing degraded service due to tight hosting capacity." % str(contract.get("name", "Contract"))
				warning_log.append(warning_message)
				if int(contract.get("warning_streak", 0)) >= 2:
					var contract_penalty := 10
					contract["penalty_total"] = int(contract.get("penalty_total", 0)) + contract_penalty
					penalty_total += contract_penalty
				if int(contract.get("warning_streak", 0)) >= 2 and int(contract.get("satisfaction", 100)) <= 76:
					contract["status"] = "cancelled"
					cancelled_contracts.append(contract.duplicate(true))
					cancelled_ids.append(String(contract.get("id", "")))
					active_contracts.remove_at(index)
					continue
		else:
			contract["warning_streak"] = 0
		payout_total += contract_payout

		if int(contract.get("months_remaining", 0)) <= 0:
			contract["status"] = "completed"
			completed_contracts.append(contract.duplicate(true))
			completed_ids.append(String(contract.get("id", "")))
			active_contracts.remove_at(index)
		else:
			active_contracts[index] = contract

	_prune_support_incidents()
	economy_state["cash"] = int(economy_state.get("cash", 0)) + payout_total - expense_total - penalty_total
	_refresh_available_contracts()
	_refresh_economy_state()
	var report := {
		"payout_total": payout_total,
		"expense_total": expense_total,
		"warning_count": warning_count,
		"penalty_total": penalty_total,
		"incident_count": int(incident_report.get("incident_count", 0)),
		"resolved_incident_count": int(incident_report.get("resolved_count", 0)),
		"neglected_incident_count": int(incident_report.get("neglected_count", 0)),
		"completed_contract_ids": completed_ids,
		"cancelled_contract_ids": cancelled_ids,
	}
	last_monthly_report = report.duplicate(true)
	_refresh_economy_hud()
	_refresh_main_tab_summaries()
	_refresh_log_panel()
	save_game()
	return report

func get_contract_snapshot(contract_id: String) -> Dictionary:
	for contract in active_contracts:
		if String(contract.get("id", "")) == contract_id:
			return contract.duplicate(true)
	for contract in completed_contracts:
		if String(contract.get("id", "")) == contract_id:
			return contract.duplicate(true)
	for contract in cancelled_contracts:
		if String(contract.get("id", "")) == contract_id:
			return contract.duplicate(true)
	for contract in pending_contracts:
		if String(contract.get("id", "")) == contract_id:
			var accepted: Dictionary = contract.duplicate(true)
			accepted["status"] = "accepted"
			return accepted
	for contract in available_contracts:
		if String(contract.get("id", "")) == contract_id:
			var pending: Dictionary = contract.duplicate(true)
			pending["status"] = "available"
			return pending
	return {}

func get_warning_log() -> Array:
	return warning_log.duplicate(true)

func get_active_incidents() -> Array:
	return active_incidents.duplicate(true)

func get_capacity_snapshot() -> Dictionary:
	return get_shared_capacity_snapshot()

func get_shared_capacity_snapshot() -> Dictionary:
	_initialize_gameplay_state()
	var used := _calculate_used_capacity()
	var snapshot := {}
	var total_capacity := starter_hardware.duplicate(true)

	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if not bool(purchased_expansions.get(expansion_id, false)):
			continue

		var hardware := _get_expansion_hardware(expansion_data)
		for key in ["cpu", "ram_gb", "storage_gb", "bandwidth_mbps"]:
			total_capacity[key] = int(total_capacity.get(key, 0)) + int(hardware.get(key, 0))

	for key in ["cpu", "ram_gb", "storage_gb", "bandwidth_mbps"]:
		snapshot[key] = {
			"total": total_capacity.get(key, 0),
			"used": used.get(key, 0),
		}

	return snapshot

func get_monthly_cost_breakdown() -> Dictionary:
	var active_hosting_count := active_contracts.size()
	var purchased_server_count := 0
	var hardware_upkeep_cost := 0
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if bool(purchased_expansions.get(expansion_id, false)):
			purchased_server_count += 1
			hardware_upkeep_cost += int(expansion_data.get("monthly_expense", 0))

	var power_cost := 8 + (active_hosting_count * 5) + (purchased_server_count * 12)
	var internet_cost := 12 + (active_hosting_count * 4) + (purchased_server_count * 6)

	return {
		"power_cost": power_cost,
		"internet_cost": internet_cost,
		"hardware_upkeep_cost": hardware_upkeep_cost,
	}

func purchase_expansion(expansion_id: String) -> Dictionary:
	_initialize_gameplay_state()
	var expansion := _find_expansion_target(expansion_id)
	if expansion.is_empty():
		return {"ok": false, "reason": "Unknown expansion."}
	if bool(purchased_expansions.get(expansion_id, false)):
		return {"ok": false, "reason": "Expansion already purchased."}
	var expansion_cost := int(expansion.get("cost", 0))
	if int(economy_state.get("cash", 0)) < expansion_cost:
		return {"ok": false, "reason": "You need $%s cash on hand before buying the %s." % [str(expansion_cost), str(expansion.get("label", "upgrade"))]}

	var scene_node_path := String(expansion.get("scene_node_path", ""))
	if scene_node_path != "":
		var expansion_node := get_node_or_null(scene_node_path)
		if expansion_node == null:
			return {"ok": false, "reason": "%s is unavailable." % str(expansion.get("label", "Expansion"))}

	economy_state["cash"] = int(economy_state.get("cash", 0)) - expansion_cost
	purchased_expansions[expansion_id] = true
	_sync_selected_expansion()
	_sync_expansion_nodes()
	_refresh_available_contracts()
	_refresh_economy_state()
	_refresh_economy_hud()
	_refresh_main_tab_summaries()
	_refresh_log_panel()
	_refresh_live_service_warning_state()
	save_game()
	return {"ok": true, "status": "%s purchased, cash updated, and added to the shared capacity pool." % str(expansion.get("label", "Expansion"))}

func get_expansion_targets() -> Array:
	_initialize_gameplay_state()
	var targets: Array = []
	for expansion in expansion_catalog:
		var expansion_data := (expansion as Dictionary).duplicate(true)
		var expansion_id := String(expansion_data.get("id", ""))
		expansion_data["purchased"] = bool(purchased_expansions.get(expansion_id, false))
		expansion_data["selected"] = expansion_id == selected_expansion_id
		targets.append(expansion_data)
	return targets

func _initialize_gameplay_state() -> void:
	_resolve_nodes()
	if gameplay_state_initialized:
		return
	gameplay_state_initialized = true

	var computer_station: StaticBody2D = get_node_or_null("Workstations/ComputerStation")

	if computer_station != null:
		starter_hardware = computer_station.get_hardware_specs() if computer_station.has_method("get_hardware_specs") else computer_station.hardware_specs.duplicate(true)

	expansion_catalog = _load_expansion_catalog()
	_sync_selected_expansion()

	_load_all_contracts()
	_refresh_available_contracts()

	_sync_expansion_nodes()

func _build_station_details(station: StaticBody2D, data: Dictionary) -> String:
	_initialize_gameplay_state()
	var lines: PackedStringArray = []

	if station.station_type == "computer":
		var capacity: Dictionary = get_shared_capacity_snapshot()
		lines.append("[b]Shared Capacity Pool[/b]")
		lines.append("CPU: %s cores" % str(capacity["cpu"]["total"]))
		lines.append("RAM: %s GB" % str(capacity["ram_gb"]["total"]))
		lines.append("Storage: %s GB" % str(capacity["storage_gb"]["total"]))
		lines.append("Bandwidth: %s Mbps" % str(capacity["bandwidth_mbps"]["total"]))
		lines.append("")
		lines.append("[b]Used Capacity[/b]")
		lines.append("CPU: %s cores" % str(capacity["cpu"]["used"]))
		lines.append("RAM: %s GB" % str(capacity["ram_gb"]["used"]))
		lines.append("Storage: %s GB" % str(capacity["storage_gb"]["used"]))
		lines.append("Bandwidth: %s Mbps" % str(capacity["bandwidth_mbps"]["used"]))
		lines.append("")
		lines.append("[b]Selected Contract[/b]")
		if selected_contract_id == "":
			lines.append("No accepted contract is queued for hosting.")
		else:
			var selected := _find_pending_contract_by_id(selected_contract_id)
			lines.append("%s ready to activate." % str(selected.get("name", "Unknown contract")))
		if not pending_contracts.is_empty():
			lines.append("")
			lines.append("[b]Accepted Queue[/b]")
			for contract in pending_contracts:
				lines.append("%s" % str(contract.get("name", "Contract")))
		if not active_contracts.is_empty():
			lines.append("")
			lines.append("[b]Active Hosting[/b]")
			for contract in active_contracts:
				lines.append("%s (%s mo left, Satisfaction %s)" % [
					str(contract.get("name", "Contract")),
					str(contract.get("months_remaining", 0)),
					str(contract.get("satisfaction", 100)),
				])
		if not completed_contracts.is_empty():
			lines.append("")
			lines.append("[b]Completed Contracts[/b]")
			for contract in completed_contracts:
				lines.append("%s (Satisfaction %s)" % [
					str(contract.get("name", "Contract")),
					str(contract.get("satisfaction", 100)),
				])
		if not cancelled_contracts.is_empty():
			lines.append("")
			lines.append("[b]Cancelled Contracts[/b]")
			for contract in cancelled_contracts:
				lines.append("%s (Satisfaction %s, Penalties $%s)" % [
					str(contract.get("name", "Contract")),
					str(contract.get("satisfaction", 100)),
					str(contract.get("penalty_total", 0)),
				])
			lines.append("")
			lines.append("[b]Expansion Status[/b]")
			var purchased_upgrade_names: PackedStringArray = []
			for expansion in expansion_catalog:
				var expansion_data := expansion as Dictionary
				var expansion_id := String(expansion_data.get("id", ""))
				if bool(purchased_expansions.get(expansion_id, false)):
					purchased_upgrade_names.append(str(expansion_data.get("label", "Upgrade")))
			if purchased_upgrade_names.is_empty():
				lines.append("Purchased upgrades: none yet.")
			else:
				lines.append("Purchased upgrades: %s" % ", ".join(purchased_upgrade_names))
			var next_expansion := _get_next_expansion_target()
			if next_expansion.is_empty():
				lines.append("All current infrastructure upgrades are online.")
			else:
				var selected_expansion := _get_selected_expansion_target()
				lines.append("Selected upgrade: %s ($%s)" % [
					str(selected_expansion.get("label", "Expansion")),
					str(selected_expansion.get("cost", 0)),
				])
				lines.append("Upcoming in ladder: %s ($%s)" % [
					str(next_expansion.get("label", "Expansion")),
					str(next_expansion.get("cost", 0)),
				])
	elif station.station_type == "contracts":
		lines.append("[b]Available Leads[/b]")
		if available_contracts.is_empty():
			lines.append("No contracts are currently available on the board.")
		else:
			var contract := _find_available_contract_by_id(board_selected_contract_id)
			var resources: Dictionary = contract.get("resources", {})
			lines.append("%s: %s/mo for %s month(s)" % [
				str(contract.get("name", "Contract")),
				str(contract.get("monthly_revenue", "$0")),
				str(contract.get("duration_months", 1)),
			])
			lines.append("Risk: %s | Support: %s" % [
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
			lines.append("Needs %s CPU, %s GB RAM, %s GB storage, %s Mbps bandwidth" % [
				str(resources.get("cpu", 0)),
				str(resources.get("ram_gb", 0)),
				str(resources.get("storage_gb", 0)),
				str(resources.get("bandwidth_mbps", 0)),
			])
			lines.append(_build_contract_unlock_status(contract))
			lines.append("Remaining leads on board: %s" % str(available_contracts.size()))
		var locked_contracts := _get_locked_contracts()
		if locked_contracts.is_empty():
			lines.append("All current local leads are unlocked.")
		else:
			lines.append("")
			lines.append("[b]Locked Leads[/b]")
			for locked_contract in locked_contracts.slice(0, 2):
				lines.append("%s" % str(locked_contract.get("name", "Locked contract")))
				lines.append("%s" % str(locked_contract.get("unlock_status", "Locked.")))
	elif station.station_type == "expansion":
		var hardware: Dictionary = data.get("hardware", {})
		lines.append("[b]Added Capacity[/b]")
		lines.append("CPU: +%s cores" % str(hardware.get("cpu", 0)))
		lines.append("RAM: +%s GB" % str(hardware.get("ram_gb", 0)))
		lines.append("Storage: +%s GB" % str(hardware.get("storage_gb", 0)))
		lines.append("Bandwidth: +%s Mbps" % str(hardware.get("bandwidth_mbps", 0)))
		lines.append("")
		lines.append("[b]Support Role[/b]")
		lines.append("This unit hints at overflow hosting, managed backups, and the first always-on support responsibilities beyond the starter PC.")
	else:
		var hardware: Dictionary = data.get("hardware", {})
		if not hardware.is_empty():
			lines.append("[b]Starter Hardware[/b]")
			lines.append("CPU: %s cores" % str(hardware.get("cpu", 0)))

	return "\n".join(lines)

func _build_station_hint(station: StaticBody2D) -> String:
	if station.station_type == "contracts":
		return "Use Previous and Next to browse available leads, then Accept Contract to move one into the hosting queue. Higher-tier work unlocks as the garage grows. Press E or Esc to close."
	if station.station_type == "computer":
		var base_hint := "Use Activate Hosting or Clear Selection for the current contract. Press E or Esc to close."
		var selected_upgrade := _get_selected_expansion_target()
		if selected_upgrade.is_empty():
			return base_hint
		return "%s\n%s" % [base_hint, _build_selected_upgrade_hint(selected_upgrade)]
	if station.station_type == "expansion":
		return "Review this server upgrade target here. The full expansion flow will connect in the next step. Press E or Esc to close."
	return "Press E or Esc to close"

func _render_station_actions(station: StaticBody2D) -> void:
	_clear_station_actions()

	for action in get_station_actions(station):
		var button := Button.new()
		button.text = str(action.get("label", "Action"))
		button.pressed.connect(_on_station_action_pressed.bind(String(action.get("id", ""))))
		panel_actions.add_child(button)

func _clear_station_actions() -> void:
	if panel_actions == null:
		return

	while panel_actions.get_child_count() > 0:
		var child := panel_actions.get_child(0)
		panel_actions.remove_child(child)
		child.queue_free()

func _on_station_action_pressed(action_id: String) -> void:
	call_deferred("_run_station_action_from_button", action_id)

func _run_station_action_from_button(action_id: String) -> void:
	press_station_action(action_id)

func _refresh_economy_hud() -> void:
	_resolve_nodes()
	var snapshot := get_economy_snapshot()
	if economy_cash_value != null:
		economy_cash_value.text = "$%s" % str(snapshot.get("cash", 0))
	if economy_income_value != null:
		economy_income_value.text = "$%s/mo" % str(snapshot.get("monthly_income", 0))
	if economy_expenses_value != null:
		economy_expenses_value.text = "$%s/mo" % str(snapshot.get("monthly_expenses", 0))
	if economy_next_cost_value != null:
		economy_next_cost_value.text = str(snapshot.get("next_cost", ""))

func _refresh_log_panel() -> void:
	_resolve_nodes()
	if log_contracts_body != null:
		var contract_lines: PackedStringArray = []
		var locked_contracts := _get_locked_contracts()
		contract_lines.append("Available leads: %s" % str(available_contracts.size()))
		contract_lines.append("Locked leads: %s" % str(locked_contracts.size()))
		contract_lines.append("Accepted queue: %s" % str(pending_contracts.size()))
		contract_lines.append("Active hosting: %s" % str(active_contracts.size()))
		contract_lines.append("Support incidents: %s" % str(active_incidents.size()))
		if not active_contracts.is_empty():
			var active_names: PackedStringArray = []
			for contract in active_contracts:
				active_names.append(str(contract.get("name", "Contract")))
			contract_lines.append("Live: %s" % ", ".join(active_names))
		elif not pending_contracts.is_empty():
			var pending_names: PackedStringArray = []
			for contract in pending_contracts:
				pending_names.append(str(contract.get("name", "Contract")))
			contract_lines.append("Queue: %s" % ", ".join(pending_names))
		if not active_incidents.is_empty():
			contract_lines.append("Incident desk: %s" % str((active_incidents[0] as Dictionary).get("headline", "Support issue on deck.")))
		if not locked_contracts.is_empty():
			contract_lines.append("Next unlock: %s" % str((locked_contracts[0] as Dictionary).get("unlock_status", "Locked.")))
		log_contracts_body.text = "\n".join(contract_lines)

	if log_progress_body != null:
		var progress_lines: PackedStringArray = []
		progress_lines.append("Hardware stage: %s" % _get_hardware_stage_label())
		var next_expansion := _get_next_expansion_target()
		if next_expansion.is_empty():
			progress_lines.append("Upgrade path: Current planned shop hardware is online.")
		else:
			progress_lines.append("Upgrade path: Next is %s." % str(next_expansion.get("label", "the next upgrade")))
		var locked_contracts := _get_locked_contracts()
		if not locked_contracts.is_empty():
			progress_lines.append("Contract gate: %s" % str((locked_contracts[0] as Dictionary).get("name", "Locked contract")))
		else:
			progress_lines.append("Contract gate: All current local leads are open.")
		progress_lines.append("Support pressure: %s" % _get_support_pressure_summary())
		progress_lines.append("Completed contracts: %s" % str(completed_contracts.size()))
		progress_lines.append("Cancelled contracts: %s" % str(cancelled_contracts.size()))
		log_progress_body.text = "\n".join(progress_lines)

	_apply_log_panel_state()

func _refresh_main_tab_summaries() -> void:
	_resolve_nodes()
	var onboarding_label: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/OnboardingPanel/Margin/Content/Body")
	var available_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/AvailableSection/Margin/Content/Body")
	var queue_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/QueueSection/Margin/Content/Body")
	var active_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ActiveSection/Margin/Content/Body")
	var outcomes_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/OutcomesSection/Margin/Content/Body")
	var response_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Body")
	var progress_body: Label = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ProgressTab/ProgressSummary/Margin/Content/Body")
	if onboarding_label != null:
		onboarding_body = onboarding_label
		onboarding_label.text = _build_onboarding_summary()
	if available_body != null:
		contracts_available_body = available_body
		available_body.text = _build_contracts_available_summary()
	if queue_body != null:
		contracts_queue_body = queue_body
		queue_body.text = _build_contracts_queue_summary()
	if active_body != null:
		contracts_active_body = active_body
		active_body.text = _build_contracts_active_summary()
	if outcomes_body != null:
		contracts_outcomes_body = outcomes_body
		outcomes_body.text = _build_contracts_outcomes_summary()
	if response_body != null:
		contracts_response_body = response_body
		response_body.text = _build_contracts_response_summary()
	if progress_body != null:
		progress_summary_body = progress_body
		progress_body.text = _build_progress_tab_summary()

func _build_contracts_available_summary() -> String:
	var lines: PackedStringArray = []
	var locked_contracts := _get_locked_contracts()
	lines.append("Available leads: %s" % str(available_contracts.size()))
	lines.append("Locked leads: %s" % str(locked_contracts.size()))
	if not available_contracts.is_empty():
		var lead_lines: PackedStringArray = []
		for contract in available_contracts:
			lead_lines.append("%s | Risk: %s | Support: %s" % [
				str(contract.get("name", "Contract")),
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
		lines.append("Board: %s" % " / ".join(lead_lines))
		var selected_contract := _find_available_contract_by_id(board_selected_contract_id)
		if not selected_contract.is_empty():
			lines.append("Board focus: %s" % str(selected_contract.get("name", "Current board lead")))
	if not locked_contracts.is_empty():
		var next_locked := locked_contracts[0] as Dictionary
		lines.append("Next unlock: %s | %s" % [
			str(next_locked.get("name", "Locked contract")),
			str(next_locked.get("unlock_status", "Locked.")),
		])
	lines.append("Level-two readiness:")
	lines.append_array(_build_mitigation_readiness_lines())
	return "\n".join(lines)

func _build_onboarding_summary() -> String:
	var lines: PackedStringArray = []
	lines.append("Hardware stage: %s" % _get_hardware_stage_label())
	lines.append("Support pressure: %s" % _get_support_pressure_summary())
	var locked_contracts := _get_locked_contracts()
	if not locked_contracts.is_empty():
		var next_locked := locked_contracts[0] as Dictionary
		lines.append("Next lead gate: %s" % str(next_locked.get("unlock_status", "Locked.")))
	else:
		lines.append("Next lead gate: All current local leads are open.")
	lines.append("Level-two readiness:")
	lines.append_array(_build_mitigation_readiness_lines())

	if active_contracts.is_empty() and pending_contracts.is_empty():
		lines.append("Walk to the contract board, accept a small client, then return to the workstation to activate hosting.")
		lines.append("Watch for overload warnings as you stack jobs, and plan toward your first server upgrade.")
	elif not pending_contracts.is_empty():
		lines.append("You have accepted work from the contract board.")
		lines.append("Walk back to the workstation and activate hosting so the new client starts paying on the next month.")
	elif _is_service_overloaded():
		lines.append("Your workstation is carrying too much load.")
		lines.append("Clear the pressure, avoid overload warnings, and keep support expectations steady before clients start slipping.")
	elif not bool(purchased_expansions.get("external_server_unit", false)):
		lines.append("Hosting is live.")
		lines.append("Keep one eye on overload risk and start saving for the first server so the garage can support more demanding contracts.")
	else:
		lines.append("The first server is online.")
		lines.append("Keep rotating between the contract board and workstation while you grow into higher-support workloads.")
	return "\n".join(lines)

func _build_contracts_queue_summary() -> String:
	var lines: PackedStringArray = []
	lines.append("Accepted queue: %s" % str(pending_contracts.size()))
	if not pending_contracts.is_empty():
		var pending_names: PackedStringArray = []
		for contract in pending_contracts:
			pending_names.append("%s | Risk: %s | Support: %s" % [
				str(contract.get("name", "Contract")),
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
		lines.append("Queue: %s" % ", ".join(pending_names))
	else:
		lines.append("Queue: No accepted work waiting for hosting.")
	return "\n".join(lines)

func _build_contracts_active_summary() -> String:
	var lines: PackedStringArray = []
	lines.append("Active hosting: %s" % str(active_contracts.size()))
	lines.append("Support incidents: %s" % str(active_incidents.size()))
	if not active_contracts.is_empty():
		var active_names: PackedStringArray = []
		for contract in active_contracts:
			active_names.append("%s (%s mo left) | Risk: %s | Support: %s" % [
				str(contract.get("name", "Contract")),
				str(contract.get("months_remaining", 0)),
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
		lines.append("Live: %s" % ", ".join(active_names))
	else:
		lines.append("Live: No contracts are currently hosted.")
	if not active_incidents.is_empty():
		var incident_lines: PackedStringArray = []
		for incident in active_incidents.slice(0, 2):
			incident_lines.append(str((incident as Dictionary).get("headline", "Support incident")))
		lines.append("Incident desk: %s" % " / ".join(incident_lines))
	return "\n".join(lines)

func _build_contracts_outcomes_summary() -> String:
	var lines: PackedStringArray = []
	lines.append("Completed contracts: %s" % str(completed_contracts.size()))
	lines.append("Cancelled contracts: %s" % str(cancelled_contracts.size()))
	if not completed_contracts.is_empty():
		var completed_names: PackedStringArray = []
		for contract in completed_contracts:
			completed_names.append("%s | Risk: %s | Support: %s" % [
				str(contract.get("name", "Contract")),
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
		lines.append("Completed: %s" % ", ".join(completed_names))
	if not cancelled_contracts.is_empty():
		var cancelled_names: PackedStringArray = []
		for contract in cancelled_contracts:
			cancelled_names.append("%s | Risk: %s | Support: %s" % [
				str(contract.get("name", "Contract")),
				str(contract.get("risk_tag", "Standard")),
				str(contract.get("support_tag", "Standard")),
			])
		lines.append("Cancelled: %s" % ", ".join(cancelled_names))
	return "\n".join(lines)

func _build_contracts_response_summary() -> String:
	var lines: PackedStringArray = []
	lines.append("New hosting hold: %s" % ("Enabled" if hosting_hold_enabled else "Off"))
	lines.append("Support incidents: %s" % str(active_incidents.size()))
	if not active_incidents.is_empty():
		var incident := active_incidents[0] as Dictionary
		lines.append("Next issue: %s" % str(incident.get("headline", "Support issue on deck.")))
		lines.append("Suggested fix: %s" % str(incident.get("mitigation_hint", "Use the response tools to work the queue.")))
	if hosting_hold_enabled and not active_incidents.is_empty():
		lines.append("Hold is covering the live support queue this cycle so you can work the issue list before it costs goodwill.")
	elif hosting_hold_enabled:
		lines.append("Throttle mode active. New activation is paused while overloaded contracts are protected.")
	elif not active_incidents.is_empty():
		lines.append("Support incidents are live. Use Backup Pass, Late Shift, or Stabilize Line before falling back to a full hold.")
	elif _is_service_overloaded():
		lines.append("Service warnings are active. Enable Hold to pause new hosting and avoid satisfaction loss at reduced revenue.")
	else:
		lines.append("Enable Hold to pause new activation if service warnings start stacking, or keep the mitigation tools ready for tier-two work.")
	lines.append("Level-two readiness:")
	lines.append_array(_build_mitigation_readiness_lines())
	return "\n".join(lines)

func _build_progress_tab_summary() -> String:
	var lines: PackedStringArray = []
	var purchased_count := 0
	var purchased_labels: PackedStringArray = []
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if bool(purchased_expansions.get(expansion_id, false)):
			purchased_count += 1
			purchased_labels.append(str(expansion_data.get("label", "Upgrade")))

	var next_expansion := _get_next_expansion_target()
	if next_expansion.is_empty():
		lines.append("All planned infrastructure upgrades are online.")
	else:
		lines.append("Goal: Purchase the %s." % str(next_expansion.get("label", "next upgrade")))
	lines.append("Hardware stage: %s" % _get_hardware_stage_label())
	lines.append("Infrastructure online: %s/%s" % [str(purchased_count), str(expansion_catalog.size())])
	if purchased_labels.is_empty():
		lines.append("Purchased hardware: Starter PC only.")
	else:
		lines.append("Purchased hardware: Starter PC, %s" % ", ".join(purchased_labels))
	var available_labels: PackedStringArray = []
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if not bool(purchased_expansions.get(expansion_id, false)):
			available_labels.append(str(expansion_data.get("label", "Upgrade")))
	if not available_labels.is_empty():
		lines.append("Available upgrades: %s" % ", ".join(available_labels))
	lines.append("Completed contracts: %s" % str(completed_contracts.size()))
	var locked_contracts := _get_locked_contracts()
	if not locked_contracts.is_empty():
		var next_locked := locked_contracts[0] as Dictionary
		lines.append("Contract unlock track: %s" % str(next_locked.get("unlock_status", "Locked.")))
	else:
		lines.append("Contract unlock track: All current local leads are open.")
	lines.append("Support incidents: %s" % str(active_incidents.size()))
	lines.append("Support pressure: %s" % _get_support_pressure_summary())
	lines.append("Level-two readiness:")
	lines.append_array(_build_mitigation_readiness_lines())
	lines.append("Cancelled contracts: %s" % str(cancelled_contracts.size()))
	lines.append("Warnings this cycle: %s" % str(warning_log.size()))
	return "\n".join(lines)

func _refresh_time_controls() -> void:
	_resolve_nodes()
	var snapshot := get_time_snapshot()
	if time_date_label != null:
		time_date_label.text = "%s %s" % [str(snapshot["month_name"]), str(snapshot["day"])]
	if time_clock_label != null:
		time_clock_label.text = "%02d:%02d" % [int(snapshot["hour"]), int(snapshot["minute"])]
	if time_speed_label != null:
		time_speed_label.text = "Speed %sx" % str(snapshot["speed"])
	if time_status_label != null:
		time_status_label.text = "Status: %s" % ("Running" if clock_running else "Paused")
	if time_controls != null:
		time_controls.modulate = Color(1, 1, 1, 1) if clock_running else Color(0.72, 0.76, 0.82, 1)
	_refresh_speed_button_highlights(int(snapshot["speed"]))

func _refresh_speed_button_highlights(active_speed: int) -> void:
	var active_color := Color(0.972549, 0.788235, 0.352941, 1)
	var inactive_color := Color(0.78, 0.82, 0.86, 1)
	var speed_buttons := {
		1: time_speed_1_button,
		2: time_speed_2_button,
		3: time_speed_3_button,
	}

	for speed in speed_buttons.keys():
		var button: Button = speed_buttons[speed]
		if button == null:
			continue
		button.disabled = int(speed) == active_speed
		button.modulate = active_color if int(speed) == active_speed else inactive_color

func _refresh_monthly_summary(report: Dictionary) -> void:
	_resolve_nodes()
	if monthly_summary == null or monthly_summary_text == null:
		return

	var current_month_name: String = month_names[max(0, int(clock_state["month"]) - 1)]
	var summary_lines: PackedStringArray = []
	summary_lines.append("%s opened with a fresh ledger." % current_month_name)
	summary_lines.append("Payouts: $%s | Expenses: $%s" % [
		str(report.get("payout_total", 0)),
		str(report.get("expense_total", 0) + report.get("penalty_total", 0)),
	])
	summary_lines.append("Warnings: %s | Completed: %s | Cancelled: %s" % [
		str(report.get("warning_count", 0)),
		str((report.get("completed_contract_ids", []) as Array).size()),
		str((report.get("cancelled_contract_ids", []) as Array).size()),
	])
	if int(report.get("incident_count", 0)) > 0:
		summary_lines.append("Support: %s handled | %s neglected" % [
			str(report.get("resolved_incident_count", 0)),
			str(report.get("neglected_incident_count", 0)),
		])
	monthly_summary_text.text = "\n".join(summary_lines)
	monthly_summary.visible = true

func _refresh_warning_alert(report: Dictionary) -> void:
	_resolve_nodes()
	if warning_alert == null or warning_alert_text == null:
		return
	if warning_log.is_empty() and int(report.get("warning_count", 0)) <= 0 and (report.get("cancelled_contract_ids", []) as Array).is_empty():
		hide_warning_alert()
		return

	var lines: PackedStringArray = []
	for warning_message in warning_log:
		lines.append(str(warning_message))

	if int(report.get("resolved_incident_count", 0)) > 0:
		lines.append("Support incidents were handled before they turned into client-visible damage.")
	if int(report.get("warning_count", 0)) > 0:
		lines.append("Ongoing issues will lower contract satisfaction.")
	if int(report.get("penalty_total", 0)) > 0:
		lines.append("Unresolved problems are already causing financial penalties.")
	else:
		lines.append("If the issue continues, penalties and cancellations can follow.")
	if not (report.get("cancelled_contract_ids", []) as Array).is_empty():
		lines.append("Some contracts have already been cancelled due to service failures.")

	warning_alert_text.text = "\n".join(lines)
	warning_alert.visible = true

func _build_save_payload() -> Dictionary:
	return {
		"available_contracts": available_contracts.duplicate(true),
		"pending_contracts": pending_contracts.duplicate(true),
		"selected_contract_id": selected_contract_id,
		"board_selected_contract_id": board_selected_contract_id,
		"active_contracts": active_contracts.duplicate(true),
		"completed_contracts": completed_contracts.duplicate(true),
		"cancelled_contracts": cancelled_contracts.duplicate(true),
		"active_incidents": active_incidents.duplicate(true),
			"warning_log": warning_log.duplicate(true),
			"starter_hardware": starter_hardware.duplicate(true),
			"selected_contract_index": selected_contract_index,
			"selected_expansion_id": selected_expansion_id,
			"purchased_expansions": purchased_expansions.duplicate(true),
		"hosting_hold_enabled": hosting_hold_enabled,
		"clock_state": clock_state.duplicate(true),
		"last_monthly_report": last_monthly_report.duplicate(true),
		"clock_running": clock_running,
		"pending_time_minutes": pending_time_minutes,
		"economy_state": economy_state.duplicate(true),
		"active_main_tab": active_main_tab,
		"log_panel_collapsed": log_panel_collapsed,
	}

func _apply_save_payload(payload: Dictionary) -> void:
	available_contracts = (payload.get("available_contracts", []) as Array).duplicate(true)
	pending_contracts = (payload.get("pending_contracts", []) as Array).duplicate(true)
	selected_contract_id = str(payload.get("selected_contract_id", ""))
	board_selected_contract_id = str(payload.get("board_selected_contract_id", ""))
	active_contracts = (payload.get("active_contracts", []) as Array).duplicate(true)
	completed_contracts = (payload.get("completed_contracts", []) as Array).duplicate(true)
	cancelled_contracts = (payload.get("cancelled_contracts", []) as Array).duplicate(true)
	active_incidents = (payload.get("active_incidents", []) as Array).duplicate(true)
	warning_log = (payload.get("warning_log", []) as Array).duplicate(true)
	starter_hardware = (payload.get("starter_hardware", {}) as Dictionary).duplicate(true)
	expansion_catalog = _load_expansion_catalog()
	selected_contract_index = int(payload.get("selected_contract_index", -1))
	selected_expansion_id = str(payload.get("selected_expansion_id", ""))
	purchased_expansions = (payload.get("purchased_expansions", {}) as Dictionary).duplicate(true)
	_sync_selected_expansion()
	hosting_hold_enabled = bool(payload.get("hosting_hold_enabled", false))
	clock_state = (payload.get("clock_state", {}) as Dictionary).duplicate(true)
	last_monthly_report = (payload.get("last_monthly_report", {}) as Dictionary).duplicate(true)
	clock_running = bool(payload.get("clock_running", true))
	pending_time_minutes = float(payload.get("pending_time_minutes", 0.0))
	economy_state = (payload.get("economy_state", {}) as Dictionary).duplicate(true)
	active_main_tab = str(payload.get("active_main_tab", "operations"))
	log_panel_collapsed = bool(payload.get("log_panel_collapsed", false))
	gameplay_state_initialized = true
	opened_station = null
	focused_station = null

	_sync_expansion_nodes()

	hide_station_panel()
	hide_monthly_summary()
	if warning_log.is_empty():
		hide_warning_alert()
	else:
		_refresh_warning_alert({
			"warning_count": warning_log.size(),
			"penalty_total": 0,
			"cancelled_contract_ids": [],
		})

	_select_main_tab(active_main_tab)
	_refresh_economy_state()
	_refresh_economy_hud()
	_refresh_time_controls()
	_load_all_contracts()
	_refresh_available_contracts()
	_refresh_main_tab_summaries()
	_refresh_log_panel()

func _reset_runtime_state() -> void:
	all_contracts.clear()
	available_contracts.clear()
	pending_contracts.clear()
	selected_contract_id = ""
	board_selected_contract_id = ""
	active_contracts.clear()
	completed_contracts.clear()
	cancelled_contracts.clear()
	active_incidents.clear()
	warning_log.clear()
	starter_hardware.clear()
	expansion_catalog.clear()
	selected_expansion_id = ""
	selected_contract_index = -1
	purchased_expansions.clear()
	hosting_hold_enabled = false
	gameplay_state_initialized = false
	active_main_tab = "operations"
	log_panel_collapsed = false
	clock_state = {
		"month": 1,
		"day": 1,
		"hour": 8,
		"minute": 0,
		"speed": 1,
	}
	last_monthly_report = {}
	clock_running = true
	pending_time_minutes = 0.0
	economy_state = {
		"cash": 1200,
		"monthly_income": 0,
		"monthly_expenses": 0,
		"power_cost": 0,
		"internet_cost": 0,
		"hardware_upkeep_cost": 0,
		"next_cost": "External Server Unit purchase: $450",
	}
	opened_station = null
	focused_station = null
	_initialize_gameplay_state()
	hide_station_panel()
	hide_monthly_summary()
	hide_warning_alert()
	_select_main_tab(active_main_tab)
	_refresh_economy_state()
	_refresh_economy_hud()
	_refresh_time_controls()
	_refresh_main_tab_summaries()
	_refresh_log_panel()

func _update_save_controls(status_text: String = "") -> void:
	_resolve_nodes()
	var has_save := has_save_data()
	if continue_button != null:
		continue_button.disabled = not has_save
	if save_status_label != null:
		if status_text != "":
			save_status_label.text = status_text
		elif has_save:
			save_status_label.text = "Local save ready to continue."
		else:
			save_status_label.text = "No local save yet."

func _refresh_live_service_warning_state() -> void:
	warning_log.clear()
	if not active_incidents.is_empty():
		for incident in active_incidents:
			var incident_data := incident as Dictionary
			if hosting_hold_enabled:
				warning_log.append("Support hold is active for %s." % str(incident_data.get("headline", "a support incident")))
			else:
				warning_log.append(str(incident_data.get("warning_message", "A support issue needs attention.")))

	if active_contracts.is_empty():
		if warning_log.is_empty():
			hide_warning_alert()
		else:
			_refresh_warning_alert({
				"warning_count": warning_log.size(),
				"penalty_total": 0,
				"cancelled_contract_ids": [],
				"resolved_incident_count": 0,
			})
		_refresh_main_tab_summaries()
		_refresh_log_panel()
		return

	if not _is_service_overloaded():
		if warning_log.is_empty():
			hide_warning_alert()
		else:
			_refresh_warning_alert({
				"warning_count": warning_log.size(),
				"penalty_total": 0,
				"cancelled_contract_ids": [],
				"resolved_incident_count": 0,
			})
		_refresh_main_tab_summaries()
		_refresh_log_panel()
		return

	for contract in active_contracts:
		if hosting_hold_enabled:
			warning_log.append("%s is being throttled under a hosting hold to protect service quality." % str(contract.get("name", "Contract")))
		else:
			warning_log.append("%s is experiencing degraded service due to tight hosting capacity." % str(contract.get("name", "Contract")))

	_refresh_warning_alert({
		"warning_count": warning_log.size(),
		"penalty_total": 0,
		"cancelled_contract_ids": [],
		"resolved_incident_count": 0,
	})
	_refresh_main_tab_summaries()
	_refresh_log_panel()

func _queue_support_incident_for_contract(contract: Dictionary) -> void:
	var contract_id := String(contract.get("id", ""))
	if contract_id == "" or _find_active_incident_index_by_contract_id(contract_id) >= 0:
		_refresh_live_service_warning_state()
		return

	var incident := _build_support_incident_for_contract(contract)
	if incident.is_empty():
		_refresh_live_service_warning_state()
		return

	active_incidents.append(incident)
	_refresh_live_service_warning_state()

func _build_support_incident_for_contract(contract: Dictionary) -> Dictionary:
	var contract_id := String(contract.get("id", ""))
	var contract_name := str(contract.get("name", "Contract"))
	var risk_tag := str(contract.get("risk_tag", "Standard"))
	var support_tag := str(contract.get("support_tag", "Standard"))

	if support_tag.contains("After-hours"):
		return _apply_incident_preparedness({
			"id": "%s_after_hours" % contract_id,
			"contract_id": contract_id,
			"incident_type": "after_hours",
			"headline": "%s needs a late-night content push." % contract_name,
			"warning_message": "%s called after hours because a promo update is stuck on the old site copy." % contract_name,
			"mitigation_hint": "Use Late Shift to clear the after-hours request before month end.",
			"penalty": 5,
			"satisfaction_loss": 7,
		})
	if risk_tag.contains("Traffic Spikes"):
		return _apply_incident_preparedness({
			"id": "%s_internet_slowdown" % contract_id,
			"contract_id": contract_id,
			"incident_type": "internet_slowdown",
			"headline": "%s is hitting a consumer internet slowdown." % contract_name,
			"warning_message": "%s is seeing evening slowdown on the home internet line and wants you to stabilize it." % contract_name,
			"mitigation_hint": "Use Stabilize Line to work the connection issue before it becomes a penalty.",
			"penalty": 5,
			"satisfaction_loss": 6,
		})
	if support_tag.contains("Priority") or risk_tag.contains("Compliance Sensitive"):
		return _apply_incident_preparedness({
			"id": "%s_backup_check" % contract_id,
			"contract_id": contract_id,
			"incident_type": "backup_check",
			"headline": "%s wants a backup verification check." % contract_name,
			"warning_message": "%s asked for proof that last night's backup and security updates actually finished." % contract_name,
			"mitigation_hint": "Use Backup Pass to verify the backups and calm the support request.",
			"penalty": 4,
			"satisfaction_loss": 5,
		})
	if support_tag.contains("Community"):
		return _apply_incident_preparedness({
			"id": "%s_forum_cleanup" % contract_id,
			"contract_id": contract_id,
			"incident_type": "forum_cleanup",
			"headline": "%s is dealing with a signup rush." % contract_name,
			"warning_message": "%s needs a quick forum cleanup after a burst of new signups stressed the little server stack." % contract_name,
			"mitigation_hint": "Use Late Shift to clean up the signup rush before the community gets frustrated.",
			"penalty": 5,
			"satisfaction_loss": 6,
		})
	return {}

func _process_support_incidents_for_month() -> Dictionary:
	if active_incidents.is_empty():
		return {
			"incident_count": 0,
			"resolved_count": 0,
			"neglected_count": 0,
			"warning_count": 0,
			"penalty_total": 0,
		}

	var report := {
		"incident_count": active_incidents.size(),
		"resolved_count": 0,
		"neglected_count": 0,
		"warning_count": 0,
		"penalty_total": 0,
	}

	if hosting_hold_enabled:
		for incident in active_incidents:
			var incident_data := incident as Dictionary
			warning_log.append("Handled: %s" % str(incident_data.get("headline", "Support incident")))
		report["resolved_count"] = active_incidents.size()
		active_incidents.clear()
		return report

	for incident in active_incidents:
		var incident_data := incident as Dictionary
		var contract_id := String(incident_data.get("contract_id", ""))
		var incident_index := _find_active_contract_index_by_id(contract_id)
		if incident_index < 0:
			continue

		var contract := active_contracts[incident_index] as Dictionary
		contract["satisfaction"] = max(0, int(contract.get("satisfaction", 100)) - int(incident_data.get("satisfaction_loss", 0)))
		active_contracts[incident_index] = contract
		report["neglected_count"] = int(report.get("neglected_count", 0)) + 1
		report["warning_count"] = int(report.get("warning_count", 0)) + 1
		report["penalty_total"] = int(report.get("penalty_total", 0)) + int(incident_data.get("penalty", 0))
		warning_log.append(str(incident_data.get("warning_message", "A support issue needs attention.")))
	return report

func _prune_support_incidents() -> void:
	for index in range(active_incidents.size() - 1, -1, -1):
		var incident := active_incidents[index] as Dictionary
		if _find_active_contract_index_by_id(String(incident.get("contract_id", ""))) >= 0:
			continue
		active_incidents.remove_at(index)

func _find_active_incident_index_by_contract_id(contract_id: String) -> int:
	for index in range(active_incidents.size()):
		var incident := active_incidents[index] as Dictionary
		if String(incident.get("contract_id", "")) == contract_id:
			return index
	return -1

func _find_active_contract_index_by_id(contract_id: String) -> int:
	for index in range(active_contracts.size()):
		var contract := active_contracts[index] as Dictionary
		if String(contract.get("id", "")) == contract_id:
			return index
	return -1

func _resolve_support_incident_by_type(incident_types: Array, cash_cost: int, success_status: String) -> Dictionary:
	_initialize_gameplay_state()
	for index in range(active_incidents.size()):
		var incident := active_incidents[index] as Dictionary
		if not incident_types.has(String(incident.get("incident_type", ""))):
			continue
		var adjusted_cost: int = max(1, cash_cost - _get_incident_response_cost_reduction(String(incident.get("incident_type", ""))))
		if int(economy_state.get("cash", 0)) < adjusted_cost:
			return {"ok": false, "reason": "You need $%s cash on hand to work this issue properly." % str(adjusted_cost)}

		economy_state["cash"] = int(economy_state.get("cash", 0)) - adjusted_cost
		active_incidents.remove_at(index)
		_refresh_economy_state()
		_refresh_economy_hud()
		_refresh_live_service_warning_state()
		save_game()
		return {"ok": true, "status": success_status}

	return {"ok": false, "reason": "No matching live support issue needs that response right now."}

func _get_hardware_stage_label() -> String:
	if bool(purchased_expansions.get("managed_rack_server", false)):
		return "Small server corner"
	if bool(purchased_expansions.get("external_server_unit", false)):
		return "First tower server"
	return "Starter garage PC"

func _get_support_pressure_summary() -> String:
	if not active_incidents.is_empty():
		if hosting_hold_enabled:
			return "Hold is active while the current support queue gets handled."
		return "%s live issue(s) need attention." % str(active_incidents.size())
	if _is_service_overloaded():
		if hosting_hold_enabled:
			return "Overload is being throttled under hosting hold."
		return "Overload warnings are stacking on the starter setup."
	return "No active support fire drills right now."

func _apply_incident_preparedness(incident: Dictionary) -> Dictionary:
	var incident_type := String(incident.get("incident_type", ""))
	if incident_type == "":
		return incident

	var mitigation := _get_incident_mitigation_totals(incident_type)
	incident["penalty"] = max(0, int(incident.get("penalty", 0)) - int(mitigation.get("penalty_reduction", 0)))
	incident["satisfaction_loss"] = max(0, int(incident.get("satisfaction_loss", 0)) - int(mitigation.get("satisfaction_reduction", 0)))
	if int(mitigation.get("penalty_reduction", 0)) > 0 or int(mitigation.get("satisfaction_reduction", 0)) > 0:
		incident["mitigation_hint"] = "%s Preparedness is already softening this issue." % str(incident.get("mitigation_hint", ""))
	return incident

func _get_incident_response_cost_reduction(incident_type: String) -> int:
	return int(_get_incident_mitigation_totals(incident_type).get("response_cost_reduction", 0))

func _build_mitigation_readiness_lines() -> PackedStringArray:
	var lines: PackedStringArray = []
	lines.append(_build_incident_readiness_line("backup_check", "Backup checks", "Backup Pass"))
	lines.append(_build_incident_readiness_line("after_hours", "After-hours requests", "Late Shift"))
	lines.append(_build_incident_readiness_line("internet_slowdown", "Traffic-spike issues", "Stabilize Line"))
	return lines

func _build_incident_readiness_line(incident_type: String, label: String, manual_action: String) -> String:
	var sources := _get_incident_mitigation_sources(incident_type)
	var active_labels := sources.get("active_labels", PackedStringArray()) as PackedStringArray
	var available_labels := sources.get("available_labels", PackedStringArray()) as PackedStringArray
	var active_text := "none"
	if not active_labels.is_empty():
		active_text = ", ".join(active_labels)
	var available_text := "none"
	if not available_labels.is_empty():
		available_text = ", ".join(available_labels)
	var missing_text := "none"
	if active_labels.is_empty():
		missing_text = "hardware prep still needed"
		if available_labels.is_empty():
			missing_text = "manual response only"
	return "%s: Active %s | Available %s | Missing %s | Manual %s" % [
		label,
		active_text,
		available_text,
		missing_text,
		manual_action,
	]

func _get_incident_mitigation_sources(incident_type: String) -> Dictionary:
	var active_labels := PackedStringArray()
	var available_labels := PackedStringArray()
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var mitigation_effects := expansion_data.get("mitigation_effects", {}) as Dictionary
		if not mitigation_effects.has(incident_type):
			continue
		var expansion_label := str(expansion_data.get("label", "Upgrade"))
		var expansion_id := String(expansion_data.get("id", ""))
		if bool(purchased_expansions.get(expansion_id, false)):
			active_labels.append(expansion_label)
		else:
			available_labels.append(expansion_label)
	return {
		"active_labels": active_labels,
		"available_labels": available_labels,
	}

func _get_incident_mitigation_totals(incident_type: String) -> Dictionary:
	var totals := {
		"penalty_reduction": 0,
		"satisfaction_reduction": 0,
		"response_cost_reduction": 0,
	}
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if not bool(purchased_expansions.get(expansion_id, false)):
			continue
		var mitigation_effects := expansion_data.get("mitigation_effects", {}) as Dictionary
		var incident_effect := mitigation_effects.get(incident_type, {}) as Dictionary
		totals["penalty_reduction"] = int(totals.get("penalty_reduction", 0)) + int(incident_effect.get("penalty_reduction", 0))
		totals["satisfaction_reduction"] = int(totals.get("satisfaction_reduction", 0)) + int(incident_effect.get("satisfaction_reduction", 0))
		totals["response_cost_reduction"] = int(totals.get("response_cost_reduction", 0)) + int(incident_effect.get("response_cost_reduction", 0))
	return totals

func _on_time_speed_pressed(speed: int) -> void:
	set_time_speed(speed)

func _on_time_start_pressed() -> void:
	start_time()

func _on_main_tab_pressed(tab_id: String) -> void:
	_select_main_tab(tab_id)

func _on_contracts_board_contract_selected(contract: Dictionary) -> void:
	if contract.is_empty():
		return
	panel_action_status.text = "Selected %s from the contracts board." % str(contract.get("name", "Contract"))

func _on_log_panel_toggle_pressed() -> void:
	toggle_log_panel()

func _on_station_panel_close_pressed() -> void:
	hide_station_panel()

func _hide_contracts_board_panel() -> void:
	if contracts_board_panel == null:
		return
	if contracts_board_panel.has_method("hide_board"):
		contracts_board_panel.hide_board()
	else:
		contracts_board_panel.hide()

func _on_time_pause_pressed() -> void:
	pause_time()

func _on_time_faster_pressed() -> void:
	increase_time_speed()

func _on_time_reset_pressed() -> void:
	reset_time_speed()

func _on_time_next_month_pressed() -> void:
	advance_to_next_month()

func _on_time_speed_state_pressed(speed: int) -> void:
	set_time_speed(speed)
	if speed == 1:
		clock_running = true
		_refresh_time_controls()

func _on_monthly_summary_close_pressed() -> void:
	hide_monthly_summary()

func _on_warning_alert_close_pressed() -> void:
	hide_warning_alert()

func _on_enable_hosting_hold_pressed() -> void:
	enable_hosting_hold()

func _on_clear_hosting_hold_pressed() -> void:
	clear_hosting_hold()

func _on_run_backup_pass_pressed() -> void:
	run_backup_pass()

func _on_work_late_shift_pressed() -> void:
	work_late_shift()

func _on_stabilize_line_pressed() -> void:
	stabilize_line()

func _on_continue_pressed() -> void:
	load_game()

func _on_reset_progress_pressed() -> void:
	reset_progress()

func _connect_time_controls() -> void:
	var start_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/Transport/Start")
	var stop_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/Transport/Stop")
	var faster_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/Transport/Faster")
	var reset_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/Transport/Reset")
	var next_month_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/Transport/NextMonth")
	var speed_1_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed1")
	var speed_2_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed2")
	var speed_3_button: Button = get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/TimeControls/Margin/Content/SpeedStates/Speed3")

	if start_button != null and not start_button.pressed.is_connected(_on_time_start_pressed):
		start_button.pressed.connect(_on_time_start_pressed)
	if stop_button != null and not stop_button.pressed.is_connected(_on_time_pause_pressed):
		stop_button.pressed.connect(_on_time_pause_pressed)
	if faster_button != null and not faster_button.pressed.is_connected(_on_time_faster_pressed):
		faster_button.pressed.connect(_on_time_faster_pressed)
	if reset_button != null and not reset_button.pressed.is_connected(_on_time_reset_pressed):
		reset_button.pressed.connect(_on_time_reset_pressed)
	if next_month_button != null and not next_month_button.pressed.is_connected(_on_time_next_month_pressed):
		next_month_button.pressed.connect(_on_time_next_month_pressed)
	if speed_1_button != null and not speed_1_button.pressed.is_connected(_on_time_speed_state_pressed.bind(1)):
		speed_1_button.pressed.connect(_on_time_speed_state_pressed.bind(1))
	if speed_2_button != null and not speed_2_button.pressed.is_connected(_on_time_speed_state_pressed.bind(2)):
		speed_2_button.pressed.connect(_on_time_speed_state_pressed.bind(2))
	if speed_3_button != null and not speed_3_button.pressed.is_connected(_on_time_speed_state_pressed.bind(3)):
		speed_3_button.pressed.connect(_on_time_speed_state_pressed.bind(3))
	if monthly_summary_close_button != null and not monthly_summary_close_button.pressed.is_connected(_on_monthly_summary_close_pressed):
		monthly_summary_close_button.pressed.connect(_on_monthly_summary_close_pressed)
	if warning_alert_close_button != null and not warning_alert_close_button.pressed.is_connected(_on_warning_alert_close_pressed):
		warning_alert_close_button.pressed.connect(_on_warning_alert_close_pressed)

func _connect_main_tabs() -> void:
	if operations_tab_button != null and not operations_tab_button.pressed.is_connected(_on_main_tab_pressed.bind("operations")):
		operations_tab_button.pressed.connect(_on_main_tab_pressed.bind("operations"))
	if contracts_tab_button != null and not contracts_tab_button.pressed.is_connected(_on_main_tab_pressed.bind("contracts")):
		contracts_tab_button.pressed.connect(_on_main_tab_pressed.bind("contracts"))
	if progress_tab_button != null and not progress_tab_button.pressed.is_connected(_on_main_tab_pressed.bind("progress")):
		progress_tab_button.pressed.connect(_on_main_tab_pressed.bind("progress"))

func _connect_log_panel() -> void:
	if log_panel_collapse_button != null and not log_panel_collapse_button.pressed.is_connected(_on_log_panel_toggle_pressed):
		log_panel_collapse_button.pressed.connect(_on_log_panel_toggle_pressed)
	if log_panel_collapsed_strip != null and not log_panel_collapsed_strip.pressed.is_connected(_on_log_panel_toggle_pressed):
		log_panel_collapsed_strip.pressed.connect(_on_log_panel_toggle_pressed)

func _connect_station_panel() -> void:
	if panel_close_button != null and not panel_close_button.pressed.is_connected(_on_station_panel_close_pressed):
		panel_close_button.pressed.connect(_on_station_panel_close_pressed)

func _connect_contracts_board_panel() -> void:
	if contracts_board_panel == null:
		return
	var contract_selected_callable := Callable(self, "_on_contracts_board_contract_selected")
	if contracts_board_panel.has_signal("contract_selected") and not contracts_board_panel.is_connected("contract_selected", contract_selected_callable):
		contracts_board_panel.connect("contract_selected", contract_selected_callable)

func _connect_contract_response_controls() -> void:
	if contracts_response_enable_button != null and not contracts_response_enable_button.pressed.is_connected(_on_enable_hosting_hold_pressed):
		contracts_response_enable_button.pressed.connect(_on_enable_hosting_hold_pressed)
	if contracts_response_clear_button != null and not contracts_response_clear_button.pressed.is_connected(_on_clear_hosting_hold_pressed):
		contracts_response_clear_button.pressed.connect(_on_clear_hosting_hold_pressed)
	if contracts_response_backup_button != null and not contracts_response_backup_button.pressed.is_connected(_on_run_backup_pass_pressed):
		contracts_response_backup_button.pressed.connect(_on_run_backup_pass_pressed)
	if contracts_response_late_shift_button != null and not contracts_response_late_shift_button.pressed.is_connected(_on_work_late_shift_pressed):
		contracts_response_late_shift_button.pressed.connect(_on_work_late_shift_pressed)
	if contracts_response_stabilize_button != null and not contracts_response_stabilize_button.pressed.is_connected(_on_stabilize_line_pressed):
		contracts_response_stabilize_button.pressed.connect(_on_stabilize_line_pressed)

func _connect_save_controls() -> void:
	if continue_button != null and not continue_button.pressed.is_connected(_on_continue_pressed):
		continue_button.pressed.connect(_on_continue_pressed)
	if reset_progress_button != null and not reset_progress_button.pressed.is_connected(_on_reset_progress_pressed):
		reset_progress_button.pressed.connect(_on_reset_progress_pressed)

func _select_main_tab(tab_id: String) -> void:
	_resolve_nodes()
	active_main_tab = tab_id
	if operations_tab != null:
		operations_tab.visible = tab_id == "operations"
	if contracts_tab != null:
		contracts_tab.visible = tab_id == "contracts"
	if progress_tab != null:
		progress_tab.visible = tab_id == "progress"
	if operations_tab_button != null:
		operations_tab_button.disabled = tab_id == "operations"
	if contracts_tab_button != null:
		contracts_tab_button.disabled = tab_id == "contracts"
	if progress_tab_button != null:
		progress_tab_button.disabled = tab_id == "progress"
	_refresh_main_tab_summaries()

func _apply_log_panel_state() -> void:
	_resolve_nodes()
	if log_panel == null:
		return
	if log_panel_content != null:
		log_panel_content.visible = not log_panel_collapsed
	if log_panel_collapsed_strip != null:
		log_panel_collapsed_strip.visible = log_panel_collapsed
	if log_panel_collapse_button != null:
		log_panel_collapse_button.text = "Expand" if log_panel_collapsed else "Collapse"
	log_panel.custom_minimum_size = Vector2(52, 0) if log_panel_collapsed else Vector2(220, 0)

func _refresh_economy_state() -> void:
	var recurring_income := 0
	for contract in active_contracts:
		recurring_income += int(contract.get("monthly_revenue_value", 0))

	var monthly_costs := get_monthly_cost_breakdown()
	var recurring_expenses := int(monthly_costs.get("power_cost", 0)) + int(monthly_costs.get("internet_cost", 0)) + int(monthly_costs.get("hardware_upkeep_cost", 0))

	economy_state["monthly_income"] = recurring_income
	economy_state["power_cost"] = int(monthly_costs.get("power_cost", 0))
	economy_state["internet_cost"] = int(monthly_costs.get("internet_cost", 0))
	economy_state["hardware_upkeep_cost"] = int(monthly_costs.get("hardware_upkeep_cost", 0))
	economy_state["monthly_expenses"] = recurring_expenses
	economy_state["next_cost"] = _build_next_cost_text()

func _run_monthly_rollover() -> void:
	var report := tick_month()
	_refresh_monthly_summary(report)
	_refresh_warning_alert(report)

func clear_selected_contract() -> void:
	selected_contract_id = ""

func _handle_station_action(station: StaticBody2D, action_id: String) -> Dictionary:
	if station.station_type == "contracts":
		if action_id == "next_contract":
			return _cycle_contract_selection(1)
		if action_id == "previous_contract":
			return _cycle_contract_selection(-1)
		if action_id == "accept_contract":
			var accept_result := accept_selected_contract()
			if bool(accept_result.get("ok", false)):
				return {"ok": true, "status": "%s accepted and moved to the hosting queue." % str(accept_result["contract"].get("name", "Contract"))}
			return {"ok": false, "status": str(accept_result.get("reason", "Unable to accept contract."))}
	elif station.station_type == "computer":
		if action_id == "next_upgrade":
			return _cycle_selected_expansion(1)
		if action_id == "previous_upgrade":
			return _cycle_selected_expansion(-1)
		if action_id == "purchase_selected_upgrade":
			var selected_expansion := _get_selected_expansion_target()
			if selected_expansion.is_empty():
				return {"ok": false, "status": "No more planned upgrades are available right now."}
			var purchase_selected_result := purchase_expansion(String(selected_expansion.get("id", "")))
			if bool(purchase_selected_result.get("ok", false)):
				return {"ok": true, "status": str(purchase_selected_result.get("status", "Upgrade purchased."))}
			return {"ok": false, "status": str(purchase_selected_result.get("reason", "Unable to purchase upgrade."))}
		if action_id == "activate_hosting":
			var activation_result := activate_selected_contract()
			if bool(activation_result.get("ok", false)):
				return {"ok": true, "status": "Hosting activated on the starter workstation."}
			return {"ok": false, "status": str(activation_result.get("reason", "Unable to activate hosting."))}
		if action_id == "clear_selection":
			clear_selected_contract()
			return {"ok": true, "status": "Selected contract cleared."}
	elif station.station_type == "expansion":
		if action_id == "review_expansion":
			return {"ok": true, "status": "The external server is ready as the next growth target once the starter machine starts to feel cramped."}

	return {"ok": false, "status": "Unknown panel action."}

func _cycle_contract_selection(direction: int) -> Dictionary:
	_initialize_gameplay_state()
	if available_contracts.is_empty():
		return {"ok": false, "status": "No starter contracts are available."}

	if selected_contract_index < 0 or selected_contract_index >= available_contracts.size():
		selected_contract_index = 0
	else:
		selected_contract_index = posmod(selected_contract_index + direction, available_contracts.size())

	var contract: Dictionary = available_contracts[selected_contract_index]
	board_selected_contract_id = String(contract.get("id", ""))
	return {
		"ok": true,
		"status": "Selected %s." % str(contract.get("name", "contract")),
	}

func _find_available_contract_by_id(contract_id: String) -> Dictionary:
	for contract in available_contracts:
		if String(contract.get("id", "")) == contract_id:
			return contract
	return {}

func _find_pending_contract_by_id(contract_id: String) -> Dictionary:
	for contract in pending_contracts:
		if String(contract.get("id", "")) == contract_id:
			return contract
	return {}

func _find_available_contract_index_by_id(contract_id: String) -> int:
	for index in range(available_contracts.size()):
		var contract: Dictionary = available_contracts[index]
		if String(contract.get("id", "")) == contract_id:
			return index
	return -1

func _find_pending_contract_index_by_id(contract_id: String) -> int:
	for index in range(pending_contracts.size()):
		var contract: Dictionary = pending_contracts[index]
		if String(contract.get("id", "")) == contract_id:
			return index
	return -1

func _get_contract_manager() -> Node:
	return get_node_or_null("ContractManager")

func _load_all_contracts() -> void:
	var contract_manager := _get_contract_manager()
	if contract_manager != null and contract_manager.has_method("get_contracts"):
		all_contracts = contract_manager.get_contracts()
	else:
		all_contracts = []

func _refresh_available_contracts() -> void:
	var claimed_ids := _get_claimed_contract_ids()
	var next_available: Array = []
	for contract in all_contracts:
		var contract_data := contract as Dictionary
		var contract_id := String(contract_data.get("id", ""))
		if claimed_ids.has(contract_id):
			continue
		if _is_contract_unlocked(contract_data):
			next_available.append(contract_data.duplicate(true))

	available_contracts = next_available
	_sync_selected_available_contract()

func _sync_selected_available_contract() -> void:
	if available_contracts.is_empty():
		board_selected_contract_id = ""
		selected_contract_index = -1
		return

	if board_selected_contract_id != "":
		var existing_index := _find_available_contract_index_by_id(board_selected_contract_id)
		if existing_index >= 0:
			selected_contract_index = existing_index
			return

	selected_contract_index = clampi(selected_contract_index, 0, available_contracts.size() - 1)
	if selected_contract_index < 0:
		selected_contract_index = 0
	var selected_contract: Dictionary = available_contracts[selected_contract_index]
	board_selected_contract_id = String(selected_contract.get("id", ""))

func _get_claimed_contract_ids() -> Dictionary:
	var claimed_ids := {}
	for group in [pending_contracts, active_contracts, completed_contracts, cancelled_contracts]:
		for contract in group:
			claimed_ids[String((contract as Dictionary).get("id", ""))] = true
	return claimed_ids

func _get_locked_contracts() -> Array:
	var claimed_ids := _get_claimed_contract_ids()
	var locked_contracts: Array = []
	for contract in all_contracts:
		var contract_data := contract as Dictionary
		var contract_id := String(contract_data.get("id", ""))
		if claimed_ids.has(contract_id) or _is_contract_unlocked(contract_data):
			continue
		var locked_contract := contract_data.duplicate(true)
		locked_contract["unlock_status"] = _build_contract_unlock_status(contract_data)
		locked_contracts.append(locked_contract)
	return locked_contracts

func _is_contract_unlocked(contract: Dictionary) -> bool:
	var requirements := contract.get("unlock_requirements", {}) as Dictionary
	if requirements.is_empty():
		return true

	var required_expansions := requirements.get("required_expansions", []) as Array
	for expansion_id in required_expansions:
		if not bool(purchased_expansions.get(String(expansion_id), false)):
			return false

	var min_completed_contracts := int(requirements.get("min_completed_contracts", 0))
	if completed_contracts.size() < min_completed_contracts:
		return false

	return true

func _build_contract_unlock_status(contract: Dictionary) -> String:
	var requirements := contract.get("unlock_requirements", {}) as Dictionary
	if requirements.is_empty():
		return "Availability: Open from day one."

	var requirement_descriptions := _describe_contract_requirements(requirements)
	var requirement_text := ", ".join(requirement_descriptions)
	if _is_contract_unlocked(contract):
		return "Availability: Open now after reaching %s." % requirement_text
	return "Availability: Locked until you reach %s." % requirement_text

func _describe_contract_requirements(requirements: Dictionary) -> PackedStringArray:
	var descriptions: PackedStringArray = []
	var required_expansions := requirements.get("required_expansions", []) as Array
	for expansion_id in required_expansions:
		var expansion := _find_expansion_target(String(expansion_id))
		descriptions.append("%s online" % str(expansion.get("label", "the required upgrade")))

	var min_completed_contracts := int(requirements.get("min_completed_contracts", 0))
	if min_completed_contracts > 0:
		var suffix := "" if min_completed_contracts == 1 else "s"
		descriptions.append("%s completed contract%s" % [str(min_completed_contracts), suffix])

	return descriptions

func _load_expansion_catalog() -> Array:
	if not FileAccess.file_exists(EXPANSION_CATALOG_PATH):
		return []

	var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(EXPANSION_CATALOG_PATH))
	if not parsed is Array:
		return []

	var targets: Array = []
	for entry in parsed:
		if not entry is Dictionary:
			continue

		var expansion := (entry as Dictionary).duplicate(true)
		expansion["id"] = str(expansion.get("id", "")).strip_edges()
		expansion["label"] = str(expansion.get("label", "Expansion")).strip_edges()
		expansion["summary"] = str(expansion.get("summary", "")).strip_edges()
		expansion["cost"] = int(expansion.get("cost", 0))
		expansion["monthly_expense"] = int(expansion.get("monthly_expense", 0))
		expansion["hardware_data_path"] = str(expansion.get("hardware_data_path", "")).strip_edges()
		expansion["scene_node_path"] = str(expansion.get("scene_node_path", "")).strip_edges()
		targets.append(expansion)

	return targets

func _find_expansion_target(expansion_id: String) -> Dictionary:
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		if String(expansion_data.get("id", "")) == expansion_id:
			return expansion_data.duplicate(true)
	return {}

func _get_available_expansion_targets() -> Array:
	var targets: Array = []
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if not bool(purchased_expansions.get(expansion_id, false)):
			targets.append(expansion_data.duplicate(true))
	return targets

func _has_available_expansion_targets() -> bool:
	return not _get_available_expansion_targets().is_empty()

func _get_selected_expansion_target() -> Dictionary:
	var available_targets := _get_available_expansion_targets()
	if available_targets.is_empty():
		return {}
	for expansion in available_targets:
		var expansion_data := expansion as Dictionary
		if String(expansion_data.get("id", "")) == selected_expansion_id:
			return expansion_data.duplicate(true)
	return (available_targets[0] as Dictionary).duplicate(true)

func _get_next_expansion_target() -> Dictionary:
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var expansion_id := String(expansion_data.get("id", ""))
		if not bool(purchased_expansions.get(expansion_id, false)):
			return expansion_data.duplicate(true)
	return {}

func _sync_selected_expansion() -> void:
	var selected_target := _get_selected_expansion_target()
	selected_expansion_id = String(selected_target.get("id", ""))

func _cycle_selected_expansion(direction: int) -> Dictionary:
	var available_targets := _get_available_expansion_targets()
	if available_targets.is_empty():
		selected_expansion_id = ""
		return {"ok": false, "status": "All current hardware upgrades are already purchased."}

	var current_index := 0
	for index in range(available_targets.size()):
		var expansion_data := available_targets[index] as Dictionary
		if String(expansion_data.get("id", "")) == selected_expansion_id:
			current_index = index
			break

	current_index = posmod(current_index + direction, available_targets.size())
	var selected_target := available_targets[current_index] as Dictionary
	selected_expansion_id = String(selected_target.get("id", ""))
	return {
		"ok": true,
		"status": "Selected %s." % str(selected_target.get("label", "upgrade")),
	}

func _get_expansion_hardware(expansion: Dictionary) -> Dictionary:
	var scene_node_path := String(expansion.get("scene_node_path", ""))
	if scene_node_path != "":
		var expansion_node := get_node_or_null(scene_node_path)
		if expansion_node != null and expansion_node.has_method("get_hardware_specs"):
			return expansion_node.get_hardware_specs()

	var hardware_data_path := String(expansion.get("hardware_data_path", ""))
	if hardware_data_path == "" or not FileAccess.file_exists(hardware_data_path):
		return {}

	var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(hardware_data_path))
	if not parsed is Dictionary:
		return {}
	return (parsed as Dictionary).duplicate(true)

func _build_selected_upgrade_hint(expansion: Dictionary) -> String:
	var label := str(expansion.get("label", "Upgrade"))
	var summary := str(expansion.get("summary", ""))
	var benefit_parts := PackedStringArray()
	var hardware := _get_expansion_hardware(expansion)
	var capacity_parts := PackedStringArray()
	if int(hardware.get("cpu", 0)) > 0:
		capacity_parts.append("+%s CPU" % str(hardware.get("cpu", 0)))
	if int(hardware.get("ram_gb", 0)) > 0:
		capacity_parts.append("+%s GB RAM" % str(hardware.get("ram_gb", 0)))
	if int(hardware.get("storage_gb", 0)) > 0:
		capacity_parts.append("+%s GB storage" % str(hardware.get("storage_gb", 0)))
	if int(hardware.get("bandwidth_mbps", 0)) > 0:
		capacity_parts.append("+%s Mbps bandwidth" % str(hardware.get("bandwidth_mbps", 0)))
	if not capacity_parts.is_empty():
		benefit_parts.append("Adds %s." % ", ".join(capacity_parts))

	var mitigation_effects := expansion.get("mitigation_effects", {}) as Dictionary
	if mitigation_effects.has("backup_check"):
		benefit_parts.append("Helps calm backup-check support work.")
	if mitigation_effects.has("after_hours") or mitigation_effects.has("forum_cleanup"):
		benefit_parts.append("Makes late-night and queue-cleanup support easier to handle.")
	if mitigation_effects.has("internet_slowdown"):
		benefit_parts.append("Steadies traffic-spike and shaky-line support issues.")

	var benefit_text := "Useful for modest garage-hosting growth."
	if not benefit_parts.is_empty():
		benefit_text = " ".join(benefit_parts)
	return "Selected upgrade: %s. %s %s" % [label, summary, benefit_text]

func _sync_expansion_nodes() -> void:
	for expansion in expansion_catalog:
		var expansion_data := expansion as Dictionary
		var scene_node_path := String(expansion_data.get("scene_node_path", ""))
		if scene_node_path == "":
			continue
		var expansion_node := get_node_or_null(scene_node_path)
		if expansion_node != null:
			expansion_node.visible = bool(purchased_expansions.get(String(expansion_data.get("id", "")), false))

func _build_next_cost_text() -> String:
	var next_expansion := _get_next_expansion_target()
	if next_expansion.is_empty():
		return "All planned upgrades purchased"
	return "%s purchase: $%s" % [
		str(next_expansion.get("label", "Expansion")),
		str(next_expansion.get("cost", 0)),
	]

func _calculate_used_capacity() -> Dictionary:
	var used := {
		"cpu": 0,
		"ram_gb": 0,
		"storage_gb": 0,
		"bandwidth_mbps": 0,
	}

	for contract in active_contracts:
		var resources: Dictionary = contract.get("resources", {})
		for key in used.keys():
			used[key] += int(resources.get(key, 0))

	return used

func _can_host_contract(contract: Dictionary) -> bool:
	var used := _calculate_used_capacity()
	var resources: Dictionary = contract.get("resources", {})

	for key in ["cpu", "ram_gb", "storage_gb", "bandwidth_mbps"]:
		var total_value: int = int(starter_hardware.get(key, 0))
		var used_value: int = int(used.get(key, 0))
		var required_value: int = int(resources.get(key, 0))
		if used_value + required_value > total_value:
			return false

	return true

func _is_service_overloaded() -> bool:
	var capacity := get_shared_capacity_snapshot()
	for key in ["cpu", "ram_gb", "storage_gb", "bandwidth_mbps"]:
		if int(capacity[key]["used"]) > int(capacity[key]["total"]):
			return true
	return false
