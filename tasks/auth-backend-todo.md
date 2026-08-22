# TODO: Authentication Backend Foundation

## Foundation

- [x] Scaffold ASP.NET Core API and test projects.
- [x] Add validated configuration, SQLite context and initial migration.
- [x] Add health/readiness endpoints and local run documentation.

## Authentication vertical slices

- [x] Register a user and return an authenticated session.
- [x] Log in with username or email.
- [x] Rotate a refresh token and reject reuse.
- [x] Revoke a refresh token on logout.
- [x] Return the authenticated user from `/api/auth/me`.

## Unity integration

- [x] Add network configuration and typed request/response models.
- [x] Add Unity HTTP client and in-memory authentication session.
- [x] Add an authentication service with register/login/refresh/logout methods.

## Verification

- [x] Backend tests pass.
- [x] Backend release build succeeds.
- [x] Local HTTP smoke test passes.
- [x] Unity recompiles without errors.
- [x] Security, quality and code-simplification reviews find no blockers.
