COMPOSE  := docker compose
PROFILES := --profile app --profile sonar
API      := src/PersonalData.Api

.DEFAULT_GOAL := help
.PHONY: help dev app db sonar test e2e api-test ci logs down clean

help: ## List available targets
	@awk 'BEGIN {FS = ":.*## "} /^[a-z0-9-]+:.*## / {printf "  make %-8s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

dev: db ## MySQL in Docker + API natively with hot reload on http://localhost:5277
	dotnet watch --project $(API)

app: ## MySQL + API in Docker on http://localhost:8080 (no .NET SDK needed)
	$(COMPOSE) --profile app up -d --build --wait
	@echo API: http://localhost:8080

db: ## Start MySQL on localhost:3307 and wait until the data is imported
	$(COMPOSE) up -d --wait db

sonar: ## Start SonarQube on http://localhost:9000, no login needed (first start takes a few minutes)
	$(COMPOSE) --profile sonar up -d --wait sonarqube
	@# Dashboard readable without login, so the default admin/admin (used by scripts/ci.sh) never has to change.
	@curl -sf -u admin:admin -X POST http://localhost:9000/api/settings/set -d key=sonar.forceAuthentication -d value=false \
		|| echo "Could not log in as admin/admin; if the admin password was changed, 'make clean' resets SonarQube."
	@echo SonarQube: http://localhost:9000

test: ## Run all test projects (needs `make app` running: the E2E tests use http://localhost:8080)
	dotnet test

e2e: app ## Run only the Playwright E2E tests against the app in Docker (starts it if needed)
	dotnet test tests/PersonalData.E2ETests

api-test: ## Run the Postman collection with Newman against the API in Docker (starts it if needed)
	$(COMPOSE) --profile app run --rm newman

ci: ## Local CI: stack up, build + all tests + SonarQube analysis, API tests (needs `make sonar`)
	./scripts/ci.sh

logs: ## Follow logs of all running containers
	$(COMPOSE) $(PROFILES) logs -f

down: ## Stop all containers, keep data
	$(COMPOSE) $(PROFILES) down

clean: ## Stop all containers and delete volumes (DB is re-imported on next start)
	$(COMPOSE) $(PROFILES) down -v
