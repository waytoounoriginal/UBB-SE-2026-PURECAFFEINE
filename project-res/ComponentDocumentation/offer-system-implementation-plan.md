# Offer System Implementation Plan

Replaces the mock ChatView with a notification-driven offer workflow. Users create requests for games, owners offer via green buttons, and requesters approve/deny through actionable notifications.

---

## Design Summary

**Core flow:**
1. User A creates a Request ("I want to rent game X" with dates)
2. Game owner (User B) sees it in "Others' Requests" with a green **Offer** button
3. User B clicks Offer -> Request status becomes `OfferPending`, notification sent to User A
4. User A sees actionable notification with **Approve** / **Deny** buttons
5. Approve -> Rental created, request removed, result notification to both parties
6. Deny -> Request reset to `Open`, result notification to both parties

**Guardrails:** Only one offer per request at a time. Button disabled when status != `Open`. If approved, request is deactivated. If denied, request reopens for new offers.

---

## Phase 1: Data Model & Database Schema Changes

### 1.1 Add columns to `Requests` table

**File:** `Property_and_Management/Scripts/generate_db.sql`

Add an `ALTER TABLE` migration script (new file recommended: `Scripts/migrate_offer_system.sql`):

```sql
-- Add status column: 0 = Open, 1 = OfferPending, 2 = Accepted, 3 = Cancelled
ALTER TABLE Requests ADD status INT NOT NULL DEFAULT 0;

-- Add offering_user_id: the user who clicked "Offer" (nullable)
ALTER TABLE Requests ADD offering_user_id INT NULL;
ALTER TABLE Requests ADD CONSTRAINT FK_Request_OfferingUser
    FOREIGN KEY (offering_user_id) REFERENCES [Users](id);
```

### 1.2 Add columns to `Notifications` table

```sql
-- Add type column: 0 = Informational, 1 = OfferReceived, 2 = OfferResult
ALTER TABLE Notifications ADD type INT NOT NULL DEFAULT 0;

-- Add related_request_id (nullable, links actionable notifications to a request)
ALTER TABLE Notifications ADD related_request_id INT NULL;
ALTER TABLE Notifications ADD CONSTRAINT FK_Notification_Request
    FOREIGN KEY (related_request_id) REFERENCES Requests(request_id);
```

### 1.3 Add `RequestStatus` enum

**New file:** `Property_and_Management/src/Model/RequestStatus.cs`

```csharp
namespace Property_and_Management.src.Model
{
    public enum RequestStatus
    {
        Open = 0,
        OfferPending = 1,
        Accepted = 2,
        Cancelled = 3
    }
}
```

### 1.4 Add `NotificationType` enum

**New file:** `Property_and_Management/src/Model/NotificationType.cs`

```csharp
namespace Property_and_Management.src.Model
{
    public enum NotificationType
    {
        Informational = 0,
        OfferReceived = 1,
        OfferResult = 2
    }
}
```

### 1.5 Update `Request` model

**File:** `Property_and_Management/src/Model/Request.cs`

Add:
```csharp
public RequestStatus Status { get; set; } = RequestStatus.Open;
public User? OfferingUser { get; set; }
```

Update the constructor to accept the new fields.

### 1.6 Update `Notification` model

**File:** `Property_and_Management/src/Model/Notification.cs`

Add:
```csharp
public NotificationType Type { get; set; } = NotificationType.Informational;
public int? RelatedRequestId { get; set; }
```

Update the constructor.

### 1.7 Update `RequestDTO`

**File:** `Property_and_Management/src/DTO/RequestDTO.cs`

Add:
```csharp
public RequestStatus Status { get; set; } = RequestStatus.Open;
public UserDTO? OfferingUser { get; set; }
public bool CanOffer => Status == RequestStatus.Open;
```

### 1.8 Update `NotificationDTO`

**File:** `Property_and_Management/src/DTO/NotificationDTO.cs`

Add:
```csharp
public NotificationType Type { get; set; } = NotificationType.Informational;
public int? RelatedRequestId { get; set; }
public bool IsActionable => Type == NotificationType.OfferReceived;
```

### 1.9 Update Mappers

**Files:**
- `Property_and_Management/src/Mapper/RequestMapper.cs` -- map `Status` and `OfferingUser`
- `Property_and_Management/src/Mapper/NotificationMapper.cs` -- map `Type` and `RelatedRequestId`

---

## Phase 2: Repository Layer Updates

### 2.1 Update `RequestRepository` SQL queries

**File:** `Property_and_Management/src/Repository/RequestRepository.cs`

All SELECT queries (in `GetAll`, `Get`, `GetRequestsByOwner`, `GetRequestsByRenter`, `GetRequestsByGame`) must:
- Select `r.status` and `r.offering_user_id` columns
- LEFT JOIN on `Users ou2 ON ou2.id = r.offering_user_id` to get offering user display name
- Populate `Status` and `OfferingUser` on the `Request` object

The INSERT in `Add()` must include `status` column (default `0`).

The `Update()` method must include `status` and `offering_user_id`.

Add a new method:
```csharp
void UpdateStatus(int requestId, RequestStatus status, int? offeringUserId);
```

**File:** `Property_and_Management/src/Interface/IRequestRepository.cs`

Add the `UpdateStatus` method to the interface.

### 2.2 Update `NotificationRepository` SQL queries

**File:** `Property_and_Management/src/Repository/NotificationRepository.cs`

All SELECT queries must include `n.type` and `n.related_request_id`.

The INSERT in `Add()` must include `type` and `related_request_id`.

Add a new query method:
```csharp
ImmutableList<Notification> GetActionableByRequestId(int requestId);
```

This is used for deduplication: check if an `OfferReceived` notification already exists for a given request.

**File:** `Property_and_Management/src/Interface/INotificationRepository.cs`

Add the method to the interface.

---

## Phase 3: Service Layer - Offer Workflow

### 3.1 Add `OfferGame` method to `RequestService`

**File:** `Property_and_Management/src/Service/RequestService.cs`

New enum:
```csharp
public enum OfferError
{
    NOT_FOUND = -1,
    NOT_OWNER = -2,
    ALREADY_HAS_OFFER = -3,
    REQUEST_NOT_OPEN = -4
}
```

New method:
```csharp
public int OfferGame(int requestId, int offeringUserId)
```

Logic:
1. Fetch request by ID. Return `NOT_FOUND` if missing.
2. Check `request.Owner?.Id == offeringUserId`. Return `NOT_OWNER` if not the game owner.
3. Check `request.Status == RequestStatus.Open`. Return `REQUEST_NOT_OPEN` if not.
4. Call `_requestRepository.UpdateStatus(requestId, RequestStatus.OfferPending, offeringUserId)`.
5. Send notification to requester (renter):
   - Type: `OfferReceived`
   - RelatedRequestId: `requestId`
   - Title: `"Game Offer Received"`
   - Body: `"{ownerName} is offering you {gameName} for {startDate} - {endDate}"`
6. Return `requestId` on success.

### 3.2 Add `ApproveOffer` method to `RequestService`

**File:** `Property_and_Management/src/Service/RequestService.cs`

New enum:
```csharp
public enum ApproveOfferError
{
    NOT_FOUND = -1,
    NOT_RENTER = -2,
    NO_PENDING_OFFER = -3,
    TRANSACTION_FAILED = -4
}
```

New method:
```csharp
public int ApproveOffer(int requestId, int renterId)
```

Logic:
1. Fetch request. Return `NOT_FOUND` if missing.
2. Check `request.Renter?.Id == renterId`. Return `NOT_RENTER` if not.
3. Check `request.Status == RequestStatus.OfferPending`. Return `NO_PENDING_OFFER` if not.
4. Inside a serializable transaction:
   a. Create `Rental` (same as current `ApproveRequest` logic).
   b. Delete overlapping requests + notify their renters (same as current logic).
   c. Delete the approved request itself.
   d. Commit transaction.
5. Send result notification to **owner** (offering user):
   - Type: `OfferResult`
   - Title: `"Offer Accepted"`
   - Body: `"{renterName} accepted your offer for {gameName}"`
6. Send result notification to **renter**:
   - Type: `OfferResult`
   - Title: `"Rental Confirmed"`
   - Body: `"You accepted the offer for {gameName} from {ownerName}"`
7. Delete the `OfferReceived` notification for this request (clean up actionable notification).
8. Schedule upcoming rental reminder.
9. Return `rental.Id`.

### 3.3 Add `DenyOffer` method to `RequestService`

New enum:
```csharp
public enum DenyOfferError
{
    NOT_FOUND = -1,
    NOT_RENTER = -2,
    NO_PENDING_OFFER = -3
}
```

New method:
```csharp
public int DenyOffer(int requestId, int renterId)
```

Logic:
1. Fetch request. Validate renter and status.
2. Call `_requestRepository.UpdateStatus(requestId, RequestStatus.Open, null)` -- reset to Open, clear offering user.
3. Send result notification to **owner**:
   - Type: `OfferResult`
   - Title: `"Offer Denied"`
   - Body: `"{renterName} denied your offer for {gameName}"`
4. Send result notification to **renter**:
   - Type: `OfferResult`
   - Title: `"Offer Declined"`
   - Body: `"You declined the offer for {gameName} from {ownerName}"`
5. Delete the `OfferReceived` notification for this request.
6. Return `requestId`.

### 3.4 Update `IRequestService` interface

**File:** `Property_and_Management/src/Interface/IRequestService.cs`

Add:
```csharp
int OfferGame(int requestId, int offeringUserId);
int ApproveOffer(int requestId, int renterId);
int DenyOffer(int requestId, int renterId);
```

### 3.5 Update `INotificationService` interface

**File:** `Property_and_Management/src/Interface/INotificationService.cs`

Add:
```csharp
void DeleteNotificationsByRequestId(int requestId);
```

Implement in `NotificationService` -- deletes all actionable notifications tied to a specific request (cleanup after approve/deny).

---

## Phase 4: Create Request Feature (UI)

### 4.1 Create the `CreateRequestView` page

**New files:**
- `Property_and_Management/src/Views/CreateRequestView.xaml`
- `Property_and_Management/src/Views/CreateRequestView.xaml.cs`

UI layout (modeled after `CreateGameView.xaml`):
- Title: "Create New Request"
- **Game picker**: `ComboBox` bound to a list of all games from DB (excluding user's own games). Display: game name + owner name.
- **Start Date**: `CalendarDatePicker`
- **End Date**: `CalendarDatePicker`
- **Save button**: Calls `RequestService.CreateRequest()`
- On success: navigate back to My Requests page

### 4.2 Create the `CreateRequestViewModel`

**New file:** `Property_and_Management/src/Viewmodels/CreateRequestViewModel.cs`

Properties:
- `ObservableCollection<GameDTO> AvailableGames` -- all games not owned by current user
- `GameDTO SelectedGame`
- `DateTimeOffset? StartDate`
- `DateTimeOffset? EndDate`

Methods:
- `LoadGames()` -- calls `IGameService` to get all games, filters out current user's games
- `SaveRequest()` -- validates inputs, calls `IRequestService.CreateRequest()`

### 4.3 Add "Create Request" button to `RequestsToOthersPage`

**File:** `Property_and_Management/src/Views/RequestsToOthersPage.xaml`

Add a header row (same pattern as `ListingsPage.xaml`):
```xml
<Grid Grid.Row="0" Margin="0,0,0,20">
    <TextBlock Text="My Requests" ... />
    <Button Content="+ Create Request" HorizontalAlignment="Right"
            Style="{StaticResource AccentButtonStyle}"
            Click="CreateRequestButton_Click"/>
</Grid>
```

**File:** `Property_and_Management/src/Views/RequestsToOthersPage.xaml.cs`

Add handler:
```csharp
private void CreateRequestButton_Click(object sender, RoutedEventArgs e)
{
    Frame?.Navigate(typeof(CreateRequestView));
}
```

### 4.4 Add `IGameService.GetAllGames()` method (if not already present)

**File:** `Property_and_Management/src/Interface/IGameService.cs`
**File:** `Property_and_Management/src/Service/GameService.cs`

Ensure there's a method to retrieve all games (not just by owner). This is needed for the game picker in CreateRequestView. If `GetAll()` exists on the repository, expose it through the service.

### 4.5 Register new ViewModel in DI

**File:** `Property_and_Management/App.xaml.cs`

Add:
```csharp
serviceCollection.AddTransient<CreateRequestViewModel>();
```

---

## Phase 5: Others' Requests Page - Green Offer Button

### 5.1 Update `RequestsFromOthersPage.xaml`

**File:** `Property_and_Management/src/Views/RequestsFromOthersPage.xaml`

Replace the red "Delete" button with a green "Offer" button:
```xml
<Button Grid.Column="2"
        Content="Offer"
        Tag="{Binding Id}"
        Click="OfferButton_Click"
        IsEnabled="{Binding CanOffer}"
        Margin="8,0,0,0"
        Background="#107C10"
        Foreground="White"
        VerticalAlignment="Center"/>
```

The `IsEnabled="{Binding CanOffer}"` binding ensures the button is grayed out when `Status != Open`.

Remove the `Tapped="RequestItem_Tapped"` and `DoubleTapped="RequestItem_Tapped"` handlers (no more navigation to ChatView).

### 5.2 Update `RequestsFromOthersPage.xaml.cs`

**File:** `Property_and_Management/src/Views/RequestsFromOthersPage.xaml.cs`

Replace `DenyButton_Click` handler with:
```csharp
private async void OfferButton_Click(object sender, RoutedEventArgs e)
{
    // Get request ID from button Tag
    // Show confirmation dialog: "Offer your game {gameName} to {renterName}?"
    // Call ViewModel.OfferGame(requestId)
    // Show success/error dialog
    // Refresh list
}
```

Remove `RequestItem_Tapped` handler (no ChatView navigation).

### 5.3 Update `RequestsFromOthersViewModel`

**File:** `Property_and_Management/src/Viewmodels/RequestsFromOthersViewModel.cs`

Add method:
```csharp
public int OfferGame(int requestId)
{
    return _requestService.OfferGame(requestId, _currentUserId);
}
```

---

## Phase 6: Actionable Notifications

### 6.1 Update `NotificationsPage.xaml`

**File:** `Property_and_Management/src/Views/NotificationsPage.xaml`

Update the `DataTemplate` to conditionally show different button layouts:

For **Informational** notifications: keep the existing red delete button.

For **OfferReceived** notifications: replace delete button with Approve (green) + Deny (red) buttons:
```xml
<!-- Shown when Type == OfferReceived -->
<StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4"
            Visibility="{Binding IsActionable, Converter={StaticResource BoolToVisibility}}">
    <Button Content="Approve"
            Tag="{Binding RelatedRequestId}"
            Click="ApproveOffer_Click"
            Background="#107C10" Foreground="White"
            VerticalAlignment="Center"/>
    <Button Content="Deny"
            Tag="{Binding RelatedRequestId}"
            Click="DenyOffer_Click"
            Background="#C50F1F" Foreground="White"
            VerticalAlignment="Center"/>
</StackPanel>

<!-- Shown when Type != OfferReceived -->
<Button Grid.Column="2"
        Visibility="{Binding IsActionable, Converter={StaticResource InverseBoolToVisibility}}"
        Click="DeleteButton_Click" ... />
```

A `BooleanToVisibilityConverter` and its inverse will be needed (or use an existing one if available).

### 6.2 Update `NotificationsPage.xaml.cs`

**File:** `Property_and_Management/src/Views/NotificationsPage.xaml.cs`

Add handlers:
```csharp
private async void ApproveOffer_Click(object sender, RoutedEventArgs e)
{
    var requestId = (int)((Button)sender).Tag;
    var result = ViewModel.ApproveOffer(requestId);
    // Show result dialog
    // Refresh notifications list
}

private async void DenyOffer_Click(object sender, RoutedEventArgs e)
{
    var requestId = (int)((Button)sender).Tag;
    var result = ViewModel.DenyOffer(requestId);
    // Show result dialog
    // Refresh notifications list
}
```

### 6.3 Update `NotificationsViewModel`

**File:** `Property_and_Management/src/Viewmodels/NotificationsViewModel.cs`

Add dependency on `IRequestService` (inject via constructor).

Add methods:
```csharp
public int ApproveOffer(int requestId)
{
    var result = _requestService.ApproveOffer(requestId, _currentUserId);
    if (result > 0) LoadNotifications();  // Refresh after action
    return result;
}

public int DenyOffer(int requestId)
{
    var result = _requestService.DenyOffer(requestId, _currentUserId);
    if (result > 0) LoadNotifications();  // Refresh after action
    return result;
}
```

---

## Phase 7: Remove ChatView & Clean Up

### 7.1 Delete ChatView files

**Delete:**
- `Property_and_Management/src/Views/ChatView.xaml`
- `Property_and_Management/src/Views/ChatView.xaml.cs`
- `Property_and_Management/src/Viewmodels/ChatViewModel.cs`

### 7.2 Remove ChatView navigation from request/rental pages

**Files to update:**
- `Property_and_Management/src/Views/RequestsFromOthersPage.xaml.cs` -- remove `RequestItem_Tapped` (already done in Phase 5)
- `Property_and_Management/src/Views/RequestsToOthersPage.xaml.cs` -- remove `RequestItem_Tapped` handler
- `Property_and_Management/src/Views/RentalsFromOthersPage.xaml.cs` -- remove navigation to ChatView on tap
- `Property_and_Management/src/Views/RentalsToOthersPage.xaml.cs` -- remove navigation to ChatView on tap

**XAML files to update** (remove `Tapped` and `DoubleTapped` attributes from Grid elements):
- `Property_and_Management/src/Views/RequestsFromOthersPage.xaml`
- `Property_and_Management/src/Views/RequestsToOthersPage.xaml`
- `Property_and_Management/src/Views/RentalsFromOthersPage.xaml`
- `Property_and_Management/src/Views/RentalsToOthersPage.xaml`

### 7.3 Remove ChatView from DI registration

**File:** `Property_and_Management/App.xaml.cs`

Remove line:
```csharp
serviceCollection.AddTransient<ChatViewModel>();
```

---

## Phase 8: Diagram Updates

### 8.1 Database ER Diagram

**File:** `project-res/Database.drawio` (update) + re-export `project-res/Database.png`

Changes to `Requests` table:
- Add column: `status INT NOT NULL DEFAULT 0`
- Add column: `offering_user_id INT NULL`
- Add FK relationship: `offering_user_id -> Users.id`

Changes to `Notifications` table:
- Add column: `type INT NOT NULL DEFAULT 0`
- Add column: `related_request_id INT NULL`
- Add FK relationship: `related_request_id -> Requests.request_id`

### 8.2 UML Class Diagram

**File:** `project-res/UmlDiagram.drawio` (update) + re-export `project-res/UmlDiagram.png`

**New enums to add:**
- `RequestStatus` { Open, OfferPending, Accepted, Cancelled }
- `NotificationType` { Informational, OfferReceived, OfferResult }
- `OfferError` { NOT_FOUND, NOT_OWNER, ALREADY_HAS_OFFER, REQUEST_NOT_OPEN }
- `ApproveOfferError` { NOT_FOUND, NOT_RENTER, NO_PENDING_OFFER, TRANSACTION_FAILED }
- `DenyOfferError` { NOT_FOUND, NOT_RENTER, NO_PENDING_OFFER }

**Updated classes:**

`Request`:
- + Status: RequestStatus
- + OfferingUser: User?

`Notification`:
- + Type: NotificationType
- + RelatedRequestId: int?

`RequestDTO`:
- + Status: RequestStatus
- + OfferingUser: UserDTO?
- + CanOffer: bool (computed)

`NotificationDTO`:
- + Type: NotificationType
- + RelatedRequestId: int?
- + IsActionable: bool (computed)

`IRequestService`:
- + OfferGame(requestId, offeringUserId): int
- + ApproveOffer(requestId, renterId): int
- + DenyOffer(requestId, renterId): int

`IRequestRepository`:
- + UpdateStatus(requestId, status, offeringUserId): void

`INotificationRepository`:
- + GetActionableByRequestId(requestId): ImmutableList<Notification>

`INotificationService`:
- + DeleteNotificationsByRequestId(requestId): void

**New classes:**
- `CreateRequestViewModel` (with AvailableGames, SelectedGame, StartDate, EndDate, SaveRequest())
- `CreateRequestView` (page)

**Removed classes:**
- `ChatView`
- `ChatViewModel`

### 8.3 Use Case Diagram

**File:** `project-res/BookingUseCaseDiagram.drawio`

Add new use cases:
- Renter: "Create Request" (from My Requests page)
- Owner: "Offer Game" (from Others' Requests page)
- Renter: "Approve Offer" (from Notifications page)
- Renter: "Deny Offer" (from Notifications page)

Remove:
- "Chat with Owner/Renter" (mock chat removed)

---

## Phase 9: Migration Script

**New file:** `Property_and_Management/Scripts/migrate_offer_system.sql`

Complete migration script to run on existing databases:

```sql
USE BoardRent;
GO

-- Add status and offering_user_id to Requests
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'status')
BEGIN
    ALTER TABLE Requests ADD status INT NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'offering_user_id')
BEGIN
    ALTER TABLE Requests ADD offering_user_id INT NULL;
    ALTER TABLE Requests ADD CONSTRAINT FK_Request_OfferingUser
        FOREIGN KEY (offering_user_id) REFERENCES [Users](id);
END;

-- Add type and related_request_id to Notifications
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'type')
BEGIN
    ALTER TABLE Notifications ADD type INT NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'related_request_id')
BEGIN
    ALTER TABLE Notifications ADD related_request_id INT NULL;
    ALTER TABLE Notifications ADD CONSTRAINT FK_Notification_Request
        FOREIGN KEY (related_request_id) REFERENCES Requests(request_id);
END;
GO
```

---

## Implementation Order

| Order | Phase | Description | Dependencies |
|-------|-------|-------------|--------------|
| 1 | Phase 1 | Data models, enums, DTOs, mappers | None |
| 2 | Phase 9 | Migration script + update generate_db.sql | Phase 1 |
| 3 | Phase 2 | Repository layer updates | Phase 1 |
| 4 | Phase 3 | Service layer (OfferGame, ApproveOffer, DenyOffer) | Phase 2 |
| 5 | Phase 4 | Create Request UI | Phase 3 |
| 6 | Phase 5 | Green Offer button on Others' Requests | Phase 3 |
| 7 | Phase 6 | Actionable notifications (Approve/Deny) | Phase 3 |
| 8 | Phase 7 | Remove ChatView + clean up | Phases 5, 6 |
| 9 | Phase 8 | Diagram updates | All phases |

Phases 5, 6, and 4 can be implemented in parallel after Phase 3 is complete.

---

## Files Changed Summary

### New Files
| File | Purpose |
|------|---------|
| `src/Model/RequestStatus.cs` | Request status enum |
| `src/Model/NotificationType.cs` | Notification type enum |
| `src/Views/CreateRequestView.xaml` | Create request page |
| `src/Views/CreateRequestView.xaml.cs` | Create request code-behind |
| `src/Viewmodels/CreateRequestViewModel.cs` | Create request view model |
| `Scripts/migrate_offer_system.sql` | DB migration script |

### Modified Files
| File | Change |
|------|--------|
| `src/Model/Request.cs` | Add Status, OfferingUser fields |
| `src/Model/Notification.cs` | Add Type, RelatedRequestId fields |
| `src/DTO/RequestDTO.cs` | Add Status, OfferingUser, CanOffer |
| `src/DTO/NotificationDTO.cs` | Add Type, RelatedRequestId, IsActionable |
| `src/Mapper/RequestMapper.cs` | Map new fields |
| `src/Mapper/NotificationMapper.cs` | Map new fields |
| `src/Repository/RequestRepository.cs` | New columns in SQL, UpdateStatus method |
| `src/Repository/NotificationRepository.cs` | New columns in SQL, GetActionableByRequestId |
| `src/Interface/IRequestRepository.cs` | Add UpdateStatus |
| `src/Interface/INotificationRepository.cs` | Add GetActionableByRequestId |
| `src/Service/RequestService.cs` | Add OfferGame, ApproveOffer, DenyOffer |
| `src/Interface/IRequestService.cs` | Add new method signatures |
| `src/Service/NotificationService.cs` | Add DeleteNotificationsByRequestId |
| `src/Interface/INotificationService.cs` | Add new method signature |
| `src/Views/RequestsFromOthersPage.xaml` | Green Offer button, remove tap navigation |
| `src/Views/RequestsFromOthersPage.xaml.cs` | OfferButton_Click handler |
| `src/Viewmodels/RequestsFromOthersViewModel.cs` | Add OfferGame method |
| `src/Views/RequestsToOthersPage.xaml` | Add Create Request button, remove tap |
| `src/Views/RequestsToOthersPage.xaml.cs` | Add CreateRequest navigation, remove tap |
| `src/Views/NotificationsPage.xaml` | Conditional Approve/Deny buttons |
| `src/Views/NotificationsPage.xaml.cs` | ApproveOffer/DenyOffer handlers |
| `src/Viewmodels/NotificationsViewModel.cs` | Add IRequestService dep, offer methods |
| `src/Views/RentalsFromOthersPage.xaml` | Remove tap navigation |
| `src/Views/RentalsFromOthersPage.xaml.cs` | Remove ChatView navigation |
| `src/Views/RentalsToOthersPage.xaml` | Remove tap navigation |
| `src/Views/RentalsToOthersPage.xaml.cs` | Remove ChatView navigation |
| `App.xaml.cs` | Register CreateRequestViewModel, remove ChatViewModel |
| `Scripts/generate_db.sql` | Add new columns to CREATE TABLE statements |
| `project-res/Database.drawio` | ER diagram updates |
| `project-res/UmlDiagram.drawio` | Class diagram updates |
| `project-res/BookingUseCaseDiagram.drawio` | Use case updates |

### Deleted Files
| File | Reason |
|------|--------|
| `src/Views/ChatView.xaml` | Mock chat removed |
| `src/Views/ChatView.xaml.cs` | Mock chat removed |
| `src/Viewmodels/ChatViewModel.cs` | Mock chat removed |
