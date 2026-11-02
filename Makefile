COMPOSE  := docker compose
PROFILES := --profile app --profile sonar
API      := src/PersonalData.Api

.DEFAULT_GOAL := help
.PHONY: help dev app db sonar test logs down clean

help: ## List available targets
	@awk 'BEGIN {FS = ":.*## "} /^[a-z]+:.*## / {printf "  make %-6s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

dev: db ## MySQL in Docker + API natively with hot reload on http://localhost:5277
	dotnet watch --project $(API)

app: ## MySQL + API in Docker on http://localhost:8080 (no .NET SDK needed)
	$(COMPOSE) --profile app up -d --build --wait
	@echo API: http://localhost:8080

db: ## Start MySQL on localhost:3306 and wait until the data is imported
	$(COMPOSE) up -d --wait db

sonar: ## Start SonarQube on http://localhost:9000 (first start takes a few minutes)
	$(COMPOSE) --profile sonar up -d --wait sonarqube
	@echo SonarQube: http://localhost:9000

test: ## Run all test projects (run `make db` first once tests need MySQL)
	dotnet test

logs: ## Follow logs of all running containers
	$(COMPOSE) $(PROFILES) logs -f

down: ## Stop all containers, keep data
	$(COMPOSE) $(PROFILES) down

clean: ## Stop all containers and delete volumes (DB is re-imported on next start)
	$(COMPOSE) $(PROFILES) down -v
