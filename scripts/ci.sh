#!/usr/bin/env bash
# Local CI pipeline (`make ci`). Same steps the GitHub Actions workflow will run.
# Needs SonarQube running (`make sonar`). Shows the important lines of each step; the full output is in $LOGS.
set -euo pipefail

SONAR_HOST_URL=http://localhost:9000
SONAR_PROJECT=personal-test-data-generator
LOGS=TestResults/logs

rm -rf TestResults
mkdir -p "$LOGS"

# run <name> <filter> <command...>: full output to $LOGS/<name>.log, the lines <filter> picks on screen.
# On failure it also shows the end of the log if the filter found nothing, then stops.
run() {
  local log="$LOGS/$1.log" filter=$2 status=0 shown
  shift 2
  "$@" >"$log" 2>&1 || status=$?
  shown=$("$filter" <"$log" || true)
  [[ -z $shown ]] || echo "$shown"
  if (( status != 0 )); then
    [[ -n $shown ]] || tail -n 20 "$log"
    echo "FAILED, full output: $log"
    exit "$status"
  fi
}

# Docker: image built, containers healthy.
stack_lines() { grep -E ' (Built|Healthy) *$' | awk '!seen[$0]++'; }

# MSBuild: one line per project, each warning and error once, totals. Paths relative to the repo.
build_lines() {
  grep -E ' -> |: (warning|error) |^Build (succeeded|FAILED)|[0-9]+ (Warning|Error)\(s\)' \
    | sed -E -e "s|$PWD/||g" -e 's| \[[^]]*\]$||' | awk '!seen[$0]++'
}

# VSTest: the result of each test project, and the error message (without stack trace) of every failed test.
test_lines() {
  awk '
    /^  Failed / { failed = 1 }
    /^  Stack Trace:/ { failed = 0 }
    failed || /^(Passed|Failed)!/ { print; next }
    /No test is available in/ { sub(/\.dll\..*/, ""); sub(/.*\//, ""); print "No tests in " $0 }'
}

# SonarScanner: warnings, errors, the coverage import and the result line, without timestamps.
sonar_lines() {
  grep -E 'WARN|ERROR|Coverage Report Statistics|ANALYSIS SUCCESSFUL' \
    | sed -E -e 's/^[0-9:.]+ +//' -e 's/^INFO: //' -e 's/WARNING: WARN: /WARNING: /'
}

# Newman: every request with its status, failed assertions and the summary table, not the ✓ of each passed assertion.
newman_lines() { grep -vE '^ *✓ |^ Container '; }

# Waits until SonarQube has processed the uploaded report, then prints the quality gate result and coverage.
quality_gate() {
  local ce_url status gate coverage
  ce_url=$(sed -n 's/^ceTaskUrl=//p' .sonarqube/out/.sonar/report-task.txt)
  for _ in {1..60}; do
    status=$(curl -s -u admin:admin "$ce_url" | sed -E 's/.*"status":"([A-Z_]+)".*/\1/')
    [[ $status == PENDING || $status == IN_PROGRESS ]] || break
    sleep 1
  done
  if [[ $status != SUCCESS ]]; then
    echo "SonarQube did not finish processing the report (status: $status)"
    return
  fi
  gate=$(curl -s -u admin:admin "$SONAR_HOST_URL/api/qualitygates/project_status?projectKey=$SONAR_PROJECT")
  coverage=$(curl -s -u admin:admin "$SONAR_HOST_URL/api/measures/component?component=$SONAR_PROJECT&metricKeys=coverage" \
    | sed -E 's/.*"value":"([^"]+)".*/\1/')
  case $(sed -E 's/^\{"projectStatus":\{"status":"([A-Z]+)".*/\1/' <<<"$gate") in
    OK) echo "Quality gate: passed, coverage: $coverage %" ;;
    ERROR)
      echo "Quality gate: FAILED, coverage: $coverage %"
      grep -oE '\{"status":"ERROR","metricKey":"[^}]*\}' <<<"$gate" \
        | sed -E 's/.*"metricKey":"([^"]+)".*"errorThreshold":"([^"]+)","actualValue":"([^"]+)".*/  \1: \3 (limit \2)/' ;;
    *) echo "Quality gate: none, coverage: $coverage %" ;;
  esac
}

# SonarQube only accepts analysis from a logged-in user. Everything runs locally, so log in with the default
# admin/admin and create a fresh analysis token for this run. `make sonar` opens the dashboard without login,
# so nobody is ever forced to change that password.
curl -s -u admin:admin -X POST "$SONAR_HOST_URL/api/user_tokens/revoke" -d name=ci >/dev/null || true
SONAR_TOKEN=$(curl -sf -u admin:admin -X POST "$SONAR_HOST_URL/api/user_tokens/generate" -d name=ci -d type=GLOBAL_ANALYSIS_TOKEN \
  | sed -E 's/.*"token":"([^"]+)".*/\1/') \
  || { echo "Cannot log in to SonarQube at $SONAR_HOST_URL as admin/admin. Is it running (make sonar)?" >&2; exit 1; }

# Stop the app stack however the script exits; volumes (DB data) are kept.
stop() {
  echo "==> Stop MySQL + API"
  docker compose --profile app down >"$LOGS/down.log" 2>&1 || true
  grep -E ' Removed *$' "$LOGS/down.log" || true
  echo "Full logs: $LOGS/"
}
trap stop EXIT

build() {
  dotnet tool restore &&
    dotnet dotnet-sonarscanner begin \
      /k:"$SONAR_PROJECT" \
      /d:sonar.host.url="$SONAR_HOST_URL" \
      /d:sonar.token="$SONAR_TOKEN" \
      /d:sonar.coverage.exclusions="**/wwwroot/**" \
      /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml" &&
    dotnet build
}

echo "==> Start MySQL + API"
run stack stack_lines docker compose --profile app up -d --build --wait

echo "==> Build"
run build build_lines build

echo "==> Tests"
run test test_lines dotnet test --no-build --results-directory TestResults --collect "XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

echo "==> SonarQube analysis"
run sonar sonar_lines dotnet dotnet-sonarscanner end /d:sonar.token="$SONAR_TOKEN"
quality_gate

echo "==> API tests (Newman)"
run newman newman_lines docker compose --profile app run --rm -T newman
