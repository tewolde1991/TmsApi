# TMS API Versioning Policy

## Breaking Changes (require new version)

- Removing or renaming a JSON field
- Changing a field's data type
- Changing an HTTP status code for an existing case
- Tightening validation rules (e.g. making an optional field required)
- Changing the default sort order of a collection

## Non-Breaking Changes (safe to ship to existing version)

- Adding a new optional response field
- Adding a new optional query parameter
- Adding a new endpoint
- Relaxing validation (e.g. making a required field optional)

## Sunset Window

V1 runs for a minimum of 6 months after V2 ships.
This ensures rural training centres on quarterly
maintenance cycles have at least one full cycle to migrate.

Current sunset: 31 December 2026
(Deprecation / Sunset / Link headers on every V1 response from day one of V2.)

## Communication Plan

1. Deprecation headers on every V1 response from V2 launch day.
2. CHANGELOG entry with migration guide link.
3. Email to every team holding an API key.
4. Calendar invite for the V1 shutdown date.

## Version Skipping

Clients may migrate V1 → V3 directly.
No intermediate version is required.

## Version Reading

Primary: URL segment /api/v{n}/...
Opt-in: X-Api-Version header (partner-by-partner basis)
