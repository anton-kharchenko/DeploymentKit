# DeploymentKit

[![build](https://github.com/anton-kharchenko/DeploymentKit/actions/workflows/build.yml/badge.svg)](https://github.com/anton-kharchenko/DeploymentKit/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/DeploymentKit.svg?label=NuGet)](https://www.nuget.org/packages/DeploymentKit)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-11.0-512BD4)](https://dotnet.microsoft.com)

DeploymentKit is an opinionated, validation-first infrastructure-as-code library that provisions production-grade Azure environments with **Pulumi** behind a single fluent entry point: PostgreSQL, Redis, Container Apps, Key Vault, networking, monitoring, green-blue releases, and more. It is designed to be driven safely by humans **and** AI coding agents.

---

## Why DeploymentKit

Getting a .NET application to production on Azure normally means hand-wiring a VNet, subnets, private endpoints, a database, a cache, a registry, a container environment, secrets, certificates, and monitoring across hundreds of lines of infrastructure code. Every one of those resources has its own naming rules, SKU constraints, and failure modes.

DeploymentKit collapses that work into one fluent builder and a validation pipeline that runs **before** anything is created in Azure:

- **One entry point.** `InfrastructureDeployer.DeployAsync(...)` plus `AddNetworking()`, `AddDatabase()`, `AddContainerApps()` — a small API surface that is easy to review and hard to misuse.
- **Fail fast.** Settings, naming, subscription, and Azure-state validation run before Pulumi touches your subscription.
- **Secrets stay secret.** `.env` files are routed into Key Vault, and sensitive configuration keys are automatically marked as Pulumi secrets.
- **Self-healing deployments.** State drift is detected between Pulumi state and real Azure resources, and is recovered automatically where possible.
- **Safe releases.** Green-blue container app deployments with traffic splitting, health-gated switching, and rollback.

## Features

| Area | Capabilities |
|------|--------------|
| **Compute** | Azure Container Apps (API + jobs), Azure Container Registry, container ingress, green-blue slots with traffic management |
| **Data** | PostgreSQL Flexible Server, Azure Managed Redis (with legacy Azure Cache for Redis support), Cosmos DB, Storage accounts, Blob Storage, Table Storage, Event Hubs |
| **Migrations** | EF Core migrations, FluentMigrator, SQL scripts — optionally executed during deployment |
| **Security** | Key Vault with RBAC, `.env`-to-Key-Vault secrets bridge, secret classification for Pulumi, managed certificates, Key Vault or uploaded certificates, private endpoints |
| **Networking** | VNet and subnets, private endpoints, P2S VPN, Application Gateway, DNS records, custom domains, Azure Front Door with optional WAF, CDN/Traffic Manager domain optimization |
| **Observability** | Log Analytics, Application Insights, infrastructure health checks (`IHealthCheck` implementations for cache, database, storage) |
| **Operations** | Pre-deployment validation with four modes, drift detection and state recovery, resource naming registry, correlation-ID scoped logging |
| **Configuration** | Fluent builder, strongly typed `InfrastructureSettings`, `INFRA_*` environment variables, Pulumi config, Pulumi ESC, environment fallback sources |
| **Integration** | First-class DI registration (`AddInfrastructureServices()`), strongly typed `Output<T>` results for every provisioned resource |

## Quick start

### 1. Install

```bash
dotnet add package DeploymentKit
```

### 2. Configure and deploy

```csharp
using DeploymentKit.Deployer;
using DeploymentKit.Enums;
using DeploymentKit.Models.Outputs;

InfrastructureDeploymentOutputs outputs = await InfrastructureDeployer.DeployAsync(builder => builder
    .SetName("trips")
    .SetEnvironment("prod")
    .SetLocation("westeurope")
    .SetNamingPrefix("trips")
    .AddNetworking()
    .AddInsights()
    .AddKeyVault(".env.production", applyToContainerApps: true)
    .AddDatabase()
    .AddRedis()
    .AddContainerRegistry()
    .AddContainerApps()
    .AddStorage()
    .EnableGreenBlueDeployment()
    .SetValidationMode(ValidationMode.Full));
```

All builder methods are optional — the library only provisions what you add. Start with `SetName`, `SetEnvironment`, `SetLocation`, `SetNamingPrefix`, a `SetSubscriptionId` (or environment-based credentials), and the resources you need.

### 3. Read the outputs

Every deployment returns strongly typed Pulumi outputs:

| Output | Description |
|--------|-------------|
| `ApiUrl` | Public API endpoint on Container Apps |
| `ResourceGroupName` | Resource group that was created or reused |
| `AcrLoginServer` | Azure Container Registry login server |
| `PostgresHost` | PostgreSQL Flexible Server host |
| `RedisHost` | Redis host |
| `ContainerApps` | Container app names, ingress FQDNs, green-blue slot state |
| `KeyVault` | Key Vault URI and secret names |
| `Monitoring` | Log Analytics and Application Insights resources |
| `Network`, `Storage`, `EventHubs`, `CosmosDb` (when enabled) | Nested per-service outputs |

### 4. Or deploy pre-built settings

`InfrastructureSettings` values can also be produced by `builder.Build()`, loaded from Pulumi configuration, or constructed directly:

```csharp
InfrastructureSettings settings = await builder.BuildAsync();
await InfrastructureDeployer.DeployAsync(settings);
```

## Configuration

### Pulumi config with environment fallbacks

```csharp
using Pulumi;

await InfrastructureDeployer.DeployAsync(
    builder => builder
        .SetName("trips")
        .SetEnvironment("prod")
        .SetLocation("westeurope")
        .SetNamingPrefix("trips")
        .AddDatabase()
        .AddContainerApps(),
    pulumiConfig: new Config("trips-prod"),
    secretMappings: new Dictionary<string, string[]>
    {
        // Read from Pulumi config (escaped: trips-prod:database:adminPassword);
        // fall back to the INFRA_DB_ADMIN_PASSWORD environment variable when absent.
        ["database:adminPassword"] = ["INFRA_DB_ADMIN_PASSWORD"],
    });
```

Configuration is resolved from Pulumi configuration and Pulumi ESC first, with environment-variable fallbacks for every `INFRA_*` setting (see `Constants/EnvironmentVariableNames.cs` for the full list).

### Secrets

- `AddKeyVault(".env.production", applyToContainerApps: true)` parses a `.env` file and stores each entry as a Key Vault secret, optionally injecting them into Container Apps as environment variables.
- Secrets are resolved at deploy time — never commit them. `SensitiveConfigurationKeyClassifier` marks password/secret/token/API-key/connection-string values as Pulumi secrets so they are redacted from logs and state diffs.
- Exclude patterns let you keep debug/test variables out of Key Vault: `AddKeyVault(".env", excludePatterns: ["DEBUG_*", "TEMP_*"])`.

### Dependency injection

```csharp
using DeploymentKit.Extensions;

services.AddInfrastructureServices();          // full deployment stack
services.AddInfrastructureValidation();        // validators only
services.AddInfrastructureHealthChecks();      // ASP.NET Core health checks
```

## Validation modes

Validation runs before Pulumi creates any resource. Modes let you trade depth for speed:

| Mode | What runs | Typical use |
|------|-----------|-------------|
| `Full` | Settings, naming, subscription/resource group, Azure state, drift detection | Production deployments (default) |
| `Basic` | Settings and naming validation only | First deployments, restricted networks |
| `Minimal` | Required fields only | CI smoke checks |
| `Skip` | No validation | Not recommended |

## Drift detection and recovery

Azure resources are sometimes modified outside Pulumi, leaving state and reality out of sync. `DriftDetectionService` compares Pulumi state against actual Azure resources (PostgreSQL, Event Hubs, VNet, Storage, Key Vault, ACR) and reports discrepancies. When a deployment fails with a state-drift error, `StateDriftRecoveryService` automatically runs recovery (including `pulumi refresh`) and retries the deployment once — in CI this refresh happens proactively before deploying.

## Green-blue deployments

`EnableGreenBlueDeployment()` provisions paired container app slots and manages the release:

- traffic splitting between active and inactive slots,
- health-gated automatic switching (`GreenBlueHealthCheckService`),
- configurable rollback thresholds and gradual/canary traffic shifts (`TrafficManagementService`).

## Built for AI-assisted development

Vibe coding gets an application written in minutes. Taking it to production is where most AI-assisted projects stall: infrastructure, secrets, and release safety are exactly where generated code tends to be confidently wrong.

DeploymentKit is designed so that an AI agent — or a developer pairing with one — can take a working app to production with guardrails:

- **A small, learnable API surface.** An agent only needs the fluent builder (`SetName`, `AddDatabase`, `AddContainerApps`, ...) and `DeployAsync`. Correct usage fits in a few lines, which means fewer hallucinated resource graphs.
- **Validation before provisioning.** `ValidationOrchestratorService` catches bad settings, naming collisions, and inconsistent Azure state before a single resource is created — a failed `dotnet build`-equivalent for infrastructure.
- **Secrets handled by construction.** `.env` → Key Vault bridging and automatic Pulumi secret classification remove the single most common vibe-coding failure mode: credentials pasted into source or state.
- **Self-healing state.** Drift detection and automatic recovery mean an agent's deployment survives the messy real world (manually changed resources, stale state) instead of failing in a way that is hard to debug.
- **Safe releases for agent-generated changes.** Green-blue slots with health-gated switching and rollback make continuous "deploy from the agent" workflows viable.
- **An agent-ready repository.** `AGENTS.md`, `CLAUDE.md`, and `GEMINI.md` document how agents should work in this codebase; verification is a deterministic `dotnet build`.

Example: point Claude Code (or any agent) at your API repository and use a prompt like:

```text
Add production deployment for this API using DeploymentKit:
- builder flow: SetName, SetEnvironment("prod"), SetLocation, SetNamingPrefix,
  AddNetworking, AddKeyVault(".env.production", applyToContainerApps: true),
  AddDatabase, AddRedis, AddContainerRegistry, AddContainerApps
- keep secrets in .env.production, never in code
- use ValidationMode.Full and fail the build if validation fails
- leave a comment pointing to docs/ai-assisted-deployment.md
```

See [docs/ai-assisted-deployment.md](docs/ai-assisted-deployment.md) for a full walkthrough of the agent workflow.

## Repository structure

| Path | Contents |
|------|----------|
| `Deployer/` | `InfrastructureDeployer` — the public deployment facade |
| `Infrastructure/` | `InfrastructureOrchestrator` — ordered, partially parallel deployment pipeline |
| `Components/` | Pulumi `ComponentResource` wrappers for apps, databases, and networking |
| `Services/` | Azure service implementations, configuration sources, drift detection, orchestration |
| `Settings/` | Strongly typed configuration (`InfrastructureSettings` and per-service settings) |
| `Validators/` | Pre-deployment, naming, subscription, and Azure-state validators |
| `HealthChecks/` | ASP.NET Core `IHealthCheck` implementations |
| `Extensions/` | DI registration and helper extensions |
| `Constants/`, `Enums/`, `Models/`, `Utilities/`, `Helpers/` | Shared building blocks |

## Requirements

- .NET SDK 11.0 (preview)
- Pulumi CLI (authenticated via `pulumi login`)
- Azure CLI (`az login`) or Azure service principal credentials
- An Azure subscription

## Security

- Never commit secrets. Credentials are resolved from environment variables, Pulumi configuration, or Azure Key Vault at deploy time.
- Sensitive values are marked as Pulumi secrets and redacted from logs and state output.
- To report a vulnerability, follow [SECURITY.md](SECURITY.md).

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for branching, coding standards, and commit conventions. `dotnet build` must succeed before opening a pull request.

## License

MIT — see [LICENSE](LICENSE).

## Status

`1.0.0-preview.1` — the public API may change between preview releases.
