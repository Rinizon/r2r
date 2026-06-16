extends Node

## Contract data manager - loads and exposes contracts from starter_contracts.json

const DEFAULT_CONTRACTS_PATH := "res://data/contracts/starter_contracts.json"

@export_file("*.json") var contracts_data_path: String = DEFAULT_CONTRACTS_PATH

var contracts: Array = []

func _ready() -> void:
	load_contracts()

func load_contracts() -> void:
	contracts = load_contracts_from_path(contracts_data_path)

func load_contracts_from_path(file_path: String) -> Array:
	if not ResourceLoader.exists(file_path):
		push_error("Contract file not found: %s" % file_path)
		return []

	var json_string := FileAccess.get_file_as_string(file_path)
	if json_string.is_empty():
		push_error("Contract file is empty: %s" % file_path)
		return []

	var json := JSON.new()
	json.parse(json_string)
	if json.get_error_line() > 0:
		push_error("JSON parse error at line %d: %s" % [json.get_error_line(), json.get_error_message()])
		return []

	var data = json.get_data()
	if not data is Array:
		push_error("Contract file root is not an array")
		return []

	var loaded_contracts := _shape_contracts(data as Array)
	print("Loaded %d contracts from %s" % [loaded_contracts.size(), file_path])
	for contract in loaded_contracts:
		print("  - %s [%s]: $%d/mo, %d months" % [
			contract.get("name", "Unknown"),
			contract.get("id", "unknown"),
			contract.get("monthly_revenue_value", 0),
			contract.get("duration_months", 0),
		])
	return loaded_contracts

func get_contracts() -> Array:
	"""Return all loaded contracts"""
	if contracts.is_empty():
		load_contracts()
	return contracts.duplicate(true)

func get_contract_by_name(name: String) -> Dictionary:
	"""Find a contract by name; returns empty dict if not found"""
	if contracts.is_empty():
		load_contracts()
	for contract in contracts:
		if contract is Dictionary and contract.get("name") == name:
			return contract.duplicate(true)
	return {}

func get_contract_by_id(contract_id: String) -> Dictionary:
	if contracts.is_empty():
		load_contracts()
	for contract in contracts:
		if contract is Dictionary and String(contract.get("id", "")) == contract_id:
			return contract.duplicate(true)
	return {}

func get_contract_count() -> int:
	"""Return total number of contracts"""
	if contracts.is_empty():
		load_contracts()
	return contracts.size()

func _shape_contracts(source_contracts: Array) -> Array:
	var shaped_contracts: Array = []
	var used_ids := {}

	for index in range(source_contracts.size()):
		var raw_contract: Variant = source_contracts[index]
		if not raw_contract is Dictionary:
			continue

		var contract: Dictionary = _normalize_contract(raw_contract as Dictionary, index, used_ids)
		shaped_contracts.append(contract)

	return shaped_contracts

func _normalize_contract(raw_contract: Dictionary, index: int, used_ids: Dictionary) -> Dictionary:
	var name := str(raw_contract.get("name", "Contract %d" % (index + 1))).strip_edges()
	if name == "":
		name = "Contract %d" % (index + 1)

	var requested_id := str(raw_contract.get("id", "")).strip_edges()
	var contract_id := _make_unique_contract_id(requested_id if requested_id != "" else _slugify_name(name), index, used_ids)
	var monthly_revenue_value := int(raw_contract.get("monthly_revenue_value", 0))
	var monthly_revenue_text := str(raw_contract.get("monthly_revenue", "")).strip_edges()
	if monthly_revenue_text == "":
		monthly_revenue_text = "$%d" % monthly_revenue_value

	var resources := raw_contract.get("resources", {}) as Dictionary
	var normalized_resources := {
		"cpu": int(resources.get("cpu", 0)),
		"ram_gb": int(resources.get("ram_gb", 0)),
		"storage_gb": int(resources.get("storage_gb", 0)),
		"bandwidth_mbps": int(resources.get("bandwidth_mbps", 0)),
	}

	var contract := raw_contract.duplicate(true)
	contract["id"] = contract_id
	contract["name"] = name
	contract["monthly_revenue_value"] = monthly_revenue_value
	contract["monthly_revenue"] = monthly_revenue_text
	contract["duration_months"] = int(raw_contract.get("duration_months", 1))
	contract["risk_tag"] = str(raw_contract.get("risk_tag", "Standard")).strip_edges()
	contract["support_tag"] = str(raw_contract.get("support_tag", "Standard")).strip_edges()
	contract["tier"] = int(raw_contract.get("tier", 1))
	contract["unlock_requirements"] = (raw_contract.get("unlock_requirements", {}) as Dictionary).duplicate(true)
	contract["resources"] = normalized_resources
	return contract

func _make_unique_contract_id(base_id: String, index: int, used_ids: Dictionary) -> String:
	var candidate := base_id if base_id != "" else "contract_%d" % index
	if not used_ids.has(candidate):
		used_ids[candidate] = true
		return candidate

	var suffix := index + 1
	while used_ids.has("%s_%d" % [candidate, suffix]):
		suffix += 1

	var unique_id := "%s_%d" % [candidate, suffix]
	used_ids[unique_id] = true
	return unique_id

func _slugify_name(value: String) -> String:
	var slug := ""
	var previous_was_separator := false

	for character in value.to_lower():
		var is_alphanumeric := (character >= "a" and character <= "z") or (character >= "0" and character <= "9")
		if is_alphanumeric:
			slug += character
			previous_was_separator = false
		elif not previous_was_separator:
			slug += "_"
			previous_was_separator = true

	slug = slug.strip_edges()
	slug = slug.trim_suffix("_")
	slug = slug.trim_prefix("_")
	return slug if slug != "" else "contract"
