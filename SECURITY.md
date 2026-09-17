# Security Policy

## Supported Versions

DeploymentKit is currently in preview. Security fixes are applied to the latest release on the `main` branch.

| Version | Supported |
|---------|-----------|
| 1.0.0-preview.x | Yes |
| < 1.0.0-preview.1 | No |

## Reporting a Vulnerability

Please do not report security vulnerabilities through public GitHub issues.

Instead, use GitHub's private vulnerability reporting:

1. Open the repository's **Security** tab.
2. Click **Report a vulnerability**.
3. Provide a description, reproduction steps, and the affected version.

You will receive a response as soon as possible. Once the issue is confirmed and fixed, a security advisory will be published.

## Handling Secrets

DeploymentKit never requires secrets in source control. Credentials are resolved at deployment time through environment variables, Pulumi configuration, or Azure Key Vault. If you believe a secret handling path in the library leaks sensitive values (for example into Pulumi state or logs), treat it as a security vulnerability and report it as described above.
