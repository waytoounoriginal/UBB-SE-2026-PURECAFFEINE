# Notification Component Documentation

A concise technical overview of the Notification architecture and its implementation details.

## System Architecture

The notification system consists of three parts:
1. **ServerCommunication**: Shared message contracts.
2. **NotificationServer**: Standalone UDP message router.
3. **Property_and_Management (App)**: The client application handling UI, DB, and network I/O.

### 1. ServerCommunication
Shared library containing UDP message definitions and serialization constraints.
- `MessageBase`: Base type for UDP messages.
- `SubscribeToServerMessage`: Maps a client app's `IPEndPoint` to a `UserId` on the server.
- `SendNotificationMessage`: Action payload carrying notification data targeted at a specific `UserId`.

### 2. NotificationServer (UDP)
A standalone console application (`UdpNotificationServer`) acting as a message broker.
- **Registration**: Listens for `SubscribeToServerMessage` to map and cache `UserId` -> `IPEndPoint`.
- **Relaying**: Listens for `SendNotificationMessage` and forwards the UDP datagram directly to the target user's registered endpoint.

### 3. Client App (Property_and_Management)
Handles network interactions, local persistence, and UI updates.

**Network Layer (`NotificationClient`)**
- Implements `IServerClient` using `UdpClient(0)` (auto-assigned port).
- Publishes incoming messages to subscribers using the `IObservable<MessageBase>` pattern.

**Service Layer (`NotificationService`)**
- Implements `INotificationService`. Acts as the central orchestrator.
- **Sending**: Persists via `NotificationRepository`, then calls `IServerClient.SendNotification` to push over UDP.
- **Receiving**: Subscribes to `NotificationClient`. Converts incoming messages to `NotificationDTO`, shows a native OS Toast (`AppNotificationManager.Default.Show`), and broadcasts to UI subscribers.
- **Scheduling**: Handles delayed logic (e.g., `ScheduleUpcomingRentalReminder` 24h prior) using background `Task.Delay`.

**Data Layer (`NotificationRepository`, Model, DTO)**
- `Notification`: Internal entity model with `Id`, `User`, `Timestamp`, `Title`, `Body`.
- `NotificationDTO`: View representation. 
- `NotificationRepository`: Executes CRUD operations against the database.

**UI Layer (`NotificationsViewModel` & View)**
- Subscribes to `NotificationService` (`IObservable<NotificationDTO>`).
- Dynamically updates the `ObservableCollection` bound to `NotificationsPage.xaml` so the UI reflects incoming UDP messages in real time.
