# Lava and SQL

Load when the code touches Lava templates, Lava commands, filters, or shortcodes,
`ResolveMergeFields`, SQL, the `sql` command, or RunSQL workflow actions.

## Lava

- **No Lava injection.** User input is never rendered as Lava, even with no commands enabled.
  Filters need no command: `PersonById` has no security check, and `PersonTokenCreate` mints a
  sign-in token. Look for `ResolveMergeFields` on typed text, and for Lava output that holds
  user values being rendered a second time.
- **Explicit enabled commands.** New code that renders Lava passes an explicit
  `EnabledLavaCommands` value, usually from a block setting. The overloads without one silently
  use the global default. Enabling `Sql`, `Execute`, `WebRequest`, `RockEntity*`, or `All`
  needs a clear reason.
- **Entity commands.** Entity commands check VIEW by default (`securityenabled` defaults to
  true), but `{% person %}` never checks security. Any `securityenabled:'false'` or
  `{% person %}` fed by a page parameter or user input is a finding.
- **New Lava commands.** A new command that reads or writes data implements `ILavaSecured` and
  calls `IsAuthorized( context )` at the top of `OnRender`. `ILavaSecured` alone only lists the
  command in the picker. It enforces nothing.
- **Shortcodes.** A shortcode with its own enabled commands must not pass them on to the
  calling template, and parameters passed into it are data, not Lava. User values in shortcode
  parameters are a Lava injection path.
- **`WebRequest` and `Execute`** never take a URL or code built from user input.

## SQL

- **Parameters, not string building.** Lava values in SQL are passed as parameters:
  `{% sql personid:'{{ Id }}' %} ... WHERE [Id] = @personid`, or RunSQL's Parameters setting.
  In C#, no SQL built from input with `+` or interpolated `$@"..."` strings. Use LINQ or
  parameterized queries.
- **`SanitizeSql` only doubles single quotes.** It is safe only inside a quoted string. It does
  nothing for numbers, `IN` lists, or identifiers. This includes system-shipped workflow
  actions.
