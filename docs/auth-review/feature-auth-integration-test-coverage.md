---
name: Feature request
about: Suggest an idea for this project
title: 'Add backend integration tests for auth flows'
labels: 'enhancement, auth, tests'
assignees: ''

---

**Is your feature request related to a problem? Please describe.**
The auth feature has several cross-cutting paths: CSRF, captcha, signup verification, OTP, OAuth, refresh-token rotation, logout, password reset, and device verification. These flows rely on Redis/cache state, database state, request metadata, and cookies. Without integration tests, regressions in one layer can silently break end-to-end auth behavior.

**Describe the solution you'd like**
Add backend integration tests that cover:
 - Local signup plus email link verification.
 - Local signup plus OTP verification.
 - Login success and invalid-credential failure.
 - Password reset by link and by OTP.
 - OTP one-time use and failed-attempt/rate-limit behavior.
 - Refresh token rotation, reuse detection, logout revocation, and password-change session revocation.
 - Browser cookie refresh-token flow and non-browser body/header refresh-token flow.
 - New-device login challenge and device verification.
 - OAuth existing-user linking and provider-ID collision cases.

**Describe alternatives you've considered**
Unit-test individual services only, but that would miss interactions between controller filters, request metadata, cache keys, cookies, and database writes.

**Additional context**
Priority: Medium

Complexity: Medium

Relevant code:
 - `backend/src/main/controllers/implementation/AuthController.cs`
 - `backend/src/main/services/implementation/AuthService.cs`
 - `backend/src/main/services/implementation/TokenService.cs`
 - `backend/src/main/services/implementation/DeviceService.cs`
 - `backend/src/main/configurations/security/CsrfConfiguration.cs`

