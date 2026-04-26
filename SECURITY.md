# Security Policy

## Supported versions

Security fixes are applied to the actively maintained state of this repository on the default branch.

At the moment, we do not maintain multiple long-term supported release lines. If you are running a fork or an older snapshot, please reproduce the issue against the latest code before reporting it.

## Reporting a vulnerability

Please do not open public GitHub issues for security vulnerabilities.

Report vulnerabilities privately to:

- `khanalex301@gmail.com`

If your report involves sensitive data, include only the minimum information needed for initial triage and wait for follow-up instructions before sharing additional material.

## What to include in a report

Please include:

- a clear description of the vulnerability
- affected area, module, endpoint, or configuration
- impact assessment
- reproduction steps or proof of concept
- any mitigations or suspected root cause, if known

Useful examples for this repository include:

- authentication or authorization bypass
- insecure default configuration
- token, password, or secret handling issues
- injection vulnerabilities
- data exposure through Swagger, logs, or exception payloads
- privilege escalation across modules or policies
- unsafe communication or notification behavior

## Response expectations

We aim to:

- acknowledge receipt within 2 business days
- assess severity and reproduce the issue as quickly as possible
- keep the reporter informed during triage and remediation

Actual resolution time depends on severity, complexity, and release readiness.

## Disclosure process

We ask researchers and contributors to follow responsible disclosure:

1. Report the issue privately.
2. Allow time for validation and remediation.
3. Coordinate on disclosure timing before publishing details.

If the report is valid, we will work toward a fix and decide whether to publish advisories, release notes, or additional mitigation guidance.

## Security practices for contributors

When contributing to Alphabet:

- do not commit secrets, tokens, passwords, or connection strings intended for real environments
- use configuration and options binding instead of hardcoded credentials
- keep public error responses free of sensitive internals
- preserve authorization boundaries and policy checks
- document security-sensitive behavior changes in pull requests
- update Swagger and docs carefully so examples do not expose secrets or unsafe defaults

## Out of scope

The following are generally out of scope unless they demonstrate real security impact:

- missing best-practice headers without exploitability
- development-only placeholder credentials that are clearly documented for local use
- denial-of-service scenarios that require unrealistic local-only conditions
- findings in third-party dependencies without a demonstrated impact path in this repository

## Recognition

We appreciate responsible disclosure and may acknowledge reporters in release notes or project documentation when appropriate and when the reporter wants to be credited.
