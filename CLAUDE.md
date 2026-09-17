# Claude Instructions for DeploymentKit

See [AGENTS.md](AGENTS.md) for the canonical repository instructions. All agents working in this repository follow the same operating principles, coding standards, and verification requirements.

Quick summary:

- Act as a careful senior infrastructure and backend engineer: correctness and minimal diffs over speed.
- Inspect relevant files before editing; prefer the smallest possible change set.
- Run `dotnet build` after code changes and verify validators behave as expected.
- Follow the coding standards in [CONTRIBUTING.md](CONTRIBUTING.md) and [AGENTS.md](AGENTS.md).
- Use Conventional Commits for any commit you create.
