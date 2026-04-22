# Notifications Verification Status

Current branch notification coverage and manual verification status.

## 1) Booking Unavailable
- Trigger: request approved and overlapping requests are auto-cancelled (`ApproveRequest`)
- Recipient: overlapping request renter(s)
- Type: `Informational`
- Source: `Property_and_Management/src/Service/RequestService.cs` (ApproveRequest, around line 186)
- Status: **Reviewed, not fully verified yet**
- Notes: cannot be fully verified in current setup; needs at least 3 users / a test scenario that reliably creates overlapping requests and then approves one.

## 2) Rental request declined
- Trigger: owner denies request (`DenyRequest`)
- Recipient: renter
- Type: `Informational`
- Source: `Property_and_Management/src/Service/RequestService.cs` (DenyRequest, around line 262)
- Status: **Completed and verified**

## 3) Rental request cancelled
- Trigger: game deleted and message to the existing requests on that game(`OnGameDeactivated`)
- Recipient: renter(s) with pending request
- Type: `Informational`
- Source: `Property_and_Management/src/Service/RequestService.cs` (OnGameDeactivated, around line 298)
- Status: **Completed and verified**

## 4) Rental Confirmed
- Trigger: N/A in current UI flow
- Recipient: N/A
- Type: N/A
- Source: `Property_and_Management/src/Service/RequestService.cs` (`ApproveOffer` legacy path)
- Status: **Not used in active flow**

## 5) Offer Denied
- Trigger: N/A in current UI flow
- Recipient: N/A
- Type: N/A
- Source: `Property_and_Management/src/Service/RequestService.cs` (`DenyOffer` legacy path)
- Status: **Not used in active flow**

## 6) Offer Declined
- Trigger: N/A in current UI flow
- Recipient: N/A
- Type: N/A
- Source: `Property_and_Management/src/Service/RequestService.cs` (`DenyOffer` legacy path)
- Status: **Not used in active flow**

## 7) Upcoming Rental Reminder
- Trigger: scheduled 24h before rental start
- Recipient: renter and owner
- Type: `Informational` (default)
- Source: `Property_and_Management/src/Service/NotificationService.cs` (ScheduleUpcomingRentalReminder, around line 150)
- Status: **Not yet verified**
