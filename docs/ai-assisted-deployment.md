# Deploying AI-Built Apps with DeploymentKit

AI coding agents can produce a working .NET application in minutes. Production infrastructure is where most AI-assisted projects stall: resource graphs, naming rules, secrets, and release safety are exactly the areas where generated code is confidently wrong and where mistakes are expensive.

This document describes a workflow in which an agent builds the application and DeploymentKit owns the deployment — with guardrails that keep the agent productive and the Azure subscription safe.

## The workflow

```text
1. Agent writes the application            (Claude Code, any coding agent)
2. Agent wires the deployment              (fluent builder, ~10 lines)
3. Validation runs                         (before any Azure resource exists)
4. Deployment runs                         (Pulumi, idempotent)
5. Outputs feed back to the agent          (URLs, hosts, resource names)
6. Releases are green-blue and reversible  (health-gated switching)
```

### Step 1 — Agent invokes the builder

Point the agent at the application repository and give it a deployment task. A prompt that works well:

```text
Add production deployment for this API using DeploymentKit.

Requirements:
- Use InfrastructureDeployer.DeployAsync with the fluent builder.
- Set: name, environment, location, naming prefix, subscription id.
- Resources: networking, key vault from .env.production (applyToContainerApps: true),
  PostgreSQL, Redis, container registry, container apps, insights.
- Keep all secrets in .env.production. Never inline secrets in code.
- Set ValidationMode.Full and treat validation failure as a build failure.
- Add a short comment linking to docs/ai-assisted-deployment.md.
```

The agent only needs to learn the fluent builder — a deliberately small API surface (`SetName`, `SetEnvironment`, `AddDatabase`, `AddContainerApps`, ...) that fits in a single prompt context.

### Step 2 — Guardrails run before Azure does

`ValidationOrchestratorService` and the validators in `Validators/` execute before Pulumi touches the subscription:

| Guardrail | Failure it prevents |
|-----------|--------------------|
| Settings validation | Missing/contradictory configuration produced by incorrect assumptions |
| Naming validation | Resource names that violate Azure constraints or collide with existing resources |
| Subscription / resource group validation | Deploying into the wrong subscription or a resource group that does not exist |
| Azure state validation | Deploying on top of drifted or half-provisioned infrastructure |
| Drift detection | Silent divergence between Pulumi state and real Azure resources |

A validation failure is the infrastructure equivalent of a failing compile: the agent gets a clear error and can fix the configuration before anything is created.

### Step 3 — Secrets are handled by construction

The most common vibe-coding failure is a credential that ends up in source control or in deployment logs. DeploymentKit removes the opportunity:

- `AddKeyVault(".env.production", applyToContainerApps: true)` parses a `.env` file and stores each entry in Azure Key Vault, then injects them into Container Apps as environment variables.
- `SensitiveConfigurationKeyClassifier` detects password/secret/token/API-key/connection-string keys and marks them as Pulumi secrets, so they are redacted from logs and state.
- Nothing in the library requires a secret literal in source code.

**Rule for agents:** `.env` files are read locally and never committed. Add `.env` to `.gitignore` (this repository already ignores it).

### Step 4 — Deployments are idempotent and self-healing

Pulumi deployments are declarative, so re-running the same deployment converges instead of duplicating. When reality drifts (a resource was changed in the portal, state is stale), `StateDriftRecoveryService` detects the state-drift error, runs recovery including `pulumi refresh`, and retries once.

For agents this matters because retry storms are replaced by a single, well-understood recovery path.

### Step 5 — Outputs close the loop

`DeployAsync` returns `InfrastructureDeploymentOutputs` with strongly typed `Output<T>` values — the API URL, PostgreSQL host, Redis host, Key Vault URI, ACR login server, and nested per-service outputs. Agents can feed these directly into the next task (wiring the frontend, running migrations, smoke tests) without scraping the Azure portal.

### Step 6 — Release changes safely

`EnableGreenBlueDeployment()` provisions paired slots and moves traffic only when health checks pass. Rollback is a slot swap. This lets an agent-driven workflow ship frequently without making every deploy a bet.

## Safe defaults for AI-assisted projects

| Setting | Recommendation | Why |
|---------|----------------|-----|
| `ValidationMode` | `Full` for `prod`, `Basic` elsewhere | Never let a typo reach a paid subscription |
| Green-blue deployment | Enable for user-facing APIs | Health-gated switch + instant rollback |
| Database SKU / storage | Start with the smallest SKU, scale deliberately | Agents tend to copy oversized defaults |
| `SkipAzureAuthValidation` | Only when using Azure CLI auth locally | Keep credential validation in CI |
| `.env` files | Git-ignored, local only | The one secret rule that is never negotiable |

## Agent instructions in this repository

The repository ships instructions for coding agents so they work predictably in this codebase:

- [`AGENTS.md`](../AGENTS.md) — canonical repository instructions (workflow, coding standards, verification).
- [`CLAUDE.md`](../CLAUDE.md) — pointer for Claude Code.
- [`GEMINI.md`](../GEMINI.md) — pointer for Gemini.

## Example: an agent task end-to-end

```csharp
// Program.cs / Deployment/Program.cs in the application repository
using DeploymentKit.Deployer;
using DeploymentKit.Enums;
using DeploymentKit.Models.Outputs;

InfrastructureDeploymentOutputs outputs = await InfrastructureDeployer.DeployAsync(builder => builder
    .SetName("myapp")
    .SetEnvironment("prod")
    .SetLocation("westeurope")
    .SetNamingPrefix("myapp")
    .AddNetworking()
    .AddKeyVault(".env.production", applyToContainerApps: true)
    .AddDatabase()
    .AddRedis()
    .AddContainerRegistry()
    .AddContainerApps()
    .AddInsights()
    .EnableGreenBlueDeployment()
    .SetValidationMode(ValidationMode.Full));

// outputs.ApiUrl, outputs.PostgresHost, outputs.KeyVault, ... are available to the next agent step.
```

Verification for the agent: `dotnet build` must succeed, and the deployment must pass `ValidationMode.Full` before Azure resources are created. There is no "deploy and hope" path.
