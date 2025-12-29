# Define **Standard Exceptions**

- [X] Request For Comments (Gathering Feedback)
- [X] Internal Development (High Rate of Change)
- [ ] On-going Maintenance (Low Rate of Change)

## Motivation

Introduce a small set of Standard Exceptions on top of an existing (large) set
of all exceptions already available in .NET and libraries. The "standard" provided
by new exceptions includes:

- **Stable Message** that is not changed by localization or formatting of dynamic parameters.
  Stable Message might still include information static in given context (e.g. argument name but not argument value).
  This can provide an additional reference point for tracking recurring errors between different versions of application,
  supporing stack traces and other such data points.
- Consistent way of attaching and retrieving of Exception Data as it travels moves through call stack.
- Consistent way of decorating exception with HTTP Status Code-like information to mark the origin of the problem
  (incoming client connection, internal server problem, outgoing downstream system).
- Other such scenarios.
