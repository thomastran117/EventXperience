---
name: Bug report
about: Create a report to help us improve
title: 'Login requests incorrectly enforce new-password complexity rules'
labels: 'bug, auth, validation'
assignees: ''

---

**Describe the bug**
`LoginRequest` inherits from `AuthRequest`, where the password field has `[StrongPassword]`. This means login can be rejected by model validation before credentials are checked if the submitted password does not match the current password policy.

This is a login-specific problem because password complexity should be enforced when setting a password, not when authenticating with an existing password.

**To Reproduce**
Steps to reproduce the behavior:
1. Have an existing account with a password that is valid in storage but does not meet the current `StrongPassword` rules.
2. Attempt to log in with the correct email and password.
3. Observe that request validation can reject the request before `AuthService.LoginAsync` verifies the BCrypt hash.

**Expected behavior**
Login should require a non-empty password and then return a generic authentication result. Password complexity rules should apply to signup and password change/reset flows only.

**Screenshots**
Not applicable.

**Desktop (please complete the following information):**
 - OS: Not applicable
 - Browser Not applicable
 - Version Not applicable

**Smartphone (please complete the following information):**
 - Device: Not applicable
 - OS: Not applicable
 - Browser Not applicable
 - Version Not applicable

**Additional context**
Priority: Medium

Complexity: Low

Relevant code:
 - `backend/src/main/dtos/requests/auth/AuthRequest.cs`: `Password` is decorated with `[StrongPassword]`.
 - `backend/src/main/dtos/requests/auth/LoginRequest.cs`: `LoginRequest` inherits from `AuthRequest`.

Suggested fix:
Split auth request DTOs so login uses `[Required]` only for password, while signup and password reset/change continue using `[StrongPassword]`.

