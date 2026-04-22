# BoardRent Codebase Audit & Improvement Roadmap

---

## Table of Contents

- [Critical Bugs (P0)](#critical-bugs-p0--must-fix)
- [High Severity (P1)](#high-severity-p1--should-fix)
- [Medium Severity (P2)](#medium-severity-p2--should-address)
- [Low Severity (P3)](#low-severity-p3--nice-to-have)
- [Recommended Fix Order (Roadmap)](#recommended-fix-order-roadmap)

---

## Critical Bugs (P0 — Must Fix)

### 0. ~~ChatView Approve/Deny Always Fails With "Not Authorized" When Navigating From Renter Page~~ [FIXED]

**Files:**

- `Property_and_Management/src/Views/ChatView.xaml.cs:54-104`
- `Property_and_Management/src/Viewmodels/ChatViewModel.cs:23-31`
- `Property_and_Management/src/Views/RequestsToOthersPage.xaml.cs:42-45`

**Symptom:** Clicking Approve or Deny on the ChatView shows: *"Operation failed: you are not authorized for this request."*

**Root Cause:** `ChatView` is reachable from **two different pages** that have opposite user roles:

| Source Page | Current User Role | Approve/Deny Result |
|---|---|---|
| `RequestsFromOthersPage` | **Owner** (correct) | Should work |
| `RequestsToOthersPage` | **Renter** (wrong) | Always fails with UNAUTHORIZED |

Both pages navigate to `ChatView` with only `request.Id` as the parameter. `ChatViewModel` then calls `ApproveRequest(RequestId, CurrentUserId)` — passing the current user's ID as the `ownerId`. When the current user is the **renter** (navigated from `RequestsToOthersPage`), the authorization check in `RequestService.ApproveRequest` (line 119) fails:

```csharp
if (request.Owner?.Id != ownerId)   // renter's ID != owner's ID → UNAUTHORIZED
    return (int)ApproveRequestError.UNAUTHORIZED_ERROR;
```

**Fix (choose one):**

**Option A — Hide buttons based on role (recommended):** Pass the user's role (owner/renter) as a navigation parameter alongside the request ID. In `ChatView`, hide the Approve/Deny buttons when the user is the renter. For example, navigate with a tuple or a small DTO:

```csharp
// In RequestsFromOthersPage (owner):
Frame?.Navigate(typeof(ChatView), (request.Id, true));   // true = isOwner

// In RequestsToOthersPage (renter):
Frame?.Navigate(typeof(ChatView), (request.Id, false));   // false = isOwner
```

Then in `ChatView.OnNavigatedTo`:

```csharp
if (e.Parameter is (int requestId, bool isOwner))
{
    ViewModel.RequestId = requestId;
    ApproveButton.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
    DenyButton.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
}
```

**Option B — Check ownership in ChatViewModel:** After setting `RequestId`, fetch the request and compare `request.Owner.Id` against `CurrentUserId`. Disable buttons if they don't match.

**Option C — Remove ChatView navigation from `RequestsToOthersPage`:** If renters should not see the chat/approve UI at all, remove the `RequestItem_Tapped` handlers from `RequestsToOthersPage.xaml.cs` (lines 36-46).

---

### 1. `NotificationsViewModel.OnProperyChanged` — Typo in Method Name

**File:** `Property_and_Management/src/Viewmodels/NotificationsViewModel.cs:215`

The method is named `OnProperyChanged` (missing 't' in "Property"). While it works because it is consistently called internally, it breaks the `[CallerMemberName]` convention and would confuse any developer extending the code. Any external code calling the standard `OnPropertyChanged` will not find it.

**Fix:** Rename `OnProperyChanged` to `OnPropertyChanged` across the file (~15 call sites).

---

### 2. ~~Race Condition in `RequestService.ApproveRequest` (TOCTOU)~~ [FIXED]

**File:** `Property_and_Management/src/Service/RequestService.cs:125-131`

Overlapping requests are queried **outside** the transaction (lines 126-131), then deleted **inside** the transaction (lines 149-150). Between the query and the delete, another thread could create new overlapping requests that would not be caught.

**Fix:** Move the overlapping request query inside the transaction, or use serializable isolation level.

---

### 3. ~~Race Condition in `RentalService.CreateConfirmedRental` (TOCTOU)~~ [FIXED]

**File:** `Property_and_Management/src/Service/RentalService.cs:47-50`

`IsSlotAvailable()` is checked, then `Add()` is called separately. Another thread can reserve the same slot between the check and the add.

**Fix:** Wrap availability check + insert in a single transaction with appropriate isolation.

---

### 4. ~~Buffer Period Inconsistency — 48 Hours vs 2 Days~~ [FIXED]

**Files:**

| Location | Approach |
|----------|----------|
| `RequestService.cs:122-123` | `AddHours(48)` / `AddHours(-48)` |
| `RequestService.cs:309-310` | `AddDays(s_bufferPeriodInDays)` (2 days) |
| `RentalService.cs:29` | `bufferHours = 48` |
| `RequestService.cs:281` | `AddHours(48)` |

Due to Daylight Saving Time, 2 days is not always equal to 48 hours. This inconsistency means requests can slip through availability checks during DST transitions.

**Fix:** Standardize on one approach across the entire codebase. Use `AddDays(2)` everywhere or `AddHours(48)` everywhere.

---

### 5. ~~`NotificationService` / `NotificationClient` — Resource Leaks (No `IDisposable`)~~ [FIXED]

**Files:**

- `NotificationService.cs:33` — creates `NotificationClient` (wraps `UdpClient`) but never disposes it
- `NotificationClient.cs:19,21` — `UdpClient` and `CancellationTokenSource` created but never disposed

**Fix:** Implement `IDisposable` on `NotificationClient` and `NotificationService`. Dispose in `App.xaml.cs` process exit handler.

---

### 6. ~~`UdpNotificationServer.HandleSendNotificationMessage` — `KeyNotFoundException` Crash~~ [FIXED]

**File:** `NotificationServer/UdpNotificationServer.cs:61`

```csharp
Console.WriteLine($"Sending notification to user: {unwrappedMessage.UserId}({_userIpMap[unwrappedMessage.UserId]})...");
```

Direct dictionary access without checking if key exists. If the target user is not subscribed, the server crashes (caught by generic handler, but still disrupts flow). The `SendMessage` method (line 30) correctly uses `TryGetValue`, but this log line does not.

**Fix:** Use `TryGetValue` or conditional access before the log statement.

---

### 7. ~~Null Reference in Multiple ViewModels — Missing `?.` on `App.Current`~~ [FIXED]

**Files:**

- `CreateGameViewModel.cs:30` — `(App.Current as App).CurrentUserID` (no `?.`)
- `RentalsFromOthersViewModel.cs:20` — same pattern
- `RentalsToOthersViewModel.cs:19` — same pattern
- `RequestsFromOthersViewModel.cs:22` — same pattern
- `RequestsToOthersViewModel.cs:21` — same pattern

Contrast with `ChatViewModel.cs:15` which correctly uses `(App.Current as App)?.CurrentUserID ?? 1`.

**Fix:** Add null-conditional operator consistently across all ViewModels.

---

## High Severity (P1 — Should Fix)

### 8. ~~`DateTime.Now` vs `DateTime.UtcNow` Mixed Usage~~ [FIXED]

**Files:**

| Location | Usage |
|----------|-------|
| `RequestService.cs:172` | `DateTime.UtcNow` (approval notification) |
| `RequestService.cs:213` | `DateTime.Now` (denial notification) |
| `RequestService.cs:253` | `DateTime.Now` (cancellation notification) |
| `RequestService.cs:274,275` | `DateTime.Now` (month/year defaults) |
| `NotificationService.cs:53` | `DateTime.UtcNow` (timestamp fallback) |
| `NotificationService.cs:159` | `scheduledTime.ToUniversalTime()` mixed with `DateTime.UtcNow` |

**Fix:** Standardize on `DateTime.UtcNow` everywhere, convert to local only for display.

---

### 9. ~~Repository Delete Methods — Non-Atomic Get-Then-Delete~~ [FIXED]

**Files:** All repositories:

- `GameRepository.cs:138-152`
- `RequestRepository.cs:76-89`
- `RentalRepository.cs:181-195`
- `NotificationRepository.cs:57-71`
- `UserRepository.cs:55-69`

Every `Delete()` method calls `Get()` first (separate connection), then deletes. Another thread can delete the same record between the two calls.

**Fix:** Use `OUTPUT` clause in DELETE SQL to return the deleted row atomically, or wrap in a transaction.

---

### 10. ~~`NotificationService.OnNext` — Missing `User` Property on NotificationDTO~~ [FIXED]

**File:** `NotificationService.cs:82-87`

When receiving a `SendNotificationMessage` from UDP, the created `NotificationDTO` does not set the `User` property. Downstream subscribers may get a DTO with `User = null`.

**Fix:** Set `User = new UserDTO { Id = message.UserId }` in the DTO.

---

### 11. ~~Fire-and-Forget Scheduled Notifications — Lost on App Exit~~ [FIXED]

**File:** `NotificationService.cs:175-186`

`Task.Run(async () => { await Task.Delay(delay); ... })` is fire-and-forget. If the app exits before the delay completes, the reminder is lost forever.

**Fix:** Use a persistent scheduling mechanism (database-backed job queue or Windows Task Scheduler) for reminders.

---

### 12. ~~`NotificationClient` — Infinite Retry Loop Without Backoff or Limit~~ [FIXED]

**File:** `Property_and_Management/src/Service/Listeners/NotificationClient.cs`

On `SocketException`, the client retries with a fixed delay and no maximum retry count. If the server never starts, this loops forever consuming resources.

**Fix:** Add exponential backoff and a maximum retry count.

---

### 13. ~~`GameService.SetGameRepository` — Mutable Dependency Anti-Pattern~~ [FIXED]

**File:** `Property_and_Management/src/Service/GameService.cs`

`_gameRepository` is not `readonly`, and `SetGameRepository()` allows swapping the repo post-construction. This breaks DI principles and can cause race conditions.

**Fix:** Make `_gameRepository` readonly, remove `SetGameRepository()`.

---

### 14. ~~Observer Subscription Leak in `NotificationsViewModel`~~ [FIXED]

**File:** `Property_and_Management/src/Viewmodels/NotificationsViewModel.cs:94`

`notificationService.Subscribe(this)` stores a reference to the ViewModel in the service. The returned `IDisposable` is never stored or called. The ViewModel can never be garbage collected.

**Fix:** Store the `IDisposable` subscription and dispose it when the ViewModel is no longer needed.

---

### 15. ~~Event Subscription Leak in `MenuBarPage`~~ [FIXED]

**File:** `Property_and_Management/src/Views/MenuBarPage.xaml.cs:24`

`ViewModel.RequestNavigation += OnViewModelRequestedNavigation` is never unsubscribed.

**Fix:** Unsubscribe in `Unloaded` event handler.

---

## Medium Severity (P2 — Should Address)

### 16. No Unit Test Framework

The project has no unit tests (only a UDP test client). Critical business logic in `RequestService`, `RentalService`, and `NotificationService` is completely untested.

**Fix:** Add xUnit or NUnit project with tests for service layer.

---

### 17. Missing Input Validation in `RequestService.CreateRequest`

**File:** `Property_and_Management/src/Service/RequestService.cs:84-106`

- No check for `startDate > endDate`
- No check for dates in the past
- Only checks future limit (1 month), not past dates

**Fix:** Add validation: `startDate < endDate`, `startDate >= DateTime.Today`.

---

### 18. File I/O on UI Thread in `NotificationsViewModel`

**File:** `Property_and_Management/src/Viewmodels/NotificationsViewModel.cs:177-211`

`LoadDismissedIdsForCurrentUser()` and `SaveDismissedIdsForCurrentUser()` do synchronous file I/O, called from property setters and public methods. This can freeze the UI.

**Fix:** Make these methods async or offload to background thread.

---

### 19. Bare `catch` Blocks Swallowing Exceptions

**Files:**

- `CreateGameViewModel.cs:66-68` — `catch { Image = new byte[0]; }`
- `EditGameViewModel.cs:85-87` — same pattern

**Fix:** Catch specific exceptions, add logging.

---

### 20. Repository Code Duplication (DRY Violations)

**Files:** `RequestRepository.cs`, `RentalRepository.cs`

`GetAll()`, `GetByOwner()`, `GetByRenter()`, `GetByGame()` methods each duplicate identical JOIN + object construction logic (4x in each file).

**Fix:** Extract a shared `ReadFromReader()` helper method.

---

### 21. Unused Imports in `UdpNotificationServer.cs`

**File:** `NotificationServer/UdpNotificationServer.cs:3,10`

- `using System.Formats.Tar;` — unused
- `using Microsoft.VisualBasic;` — unused

**Fix:** Remove unused imports.

---

### 22. Duplicate `using` Statements in `ChatView.xaml.cs`

**File:** `Property_and_Management/src/Views/ChatView.xaml.cs:7-19`

Multiple duplicate using directives.

**Fix:** Remove duplicates.

---

### 23. Unused `pageSize` Parameters in Multiple ViewModels

**Files:**

- `RentalsFromOthersViewModel.cs:83`
- `RentalsToOthersViewModel.cs:82`
- `RequestsFromOthersViewModel.cs:86`
- `RequestsToOthersViewModel.cs:85`

`LoadRequests(int page, int pageSize)` / `LoadRentals(int page, int pageSize)` accept `pageSize` but always use the constant `PageSize`.

**Fix:** Remove the unused parameter or use it.

---

### 24. Unprofessional Comment in `NotificationManager.cs`

**File:** `Property_and_Management/NotificationManager.cs:12`

Contains "F Microslop" comment.

**Fix:** Remove or replace with a professional comment.

---

## Low Severity (P3 — Nice to Have)

| # | Issue | Location |
|---|-------|----------|
| 25 | `Debug.WriteLine` used instead of proper logging framework | Multiple repositories, `App.xaml.cs` |
| 26 | No error handling in repositories — SQL exceptions propagate uncaught | All repositories |
| 27 | Hardcoded connection string in `App.config` (SQL Express instance name) | `App.config` |
| 28 | `CurrentPage` could become 0 if `PageCount` is 0 | `ListingsViewModel.cs:89` |
| 29 | `async void OnNavigatedTo()` anti-pattern — hides exceptions | `EditGameView.xaml.cs:32` |
| 30 | No image size validation when uploading game images | `CreateGameView.xaml.cs`, `EditGameView.xaml.cs` |
| 31 | `ScheduleOrSendUserNotification` silently ignores `userId == 0` | `NotificationService.cs:157` |

---

## Recommended Fix Order (Roadmap)

### Phase 1 — Critical Fixes (Stability)

> Goal: Eliminate crashes and data corruption risks

1. ~~Fix ChatView authorize error — hide Approve/Deny for renters (#0)~~ [FIXED]
2. Fix `OnProperyChanged` typo (#1)
3. ~~Fix null-coalescing on `App.Current` in all ViewModels (#7)~~ [FIXED]
4. ~~Fix `UdpNotificationServer` dictionary crash (#6)~~ [FIXED]
5. ~~Standardize buffer period calculation (#4)~~ [FIXED]
6. ~~Standardize `DateTime.UtcNow` usage (#8)~~ [FIXED]

### Phase 2 — Resource & Concurrency (Reliability)

> Goal: Fix resource leaks and race conditions

6. ~~Implement `IDisposable` on `NotificationClient` / `NotificationService` (#5)~~ [FIXED]
7. ~~Fix TOCTOU in `ApproveRequest` (#2)~~ [FIXED]
8. ~~Fix TOCTOU in `CreateConfirmedRental` (#3)~~ [FIXED]
9. ~~Fix non-atomic Delete in repositories (#9)~~ [FIXED]
10. ~~Fix observer subscription leak (#14)~~ [FIXED]
11. ~~Fix event subscription leak (#15)~~ [FIXED]

### Phase 3 — Business Logic Hardening

> Goal: Improve correctness and robustness

12. ~~Set `User` property in `NotificationService.OnNext` (#10)~~ [FIXED]
13. Add input validation to `CreateRequest` (#17)
14. ~~Add retry limits to `NotificationClient` (#12)~~ [FIXED]
15. ~~Make `GameService._gameRepository` readonly (#13)~~ [FIXED]
16. ~~Replace fire-and-forget scheduled notifications (#11)~~ [FIXED]

### Phase 4 — Code Quality

> Goal: Improve maintainability and developer experience

17. Add unit test project (#16)
18. Make file I/O async in `NotificationsViewModel` (#18)
19. Fix bare catch blocks (#19)
20. Extract shared reader helpers in repositories (#20)
21. Clean up unused imports, duplicates, parameters (#21-24)

### Phase 5 — Polish

> Goal: Address minor issues and improve professionalism

22. Address all P3 items (#25-31)
