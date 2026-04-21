---
name: Bug report
about: Create a report to help us improve
title: 'OAuth sign-in does not persist provider IDs for existing email accounts'
labels: 'bug, auth, oauth'
assignees: ''

---

**Describe the bug**
Google and Microsoft sign-in first look up users by email, then by provider ID. If a user exists by email but has no matching provider ID stored, auth succeeds without persisting the provider ID. The repository already exposes `UpdateProviderIdsAsync`, but the auth service does not use it in these flows.

This makes provider ID uniqueness less useful, weakens auditability, and can keep the account in a partially linked state.

**To Reproduce**
Steps to reproduce the behavior:
1. Create a local account with an email address and no `GoogleID` or `MicrosoftID`.
2. Sign in with Google or Microsoft using the same email.
3. Observe that authentication succeeds.
4. Inspect the user record and observe that the provider ID remains null.

**Expected behavior**
When OAuth sign-in is allowed for an existing email account, the backend should persist the verified provider ID after checking that the provider ID is not already linked to another user.

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
 - `backend/src/main/services/implementation/AuthService.cs`: `GoogleAsync` and `MicrosoftAsync` look up by email before provider ID and only set provider IDs for newly created users.
 - `backend/src/main/repositories/implementation/UserRepository.cs`: `UpdateProviderIdsAsync` exists but is not used by the OAuth auth flow.

Suggested fix:
When an email match is found, verify the incoming provider ID is not linked elsewhere, then save the missing provider ID on that existing user. Add tests for local-to-OAuth linking and provider-ID collision handling.

