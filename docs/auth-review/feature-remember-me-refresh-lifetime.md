---
name: Feature request
about: Suggest an idea for this project
title: 'Implement remember-me refresh token lifetimes'
labels: 'enhancement, auth'
assignees: ''

---

**Is your feature request related to a problem? Please describe.**
`RememberMe` exists on auth request DTOs, but the backend refresh-token lifetime is always seven days. Users and clients cannot choose between a shorter session and a remembered session, and the API field currently has no observable effect.

**Describe the solution you'd like**
Make refresh token TTL configurable by login mode:
 - Short default session when `RememberMe` is false.
 - Longer remembered session when `RememberMe` is true.
 - Matching cookie `MaxAge`/`Expires` and Redis TTL.
 - Clear API contract so non-browser clients understand whether a refresh token is returned in the body or cookie.

**Describe alternatives you've considered**
Remove `RememberMe` from DTOs until it is supported, or keep one fixed refresh lifetime and document that the client should not show a remember-me option.

**Additional context**
Priority: Medium

Complexity: Medium

Relevant code:
 - `backend/src/main/dtos/requests/auth/AuthRequest.cs` and `LoginRequest.cs`: `RememberMe` is defined.
 - `backend/src/main/services/implementation/TokenService.cs`: `REFRESH_TTL` is fixed at seven days.
 - `backend/src/main/utilities/implementations/HttpUtility.cs`: refresh cookie lifetime is fixed at seven days.

