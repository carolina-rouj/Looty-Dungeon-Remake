#!/usr/bin/env bash
set -euo pipefail

UNITY_BIN="${UNITY_BIN:-/home/maziu/Unity/Hub/Editor/6000.4.4f1/Editor/Unity}"
PROJECT_PATH="${PROJECT_PATH:-/home/maziu/uni/VJ-3D/repo}"
LOG_PREFIX="${LOG_PREFIX:-/tmp/vj-pre-merge}"
LOG_ERROR_PATTERN='error CS[0-9]+|MissingComponentException|NullReferenceException|MissingReferenceException'

check_log() {
  local log_file="$1"
  if grep -nE "${LOG_ERROR_PATTERN}" "${log_file}"; then
    echo "[validation] ERROR markers found in ${log_file}" >&2
    return 1
  fi
}

run_editor_quit() {
  local method="$1"
  local log_file="$2"
  echo "[validation] ${method} -> ${log_file}"
  "${UNITY_BIN}" -quit -batchmode -projectPath "${PROJECT_PATH}" -executeMethod "${method}" -logFile "${log_file}"
  check_log "${log_file}"
}

run_editor_playmode() {
  local method="$1"
  local log_file="$2"
  echo "[validation] ${method} -> ${log_file}"
  "${UNITY_BIN}" -batchmode -projectPath "${PROJECT_PATH}" -executeMethod "${method}" -logFile "${log_file}"
  check_log "${log_file}"
}

cd "${PROJECT_PATH}"

echo "[validation] git diff --check"
git diff --check

run_editor_quit DungeonBatchValidator.Run "${LOG_PREFIX}-validator.log"
run_editor_quit DungeonDeliveryValidator.Run "${LOG_PREFIX}-delivery.log"
run_editor_playmode DungeonRequirementsSmoke.Run "${LOG_PREFIX}-requirements.log"
run_editor_playmode DungeonMenuSmoke.Run "${LOG_PREFIX}-menu.log"
run_editor_playmode DungeonPauseSmoke.Run "${LOG_PREFIX}-pause.log"
run_editor_playmode DungeonPlaySmoke.Run "${LOG_PREFIX}-play.log"
run_editor_playmode DungeonAllLevelsSmoke.Run "${LOG_PREFIX}-all-levels.log"
run_editor_playmode DungeonScreenshotCapture.Run "${LOG_PREFIX}-screenshot.log"
run_editor_quit DungeonBuildSmoke.Run "${LOG_PREFIX}-build.log"

echo "[validation] OK"
