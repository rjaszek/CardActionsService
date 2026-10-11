# CardActionsService
Recruitment task for a .NET Developer position

## Goal
A C# microservice for generating allowed actions for a user’s card. 
The microservice exposes an API endpoint that accepts a user ID and a card number and returns the allowed actions for that card.

## Endpoint
GET `api/users/{userId}/cards/{cardNumber}/actions`

**Parameters**
- `userId` (string, route) – user identifier
- `cardNumber` (string, route) – card number

**Responses**
- `200` – list of actions found - can be empty
- `404` – card for user not found

## Conditions file - input data
As conditions file data.json is used. Rules used in that file:
- All keys are written in PascalCase (AllowedActions, Name, Conditions, CardKind, CardStatus)
- CardKind - allowed enum values: Prepaid, Debit, Credit
- CardStatus - dictionary (key: CardStatus, value: PinStatus) 
	(Card is always in one state and only one pin state can be set)
- CardStatus key - allowed enum values: Ordered, Inactive, Active, Restricted, Blocked, Expired, Closed
- PinStatus - allowed enum values: Any, PinSet, PinNotSet
- File is edited by a human

example:
```
{
  "AllowedActions": [
    {
      "Name": "ACTION1",
      "Conditions": {
        "CardKind": [
          "Prepaid",
          "Debit",
          "Credit"
        ],
        "CardStatus": {
          "Active": "Any"
        }
      }
    },
    {
      "Name": "ACTION2",
      "Conditions": {
        "CardKind": [
          "Prepaid",
          "Debit",
          "Credit"
        ],
        "CardStatus": {
          "Inactive": "Any"
        }
      }
    }
  ]
}
```

## Validation
- data.json is validated on startup and on every file change
- Returns all errors at once
- What is validated?
	- AllowedActions section exists and is not empty
	- Action name exists and is not empty
	- Action names are unique
	- Action conditions section exists and is not empty
	- CardKind and CardStatus are not empty
	- Each value of CardKind
	- Each key of CardStatus and its PinStatus
	- PinStatus cannot be set as Undefined
- Invalid file on startup: the application fails to start
  (`ValidateOnStart` throws `OptionsValidationException` with the list of all errors - no HTTP requests are served)
- File broken while the application is running: the file is reloaded and validated again on the next request - the request fails with HTTP 500 until the file is fixed

## Validation design decisions
### Validation against raw configuration, not bound objects
- ConfigurationBinder silently skips list items that cannot be converted to an enum (e.g. "Prepaid1" disappears from the list without an error)
- The binder converts numeric strings to enums ("1" becomes the enum value with number 1)
- For this reason, enum values are checked against the raw IConfiguration (GetChildren() + Enum.TryParse + Enum.IsDefined)
- Bound objects are used only by the application

### Rejecting numeric values
- Where an enum name is expected, numbers ("0", "1", "-1", " 1 ") are rejected as an unknown value, even if they fall within the enum’s range, 
  because the data.json is edited by a human and also reordering the enum members would silently change the meaning of a number in the file

### Case sensitivity
- Enum names are accepted regardless of case (prepaid, PINSET are valid)
- Configuration keys in .NET are also case-insensitive, so Active and active within a single CardStatus are the same key

### PinStatus.Undefined
- Undefined = 0 is the default value
- It makes it possible to distinguish “not set” from a deliberate choice
- It is forbidden in configuration and does not appear in the list of allowed values in error messages

## Tests
- The CardActionsService.Tests project (xUnit)
- Validator tests: valid cases, missing and empty sections, unknown values, numbers, Undefined, lowercase names, duplicates, multiple errors at once
- Rules tests: valid cases, empty list of rules, type/status/PIN matching (incl. Any), no matching, task example checked against data.json
- Technique for validator: a base configuration loaded from a file + AddInMemoryCollection to override individual keys (“last source wins”)
- Technique for rules: fake OptionsMonitor

## Comments
- Suppress CS1591 warning - not every method or class needs comment so warning about missing XML comment is not needed