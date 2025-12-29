# Define **Standard Expectations API**

- [X] Request For Comments (Gathering Feedback)
- [X] Internal Development (High Rate of Change)
- [ ] On going Maintenance (Low Rate of Change)

## Motivation

Introduce API to validate run-time expectations about application state and arguments.
Many such "Guard APIS" exist already and numerous assertions methods are available for the most specialised scenarios.
The motivation for this new API is not to replicate existing functionality, but to provide a small set of expectation checking methods
making use of similarily small set of [Standard Expcetions](Define-Standard-Exceptions.md), working in tandem to provide a more complete
experience.
