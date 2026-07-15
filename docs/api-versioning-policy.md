# API Versioning Policy

This document defines how we version the TMS API, what counts as a breaking change, and how we communicate deprecations to clients.

## 1. Versioning model

- We use **URL segment versioning**: `/api/v{version}/...` (for example, `/api/v1/courses`, `/api/v2/courses`).
- The API may serve **multiple versions simultaneously** (e.g., V1 and V2).
- V1 is treated as a **frozen contract** for existing clients; new functionality and contract changes go into higher versions (V2, V3, ...).
- Supported versions are advertised via the `api-supported-versions` response header.

## 2. What is a breaking change?

Any change that can cause existing clients to fail without modification is considered **breaking** and requires a **new API version**.

Breaking changes include, but are not limited to:

- **Response shape**
  - Removing a field from a response.
  - Renaming a field.
  - Changing the type or semantics of a field (e.g., `string` to `int`, or changing `status` enumerations).
- **Request contract**
  - Making an optional field required.
  - Tightening validation such that previously accepted values are now rejected.
- **Status codes**
  - Changing the success status code (e.g., `200` → `204`).
  - Changing error status codes in a way that breaks existing error handling (e.g., `404` → `410`).
- **Behavioral changes**
  - Changing default sort order or paging semantics for a collection endpoint.
  - Removing or repurposing an existing endpoint.

Breaking changes **must not** be shipped on an existing version (e.g., V1). Instead, they are introduced in a new version (e.g., V2).

## 3. What is additive (non‑breaking)?

Changes that preserve existing client behavior and only add information or capabilities are considered **non‑breaking** and can be shipped within an existing version.

Non‑breaking changes include:

- Adding new **optional** fields to responses.
- Adding new **optional** query parameters with safe defaults.
- Adding new endpoints (e.g., a new `GET /api/v1/courses/{id}/statistics`).
- Adding headers or metadata that clients are not required to consume.
- Performance improvements that do not change response semantics.

When in doubt, treat the change as **breaking** and consider introducing a new version.

## 4. Sunset window

When a new major API version is introduced:

- The previous version remains supported for a minimum **sunset window** of **6 months**.
- The sunset period begins when the successor version (e.g., V2) is first made publicly available.
- During the sunset window:
  - Breaking changes are **not** applied to the older version.
  - Only bug fixes and critical security fixes are allowed on the older version.

After the sunset date, the older version may be removed or return an error (e.g., `410 Gone`) according to the migration plan.

## 5. Deprecation communication

We communicate deprecation and migration paths **in-band** and **out-of-band**.

### 5.1 In-band (HTTP headers)

From the day a new version ships (e.g., V2), all responses from the older version (e.g., V1) include:

- `Deprecation: true`  
  Indicates the version is deprecated.

- `Sunset: <RFC 1123 date>`  
  The date when the version will be retired (RFC 8594).  
  Example: `Sunset: Thu, 31 Dec 2026 00:00:00 GMT`.

- `Link: <{successor-url}>; rel="successor-version"`  
  Points to the successor version for the same resource (RFC 5988).  
  Example:  
  `Link: <https://api.example.com/api/v2/courses>; rel="successor-version"`.

Supported versions are always exposed via `api-supported-versions: 1.0, 2.0, ...`.

### 5.2 Out-of-band

In addition to headers, we:

- Add an entry to the **CHANGELOG** describing the new version and the sunset date for the old version.
- Notify client teams by email or shared channel (e.g., Slack/Teams) including:
  - The new version.
  - The migration guide.
  - The sunset date.
- Optionally create a calendar event for the V1 shutdown date and invite affected teams.

## 6. Skipping versions

Clients are **not required** to adopt every intermediate version. It is acceptable to migrate directly from V1 to V3, skipping V2, as long as:

- The target version is documented and supported.
- Any breaking changes between the client’s current version and the target version are covered in migration guides.

## 7. Decision checklist

Before changing an API:

1. Does this change remove, rename, or tighten anything existing?
   - **Yes** → breaking → introduce a new version.
2. Does this change only add optional data or endpoints?
   - **Yes** → non‑breaking → can ship within the existing version.
3. If introducing a new version:
   - Add versioned controller(s) and routes (e.g., `/api/v2/...`).
   - Configure deprecation headers for the old version in middleware.
   - Update `api-supported-versions`.
   - Document the change in the CHANGELOG and notify clients.

This policy is part of the codebase and must be kept up to date as we evolve our versioning strategy
