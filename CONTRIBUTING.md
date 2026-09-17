# Contributing to DeploymentKit

Thank you for contributing to DeploymentKit! This guide outlines our development workflow, coding standards, and repository conventions to help make your contribution process as smooth as possible.

---

## Branching Strategy

Our primary branch is `main`. All development should happen on short-lived feature or bugfix branches branched off `main`:

* **Feature branches**: `feat/<short-description>` (e.g., `feat/containerapp-ingress`)
* **Bugfix branches**: `fix/<short-description>` (e.g., `fix/keyvault-null-ref`)
* **Documentation branches**: `docs/<short-description>` (e.g., `docs/add-contributing-guide`)
* **Refactoring branches**: `refactor/<short-description>` (e.g., `refactor/clean-validators`)

---

## Development Workflow

1. **Clone the repository**:
   ```bash
   git clone https://github.com/anton-kharchenko/DeploymentKit.git
   cd DeploymentKit
   ```
2. **Create a branch**:
   ```bash
   git checkout -b feat/your-feature-name
   ```
3. **Write Code**: Implement your changes following the [Coding Standards](#coding-standards).
4. **Build and Validate**: Ensure the project compiles with no warnings:
   ```bash
   dotnet build
   ```
5. **Commit Changes**: Commit your changes using [Conventional Commits](#commit-message-conventions).
6. **Push and Open a PR**: Push your branch to remote and open a Pull Request against `main`.

---

## Coding Standards

### C# / .NET Guidelines
* Target Framework: `.NET 11.0`
* Indentation: Use **4 spaces** for indentation.
* Namespace Structure: Use **file-scoped namespaces** to reduce nesting (e.g., `namespace DeploymentKit.Components;`).
* Case Conventions:
  * `PascalCase` for types, methods, properties, and public fields.
  * `camelCase` for local variables and parameters.
  * `_camelCase` for private fields (with a leading underscore).
* Interfaces: Prefix interface names with `I` (e.g., `IDeploymentStatusService`).
* Braces: Always use braces `{}` for control flow blocks (`if`, `for`, `foreach`, `while`), even if they contain a single statement.
* Nullability: Enable Nullable Reference Types and handle potential null values explicitly.

### Pulumi Infrastructure Guidelines
* **Logical Isolation**: Bundle related Azure resources into a `Pulumi.ComponentResource` subclass.
* **Naming**: Use the `ResourceNamingService` for uniform resource name prefixes across environments.
* **Redact Secrets**: Ensure sensitive configuration strings (passwords, connection strings, keys) are marked as Pulumi secrets.

---

## Commit Message Conventions

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification. Each commit message must follow this structure:

```text
<type>(<scope>): <subject>
```

### Allowed Types
* **feat**: A new feature or configuration capability.
* **fix**: A bug fix.
* **docs**: Changes to documentation.
* **style**: Formatting, missing semi-colons, etc. (no business code changes).
* **refactor**: Code changes that neither fix a bug nor add a feature.
* **perf**: Code changes that improve performance.
* **test**: Adding missing tests or correcting existing tests.
* **build**: Changes that affect the build system or external dependencies.
* **ci**: Changes to CI configuration files and scripts.
* **chore**: Other changes that don't modify src or test files.

### Examples
* `feat(app): add support for Azure Application Gateway routing rules`
* `fix(db): resolve database connection timeout configuration`
* `docs(readme): add Pulumi CLI authentication steps`

---

## Pull Request Guidelines

Before submitting a Pull Request, ensure that:

1. The project compiles successfully with `dotnet build`.
2. There are no trailing whitespaces or formatting inconsistencies.
3. Commit messages adhere to Conventional Commits.
4. Your PR description clearly explains:
   * What problem this solves.
   * What changes were made.
   * How you verified the changes.
