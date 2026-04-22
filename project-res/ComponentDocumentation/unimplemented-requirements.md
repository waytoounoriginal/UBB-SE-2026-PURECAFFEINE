# Unimplemented / Partially Implemented Requirements

This document lists all requirements that are missing or not fully compliant with the specification, along with the exact file(s) where the fix must be implemented.

---

## Task 1 — REQ-GAM-06: Field-Specific Validation Error Messages

**What is not implemented:**  
When saving a game with invalid data, the system shows one generic dialog:  
*"Please ensure all fields are filled out correctly according to the rules..."*  
The requirement states the error must specify **exactly which field** is wrong (e.g. "Name must be between 5 and 30 characters", "Price must be greater than 0").

**Where to implement:**
- `Property_and_Management/src/Viewmodels/CreateGameViewModel.cs`  
  → Replace the `bool ValidateInputs()` return with a method that returns a specific error string per failed constraint.
- `Property_and_Management/src/Views/CreateGameView.xaml.cs`  
  → Update `SaveButton_Click` to display the specific error message instead of the generic one.
- `Property_and_Management/src/Viewmodels/EditGameViewModel.cs`  
  → Apply the same change as in `CreateGameViewModel`.
- `Property_and_Management/src/Views/EditGameView.xaml.cs`  
  → Apply the same change as in `CreateGameView.xaml.cs`.

---

## Task 2 — REQ-REQ-01: ChatView Is a Mock Placeholder (Approve/Deny Screen)

**What is not implemented:**  
The approval screen (`ChatView`) is a mock page showing a hardcoded fake message:  
*"Renter: Hello! I would love to rent your board game for the weekend. Is it available?"*  
The requirement states the owner must clearly see the **renter's name, the game, and the rental time period** when deciding to approve or deny. The current UI provides none of this real data.

**Where to implement:**
- `Property_and_Management/src/Views/ChatView.xaml`  
  → Replace the hardcoded mock content with real data bindings: renter display name, game name, game picture, start date, end date.
- `Property_and_Management/src/Viewmodels/ChatViewModel.cs`  
  → Load the full `RequestDTO` from `IRequestService` using the `RequestId` that is already passed in, and expose its fields as bindable properties for the view.

---

## Task 3 — REQ-REQ-03: Decline Button Mislabeled as "Delete"

**What is not implemented:**  
The button that triggers `DenyRequest()` in the owner's requests list is labeled **"Delete"** and the confirmation dialog says *"Delete Request?"*. The action is a **decline/deny**, not a delete. The label and dialog text are semantically wrong and misleading.

**Where to implement:**
- `Property_and_Management/src/Views/RequestsFromOthersPage.xaml`  
  → Change the `Button Content` from `"Delete"` to `"Decline"`.
- `Property_and_Management/src/Views/RequestsFromOthersPage.xaml.cs`  
  → Update the confirmation dialog `Title` and `Content` text to reflect "Decline Request" instead of "Delete Request".

---

## Task 4 — REQ-NOT-01 (New Chat Message): Chat Notification Entirely Absent

**What is not implemented:**  
The specification requires a notification with:  
- Title: `'New Message'`  
- Body: sender's display name  
- Trigger: immediately upon a chat message being sent  

There is **no chat messaging system** in the application. The `ChatView` is a mock with no real message exchange, so this notification event can never be triggered. This requires implementing the full chat feature first.

**Where to implement:**
- `Property_and_Management/src/Views/ChatView.xaml`  
  → Implement a real message input/display UI (message history + send field).
- `Property_and_Management/src/Viewmodels/ChatViewModel.cs`  
  → Add `SendMessage(string text)` logic and, upon send, call `INotificationService.SendNotificationToUser()` to both the sender and the recipient with title `"New Message"` and body set to the sender's display name.
- Optionally a new `IMessageService` / `MessageRepository` may be needed under:  
  `Property_and_Management/src/Service/`  
  `Property_and_Management/src/Repository/`  
  if message history must be persisted.

---

## Task 5 — REQ-NOT-01 (Overlapping Request Cancelled): Body Uses Game ID Instead of Game Name + Missing Booking Link

**What is not implemented:**  
When an overlapping request is auto-cancelled after an approval, the notification body reads:  
*"Your request for game **5** (date–date) was declined..."*  
The requirement states the body must include the **game name** (not the ID) and **a link to the booking interface**.

**Where to implement:**
- `Property_and_Management/src/Service/RequestService.cs` — method `ApproveRequest()` (around line 163)  
  → Replace `request.Game?.Id` with `request.Game?.Name` (or load the full game object if only the ID is available on the overlapping request).  
  → Add a booking interface link or deep-link string to the notification body.

---

## Task 6 — REQ-NOT-01 (Request Declined): Body Uses Game ID Instead of Game Name + Title Casing

**What is not implemented:**  
When an owner declines a request, the notification body reads:  
*"Your request for game **5** (date–date) was declined. Reason: ..."*  
The requirement states the body must include the **game name**. Additionally the notification title is `"Rental request declined"` (all lowercase) while the spec requires `"Rental Request Declined"`.

**Where to implement:**
- `Property_and_Management/src/Service/RequestService.cs` — method `DenyRequest()` (around line 207)  
  → Replace `request.Game?.Id` with `request.Game?.Name`.  
  → Fix the `Title` string from `"Rental request declined"` to `"Rental Request Declined"`.

---

## Complete Requirements Compliance Report

### 2.1 Games

| Requirement | Title | Status |
|-------------|-------|--------|
| REQ-GAM-01 | Owner sees all listed games | ✅ Implemented |
| REQ-GAM-02 | Create a new board game listing | ✅ Implemented |
| REQ-GAM-03 | Optional picture upload with default fallback | ✅ Implemented |
| REQ-GAM-04 | Update game fields | ✅ Implemented |
| REQ-GAM-05 | Toggle Active / Inactive | ✅ Implemented |
| REQ-GAM-06 | Field-specific validation error messages | ❌ Partial — generic error message only |

---

### 2.2 Rentals

| Requirement | Title | Status |
|-------------|-------|--------|
| REQ-REN-01 | Confirmed rental cannot be altered or deleted | ✅ Implemented |
| REQ-REN-02 | Rentals shown in descending order by start date | ✅ Implemented |
| REQ-REN-03 | All active rentals visible in the list | ✅ Implemented |
| REQ-REN-04 | Past rentals visually greyed out | ✅ Implemented |
| REQ-REN-05 | Mandatory 48-hour buffer period enforced | ✅ Implemented |

---

### 2.3 Requests

| Requirement | Title | Status |
|-------------|-------|--------|
| REQ-REQ-01 | Owner sees and acts on requests with full details | ❌ Partial — ChatView is a mock placeholder |
| REQ-REQ-02 | Overlapping requests auto-declined on approval | ✅ Implemented |
| REQ-REQ-03 | Owner can decline requests | ❌ Partial — button mislabeled "Delete" |
| REQ-REQ-04 | Owner sees requests in descending order by start date | ✅ Implemented |
| REQ-REQ-05 | Renter sees their requests with game name, picture, owner, dates | ✅ Implemented |
| REQ-REQ-06 | Renter can cancel pending request | ✅ Implemented |
| REQ-REQ-07 | Renter sees requests in descending order by start date | ✅ Implemented |

---

### 2.4 Notifications

| Requirement | Event | Status |
|-------------|-------|--------|
| REQ-NOT-01 | New chat message notification | ❌ Not implemented — no chat system exists |
| REQ-NOT-01 | Overlapping request cancelled notification | ❌ Partial — uses game ID not name; no booking link in body |
| REQ-NOT-01 | Request declined notification | ❌ Partial — uses game ID not name; title casing wrong |
| REQ-NOT-01 | Upcoming rental reminder notification | ✅ Implemented |

---

### 3.1 Global Navigation

| Requirement | Title | Status |
|-------------|-------|--------|
| UI-NAV-01 | Persistent menu bar on every page | ✅ Implemented |
| UI-NAV-02 | Active menu item visually highlighted | ✅ Implemented |
| UI-NAV-03 | Clicking menu item navigates to corresponding page | ✅ Implemented |

---

### 4. Cross-Team Capabilities (System Integrations)

| Requirement | Title | Status |
|-------------|-------|--------|
| SYS-INT-01 | Availability Calendar with 48-hour buffer | ✅ Implemented |
| SYS-INT-02 | Real-Time Booking Check | ✅ Implemented |
| SYS-INT-03 | Creating a Request with owner/availability validation | ✅ Implemented |
| SYS-INT-04 | Processing Approvals — atomic conversion + auto-cancel + notify | ✅ Implemented |
