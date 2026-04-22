# Unimplemented Requirements Change Log

Track every file touched while implementing tasks from `ComponentDocumentation/unimplemented-requirements.md`.

## Task 1 - REQ-GAM-06: Field-Specific Validation Error Messages

- Updated behavior: validation popup now displays all failing field messages at once.
- Updated behavior: price validation now enforces `>= 1` and price field binding updates immediately while typing.
- Updated behavior: Save explicitly parses the current price input text before validation to avoid stale `NumberBox.Value` reads.
- Updated behavior: removed UI `Minimum` clamp from price input so negative values stay visible and are rejected only by validation.
- All files changed for Task 1:
- `Property_and_Management/src/Viewmodels/CreateGameViewModel.cs`
- `Property_and_Management/src/Views/CreateGameView.xaml`
- `Property_and_Management/src/Views/CreateGameView.xaml.cs`
- `Property_and_Management/src/Viewmodels/EditGameViewModel.cs`
- `Property_and_Management/src/Views/EditGameView.xaml`
- `Property_and_Management/src/Views/EditGameView.xaml.cs`
