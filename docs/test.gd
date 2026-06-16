extends SceneTree

const MAIN_SCENE_PATH := "res://scenes/main.tscn"
const SAVE_PATH := "user://mvp_save.json"

func _clear_save_file() -> void:
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(SAVE_PATH))

func _run_gameplay_checks(main_scene: Node, failures: Array[String]) -> void:
	var warning_alert := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert")
	var available_contracts: Array = main_scene.get_available_contracts()
	if available_contracts.size() < 3:
		failures.append("Core loop test needs several starter contracts to exercise the hosting pipeline.")
		return

	var contract_manager := main_scene.get_node_or_null("ContractManager")
	if contract_manager == null:
		failures.append("Gameplay loop checks need the ContractManager node as the contract source of truth.")
	else:
		var manager_contracts: Array = contract_manager.get_contracts() if contract_manager.has_method("get_contracts") else []
		if manager_contracts.size() <= available_contracts.size():
			failures.append("Contract manager should now expose locked future leads beyond the day-one board.")

	var locked_contracts: Array = main_scene.get_locked_contracts() if main_scene.has_method("get_locked_contracts") else []
	if locked_contracts.size() < 2:
		failures.append("Tier unlocks should keep at least two follow-on leads locked at game start.")
	else:
		var forum_locked := false
		for contract in locked_contracts:
			var contract_data := contract as Dictionary
			if String(contract_data.get("id", "")) == "riverblock_community_forum":
				forum_locked = true
				if not str(contract_data.get("unlock_status", "")).contains("Refurb Tower Server"):
					failures.append("Locked contract copy should explain the first tower-server milestone clearly.")
				break
		if not forum_locked:
			failures.append("Community/forum work should stay locked before the first server comes online.")

	var seen_contract_ids := {}
	for contract in available_contracts:
		var contract_data := contract as Dictionary
		var contract_id := String(contract_data.get("id", ""))
		if contract_id == "":
			failures.append("Starter contracts should expose stable contract IDs before any acceptance flow begins.")
			break
		if seen_contract_ids.has(contract_id):
			failures.append("Starter contracts should not duplicate runtime IDs after the shared shaping pass.")
			break
		seen_contract_ids[contract_id] = true
		if not contract_data.has("tier"):
			failures.append("Starter contracts should carry a default tier field from the shared data model.")
			break
		if not contract_data.has("unlock_requirements"):
			failures.append("Starter contracts should carry unlock requirements from the shared data model.")
			break

	var initial_snapshot: Dictionary = main_scene.get_economy_snapshot()
	var initial_cash := int(initial_snapshot.get("cash", 0))

	var first_accept: Dictionary = main_scene.accept_selected_contract()
	if not bool(first_accept.get("ok", false)):
		failures.append("Starter contract should be acceptible from the board during the gameplay loop test.")
		return

	var first_contract: Dictionary = first_accept.get("contract", {})
	var first_contract_id := String(first_contract.get("id", ""))
	if main_scene.get_pending_contracts().is_empty():
		failures.append("Accepted contracts should move into the pending hosting queue.")
		return

	var first_activation: Dictionary = main_scene.activate_selected_contract()
	if not bool(first_activation.get("ok", false)):
		failures.append("Accepted contracts should activate onto the workstation during the gameplay loop test.")
		return

	var first_live_snapshot: Dictionary = main_scene.get_contract_snapshot(first_contract_id)
	if String(first_live_snapshot.get("status", "")) != "active":
		failures.append("Activated contracts should become active hosting work.")

	var active_income := int(main_scene.get_economy_snapshot().get("monthly_income", 0))
	if active_income <= 0:
		failures.append("Active hosting should contribute recurring monthly income to the economy snapshot.")

	for _index in range(available_contracts.size() - 1):
		var accept_result: Dictionary = main_scene.accept_selected_contract()
		if not bool(accept_result.get("ok", false)):
			failures.append("All starter contracts should remain acceptible during the overload setup.")
			return
		var activation_result: Dictionary = main_scene.activate_selected_contract()
		if not bool(activation_result.get("ok", false)):
			failures.append("Accepted contracts should remain activatable during the overload setup.")
			return

	if not main_scene.get_available_contracts().is_empty():
		failures.append("Starter contracts should stay off the board once they have all been accepted.")

	var immediate_warnings: Array = main_scene.get_warning_log()
	if immediate_warnings.is_empty():
		failures.append("Over-capacity hosting should raise warnings immediately instead of waiting for month rollover.")
	if warning_alert == null or not warning_alert.visible:
		failures.append("Immediate overload warnings should surface through the popup alert panel.")

	var first_month_report: Dictionary = main_scene.tick_month()
	if int(first_month_report.get("payout_total", 0)) <= 0:
		failures.append("Month rollover should collect contract revenue from active hosting.")
	if int(first_month_report.get("expense_total", 0)) <= 0:
		failures.append("Month rollover should apply recurring infrastructure expenses.")
	if int(first_month_report.get("warning_count", 0)) <= 0:
		failures.append("Overloaded hosting should still report warnings during month rollover.")

	var post_month_snapshot: Dictionary = main_scene.get_contract_snapshot(first_contract_id)
	if int(post_month_snapshot.get("satisfaction", 100)) >= 100:
		failures.append("Overloaded hosting should reduce contract satisfaction during the monthly business tick.")

	var cash_after_first_month := int(main_scene.get_economy_snapshot().get("cash", 0))
	if cash_after_first_month == initial_cash:
		failures.append("Month rollover should change the player cash total through payouts and expenses.")

	var second_month_report: Dictionary = main_scene.tick_month()
	if int(second_month_report.get("penalty_total", 0)) <= 0:
		failures.append("Repeated overloaded hosting should eventually apply financial penalties.")
	if (second_month_report.get("cancelled_contract_ids", []) as Array).is_empty():
		failures.append("Repeated overloaded hosting should eventually cancel failing contracts.")

func _run_data_setup_checks(main_scene: Node, failures: Array[String]) -> void:
	var computer_station := main_scene.get_node_or_null("Workstations/ComputerStation")
	var external_server := main_scene.get_node_or_null("Workstations/ExternalServerUnit")
	if computer_station == null or external_server == null:
		failures.append("Data setup checks need both the starter workstation and external server nodes.")
		return

	var computer_data_path := str(computer_station.get("hardware_data_path"))
	var external_data_path := str(external_server.get("hardware_data_path"))
	var upgrade_catalog_path := "res://data/hardware/upgrade_ladder.json"
	if computer_data_path == "":
		failures.append("Computer workstation should load its hardware from a JSON data path.")
	if external_data_path == "":
		failures.append("External server should load its hardware from a JSON data path.")
	if not FileAccess.file_exists(upgrade_catalog_path):
		failures.append("Upgrade ladder step should add a reusable hardware upgrade catalog JSON file.")

	if not FileAccess.file_exists(computer_data_path):
		failures.append("Computer workstation hardware JSON should exist on disk.")
	if not FileAccess.file_exists(external_data_path):
		failures.append("External server hardware JSON should exist on disk.")

	var expansion_targets: Array = main_scene.get_expansion_targets() if main_scene.has_method("get_expansion_targets") else []
	if expansion_targets.size() < 3:
		failures.append("Upgrade ladder should expose multiple purchasable expansion targets beyond the starter machine.")
	else:
		for required_id in ["external_server_unit", "managed_rack_server", "backup_nas_cluster", "support_runbook_binder", "business_line_modem"]:
			var found := false
			for target in expansion_targets:
				if String((target as Dictionary).get("id", "")) == required_id:
					found = true
					break
			if not found:
				failures.append("Upgrade ladder should include the `%s` target." % required_id)
		if not bool((expansion_targets[0] as Dictionary).get("selected", false)):
			failures.append("Upgrade ladder should mark the first available early upgrade as selected by default.")
		var first_target := expansion_targets[0] as Dictionary
		var second_target := expansion_targets[1] as Dictionary
		var third_target := expansion_targets[2] as Dictionary
		if str(first_target.get("label", "")) != "Refurb Tower Server":
			failures.append("Phase 2 coverage should lock the humble first upgrade label to `Refurb Tower Server`.")
		if str(second_target.get("label", "")) != "Second Tower Server":
			failures.append("Phase 2 coverage should lock the second early upgrade label to `Second Tower Server`.")
		if str(third_target.get("label", "")) != "Backup NAS Box":
			failures.append("Phase 2 coverage should lock the third early upgrade label to `Backup NAS Box`.")
		if not str(third_target.get("summary", "")).contains("garage-office"):
			failures.append("Backup NAS summary should stay grounded in modest garage-office support language.")

	var starter_capacity: Dictionary = main_scene.get_capacity_snapshot()
	if int(starter_capacity.get("cpu", {}).get("total", 0)) != 2:
		failures.append("Starter workstation capacity should resolve from data-driven hardware values.")
	var initial_cash := int(main_scene.get_economy_snapshot().get("cash", 0))
	var initial_monthly_expenses := int(main_scene.get_economy_snapshot().get("monthly_expenses", 0))
	var initial_next_cost := String(main_scene.get_economy_snapshot().get("next_cost", ""))
	if not initial_next_cost.contains("Refurb Tower Server"):
		failures.append("Progression next-cost copy should point at the first upgrade before any expansion is purchased.")

	var purchase_result: Dictionary = main_scene.purchase_expansion("external_server_unit")
	if not bool(purchase_result.get("ok", false)):
		failures.append("External server should still be purchasable during the data setup test.")
		return

	var expanded_capacity: Dictionary = main_scene.get_capacity_snapshot()
	if int(expanded_capacity.get("cpu", {}).get("total", 0)) != 4:
		failures.append("Expanded shared capacity should reflect the data-driven external server hardware values.")
	var post_first_purchase_snapshot: Dictionary = main_scene.get_economy_snapshot()
	if int(post_first_purchase_snapshot.get("cash", 0)) != initial_cash - 420:
		failures.append("Buying the first tower server should deduct its full purchase cost from startup cash.")
	if int(post_first_purchase_snapshot.get("monthly_expenses", 0)) <= initial_monthly_expenses:
		failures.append("Buying the first tower server should increase recurring monthly expenses.")
	var unlocked_contracts_after_first_purchase: Array = main_scene.get_available_contracts()
	var forum_unlocked := false
	for contract in unlocked_contracts_after_first_purchase:
		if String((contract as Dictionary).get("id", "")) == "riverblock_community_forum":
			forum_unlocked = true
			break
	if not forum_unlocked:
		failures.append("Community/forum work should unlock after the first tower server comes online.")
	var locked_contracts_after_first_purchase: Array = main_scene.get_locked_contracts() if main_scene.has_method("get_locked_contracts") else []
	var accounting_still_locked := false
	for contract in locked_contracts_after_first_purchase:
		if String((contract as Dictionary).get("id", "")) == "elm_street_accounting_office":
			accounting_still_locked = true
			break
	if not accounting_still_locked:
		failures.append("Higher-support small-office work should stay locked until some starter jobs are completed.")
	var next_cost_after_first_purchase := String(main_scene.get_economy_snapshot().get("next_cost", ""))
	if not next_cost_after_first_purchase.contains("Second Tower Server"):
		failures.append("Progression next-cost copy should advance to the second tower server after the first purchase.")
	var post_first_purchase_targets: Array = main_scene.get_expansion_targets()
	var selected_after_first_purchase := false
	for target in post_first_purchase_targets:
		var target_data := target as Dictionary
		if String(target_data.get("id", "")) == "managed_rack_server" and bool(target_data.get("selected", false)):
			selected_after_first_purchase = true
			break
	if not selected_after_first_purchase:
		failures.append("Upgrade ladder should advance the selected available upgrade after a purchase.")

	var rack_purchase_result: Dictionary = main_scene.purchase_expansion("managed_rack_server")
	if not bool(rack_purchase_result.get("ok", false)):
		failures.append("Second tower server should be purchasable through the shared expansion data model.")
		return

	var rack_capacity: Dictionary = main_scene.get_capacity_snapshot()
	if int(rack_capacity.get("cpu", {}).get("total", 0)) != 6:
		failures.append("Shared capacity should include the second tower server hardware values after purchase.")
	var post_second_purchase_snapshot: Dictionary = main_scene.get_economy_snapshot()
	if int(post_second_purchase_snapshot.get("monthly_expenses", 0)) <= int(post_first_purchase_snapshot.get("monthly_expenses", 0)):
		failures.append("Adding the second tower server should continue increasing recurring monthly expenses.")

	var insufficient_purchase_result: Dictionary = main_scene.purchase_expansion("backup_nas_cluster")
	if bool(insufficient_purchase_result.get("ok", false)):
		failures.append("Upgrade purchases should fail once startup cash is below the next hardware cost.")

	var funded_scene := (load(MAIN_SCENE_PATH) as PackedScene).instantiate()
	root.add_child(funded_scene)
	funded_scene.economy_state["cash"] = 2500
	if not bool(funded_scene.purchase_expansion("external_server_unit").get("ok", false)):
		failures.append("Funded upgrade checks should still be able to buy the first tower server.")
	elif not bool(funded_scene.purchase_expansion("managed_rack_server").get("ok", false)):
		failures.append("Funded upgrade checks should still be able to buy the second tower server.")
	else:
		var funded_backup_purchase_result: Dictionary = funded_scene.purchase_expansion("backup_nas_cluster")
		if not bool(funded_backup_purchase_result.get("ok", false)):
			failures.append("Backup NAS cluster should remain purchasable when the workshop actually has enough cash on hand.")
		else:
			var final_capacity: Dictionary = funded_scene.get_capacity_snapshot()
			if int(final_capacity.get("storage_gb", {}).get("total", 0)) != 3500:
				failures.append("Shared capacity should continue growing through modest storage-oriented early upgrades.")
			var funded_snapshot: Dictionary = funded_scene.get_economy_snapshot()
			if int(funded_snapshot.get("monthly_expenses", 0)) <= int(post_second_purchase_snapshot.get("monthly_expenses", 0)):
				failures.append("Adding the backup NAS should keep recurring monthly expenses moving upward with the hardware footprint.")
	funded_scene.queue_free()

func _run_player_state_machine_checks(main_scene: Node, failures: Array[String]) -> void:
	var player_node := main_scene.get_node_or_null("Player")
	var contracts_board := main_scene.get_node_or_null("Workstations/ContractsBoard")
	var contracts_board_panel := main_scene.get_node_or_null("UI/ContractsBoardPanel")
	var player_sprite := player_node.get_node_or_null("PlayerSprite") if player_node != null else null
	if player_node == null or contracts_board == null or contracts_board_panel == null:
		failures.append("Player state machine checks need the player, contracts board, and contracts board panel in the main scene.")
		return
	if player_sprite == null:
		failures.append("Player state machine checks need the animated player sprite node in the main scene.")
		return
	if not player_node.has_method("get_state_name") or not player_node.has_method("begin_interaction") or not player_node.has_method("end_interaction"):
		failures.append("Player should expose simple state-machine helpers for idle, moving, and interacting flow.")
		return
	if not player_sprite.sprite_frames.has_animation("interact"):
		failures.append("Player sprite should expose an authored interact animation.")
	else:
		var interact_texture: Texture2D = player_sprite.sprite_frames.get_frame_texture("interact", 0)
		if not interact_texture is AtlasTexture or interact_texture.atlas.resource_path != "res://art/player_sprite_v4.png":
			failures.append("Player scene should wire the authored v4 sprite sheet into the interact animation.")
	if player_sprite.position.y != -6.0:
		failures.append("Player sprite should stay slightly lifted so the feet ground cleanly against the garage floor.")
	for animation_name in ["walk_up", "walk_down", "walk_left", "walk_right"]:
		for frame_index in range(player_sprite.sprite_frames.get_frame_count(animation_name)):
			if abs(player_sprite.sprite_frames.get_frame_duration(animation_name, frame_index) - 0.11) > 0.001:
				failures.append("Directional walk animations should use a consistent 0.11s cadence for readability.")
				break

	if player_node.get_state_name() != "idle":
		failures.append("Player should start in the idle state.")

	player_node.begin_interaction()
	if player_node.get_state_name() != "interacting":
		failures.append("Player should enter the interacting state when an interaction begins.")
	if player_sprite.animation != "interact":
		failures.append("Player should visibly switch to the interact animation during interaction state.")
	player_node.apply_movement(Vector2.RIGHT)
	if player_node.velocity.length() > 0.0:
		failures.append("Player movement should be blocked while interacting.")

	main_scene.show_station_panel(contracts_board)
	if player_node.get_state_name() != "interacting":
		failures.append("Opening the station panel should keep the player in the interacting state.")
	main_scene.hide_station_panel()
	if player_node.get_state_name() != "idle":
		failures.append("Closing the station panel should return the player to the idle state.")

	if contracts_board_panel.has_method("show_board"):
		contracts_board_panel.call("show_board")
		if player_node.get_state_name() != "interacting":
			failures.append("Opening the contracts board panel should move the player into the interacting state.")
	if contracts_board_panel.has_method("hide_board"):
		contracts_board_panel.call("hide_board")
		if player_node.get_state_name() != "idle":
			failures.append("Closing the contracts board panel should return the player to the idle state.")
	if player_sprite.animation != "idle":
		failures.append("Player should return to the idle animation after interaction state ends.")

	var computer_station := main_scene.get_node_or_null("Workstations/ComputerStation")
	if computer_station == null:
		failures.append("Player state checks need the computer workstation to verify purchase-action labeling.")
	else:
		var workstation_actions: Array = main_scene.get_station_actions(computer_station) if main_scene.has_method("get_station_actions") else []
		var expected_purchase_label := ""
		for action in workstation_actions:
			var action_data := action as Dictionary
			if String(action_data.get("id", "")) == "purchase_selected_upgrade":
				expected_purchase_label = String(action_data.get("label", ""))
				break
		if expected_purchase_label == "" or not expected_purchase_label.contains("Buy ") or not expected_purchase_label.contains("($") or not expected_purchase_label.contains("/mo"):
			failures.append("Computer workstation should generate a dynamic purchase action label with the selected upgrade name, price, and monthly upkeep.")
		main_scene.show_station_panel(computer_station)
		var action_row := main_scene.get_node_or_null("UI/StationPanel/Margin/Content/Actions")
		var hint_label := main_scene.get_node_or_null("UI/StationPanel/Margin/Content/Hint")
		if not action_row is Container:
			failures.append("Station panel should expose workstation action buttons in a real layout container.")
		else:
			var found_dynamic_purchase_label := false
			for child in action_row.get_children():
				if not child is Button:
					continue
				var action_button := child as Button
				if action_button.text == expected_purchase_label:
					found_dynamic_purchase_label = true
					break
			if not found_dynamic_purchase_label:
				failures.append("Computer workstation should render the same dynamic purchase label in the station panel that the action list generates.")
		if not hint_label is Label:
			failures.append("Computer workstation should expose hint copy under the action buttons.")
		else:
			var expected_upgrade_name := expected_purchase_label.trim_prefix("Buy ").split(" ($")[0]
			if not hint_label.text.contains("Selected upgrade:") or not hint_label.text.contains(expected_upgrade_name):
				failures.append("Computer workstation should explain the currently selected upgrade under the action buttons.")
		main_scene.hide_station_panel()

		var workstation_refresh_scene := (load(MAIN_SCENE_PATH) as PackedScene).instantiate()
		root.add_child(workstation_refresh_scene)
		var refresh_computer_station := workstation_refresh_scene.get_node_or_null("Workstations/ComputerStation")
		if refresh_computer_station == null:
			failures.append("Workstation refresh checks need the computer workstation node in a fresh scene.")
		else:
			workstation_refresh_scene.show_station_panel(refresh_computer_station)
			var initial_refresh_actions: Array = workstation_refresh_scene.get_station_actions(refresh_computer_station) if workstation_refresh_scene.has_method("get_station_actions") else []
			var initial_buy_label := ""
			for action in initial_refresh_actions:
				var action_data := action as Dictionary
				if String(action_data.get("id", "")) == "purchase_selected_upgrade":
					initial_buy_label = String(action_data.get("label", ""))
					break
			var cycle_result: Dictionary = workstation_refresh_scene.press_station_action("next_upgrade")
			if not bool(cycle_result.get("ok", false)):
				failures.append("Workstation refresh checks should be able to cycle to the next available upgrade.")
			else:
				var cycled_actions: Array = workstation_refresh_scene.get_station_actions(refresh_computer_station) if workstation_refresh_scene.has_method("get_station_actions") else []
				var cycled_buy_label := ""
				for action in cycled_actions:
					var action_data := action as Dictionary
					if String(action_data.get("id", "")) == "purchase_selected_upgrade":
						cycled_buy_label = String(action_data.get("label", ""))
						break
				if cycled_buy_label == "" or cycled_buy_label == initial_buy_label:
					failures.append("Next Upgrade should change the visible buy label to the newly selected hardware option.")
				var refresh_action_row := workstation_refresh_scene.get_node_or_null("UI/StationPanel/Margin/Content/Actions")
				var refresh_hint_label := workstation_refresh_scene.get_node_or_null("UI/StationPanel/Margin/Content/Hint")
				var rendered_cycled_buy_label := ""
				var rendered_cycled_action_count := 0
				if refresh_action_row is Container:
					rendered_cycled_action_count = refresh_action_row.get_child_count()
					for child in refresh_action_row.get_children():
						if not child is Button:
							continue
						var action_button := child as Button
						if action_button.text.begins_with("Buy "):
							rendered_cycled_buy_label = action_button.text
							break
				if rendered_cycled_buy_label != cycled_buy_label:
					failures.append("Cycling upgrades should refresh the rendered buy button label inside the open workstation panel.")
				if refresh_action_row is Container and rendered_cycled_action_count != cycled_actions.size():
					failures.append("Cycling upgrades should not duplicate workstation action buttons inside the open panel.")
				if refresh_hint_label is Label and not refresh_hint_label.text.contains(cycled_buy_label.trim_prefix("Buy ").split(" ($")[0]):
					failures.append("Cycling upgrades should refresh the under-button upgrade description to match the newly selected item.")
				var purchased_label := cycled_buy_label
				var startup_cash_value := workstation_refresh_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/StartupFinances/Margin/Content/Metrics/CashValue")
				var visible_cash_before_purchase: String = startup_cash_value.text if startup_cash_value is Label else ""
				var purchase_cycle_result: Dictionary = workstation_refresh_scene.press_station_action("purchase_selected_upgrade")
				if not bool(purchase_cycle_result.get("ok", false)):
					failures.append("Workstation refresh checks should be able to buy the currently selected cycled upgrade in a fresh scene.")
				else:
					var post_purchase_actions: Array = workstation_refresh_scene.get_station_actions(refresh_computer_station) if workstation_refresh_scene.has_method("get_station_actions") else []
					var post_purchase_buy_label := ""
					for action in post_purchase_actions:
						var action_data := action as Dictionary
						if String(action_data.get("id", "")) == "purchase_selected_upgrade":
							post_purchase_buy_label = String(action_data.get("label", ""))
							break
					if post_purchase_buy_label == "" or post_purchase_buy_label == purchased_label:
						failures.append("Buying the selected upgrade should remove that option and advance the buy label to the next available item.")
					var rendered_post_purchase_buy_label := ""
					var rendered_post_purchase_action_count := 0
					if refresh_action_row is Container:
						rendered_post_purchase_action_count = refresh_action_row.get_child_count()
						for child in refresh_action_row.get_children():
							if not child is Button:
								continue
							var action_button := child as Button
							if action_button.text.begins_with("Buy "):
								rendered_post_purchase_buy_label = action_button.text
								break
					if rendered_post_purchase_buy_label != post_purchase_buy_label:
						failures.append("Buying an upgrade should refresh the rendered buy button to the next available option inside the open workstation panel.")
					if refresh_action_row is Container and rendered_post_purchase_action_count != post_purchase_actions.size():
						failures.append("Buying an upgrade should not leave duplicated workstation action buttons in the open panel.")
					if startup_cash_value is Label and startup_cash_value.text == visible_cash_before_purchase:
						failures.append("Buying an upgrade from the workstation should immediately refresh the visible startup-finances cash value.")
		workstation_refresh_scene.queue_free()

func _run_save_slice_checks(failures: Array[String]) -> void:
	_clear_save_file()
	var packed_scene := load(MAIN_SCENE_PATH) as PackedScene
	if packed_scene == null:
		failures.append("Save slice checks need the main scene to load.")
		return

	var fresh_scene := packed_scene.instantiate()
	root.add_child(fresh_scene)

	var continue_button := fresh_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Continue")
	var reset_button := fresh_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Reset")
	if continue_button == null or reset_button == null:
		failures.append("Operations tab should expose docked Continue and Reset save controls.")
		fresh_scene.queue_free()
		return

	if not continue_button is Button or continue_button.disabled == false:
		failures.append("Continue should start disabled when no local save exists.")

	var save_result: Dictionary = fresh_scene.accept_selected_contract()
	if not bool(save_result.get("ok", false)):
		failures.append("Save slice test should still be able to accept a contract before saving.")
		fresh_scene.queue_free()
		return
	fresh_scene.activate_selected_contract()
	fresh_scene.purchase_expansion("external_server_unit")
	fresh_scene.select_board_contract("corner_dental_clinic")
	fresh_scene.accept_selected_contract()
	fresh_scene.activate_selected_contract()
	var expected_expansion_targets: Array = fresh_scene.get_expansion_targets() if fresh_scene.has_method("get_expansion_targets") else []
	var expected_locked_contracts: Array = fresh_scene.get_locked_contracts() if fresh_scene.has_method("get_locked_contracts") else []
	var expected_incidents: Array = fresh_scene.get_active_incidents() if fresh_scene.has_method("get_active_incidents") else []
	fresh_scene.advance_time(24 * 60)
	var expected_snapshot: Dictionary = fresh_scene.get_economy_snapshot()
	var expected_time: Dictionary = fresh_scene.get_time_snapshot()
	var expected_capacity: Dictionary = fresh_scene.get_capacity_snapshot()
	if not fresh_scene.has_method("save_game") or not bool(fresh_scene.save_game()):
		failures.append("Main scene should persist the current MVP state to a local save file.")
	fresh_scene.queue_free()

	if not FileAccess.file_exists(SAVE_PATH):
		failures.append("Saving should create the expected local JSON save file.")
		return

	var loaded_scene := packed_scene.instantiate()
	root.add_child(loaded_scene)
	var loaded_continue := loaded_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Continue")
	var loaded_reset := loaded_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/SaveControls/Margin/Content/Buttons/Reset")
	if loaded_scene.has_method("_update_save_controls"):
		loaded_scene._update_save_controls()
	if not loaded_continue is Button or not loaded_scene.has_method("has_save_data") or not bool(loaded_scene.has_save_data()):
		failures.append("Continue should become available once a save file exists.")
	elif not loaded_scene.has_method("load_game") or not bool(loaded_scene.load_game()):
		failures.append("Continue control should load the local save state.")

	var loaded_snapshot: Dictionary = loaded_scene.get_economy_snapshot()
	var loaded_time: Dictionary = loaded_scene.get_time_snapshot()
	var loaded_capacity: Dictionary = loaded_scene.get_capacity_snapshot()
	var loaded_expansion_targets: Array = loaded_scene.get_expansion_targets() if loaded_scene.has_method("get_expansion_targets") else []
	var loaded_locked_contracts: Array = loaded_scene.get_locked_contracts() if loaded_scene.has_method("get_locked_contracts") else []
	var loaded_incidents: Array = loaded_scene.get_active_incidents() if loaded_scene.has_method("get_active_incidents") else []
	if int(loaded_snapshot.get("cash", 0)) != int(expected_snapshot.get("cash", 0)):
		failures.append("Continue should restore saved economy progress from disk.")
	if int(loaded_time.get("day", 0)) != int(expected_time.get("day", 0)):
		failures.append("Continue should restore saved calendar progress from disk.")
	if int(loaded_capacity.get("cpu", {}).get("total", 0)) != int(expected_capacity.get("cpu", {}).get("total", 0)):
		failures.append("Continue should restore saved hardware expansion state from disk.")
	if loaded_expansion_targets.size() != expected_expansion_targets.size():
		failures.append("Continue should restore the live expansion ladder state from disk.")
	else:
		var loaded_selected_upgrade := ""
		for target in loaded_expansion_targets:
			var target_data := target as Dictionary
			if bool(target_data.get("selected", false)):
				loaded_selected_upgrade = String(target_data.get("id", ""))
				break
		if loaded_selected_upgrade != "managed_rack_server":
			failures.append("Continue should restore which available upgrade is currently selected after the first purchase.")
	if loaded_locked_contracts.size() != expected_locked_contracts.size():
		failures.append("Continue should restore the current contract lock state from disk.")
	if loaded_incidents.size() != expected_incidents.size():
		failures.append("Continue should restore active support incidents from disk.")
	elif not loaded_incidents.is_empty():
		var loaded_incident := loaded_incidents[0] as Dictionary
		if String(loaded_incident.get("contract_id", "")) != "corner_dental_clinic":
			failures.append("Continue should preserve which active contract owns a saved support incident.")

	if loaded_reset is Button:
		if loaded_scene.has_method("reset_progress"):
			loaded_scene.reset_progress()
		else:
			failures.append("Reset control should clear local progress through the main scene reset flow.")
	else:
		failures.append("Reset control should remain available after a save exists.")

	if FileAccess.file_exists(SAVE_PATH):
		failures.append("Reset should clear the local save file.")

	var reset_snapshot: Dictionary = loaded_scene.get_economy_snapshot()
	var reset_capacity: Dictionary = loaded_scene.get_capacity_snapshot()
	if int(reset_snapshot.get("cash", 0)) != 1200:
		failures.append("Reset should restore default startup cash.")
	if int(reset_capacity.get("cpu", {}).get("total", 0)) != 2:
		failures.append("Reset should restore default starter hardware capacity.")
	loaded_scene.queue_free()
	_clear_save_file()

func _run_issue_response_checks(failures: Array[String]) -> void:
	var packed_scene := load(MAIN_SCENE_PATH) as PackedScene
	if packed_scene == null:
		failures.append("Issue response checks need the main scene to load.")
		return

	var hold_scene := packed_scene.instantiate()
	root.add_child(hold_scene)
	var enable_hold_button := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/EnableHold")
	var clear_hold_button := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/ClearHold")
	var backup_pass_button := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/BackupPass")
	var late_shift_button := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/LateShift")
	var stabilize_line_button := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/StabilizeLine")
	var response_body := hold_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Body")
	if enable_hold_button == null or clear_hold_button == null or backup_pass_button == null or late_shift_button == null or stabilize_line_button == null or response_body == null:
		failures.append("Contracts tab should expose issue-response controls for hold plus the new manual mitigation actions.")
		hold_scene.queue_free()
		return

	var accept_result: Dictionary = hold_scene.accept_selected_contract()
	if not bool(accept_result.get("ok", false)):
		failures.append("Issue response test should be able to accept a contract before enabling the hosting hold.")
		hold_scene.queue_free()
		return
	if not hold_scene.has_method("enable_hosting_hold"):
		failures.append("Main scene should expose a hosting-hold response action.")
		hold_scene.queue_free()
		return
	var hold_result: Dictionary = hold_scene.enable_hosting_hold()
	if not bool(hold_result.get("ok", false)):
		failures.append("Hosting hold should enable from the contracts tab response controls.")
	elif not response_body.text.contains("Enabled"):
		failures.append("Contracts tab response section should show when the hosting hold is enabled.")
	elif not response_body.text.contains("Support incidents:"):
		failures.append("Contracts tab response section should report support incident status alongside the hosting hold.")
	elif not response_body.text.contains("Level-two readiness:"):
		failures.append("Contracts tab response section should surface level-two mitigation readiness before incidents land.")
	elif not response_body.text.contains("Backup checks:"):
		failures.append("Contracts tab response section should call out backup-check readiness.")
	elif not response_body.text.contains("After-hours requests:"):
		failures.append("Contracts tab response section should call out after-hours readiness.")
	elif not response_body.text.contains("Traffic-spike issues:"):
		failures.append("Contracts tab response section should call out traffic-spike readiness.")

	var blocked_activation: Dictionary = hold_scene.activate_selected_contract()
	if bool(blocked_activation.get("ok", false)):
		failures.append("Hosting hold should block new hosting activation while it is enabled.")
	hold_scene.queue_free()

	var throttle_scene := packed_scene.instantiate()
	root.add_child(throttle_scene)
	var available_contracts: Array = throttle_scene.get_available_contracts()
	var first_accept: Dictionary = throttle_scene.accept_selected_contract()
	if not bool(first_accept.get("ok", false)):
		failures.append("Throttle response test should be able to accept the first contract.")
		throttle_scene.queue_free()
		return
	var first_contract: Dictionary = first_accept.get("contract", {})
	var first_contract_id := String(first_contract.get("id", ""))
	if not bool(throttle_scene.activate_selected_contract().get("ok", false)):
		failures.append("Throttle response test should be able to activate the first contract.")
		throttle_scene.queue_free()
		return
	for _index in range(available_contracts.size() - 1):
		if not bool(throttle_scene.accept_selected_contract().get("ok", false)):
			failures.append("Throttle response test should be able to accept the remaining starter contracts.")
			throttle_scene.queue_free()
			return
		if not bool(throttle_scene.activate_selected_contract().get("ok", false)):
			failures.append("Throttle response test should be able to activate the remaining starter contracts.")
			throttle_scene.queue_free()
			return

	var pre_throttle_snapshot: Dictionary = throttle_scene.get_contract_snapshot(first_contract_id)
	var pre_throttle_satisfaction := int(pre_throttle_snapshot.get("satisfaction", 100))
	var throttle_result: Dictionary = throttle_scene.enable_hosting_hold()
	if not bool(throttle_result.get("ok", false)):
		failures.append("Hosting hold should also be available once overload warnings are active.")
		throttle_scene.queue_free()
		return

	var throttled_report: Dictionary = throttle_scene.tick_month()
	var post_throttle_snapshot: Dictionary = throttle_scene.get_contract_snapshot(first_contract_id)
	if int(post_throttle_snapshot.get("satisfaction", 100)) < pre_throttle_satisfaction:
		failures.append("Throttle mode should protect contract satisfaction while overload is being managed.")
	if int(throttled_report.get("penalty_total", 0)) > 0:
		failures.append("Throttle mode should prevent financial penalties while the hosting hold is active.")
	if not response_body is Label:
		failures.append("Contracts tab response body should stay readable during issue response checks.")
	throttle_scene.queue_free()

	var incident_scene := packed_scene.instantiate()
	root.add_child(incident_scene)
	if incident_scene.has_method("select_board_contract"):
		var select_incident_contract: Dictionary = incident_scene.select_board_contract("corner_dental_clinic")
		if not bool(select_incident_contract.get("ok", false)):
			failures.append("Issue response checks should be able to target a support-heavy starter contract.")
	var incident_accept: Dictionary = incident_scene.accept_selected_contract()
	if not bool(incident_accept.get("ok", false)):
		failures.append("Issue response checks should be able to accept a support-heavy starter contract.")
	elif not bool(incident_scene.activate_selected_contract().get("ok", false)):
		failures.append("Issue response checks should be able to activate a support-heavy starter contract.")
	else:
		var live_incidents: Array = incident_scene.get_active_incidents() if incident_scene.has_method("get_active_incidents") else []
		if live_incidents.is_empty():
			failures.append("Support-heavy contracts should generate a live incident state once hosting begins.")
		else:
			var first_incident := live_incidents[0] as Dictionary
			if not str(first_incident.get("headline", "")).contains("backup") and not str(first_incident.get("headline", "")).contains("Backup"):
				failures.append("Early support incidents should read like grounded small-hosting support work.")
		var neglected_report: Dictionary = incident_scene.tick_month()
		var incident_snapshot: Dictionary = incident_scene.get_contract_snapshot("corner_dental_clinic")
		if int(neglected_report.get("incident_count", 0)) <= 0:
			failures.append("Monthly reports should count support incidents when they are active.")
		if int(neglected_report.get("neglected_incident_count", 0)) <= 0:
			failures.append("Monthly reports should mark unresolved support incidents as neglected.")
		if int(neglected_report.get("penalty_total", 0)) <= 0:
			failures.append("Neglected support incidents should cause a modest financial penalty.")
		if int(incident_snapshot.get("satisfaction", 100)) >= 100:
			failures.append("Neglected support incidents should reduce contract satisfaction.")
	incident_scene.queue_free()

	var handled_incident_scene := packed_scene.instantiate()
	root.add_child(handled_incident_scene)
	if handled_incident_scene.has_method("select_board_contract"):
		handled_incident_scene.select_board_contract("corner_dental_clinic")
	var handled_accept: Dictionary = handled_incident_scene.accept_selected_contract()
	if not bool(handled_accept.get("ok", false)):
		failures.append("Handled-incident checks should be able to accept the support-heavy contract.")
	elif not bool(handled_incident_scene.activate_selected_contract().get("ok", false)):
		failures.append("Handled-incident checks should be able to activate the support-heavy contract.")
	elif not bool(handled_incident_scene.enable_hosting_hold().get("ok", false)):
		failures.append("Hosting hold should be usable to handle a live support incident.")
	else:
		var handled_report: Dictionary = handled_incident_scene.tick_month()
		var handled_snapshot: Dictionary = handled_incident_scene.get_contract_snapshot("corner_dental_clinic")
		if int(handled_report.get("resolved_incident_count", 0)) <= 0:
			failures.append("Monthly reports should show when support incidents were handled under a hosting hold.")
		if int(handled_report.get("penalty_total", 0)) > 0:
			failures.append("Handled support incidents should not add penalties that month.")
		if int(handled_snapshot.get("satisfaction", 100)) < 100:
			failures.append("Handled support incidents should protect contract satisfaction for the cycle.")
		if not response_body is Label:
			failures.append("Contracts response body should stay readable when support incidents are handled.")
	handled_incident_scene.queue_free()

	var prepared_scene := packed_scene.instantiate()
	root.add_child(prepared_scene)
	prepared_scene.purchase_expansion("backup_nas_cluster")
	prepared_scene.purchase_expansion("support_runbook_binder")
	if prepared_scene.has_method("select_board_contract"):
		prepared_scene.select_board_contract("corner_dental_clinic")
	var prepared_accept: Dictionary = prepared_scene.accept_selected_contract()
	if not bool(prepared_accept.get("ok", false)):
		failures.append("Preparedness checks should be able to accept the support-heavy contract.")
	elif not bool(prepared_scene.activate_selected_contract().get("ok", false)):
		failures.append("Preparedness checks should be able to activate the support-heavy contract.")
	else:
		var prepared_incidents: Array = prepared_scene.get_active_incidents() if prepared_scene.has_method("get_active_incidents") else []
		if prepared_incidents.is_empty():
			failures.append("Preparedness checks should still generate a live support incident to soften.")
		else:
			var prepared_incident := prepared_incidents[0] as Dictionary
			if int(prepared_incident.get("penalty", 0)) >= 4:
				failures.append("Preparedness upgrades should reduce the penalty severity of a backup-check incident.")
			if int(prepared_incident.get("satisfaction_loss", 0)) >= 5:
				failures.append("Preparedness upgrades should reduce the satisfaction hit of a backup-check incident.")
	prepared_scene.queue_free()

	var baseline_backup_scene := packed_scene.instantiate()
	root.add_child(baseline_backup_scene)
	if baseline_backup_scene.has_method("select_board_contract"):
		baseline_backup_scene.select_board_contract("corner_dental_clinic")
	var baseline_backup_accept: Dictionary = baseline_backup_scene.accept_selected_contract()
	var baseline_backup_penalty := -1
	var baseline_backup_satisfaction_loss := -1
	var baseline_backup_cost := -1
	if not bool(baseline_backup_accept.get("ok", false)):
		failures.append("Backup regression checks should be able to accept the baseline support-heavy contract.")
	elif not bool(baseline_backup_scene.activate_selected_contract().get("ok", false)):
		failures.append("Backup regression checks should be able to activate the baseline support-heavy contract.")
	else:
		var baseline_backup_incidents: Array = baseline_backup_scene.get_active_incidents() if baseline_backup_scene.has_method("get_active_incidents") else []
		if baseline_backup_incidents.is_empty():
			failures.append("Backup regression checks should create a baseline support incident before preparedness is compared.")
		else:
			var baseline_backup_incident := baseline_backup_incidents[0] as Dictionary
			baseline_backup_penalty = int(baseline_backup_incident.get("penalty", -1))
			baseline_backup_satisfaction_loss = int(baseline_backup_incident.get("satisfaction_loss", -1))
			if String(baseline_backup_incident.get("incident_type", "")) != "backup_check":
				failures.append("Backup regression checks should compare the backup-check incident path.")
		var baseline_backup_cash_before := int(baseline_backup_scene.get_economy_snapshot().get("cash", 0))
		if baseline_backup_scene.has_method("run_backup_pass"):
			baseline_backup_scene.run_backup_pass()
		var baseline_backup_cash_after := int(baseline_backup_scene.get_economy_snapshot().get("cash", 0))
		baseline_backup_cost = baseline_backup_cash_before - baseline_backup_cash_after
	baseline_backup_scene.queue_free()

	var prepared_backup_scene := packed_scene.instantiate()
	root.add_child(prepared_backup_scene)
	prepared_backup_scene.purchase_expansion("backup_nas_cluster")
	prepared_backup_scene.purchase_expansion("support_runbook_binder")
	if prepared_backup_scene.has_method("select_board_contract"):
		prepared_backup_scene.select_board_contract("corner_dental_clinic")
	var prepared_backup_accept: Dictionary = prepared_backup_scene.accept_selected_contract()
	if not bool(prepared_backup_accept.get("ok", false)):
		failures.append("Backup regression checks should be able to accept the prepared support-heavy contract.")
	elif not bool(prepared_backup_scene.activate_selected_contract().get("ok", false)):
		failures.append("Backup regression checks should be able to activate the prepared support-heavy contract.")
	else:
		var prepared_backup_incidents: Array = prepared_backup_scene.get_active_incidents() if prepared_backup_scene.has_method("get_active_incidents") else []
		if prepared_backup_incidents.is_empty():
			failures.append("Backup regression checks should still generate a prepared support incident to compare.")
		else:
			var prepared_backup_incident := prepared_backup_incidents[0] as Dictionary
			if baseline_backup_penalty >= 0 and int(prepared_backup_incident.get("penalty", 999)) >= baseline_backup_penalty:
				failures.append("Prepared backup readiness should soften backup-check penalties compared with the baseline incident.")
			if baseline_backup_satisfaction_loss >= 0 and int(prepared_backup_incident.get("satisfaction_loss", 999)) >= baseline_backup_satisfaction_loss:
				failures.append("Prepared backup readiness should soften backup-check satisfaction loss compared with the baseline incident.")
		var prepared_backup_cash_before := int(prepared_backup_scene.get_economy_snapshot().get("cash", 0))
		if prepared_backup_scene.has_method("run_backup_pass"):
			prepared_backup_scene.run_backup_pass()
		var prepared_backup_cash_after := int(prepared_backup_scene.get_economy_snapshot().get("cash", 0))
		var prepared_backup_cost := prepared_backup_cash_before - prepared_backup_cash_after
		if baseline_backup_cost >= 0 and prepared_backup_cost >= baseline_backup_cost:
			failures.append("Prepared backup readiness should reduce the cost of clearing the same backup-check incident.")
	prepared_backup_scene.queue_free()

	var baseline_after_hours_scene := packed_scene.instantiate()
	root.add_child(baseline_after_hours_scene)
	if baseline_after_hours_scene.has_method("select_board_contract"):
		baseline_after_hours_scene.select_board_contract("metro_fitness_studio")
	var baseline_after_hours_accept: Dictionary = baseline_after_hours_scene.accept_selected_contract()
	var baseline_after_hours_penalty := -1
	var baseline_after_hours_satisfaction_loss := -1
	if not bool(baseline_after_hours_accept.get("ok", false)):
		failures.append("After-hours regression checks should be able to accept the baseline support contract.")
	elif not bool(baseline_after_hours_scene.activate_selected_contract().get("ok", false)):
		failures.append("After-hours regression checks should be able to activate the baseline support contract.")
	else:
		var baseline_after_hours_incidents: Array = baseline_after_hours_scene.get_active_incidents() if baseline_after_hours_scene.has_method("get_active_incidents") else []
		if baseline_after_hours_incidents.is_empty():
			failures.append("After-hours regression checks should create a baseline support incident before preparedness is compared.")
		else:
			var baseline_after_hours_incident := baseline_after_hours_incidents[0] as Dictionary
			baseline_after_hours_penalty = int(baseline_after_hours_incident.get("penalty", -1))
			baseline_after_hours_satisfaction_loss = int(baseline_after_hours_incident.get("satisfaction_loss", -1))
			if String(baseline_after_hours_incident.get("incident_type", "")) != "after_hours":
				failures.append("After-hours regression checks should compare the late-night request incident path.")
	baseline_after_hours_scene.queue_free()

	var prepared_after_hours_scene := packed_scene.instantiate()
	root.add_child(prepared_after_hours_scene)
	prepared_after_hours_scene.purchase_expansion("support_runbook_binder")
	if prepared_after_hours_scene.has_method("select_board_contract"):
		prepared_after_hours_scene.select_board_contract("metro_fitness_studio")
	var prepared_after_hours_accept: Dictionary = prepared_after_hours_scene.accept_selected_contract()
	if not bool(prepared_after_hours_accept.get("ok", false)):
		failures.append("After-hours regression checks should be able to accept the prepared support contract.")
	elif not bool(prepared_after_hours_scene.activate_selected_contract().get("ok", false)):
		failures.append("After-hours regression checks should be able to activate the prepared support contract.")
	else:
		var prepared_after_hours_incidents: Array = prepared_after_hours_scene.get_active_incidents() if prepared_after_hours_scene.has_method("get_active_incidents") else []
		if prepared_after_hours_incidents.is_empty():
			failures.append("After-hours regression checks should still generate a prepared support incident to compare.")
		else:
			var prepared_after_hours_incident := prepared_after_hours_incidents[0] as Dictionary
			if baseline_after_hours_penalty >= 0 and int(prepared_after_hours_incident.get("penalty", 999)) >= baseline_after_hours_penalty:
				failures.append("The runbook binder should soften after-hours penalties compared with the baseline incident.")
			if baseline_after_hours_satisfaction_loss >= 0 and int(prepared_after_hours_incident.get("satisfaction_loss", 999)) >= baseline_after_hours_satisfaction_loss:
				failures.append("The runbook binder should soften after-hours satisfaction loss compared with the baseline incident.")
	prepared_after_hours_scene.queue_free()

	var baseline_traffic_scene := packed_scene.instantiate()
	root.add_child(baseline_traffic_scene)
	baseline_traffic_scene.purchase_expansion("external_server_unit")
	if baseline_traffic_scene.has_method("select_board_contract"):
		baseline_traffic_scene.select_board_contract("riverblock_community_forum")
	var baseline_traffic_accept: Dictionary = baseline_traffic_scene.accept_selected_contract()
	var baseline_traffic_penalty := -1
	var baseline_traffic_satisfaction_loss := -1
	var baseline_traffic_cost := -1
	if not bool(baseline_traffic_accept.get("ok", false)):
		failures.append("Traffic regression checks should be able to accept the baseline level-two forum contract after the first server upgrade.")
	elif not bool(baseline_traffic_scene.activate_selected_contract().get("ok", false)):
		failures.append("Traffic regression checks should be able to activate the baseline level-two forum contract.")
	else:
		var baseline_traffic_incidents: Array = baseline_traffic_scene.get_active_incidents() if baseline_traffic_scene.has_method("get_active_incidents") else []
		if baseline_traffic_incidents.is_empty():
			failures.append("Traffic regression checks should create a baseline support incident before preparedness is compared.")
		else:
			var baseline_traffic_incident := baseline_traffic_incidents[0] as Dictionary
			baseline_traffic_penalty = int(baseline_traffic_incident.get("penalty", -1))
			baseline_traffic_satisfaction_loss = int(baseline_traffic_incident.get("satisfaction_loss", -1))
			if String(baseline_traffic_incident.get("incident_type", "")) != "internet_slowdown":
				failures.append("Traffic regression checks should compare the internet-slowdown incident path.")
		var baseline_traffic_cash_before := int(baseline_traffic_scene.get_economy_snapshot().get("cash", 0))
		if baseline_traffic_scene.has_method("stabilize_line"):
			baseline_traffic_scene.stabilize_line()
		var baseline_traffic_cash_after := int(baseline_traffic_scene.get_economy_snapshot().get("cash", 0))
		baseline_traffic_cost = baseline_traffic_cash_before - baseline_traffic_cash_after
	baseline_traffic_scene.queue_free()

	var prepared_traffic_scene := packed_scene.instantiate()
	root.add_child(prepared_traffic_scene)
	prepared_traffic_scene.purchase_expansion("external_server_unit")
	prepared_traffic_scene.purchase_expansion("business_line_modem")
	if prepared_traffic_scene.has_method("select_board_contract"):
		prepared_traffic_scene.select_board_contract("riverblock_community_forum")
	var prepared_traffic_accept: Dictionary = prepared_traffic_scene.accept_selected_contract()
	if not bool(prepared_traffic_accept.get("ok", false)):
		failures.append("Traffic regression checks should be able to accept the prepared level-two forum contract after the first server upgrade.")
	elif not bool(prepared_traffic_scene.activate_selected_contract().get("ok", false)):
		failures.append("Traffic regression checks should be able to activate the prepared level-two forum contract.")
	else:
		var prepared_traffic_incidents: Array = prepared_traffic_scene.get_active_incidents() if prepared_traffic_scene.has_method("get_active_incidents") else []
		if prepared_traffic_incidents.is_empty():
			failures.append("Traffic regression checks should still generate a prepared support incident to compare.")
		else:
			var prepared_traffic_incident := prepared_traffic_incidents[0] as Dictionary
			if baseline_traffic_penalty >= 0 and int(prepared_traffic_incident.get("penalty", 999)) >= baseline_traffic_penalty:
				failures.append("The business-line modem should soften traffic-spike penalties compared with the baseline incident.")
			if baseline_traffic_satisfaction_loss >= 0 and int(prepared_traffic_incident.get("satisfaction_loss", 999)) >= baseline_traffic_satisfaction_loss:
				failures.append("The business-line modem should soften traffic-spike satisfaction loss compared with the baseline incident.")
		var prepared_traffic_cash_before := int(prepared_traffic_scene.get_economy_snapshot().get("cash", 0))
		if prepared_traffic_scene.has_method("stabilize_line"):
			prepared_traffic_scene.stabilize_line()
		var prepared_traffic_cash_after := int(prepared_traffic_scene.get_economy_snapshot().get("cash", 0))
		var prepared_traffic_cost := prepared_traffic_cash_before - prepared_traffic_cash_after
		if baseline_traffic_cost >= 0 and prepared_traffic_cost >= baseline_traffic_cost:
			failures.append("The business-line modem should reduce the cost of clearing the same traffic-spike incident.")
	prepared_traffic_scene.queue_free()

	var mitigation_scene := packed_scene.instantiate()
	root.add_child(mitigation_scene)
	var mitigation_backup_button := mitigation_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/ContractsTab/ResponseSection/Margin/Content/Buttons/BackupPass")
	if mitigation_scene.has_method("select_board_contract"):
		mitigation_scene.select_board_contract("corner_dental_clinic")
	var mitigation_accept: Dictionary = mitigation_scene.accept_selected_contract()
	if not bool(mitigation_accept.get("ok", false)):
		failures.append("Mitigation checks should be able to accept a support-heavy contract.")
	elif not bool(mitigation_scene.activate_selected_contract().get("ok", false)):
		failures.append("Mitigation checks should be able to activate a support-heavy contract.")
	elif mitigation_backup_button == null:
		failures.append("Mitigation checks need the backup-pass response button in the contracts panel.")
	else:
		var incidents_before: Array = mitigation_scene.get_active_incidents() if mitigation_scene.has_method("get_active_incidents") else []
		var cash_before := int(mitigation_scene.get_economy_snapshot().get("cash", 0))
		if mitigation_scene.has_method("run_backup_pass"):
			mitigation_scene.run_backup_pass()
		else:
			failures.append("Mitigation checks need the backup-pass gameplay action on the main scene.")
		var incidents_after: Array = mitigation_scene.get_active_incidents() if mitigation_scene.has_method("get_active_incidents") else []
		var cash_after := int(mitigation_scene.get_economy_snapshot().get("cash", 0))
		if incidents_after.size() >= incidents_before.size():
			failures.append("Manual mitigation should clear the matching live support issue from the queue.")
		if cash_after >= cash_before:
			failures.append("Manual mitigation should cost a modest amount of cash to reflect small-business support effort.")
		var mitigated_report: Dictionary = mitigation_scene.tick_month()
		if int(mitigated_report.get("neglected_incident_count", 0)) > 0:
			failures.append("A cleared support issue should not still count as neglected on the monthly report.")
	mitigation_scene.queue_free()

func _run_onboarding_checks(failures: Array[String]) -> void:
	var packed_scene := load(MAIN_SCENE_PATH) as PackedScene
	if packed_scene == null:
		failures.append("Onboarding checks need the main scene to load.")
		return

	var onboarding_scene := packed_scene.instantiate()
	root.add_child(onboarding_scene)

	var onboarding_body := onboarding_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent/OperationsTab/OnboardingPanel/Margin/Content/Body")
	if not onboarding_body is Label:
		failures.append("Operations tab should expose a compact onboarding panel with narrative callouts.")
		onboarding_scene.queue_free()
		return
	if onboarding_scene.has_method("_refresh_main_tab_summaries"):
		onboarding_scene.call("_refresh_main_tab_summaries")

	if not onboarding_body.text.contains("contract board"):
		failures.append("Onboarding callouts should point the player toward the contract board first.")
	if not onboarding_body.text.contains("workstation"):
		failures.append("Onboarding callouts should explain that hosting is activated at the workstation.")
	if not onboarding_body.text.contains("overload"):
		failures.append("Onboarding callouts should warn about overload risk in the early loop.")
	if not onboarding_body.text.contains("Hardware stage:"):
		failures.append("Operations onboarding should surface the current hardware stage at a glance.")
	if not onboarding_body.text.contains("Support pressure:"):
		failures.append("Operations onboarding should surface current support pressure at a glance.")
	if not onboarding_body.text.contains("Next lead gate:"):
		failures.append("Operations onboarding should surface the next contract unlock gate.")
	if not onboarding_body.text.contains("Level-two readiness:"):
		failures.append("Operations onboarding should surface level-two mitigation readiness at a glance.")
	if not onboarding_body.text.contains("Backup checks:"):
		failures.append("Operations onboarding should call out backup-check readiness.")
	if not onboarding_body.text.contains("After-hours requests:"):
		failures.append("Operations onboarding should call out after-hours readiness.")
	if not onboarding_body.text.contains("Traffic-spike issues:"):
		failures.append("Operations onboarding should call out traffic-spike readiness.")

	var accept_result: Dictionary = onboarding_scene.accept_selected_contract()
	if not bool(accept_result.get("ok", false)):
		failures.append("Onboarding checks should still be able to accept a starter contract.")
		onboarding_scene.queue_free()
		return
	if not onboarding_body.text.contains("activate"):
		failures.append("Onboarding callouts should pivot toward workstation activation after a contract is accepted.")

	var activate_result: Dictionary = onboarding_scene.activate_selected_contract()
	if not bool(activate_result.get("ok", false)):
		failures.append("Onboarding checks should still be able to activate a starter contract.")
		onboarding_scene.queue_free()
		return
	if not onboarding_body.text.contains("server"):
		failures.append("Onboarding callouts should signpost the first server expansion after hosting begins.")

	onboarding_scene.queue_free()

func _run_art_foundation_checks(failures: Array[String]) -> void:
	var foundation_data_path := "res://data/art/garage_art_foundation.json"
	if not FileAccess.file_exists(foundation_data_path):
		failures.append("Art foundation step should add a reusable garage art foundation JSON descriptor.")
		return

	var foundation_scene := load(MAIN_SCENE_PATH) as PackedScene
	if foundation_scene == null:
		failures.append("Art foundation checks need the main scene to load.")
		return

	var scene_instance := foundation_scene.instantiate()
	root.add_child(scene_instance)

	var foundation_node := scene_instance.get_node_or_null("Environment/ArtFoundation")
	if foundation_node == null:
		failures.append("Environment should expose an ArtFoundation node for the reusable garage visual base.")
		scene_instance.queue_free()
		return

	var foundation_texture: Variant = foundation_node.get("texture")
	if not foundation_texture is Texture2D:
		failures.append("ArtFoundation should use a real texture instead of only flat placeholder geometry.")

	var foundation_file := FileAccess.open(foundation_data_path, FileAccess.READ)
	if foundation_file == null:
		failures.append("Garage art foundation JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(foundation_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage art foundation JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var foundation_data := parsed as Dictionary
	if String(foundation_data.get("style", "")) == "":
		failures.append("Garage art foundation JSON should declare the target visual style.")
	if String(foundation_data.get("palette_name", "")) == "":
		failures.append("Garage art foundation JSON should declare the warm startup palette name.")

	var texture_path := String(foundation_data.get("scene_texture_path", ""))
	if texture_path == "":
		failures.append("Garage art foundation JSON should point to the shared scene texture.")
	elif not FileAccess.file_exists(texture_path):
		failures.append("Garage art foundation texture should exist on disk.")

	var palette: Variant = foundation_data.get("palette", {})
	if not palette is Dictionary:
		failures.append("Garage art foundation JSON should define a reusable palette dictionary.")
	else:
		for key in ["floor", "wall", "trim", "shadow", "accent"]:
			if String((palette as Dictionary).get(key, "")) == "":
				failures.append("Garage art foundation palette should define `%s`." % key)

	var materials: Variant = foundation_data.get("materials", {})
	if not materials is Dictionary:
		failures.append("Garage art foundation JSON should describe reusable material roles.")
	else:
		for key in ["floor", "wall", "trim", "shadow"]:
			if String((materials as Dictionary).get(key, "")).strip_edges() == "":
				failures.append("Garage art foundation materials should describe `%s`." % key)

	scene_instance.queue_free()

func _run_garage_shell_checks(failures: Array[String]) -> void:
	var shell_data_path := "res://data/art/garage_shell.json"
	if not FileAccess.file_exists(shell_data_path):
		failures.append("Garage shell step should add a reusable garage shell JSON descriptor.")
		return

	var shell_scene := load(MAIN_SCENE_PATH) as PackedScene
	if shell_scene == null:
		failures.append("Garage shell checks need the main scene to load.")
		return

	var scene_instance := shell_scene.instantiate()
	root.add_child(scene_instance)

	var shell_node := scene_instance.get_node_or_null("Environment/GarageShell")
	if shell_node == null:
		failures.append("Environment should expose a GarageShell node for the single-room architectural layer.")
		scene_instance.queue_free()
		return

	var shell_texture: Variant = shell_node.get("texture")
	if not shell_texture is Texture2D:
		failures.append("GarageShell should use a real texture for the room architecture layer.")

	var shell_file := FileAccess.open(shell_data_path, FileAccess.READ)
	if shell_file == null:
		failures.append("Garage shell JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(shell_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage shell JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var shell_data := parsed as Dictionary
	if String(shell_data.get("room_type", "")) != "converted_residential_garage":
		failures.append("Garage shell JSON should lock the room type to a converted residential garage.")
	if String(shell_data.get("layout_scale", "")) != "single_room_small":
		failures.append("Garage shell JSON should describe the shell as a smaller single-room layout.")

	var shell_texture_path := String(shell_data.get("scene_texture_path", ""))
	if shell_texture_path == "":
		failures.append("Garage shell JSON should point to the shell scene texture.")
	elif not FileAccess.file_exists(shell_texture_path):
		failures.append("Garage shell texture should exist on disk.")

	var shell_features: Variant = shell_data.get("architectural_features", [])
	if not shell_features is Array:
		failures.append("Garage shell JSON should list the core architectural features.")
	else:
		var feature_list := shell_features as Array
		for feature in ["rollup_door", "side_entry_door", "garage_window", "built_in_workbench"]:
			if not feature_list.has(feature):
				failures.append("Garage shell should include the architectural feature `%s`." % feature)

	var tone: Dictionary = shell_data.get("tone", {}) as Dictionary
	if String(tone.get("furniture_style", "")) != "diy":
		failures.append("Garage shell JSON should keep the workspace furniture style DIY and improvised.")

	scene_instance.queue_free()

func _run_garage_prop_checks(failures: Array[String]) -> void:
	var props_data_path := "res://data/art/garage_props.json"
	if not FileAccess.file_exists(props_data_path):
		failures.append("Prop pass step should add a reusable garage props JSON descriptor.")
		return

	var props_scene := load(MAIN_SCENE_PATH) as PackedScene
	if props_scene == null:
		failures.append("Garage prop checks need the main scene to load.")
		return

	var scene_instance := props_scene.instantiate()
	root.add_child(scene_instance)

	var props_node := scene_instance.get_node_or_null("Environment/GarageProps")
	if props_node == null:
		failures.append("Environment should expose a GarageProps node for the reusable electronics-bench clutter layer.")
		scene_instance.queue_free()
		return

	var props_texture: Variant = props_node.get("texture")
	if not props_texture is Texture2D:
		failures.append("GarageProps should use a real texture for the modular prop layer.")

	var props_file := FileAccess.open(props_data_path, FileAccess.READ)
	if props_file == null:
		failures.append("Garage props JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(props_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage props JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var props_data := parsed as Dictionary
	if String(props_data.get("prop_theme", "")) != "electronics_bench":
		failures.append("Garage props JSON should lock the prop theme to an electronics bench focus.")
	if String(props_data.get("priority", "")) != "wall_storage_and_work_surfaces":
		failures.append("Garage props JSON should prioritize wall-mounted storage and work surfaces first.")

	var props_texture_path := String(props_data.get("scene_texture_path", ""))
	if props_texture_path == "":
		failures.append("Garage props JSON should point to the prop scene texture.")
	elif not FileAccess.file_exists(props_texture_path):
		failures.append("Garage props texture should exist on disk.")

	var modules: Variant = props_data.get("modules", [])
	if not modules is Array:
		failures.append("Garage props JSON should list the reusable prop modules.")
	else:
		var module_list := modules as Array
		for module_name in ["pegboard_tools", "parts_shelves", "soldering_bench", "spare_hardware"]:
			if not module_list.has(module_name):
				failures.append("Garage props should include the reusable module `%s`." % module_name)

	scene_instance.queue_free()

func _run_garage_polish_checks(failures: Array[String]) -> void:
	var polish_data_path := "res://data/art/garage_polish.json"
	if not FileAccess.file_exists(polish_data_path):
		failures.append("Scene polish step should add a reusable garage polish JSON descriptor.")
		return

	var polish_scene := load(MAIN_SCENE_PATH) as PackedScene
	if polish_scene == null:
		failures.append("Garage polish checks need the main scene to load.")
		return

	var scene_instance := polish_scene.instantiate()
	root.add_child(scene_instance)

	var polish_node := scene_instance.get_node_or_null("Environment/GaragePolish")
	if polish_node == null:
		failures.append("Environment should expose a GaragePolish node for the final atmosphere and focus layer.")
		scene_instance.queue_free()
		return

	var polish_texture: Variant = polish_node.get("texture")
	if not polish_texture is Texture2D:
		failures.append("GaragePolish should use a real texture for the final mood layer.")

	var polish_file := FileAccess.open(polish_data_path, FileAccess.READ)
	if polish_file == null:
		failures.append("Garage polish JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(polish_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage polish JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var polish_data := parsed as Dictionary
	if String(polish_data.get("polish_focus", "")) != "overall_atmosphere":
		failures.append("Garage polish JSON should prioritize overall atmosphere first.")
	if String(polish_data.get("mood", "")) != "warm_late_night_startup":
		failures.append("Garage polish JSON should lock the mood to a warm late-night startup feel.")

	var polish_texture_path := String(polish_data.get("scene_texture_path", ""))
	if polish_texture_path == "":
		failures.append("Garage polish JSON should point to the polish scene texture.")
	elif not FileAccess.file_exists(polish_texture_path):
		failures.append("Garage polish texture should exist on disk.")

	var emphasis: Variant = polish_data.get("emphasis", [])
	if not emphasis is Array:
		failures.append("Garage polish JSON should list the main focal emphasis areas.")
	else:
		var emphasis_list := emphasis as Array
		for area in ["ambient_glow", "workstation_focus", "board_focus", "expansion_corner"]:
			if not emphasis_list.has(area):
				failures.append("Garage polish should include `%s` in its emphasis list." % area)

	scene_instance.queue_free()

func _run_unified_garage_scene_checks(failures: Array[String]) -> void:
	var unified_data_path := "res://data/art/unified_garage_scene.json"
	if not FileAccess.file_exists(unified_data_path):
		failures.append("Scene reset step should add a unified garage scene JSON descriptor.")
		return

	var unified_scene := load(MAIN_SCENE_PATH) as PackedScene
	if unified_scene == null:
		failures.append("Unified garage scene checks need the main scene to load.")
		return

	var scene_instance := unified_scene.instantiate()
	root.add_child(scene_instance)

	var unified_node := scene_instance.get_node_or_null("Environment/UnifiedGarageScene")
	if unified_node == null:
		failures.append("Environment should expose a UnifiedGarageScene node for the single authored room image.")
		scene_instance.queue_free()
		return

	var unified_texture: Variant = unified_node.get("texture")
	if not unified_texture is Texture2D:
		failures.append("UnifiedGarageScene should use a real texture for the authored room image.")

	for legacy_name in ["GarageShell", "GarageProps", "GaragePolish"]:
		var legacy_node := scene_instance.get_node_or_null("Environment/%s" % legacy_name)
		if legacy_node != null and bool(legacy_node.visible):
			failures.append("%s should no longer stay visible once the unified room image is in place." % legacy_name)

	var unified_file := FileAccess.open(unified_data_path, FileAccess.READ)
	if unified_file == null:
		failures.append("Unified garage scene JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(unified_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Unified garage scene JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var unified_data := parsed as Dictionary
	if String(unified_data.get("composition_type", "")) != "single_authored_room":
		failures.append("Unified garage scene JSON should lock the composition to a single authored room image.")
	if String(unified_data.get("room_concept", "")) != "converted_residential_startup_garage":
		failures.append("Unified garage scene JSON should keep the converted residential startup garage concept.")

	var unified_texture_path := String(unified_data.get("scene_texture_path", ""))
	if unified_texture_path == "":
		failures.append("Unified garage scene JSON should point to the authored room texture.")
	elif not FileAccess.file_exists(unified_texture_path):
		failures.append("Unified garage scene texture should exist on disk.")

	scene_instance.queue_free()

func _run_garage_node_alignment_checks(failures: Array[String]) -> void:
	var alignment_data_path := "res://data/art/garage_node_alignment.json"
	if not FileAccess.file_exists(alignment_data_path):
		failures.append("Node alignment step should add a garage node alignment JSON descriptor.")
		return

	var alignment_scene := load(MAIN_SCENE_PATH) as PackedScene
	if alignment_scene == null:
		failures.append("Garage node alignment checks need the main scene to load.")
		return

	var scene_instance := alignment_scene.instantiate()
	root.add_child(scene_instance)

	var player_node := scene_instance.get_node_or_null("Player")
	var computer_station := scene_instance.get_node_or_null("Workstations/ComputerStation")
	var contracts_board := scene_instance.get_node_or_null("Workstations/ContractsBoard")
	var external_server := scene_instance.get_node_or_null("Workstations/ExternalServerUnit")
	var desk_body := scene_instance.get_node_or_null("Environment/DeskArea/Desk")
	var desk_shape_node := scene_instance.get_node_or_null("Environment/DeskArea/Desk/CollisionShape2D")
	var board_body := scene_instance.get_node_or_null("Environment/BoardDesk")
	var board_shape_node := scene_instance.get_node_or_null("Environment/BoardDesk/CollisionShape2D")
	if player_node == null or computer_station == null or contracts_board == null or external_server == null or desk_body == null or desk_shape_node == null or board_body == null or board_shape_node == null:
		failures.append("Node alignment checks need the player, stations, board collision, and desk collision to exist in the main scene.")
		scene_instance.queue_free()
		return

	if player_node is Node2D:
		var player_position := (player_node as Node2D).position
		if player_position.x < 240.0 or player_position.x > 400.0 or player_position.y < 180.0 or player_position.y > 300.0:
			failures.append("Player start should move closer to the center of the unified garage.")

	if contracts_board is Node2D:
		var board_station_position := (contracts_board as Node2D).position
		if board_station_position.x > 110.0 or board_station_position.y > 96.0:
			failures.append("Contracts board interaction anchor should sit in the upper-left board corner of the garage.")
		elif not contracts_board.has_method("can_interact"):
			failures.append("Contracts board should still expose a direct interaction-range helper.")
		else:
			var board_nook_point := Vector2(96.0, 88.0)
			var open_floor_point := Vector2(148.0, 132.0)
			if not bool(contracts_board.call("can_interact", board_nook_point)):
				failures.append("Contracts board hotspot should still be interactable inside the top-left board nook.")
			if bool(contracts_board.call("can_interact", open_floor_point)):
				failures.append("Contracts board hotspot should no longer leak out into the open floor beside the desk nook.")

	if computer_station is Node2D:
		var workstation_position := (computer_station as Node2D).position
		if workstation_position.x < 500.0 or workstation_position.y > 128.0:
			failures.append("Computer workstation interaction anchor should sit in the upper-right workbench corner of the garage.")

	if board_body is Node2D:
		var board_body_position := (board_body as Node2D).position
		if board_body_position.x > 124.0 or board_body_position.y > 132.0:
			failures.append("Contract board collision should stay tucked into the upper-left furniture footprint.")

	if board_shape_node is CollisionShape2D:
		var board_shape := (board_shape_node as CollisionShape2D).shape
		if board_shape is RectangleShape2D:
			var board_size := (board_shape as RectangleShape2D).size
			if board_size.x < 120.0:
				failures.append("Contract board collision should be wide enough to cover the upper-left desk footprint.")
		else:
			failures.append("Contract board collision should stay as a rectangular blocker during the alignment pass.")

	if desk_body is Node2D:
		var desk_body_position := (desk_body as Node2D).position
		if desk_body_position.x < 480.0 or desk_body_position.y > 136.0:
			failures.append("Workbench collision should stay aligned to the upper-right bench footprint.")

	if desk_shape_node is CollisionShape2D:
		var desk_shape := (desk_shape_node as CollisionShape2D).shape
		if desk_shape is RectangleShape2D:
			var desk_size := (desk_shape as RectangleShape2D).size
			if desk_size.x < 200.0 or desk_size.x > 240.0:
				failures.append("Workbench collision should match the upper-right bench footprint instead of a generic oversized blocker.")
		else:
			failures.append("Desk collision should stay as a rectangular blocker during the first alignment pass.")

	var alignment_file := FileAccess.open(alignment_data_path, FileAccess.READ)
	if alignment_file == null:
		failures.append("Garage node alignment JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(alignment_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage node alignment JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var alignment_data := parsed as Dictionary
	if String(alignment_data.get("player_spawn", "")) != "centered_start":
		failures.append("Garage node alignment JSON should mark the player start as centered.")

	var collision_strategy := String(alignment_data.get("collision_strategy", ""))
	if collision_strategy != "shaped_rectangles":
		failures.append("Garage node alignment JSON should describe the first alignment pass as shaped rectangles.")

	scene_instance.queue_free()

func _run_garage_detail_pass_checks(failures: Array[String]) -> void:
	var detail_data_path := "res://data/art/garage_detail_pass.json"
	if not FileAccess.file_exists(detail_data_path):
		failures.append("Detail pass step should add a garage detail-pass JSON descriptor.")
		return

	var detail_scene := load(MAIN_SCENE_PATH) as PackedScene
	if detail_scene == null:
		failures.append("Garage detail-pass checks need the main scene to load.")
		return

	var scene_instance := detail_scene.instantiate()
	root.add_child(scene_instance)

	var unified_node := scene_instance.get_node_or_null("Environment/UnifiedGarageScene")
	if unified_node == null:
		failures.append("Garage detail-pass checks need the unified garage scene node to remain in place.")
		scene_instance.queue_free()
		return

	var unified_texture: Variant = unified_node.get("texture")
	if not unified_texture is Texture2D:
		failures.append("UnifiedGarageScene should still use a real texture during the detail pass.")

	var detail_file := FileAccess.open(detail_data_path, FileAccess.READ)
	if detail_file == null:
		failures.append("Garage detail-pass JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(detail_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage detail-pass JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var detail_data := parsed as Dictionary
	if String(detail_data.get("tone", "")) != "grounded_understated":
		failures.append("Garage detail-pass JSON should keep the tone grounded and understated.")
	if String(detail_data.get("detail_focus", "")) != "center_edges":
		failures.append("Garage detail-pass JSON should focus added detail around the center edges.")

	var detail_texture_path := String(detail_data.get("scene_texture_path", ""))
	if detail_texture_path == "":
		failures.append("Garage detail-pass JSON should point to the richer unified room texture.")
	elif not FileAccess.file_exists(detail_texture_path):
		failures.append("Garage detail-pass texture should exist on disk.")

	var embedded_zones: Variant = detail_data.get("embedded_zones", [])
	if not embedded_zones is Array:
		failures.append("Garage detail-pass JSON should list the embedded gameplay zones.")
	else:
		var zone_list := embedded_zones as Array
		for zone_name in ["workstation", "contract_board", "expansion_corner", "startup_clutter"]:
			if not zone_list.has(zone_name):
				failures.append("Garage detail-pass should embed `%s` into the unified composition." % zone_name)

	scene_instance.queue_free()

func _run_garage_readability_tune_checks(failures: Array[String]) -> void:
	var tune_data_path := "res://data/art/garage_readability_tune.json"
	if not FileAccess.file_exists(tune_data_path):
		failures.append("Readability tune step should add a garage readability-tune JSON descriptor.")
		return

	var tune_scene := load(MAIN_SCENE_PATH) as PackedScene
	if tune_scene == null:
		failures.append("Garage readability-tune checks need the main scene to load.")
		return

	var scene_instance := tune_scene.instantiate()
	root.add_child(scene_instance)

	var unified_node := scene_instance.get_node_or_null("Environment/UnifiedGarageScene")
	if unified_node == null:
		failures.append("Garage readability-tune checks need the unified garage scene node to remain in place.")
		scene_instance.queue_free()
		return

	var unified_texture: Variant = unified_node.get("texture")
	if not unified_texture is Texture2D:
		failures.append("UnifiedGarageScene should still use a real texture during the readability tune.")

	var tune_file := FileAccess.open(tune_data_path, FileAccess.READ)
	if tune_file == null:
		failures.append("Garage readability-tune JSON should be readable.")
		scene_instance.queue_free()
		return

	var parsed: Variant = JSON.parse_string(tune_file.get_as_text())
	if not parsed is Dictionary:
		failures.append("Garage readability-tune JSON should parse into a dictionary.")
		scene_instance.queue_free()
		return

	var tune_data := parsed as Dictionary
	if String(tune_data.get("readability_focus", "")) != "brighten_gameplay_anchors":
		failures.append("Garage readability-tune JSON should focus on brightening gameplay anchors.")

	var focal_balance := String(tune_data.get("focal_balance", ""))
	if focal_balance != "workstation_and_contract_board_equal":
		failures.append("Garage readability-tune JSON should keep the workstation and contract board equally emphasized.")

	var tune_texture_path := String(tune_data.get("scene_texture_path", ""))
	if tune_texture_path == "":
		failures.append("Garage readability-tune JSON should point to the tuned unified room texture.")
	elif not FileAccess.file_exists(tune_texture_path):
		failures.append("Garage readability-tune texture should exist on disk.")

	var anchor_targets: Variant = tune_data.get("anchor_targets", [])
	if not anchor_targets is Array:
		failures.append("Garage readability-tune JSON should list the gameplay anchor targets.")
	else:
		var target_list := anchor_targets as Array
		for target_name in ["contract_board", "workstation", "expansion_corner"]:
			if not target_list.has(target_name):
				failures.append("Garage readability tune should include `%s` as an anchor target." % target_name)

	scene_instance.queue_free()

func _run_gameplay_scaling_checks(failures: Array[String]) -> void:
	var viewport_width := int(ProjectSettings.get_setting("display/window/size/viewport_width", 0))
	var viewport_height := int(ProjectSettings.get_setting("display/window/size/viewport_height", 0))
	if viewport_width != 1280 or viewport_height != 720:
		failures.append("Project settings should define a 1280x720 baseline viewport for gameplay scaling.")

	var scaling_scene := load(MAIN_SCENE_PATH) as PackedScene
	if scaling_scene == null:
		failures.append("Gameplay scaling checks need the main scene to load.")
		return

	var scene_instance := scaling_scene.instantiate()
	root.add_child(scene_instance)

	var player_node := scene_instance.get_node_or_null("Player")
	var camera_node := scene_instance.get_node_or_null("Player/Camera2D")
	if player_node == null or camera_node == null:
		failures.append("Gameplay scaling checks need the player and gameplay camera in the main scene.")
		scene_instance.queue_free()
		return

	if not player_node.has_method("refresh_gameplay_camera_zoom"):
		failures.append("Player scene should expose a gameplay camera refresh method for viewport scaling.")
		scene_instance.queue_free()
		return

	if not camera_node is Camera2D:
		failures.append("Gameplay scaling checks need Camera2D on the player scene.")
		scene_instance.queue_free()
		return

	var gameplay_camera := camera_node as Camera2D
	player_node.call("refresh_gameplay_camera_zoom", Vector2(1280.0, 720.0))
	var baseline_zoom := gameplay_camera.zoom.x
	player_node.call("refresh_gameplay_camera_zoom", Vector2(800.0, 450.0))
	var compact_zoom := gameplay_camera.zoom.x
	player_node.call("refresh_gameplay_camera_zoom", Vector2(1600.0, 900.0))
	var wide_zoom := gameplay_camera.zoom.x

	if absf(baseline_zoom - 1.0) > 0.01:
		failures.append("Gameplay camera should keep a neutral 1x zoom at the baseline viewport size.")
	if compact_zoom >= baseline_zoom:
		failures.append("Gameplay camera should zoom in on smaller windows so the room stays readable.")
	if wide_zoom <= baseline_zoom:
		failures.append("Gameplay camera should zoom out slightly on larger windows so more of the garage becomes visible.")
	if compact_zoom < 0.85:
		failures.append("Gameplay camera should cap small-window zoom so the player does not lose too much room context.")
	if wide_zoom > 1.45:
		failures.append("Gameplay camera should cap large-window zoom so the room does not become too distant.")

	scene_instance.queue_free()

func _run_responsive_ui_scaling_checks(failures: Array[String]) -> void:
	var scaling_scene := load(MAIN_SCENE_PATH) as PackedScene
	if scaling_scene == null:
		failures.append("Responsive UI scaling checks need the main scene to load.")
		return

	var scene_instance := scaling_scene.instantiate()
	root.add_child(scene_instance)

	if not scene_instance.has_method("refresh_responsive_ui_layout"):
		failures.append("Main scene should expose a responsive UI layout refresh method for resize-aware panel scaling.")
		scene_instance.queue_free()
		return

	var main_panel := scene_instance.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel")
	var log_panel := scene_instance.get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel")
	var right_stack := scene_instance.get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack")
	var station_panel := scene_instance.get_node_or_null("UI/StationPanel")
	var contracts_board_panel := scene_instance.get_node_or_null("UI/ContractsBoardPanel")
	if main_panel == null or log_panel == null or right_stack == null or station_panel == null or contracts_board_panel == null:
		failures.append("Responsive UI scaling checks need the shell, station panel, and contracts board panel nodes.")
		scene_instance.queue_free()
		return
	if not contracts_board_panel.has_method("refresh_responsive_layout"):
		failures.append("Contracts board panel should expose a responsive layout refresh method for resize-aware panel scaling.")
		scene_instance.queue_free()
		return

	scene_instance.call("refresh_responsive_ui_layout", Vector2(960.0, 540.0))
	var compact_main_width := (main_panel as Control).custom_minimum_size.x
	var compact_log_width := (log_panel as Control).custom_minimum_size.x
	var compact_right_width := (right_stack as Control).custom_minimum_size.x
	var compact_station_width := (station_panel as Control).offset_right - (station_panel as Control).offset_left
	var compact_station_bottom := (station_panel as Control).offset_bottom
	var compact_board_width := (contracts_board_panel as Control).offset_right - (contracts_board_panel as Control).offset_left
	var compact_board_height := (contracts_board_panel as Control).offset_bottom - (contracts_board_panel as Control).offset_top

	scene_instance.call("refresh_responsive_ui_layout", Vector2(1600.0, 900.0))
	var wide_main_width := (main_panel as Control).custom_minimum_size.x
	var wide_log_width := (log_panel as Control).custom_minimum_size.x
	var wide_right_width := (right_stack as Control).custom_minimum_size.x
	var wide_station_width := (station_panel as Control).offset_right - (station_panel as Control).offset_left
	var wide_station_bottom := (station_panel as Control).offset_bottom
	var wide_board_width := (contracts_board_panel as Control).offset_right - (contracts_board_panel as Control).offset_left
	var wide_board_height := (contracts_board_panel as Control).offset_bottom - (contracts_board_panel as Control).offset_top

	if compact_main_width >= wide_main_width:
		failures.append("Main dock width should scale down on smaller windows instead of staying fixed at the large-window size.")
	if compact_log_width >= wide_log_width:
		failures.append("Log panel width should scale down on smaller windows instead of staying fixed at the large-window size.")
	if compact_right_width >= wide_right_width:
		failures.append("Right-side alert stack width should scale down on smaller windows instead of staying fixed at the large-window size.")
	if compact_station_width >= wide_station_width:
		failures.append("Station panel width should scale down on smaller windows instead of staying fixed at the large-window size.")
	if compact_board_width >= wide_board_width or compact_board_height >= wide_board_height:
		failures.append("Contracts board panel should scale down on smaller windows instead of staying fixed at the large-window size.")
	if compact_station_width > 320.0 or compact_board_width > 420.0:
		failures.append("Compact-window responsive panels should avoid taking an oversized share of the garage view.")
	if wide_station_width < 280.0 or wide_board_width < 420.0:
		failures.append("Large-window responsive panels should widen enough to stay readable instead of remaining cramped.")
	if compact_station_bottom > 524.0 or wide_station_bottom > 884.0:
		failures.append("Station panel should stay inside the viewport height after responsive resizing.")

	scene_instance.queue_free()

func _run_collision_and_resize_regression_checks(failures: Array[String]) -> void:
	_run_garage_node_alignment_checks(failures)
	_run_gameplay_scaling_checks(failures)
	_run_responsive_ui_scaling_checks(failures)

func _init() -> void:
	_clear_save_file()
	var packed_scene := load(MAIN_SCENE_PATH) as PackedScene
	if packed_scene == null:
		push_error("FAIL: Main scene could not be loaded.")
		quit(1)
		return

	var main_scene := packed_scene.instantiate()
	root.add_child(main_scene)

	var failures: Array[String] = []
	_run_gameplay_checks(main_scene, failures)
	_run_data_setup_checks(main_scene, failures)
	_run_player_state_machine_checks(main_scene, failures)
	_run_save_slice_checks(failures)
	_run_issue_response_checks(failures)
	_run_onboarding_checks(failures)
	_run_art_foundation_checks(failures)
	_run_garage_shell_checks(failures)
	_run_garage_prop_checks(failures)
	_run_garage_polish_checks(failures)
	_run_unified_garage_scene_checks(failures)
	_run_garage_detail_pass_checks(failures)
	_run_garage_readability_tune_checks(failures)
	_run_collision_and_resize_regression_checks(failures)

	var main_panel := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel")
	var log_panel := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel")
	var monthly_summary := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/MonthlySummary")
	var warning_alert := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack/WarningAlert")
	var dock_spacer := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/DockSpacer")
	var shell_tint := Color(1, 1, 1, 1)

	if main_panel == null or log_panel == null or monthly_summary == null or warning_alert == null:
		failures.append("Docked UI shell should include the main panel, log panel, and right-side alert panels.")
	else:
		if main_panel is Control:
			var main_control := main_panel as Control
			if bool(main_control.size_flags_horizontal & Control.SIZE_EXPAND):
				failures.append("Main management shell should stay compact instead of expanding across fullscreen width.")
			if bool(main_control.size_flags_vertical & Control.SIZE_EXPAND):
				failures.append("Main management shell should stay compact vertically instead of stretching down the window.")
		if log_panel is Control:
			var log_control := log_panel as Control
			if bool(log_control.size_flags_horizontal & Control.SIZE_EXPAND):
				failures.append("Log panel should stay compact instead of expanding across fullscreen width.")
			if bool(log_control.size_flags_vertical & Control.SIZE_EXPAND):
				failures.append("Log panel should stay compact vertically instead of stretching down the window.")
		var right_stack := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/RightStack")
		if right_stack is Control:
			var right_control := right_stack as Control
			if bool(right_control.size_flags_horizontal & Control.SIZE_EXPAND):
				failures.append("Right-side alert stack should stay compact instead of expanding across fullscreen width.")
		if dock_spacer == null:
			failures.append("Docked shell should include a flexible spacer so extra fullscreen width stays on the garage scene.")
		elif dock_spacer is Control:
			var spacer_control := dock_spacer as Control
			if not bool(spacer_control.size_flags_horizontal & Control.SIZE_EXPAND_FILL):
				failures.append("Dock spacer should absorb extra window width instead of stretching the UI panels.")

		shell_tint = main_panel.modulate
		if shell_tint == Color(1, 1, 1, 1):
			failures.append("Main management shell should use a non-default retro panel tint.")
		if log_panel.modulate != shell_tint:
			failures.append("Log panel should share the same retro tint as the main management shell.")
		if monthly_summary.modulate != shell_tint:
			failures.append("Month-end summary should share the same retro tint as the main management shell.")
		if warning_alert.modulate != shell_tint:
			failures.append("Warning alert should share the same retro tint as the main management shell.")

	var tab_buttons := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabButtons")
	var tab_content := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/MainPanel/Margin/Content/TabContent")
	if tab_buttons == null or tab_content == null:
		failures.append("Tabbed management shell should still expose tab buttons and shared tab content.")
	else:
		var operations_button := tab_buttons.get_node_or_null("Operations")
		var contracts_button := tab_buttons.get_node_or_null("Contracts")
		var progress_button := tab_buttons.get_node_or_null("Progress")
		var operations_tab := tab_content.get_node_or_null("OperationsTab")
		var contracts_tab := tab_content.get_node_or_null("ContractsTab")
		var progress_tab := tab_content.get_node_or_null("ProgressTab")

		if operations_button == null or contracts_button == null or progress_button == null:
			failures.append("Tab shell should expose Operations, Contracts, and Progress buttons.")
		elif operations_tab == null or contracts_tab == null or progress_tab == null:
			failures.append("Tab shell should expose Operations, Contracts, and Progress views.")
		else:
			var available_contracts_body := contracts_tab.get_node_or_null("AvailableSection/Margin/Content/Body")
			var queue_contracts_body := contracts_tab.get_node_or_null("QueueSection/Margin/Content/Body")
			var active_contracts_body := contracts_tab.get_node_or_null("ActiveSection/Margin/Content/Body")
			var outcomes_contracts_body := contracts_tab.get_node_or_null("OutcomesSection/Margin/Content/Body")
			var progress_summary_body := progress_tab.get_node_or_null("ProgressSummary/Margin/Content/Body")
			var operations_next_cost := operations_tab.get_node_or_null("StartupFinances/Margin/Content/Metrics/NextCostValue")
			var progress_next_cost := progress_tab.get_node_or_null("NextCostPanel/Margin/Content/Value")
			if operations_next_cost != null:
				failures.append("Operations tab should no longer keep the next cost readout in the startup finances grid.")
			if progress_next_cost == null:
				failures.append("Progress tab should now include the next cost readout.")
			if not available_contracts_body is Label or not queue_contracts_body is Label or not active_contracts_body is Label or not outcomes_contracts_body is Label:
				failures.append("Contracts tab should expose separate bodies for leads, queue, active hosting, and outcomes.")
			else:
				var contracts_panel_text := "\n".join([
					available_contracts_body.text,
					queue_contracts_body.text,
					active_contracts_body.text,
					outcomes_contracts_body.text,
				])
				if not available_contracts_body.text.contains("Available leads:"):
					failures.append("Contracts tab available section should summarize the live lead pipeline.")
				if not available_contracts_body.text.contains("Locked leads:"):
					failures.append("Contracts tab available section should include locked lead context for future tiers.")
				if not available_contracts_body.text.contains("Board focus:"):
					failures.append("Contracts tab available section should surface the current board focus without opening the station panel.")
				if not available_contracts_body.text.contains("Level-two readiness:"):
					failures.append("Contracts tab available section should surface level-two mitigation readiness before tougher leads are accepted.")
				if not available_contracts_body.text.contains("Backup checks:") or not available_contracts_body.text.contains("After-hours requests:") or not available_contracts_body.text.contains("Traffic-spike issues:"):
					failures.append("Contracts tab available section should spell out readiness for the main level-two support issues.")
				if not contracts_panel_text.contains("Risk:"):
					failures.append("Contracts tab should show visible risk tags for starter contract variety.")
				if not contracts_panel_text.contains("Support:"):
					failures.append("Contracts tab should show visible support tags for starter contract variety.")
				if not queue_contracts_body.text.contains("Accepted queue:"):
					failures.append("Contracts tab queue section should summarize accepted work waiting for hosting.")
				if not active_contracts_body.text.contains("Active hosting:"):
					failures.append("Contracts tab active section should summarize live hosted contracts.")
				if not active_contracts_body.text.contains("Support incidents:"):
					failures.append("Contracts tab active section should surface live support pressure.")
				if not outcomes_contracts_body.text.contains("Completed contracts:"):
					failures.append("Contracts tab outcomes section should summarize completed and cancelled work.")
			if not progress_summary_body is Label:
				failures.append("Progress tab should include a live summary body.")
			elif not progress_summary_body.text.contains("Completed contracts:"):
				failures.append("Progress tab should summarize live progression instead of placeholder copy.")
			elif not progress_summary_body.text.contains("Goal:") and not progress_summary_body.text.contains("online"):
				failures.append("Progress tab should call out the current upgrade goal or unlocked infrastructure status.")
			elif not progress_summary_body.text.contains("Contract unlock track:"):
				failures.append("Progress tab should explain the current contract unlock milestone.")
			elif not progress_summary_body.text.contains("Hardware stage:"):
				failures.append("Progress tab should surface the current hardware stage in the docked summary.")
			elif not progress_summary_body.text.contains("Support pressure:"):
				failures.append("Progress tab should surface current support pressure in the docked summary.")
			elif not progress_summary_body.text.contains("Level-two readiness:"):
				failures.append("Progress tab should surface level-two mitigation readiness alongside infrastructure progress.")
			elif not progress_summary_body.text.contains("Backup checks:") or not progress_summary_body.text.contains("After-hours requests:") or not progress_summary_body.text.contains("Traffic-spike issues:"):
				failures.append("Progress tab should spell out readiness for the main level-two support issues.")

			if queue_contracts_body is Label:
				var before_queue_text: String = queue_contracts_body.text
				var accept_result: Dictionary = main_scene.accept_selected_contract()
				if bool(accept_result.get("ok", false)):
					var accepted_queue_text: String = queue_contracts_body.text
					if accepted_queue_text == before_queue_text:
						failures.append("Contracts tab queue section should refresh after contracts move from the board into the accepted queue.")
					main_scene.activate_selected_contract()
					if queue_contracts_body.text == accepted_queue_text:
						failures.append("Contracts tab queue section should refresh again after accepted work is activated.")
			contracts_button.pressed.emit()
			if not contracts_tab.visible or operations_tab.visible:
				failures.append("Pressing the Contracts tab should switch the main view to Contracts.")
			progress_button.pressed.emit()
			if not progress_tab.visible or contracts_tab.visible:
				failures.append("Pressing the Progress tab should switch the main view to Progress.")
			elif progress_next_cost is Label and progress_next_cost.text.strip_edges() == "":
				failures.append("Progress tab next cost readout should stay populated.")
			elif progress_next_cost is Label and not progress_next_cost.text.contains("purchase"):
				failures.append("Progress tab next cost readout should explain the next planned infrastructure purchase.")
			operations_button.pressed.emit()
			if not operations_tab.visible or progress_tab.visible:
				failures.append("Pressing the Operations tab should restore the default management view.")

	var collapse_button := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content/HeaderRow/Collapse")
	var collapsed_strip := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/CollapsedStrip")
	var log_content := main_scene.get_node_or_null("UI/LayoutRoot/Margin/TopRow/LogPanel/Margin/Content")
	if collapse_button == null or collapsed_strip == null or log_content == null:
		failures.append("Collapsible log shell should expose both the collapse control and collapsed strip.")
	else:
		collapse_button.pressed.emit()
		if not collapsed_strip.visible or log_content.visible:
			failures.append("Collapsing the log should hide the full content and reveal the strip.")
		collapsed_strip.pressed.emit()
		if not log_content.visible or collapsed_strip.visible:
			failures.append("Pressing the collapsed strip should reopen the full log.")

	var station_panel := main_scene.get_node_or_null("UI/StationPanel")
	if station_panel == null:
		failures.append("Workstation interactions should still use a dedicated station panel.")
	elif station_panel is Control and main_panel is Control:
		var main_shell := main_panel as Control
		var station_control := station_panel as Control
		if station_control.modulate != shell_tint:
			failures.append("Station panel should match the retro panel tint of the rest of the shell.")
		if station_control.anchor_left != 1.0 or station_control.anchor_right != 1.0:
			failures.append("Station panel should be anchored to the top-right side of the playable area.")
		if station_control.offset_right != -16.0:
			failures.append("Station panel should sit flush to the right side of the playable area.")
		if station_control.offset_top != 16.0:
			failures.append("Station panel should stay aligned to the top dock edge.")
		var station_close := station_panel.get_node_or_null("Margin/Content/HeaderRow/Close")
		if station_close == null:
			failures.append("Station panel should include a close button like the other management panels.")
		else:
			var board := main_scene.get_node_or_null("Workstations/ContractsBoard")
			if board != null:
				main_scene.show_station_panel(board)
				if absf(station_control.custom_minimum_size.y - main_shell.get_combined_minimum_size().y) > 1.0:
					failures.append("Station panel should match the vertical length of the operations shell.")
				station_close.pressed.emit()
				if station_panel.visible:
					failures.append("Station panel close button should dismiss the workstation panel.")

	var contracts_board_panel := main_scene.get_node_or_null("UI/ContractsBoardPanel")
	var interaction_controller := main_scene.get_node_or_null("InteractionController")
	if contracts_board_panel == null:
		failures.append("Contracts board interactions should expose a dedicated contracts board panel.")
	elif not contracts_board_panel is Control:
		failures.append("Contracts board panel should remain a real UI control.")
	else:
		var board_panel_control := contracts_board_panel as Control
		var board_list := contracts_board_panel.get_node_or_null("Margin/Content/ContractListSection/Scroll/ContractList")
		var board_details := contracts_board_panel.get_node_or_null("Margin/Content/DetailCard/Margin/Details")
		var board_close := contracts_board_panel.get_node_or_null("Margin/Content/HeaderRow/CloseButton")
		var board_accept := contracts_board_panel.get_node_or_null("Margin/Content/ActionRow/AcceptButton")
		if board_panel_control.modulate != shell_tint:
			failures.append("Contracts board panel should share the same retro shell tint as the rest of the management UI.")
		if board_list == null or board_details == null or board_close == null or board_accept == null:
			failures.append("Contracts board panel should expose a contract list, detail body, accept action, and close button.")
		elif board_list is ItemList and board_details is Label and board_close is Button and board_accept is Button:
			var board := main_scene.get_node_or_null("Workstations/ContractsBoard")
			if board != null and interaction_controller != null and interaction_controller.has_method("show_contracts_board"):
				main_scene.show_station_panel(board)
				interaction_controller.call("show_contracts_board")
				if not board_panel_control.visible:
					failures.append("Interacting with the contracts board should show the contracts board UI.")
				if station_panel.visible:
					failures.append("Contracts board UI should replace the overlapping station panel when opened from board interaction.")
			var panel_available_contracts: Array = main_scene.get_available_contracts()
			var item_list := board_list as ItemList
			if item_list.get_item_count() != panel_available_contracts.size():
				failures.append("Contracts board panel should list the live available contracts, not a stale data snapshot.")
			elif panel_available_contracts.size() > 1 and contracts_board_panel.has_method("select_contract_at_index"):
				var selected_contract: Dictionary = panel_available_contracts[1]
				contracts_board_panel.call("select_contract_at_index", 1)
				var details_label := board_details as Label
				if not details_label.text.contains("Monthly revenue: $"):
					failures.append("Contracts board panel details should show the selected contract monthly revenue clearly.")
				if not details_label.text.contains("Duration:"):
					failures.append("Contracts board panel details should show the selected contract duration clearly.")
				if not details_label.text.contains("Support requirement:"):
					failures.append("Contracts board panel details should show the selected contract support requirements clearly.")
				(board_accept as Button).pressed.emit()
				var pending_contracts: Array = main_scene.get_pending_contracts()
				var accepted_from_board := false
				for pending_contract in pending_contracts:
					if String((pending_contract as Dictionary).get("id", "")) == String(selected_contract.get("id", "")):
						accepted_from_board = true
						break
				if not accepted_from_board:
					failures.append("Selecting a contract from the contracts board panel should drive the live board acceptance flow.")
			(board_close as Button).pressed.emit()
			if board_panel_control.visible:
				failures.append("Contracts board panel close button should dismiss the board UI.")

	if failures.is_empty():
		print("PASS: UI shell and gameplay loop checks passed.")
		quit(0)
		return

	push_error("FAIL: %s" % " ".join(failures))
	quit(1)
