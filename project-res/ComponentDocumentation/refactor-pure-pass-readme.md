# Pure Refactor Pass Checklist (No Behavior Changes)

Use this checklist for the refactor requirement and keep functionality unchanged.

## 1. Rename unclear symbols to semantic names
- `vm` -> `requestsViewModel`
- `dto` -> `notificationDto`
- `r` in lambdas -> `request` / `rental` / `notification`
- Avoid short/abbreviated locals unless universally standard (`id` is usually fine).

## 2. Remove abbreviations from lambdas
- `requests.Where(r => ...)` -> `requests.Where(request => ...)`

## 3. Replace magic numbers with named constants
- Examples to replace: `48`, `3`, `10`, validation limits, retry counts, etc.
- Define as `private const` values with semantic names.

## 4. Replace repeated literal strings with constants
- Notification titles
- Repeated dialog text
- Repeated validation messages

## 5. Extract repeated logic into methods
- Parsing/validation blocks
- Repeated dialog creation
- Repeated notification construction

## 6. Keep method intent explicit
- Prefer small methods with clear names like:
  - `BuildValidationErrors`
  - `CreateReminderBody`
  - `TryParsePriceInput`

## 7. Use IDE rename + compile after each small batch
- Prevent regressions and broken bindings.

## 8. Final verification
- Run build + smoke test all previously working flows:
  - Validation
  - Requests
  - Notifications
  - Rentals

## Suggested execution order
1. Refactor ViewModels first.
2. Refactor Services second.
3. Build and smoke test after each batch.
