---
name: Bug report
about: Create a report to help us improve
title: 'Auth OTP challenges can be reused and are not protected by the auth rate limit'
labels: 'bug, auth, security'
assignees: ''

---

**Describe the bug**
OTP verification challenges are validated mostly from the signed challenge JWT. `VerifyVerificationOtpAsync` only deletes cached state when the state still exists and matches the challenge, but it does not require that state to exist before accepting the OTP. This means reset-password OTP challenges can be replayed until JWT expiry, including after the link token has already been consumed and deleted.

The OTP endpoint also does not use the stricter `RateLimiterConfiguration.AuthPolicyName`, while login and signup do. That leaves OTP verification on the global limiter instead of the auth-specific limiter.

**To Reproduce**
Steps to reproduce the behavior:
1. Request a password reset with `POST /api/auth/forgot-password`.
2. Capture the returned `challenge` and the emailed OTP code.
3. Reset the password once through `POST /api/auth/change-password` using `code` and `challenge`.
4. Before the challenge expires, call `POST /api/auth/change-password` again with the same `code` and `challenge`.
5. Observe that the cached delivery state is not required for OTP validation, so replay can succeed until the JWT expires.

**Expected behavior**
OTP challenges should be one-time use. Verification should fail if the cached challenge state is missing, already consumed, expired, or does not match the supplied challenge. OTP verification and password reset should also be protected by auth-specific rate limiting and ideally by per-challenge failed-attempt counters.

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

Complexity: Medium

Relevant code:
 - `backend/src/main/services/implementation/TokenService.cs`: `VerifyVerificationOtpAsync` accepts the challenge after JWT/proof validation, then only deletes state when `state != null && state.OtpChallenge == challenge`.
 - `backend/src/main/services/implementation/TokenService.cs`: `VerifyVerificationToken` deletes both link token and verification state, but that does not invalidate a previously issued OTP challenge JWT.
 - `backend/src/main/controllers/implementation/AuthController.cs`: `POST verify/otp`, `POST forgot-password`, and `POST change-password` are not decorated with `EnableRateLimiting(RateLimiterConfiguration.AuthPolicyName)`.

Suggested fix:
Require a matching cached `VerificationDeliveryState` before accepting OTP verification, delete it atomically on success, store failed attempt counts by challenge/email/purpose, and apply the auth rate-limit policy to OTP/password-reset endpoints.

