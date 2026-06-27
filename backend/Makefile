SOLUTION := NewsletterPlatform.sln
API_PROJECT := src/NewsletterPlatform.Api/NewsletterPlatform.Api.csproj
INFRA_PROJECT := src/NewsletterPlatform.Infrastructure/NewsletterPlatform.Infrastructure.csproj
MIGRATIONS_DIR := Persistence/Migrations
CONFIGURATION ?= Debug
MIGRATION ?= ChangeName
PORT ?= 5120
URLS ?= http://localhost:$(PORT)

.PHONY: restore build test run watch migrate-add migrate-update clean

restore:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION) --no-restore -m:1 -c $(CONFIGURATION)

test:
	dotnet test $(SOLUTION) --no-restore -m:1 -c $(CONFIGURATION)

run:
	dotnet build $(SOLUTION) --no-restore -m:1 -c $(CONFIGURATION)
	ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=$(URLS) dotnet run --no-launch-profile --project $(API_PROJECT) -c $(CONFIGURATION) --no-build

watch:
	ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=$(URLS) dotnet watch --project $(API_PROJECT) run -c $(CONFIGURATION)

migrate-add:
	dotnet build $(SOLUTION) --no-restore -m:1 -c $(CONFIGURATION)
	dotnet ef migrations add $(MIGRATION) --no-build --project $(INFRA_PROJECT) --startup-project $(API_PROJECT) --output-dir $(MIGRATIONS_DIR)

migrate-update:
	dotnet build $(SOLUTION) --no-restore -m:1 -c $(CONFIGURATION)
	dotnet ef database update --no-build --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

clean:
	dotnet clean $(SOLUTION)
