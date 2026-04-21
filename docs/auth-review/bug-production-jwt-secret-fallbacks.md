---
name: Bug report
about: Create a report to help us improve
title: 'Backend can start with fallback JWT secrets in production'
labels: 'bug, auth, security, config'
assignees: ''

---

**Describe the bug**
JWT signing secrets have hard-coded fallback values. The environment validation method is not called during startup, and the validation dictionary checks names that do not match the actual JWT environment variables. As a result, a production deployment can accidentally run with predictable default JWT signing keys.

**To Reproduce**
Steps to reproduce the behavior:
1. Start the backend without `JWT_SECRET_ACCESS` and `JWT_SECRET_VERIFICATION`.
2. Observe that `EnvironmentSetting` falls back to default test-like secret strings.
3. Observe that `Program.cs` does not call `EnvironmentSetting.Validate()` during startup.
4. Even if `Validate()` were called, it checks `JWT_SECRET_KEY` instead of both actual JWT secret variables.

**Expected behavior**
Production startup should fail closed when JWT signing secrets are missing, short, or equal to known fallback values. Validation should check `JWT_SECRET_ACCESS` and `JWT_SECRET_VERIFICATION` separately, and the application should call validation before accepting traffic.

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
Priority: High

Complexity: Low

Relevant code:
 - `backend/src/main/configurations/environment/EnvironmentSetting.cs`: `JWT_SECRET_ACCESS` and `JWT_SECRET_VERIFICATION` use fallback values.
 - `backend/src/main/configurations/environment/EnvironmentSetting.cs`: `Validate()` checks `JWT_SECRET_KEY`, not the two actual JWT secret names.
 - `backend/src/main/Program.cs`: no call to `EnvironmentSetting.Validate()` was found.

Suggested fix:
Call environment validation during startup, validate both JWT secrets by their real names, reject known fallback values outside `development` and `test`, and consider requiring a minimum entropy/length threshold.

