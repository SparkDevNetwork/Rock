# Rock PR Policy
The Rock Community loves Pull Requests! In an effort to 'row in the same direction' and minimize wasted development time
on your part and review time on ours, we have implemented the following guidelines for PRs:
1. If you are submitting a PR for a logged Issue / Enhancement request please reference it in your commit
2. If your PR is for an enhancement that has not been discussed and approved by the core team please get that approval BEFORE submitting
the request. In fact, this approval should be received before writing the code to limit rework on your part. This will ensure that all code is working into the same vision and direction of the core project.


## Contributor Agreement
_By contributing your code, you agree to license your contribution under the [Rock Community License Agreement](https://www.rockrms.com/license)._

## Context
_What is the problem you encountered that lead to you creating this pull request?_

## Goal
_What will this pull request achieve and how will this fix the problem?_

## Strategy
_How have you implemented your solution?_

## Tests
>NOTE: _Tests are now required._

1. _Your pull request MUST include corresponding tests in the Rock.Tests project._ 
2. _Regardless of writing a test, *you* MUST always test your code before submitting a pull request._
3. _Read item 0 from the [readme.txt](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Readme.txt) in the Rock.Tests project._

### Tests That Don't Require a Database
_If your test doesn't require a database or SqlServerTypes library follow [this example](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Rock/Utility/ExtensionMethods/StringExtensionsTests.cs) or one of the other similar test classes._

### Tests That Require a Database
_Read the entire [readme.txt](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Readme.txt) in the Rock.Tests project._

_Follow [this example](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Rock/Model/AttendanceCodeTests.cs) but remove the "Skip" property from the [Fact] decoration during testing. You should use a stock database seeded with the official "Sample Data" loaded via [the block](https://github.com/SparkDevNetwork/Rock/blob/develop/RockWeb/Blocks/Examples/SampleData.ascx.cs) under Power Tools._

_Once your tests pass, add the "Skip" property back into the [Fact] decoration (for now) before you commit it._

## Possible Implications
_What could this change potentially impact? Are there any security considerations? Where could this potentially affect backwards compatibility?_

## Screenshots
_Provide us some screenshots if your pull request either alters existing UI or provides new UI. Arrows and labels are helpful._

## Documentation
_If your change effects the UI or needs to be documented in one of the existing [user guides](http://www.rockrms.com/Learn/Documentation), please provide the brief write-up here:_

## Migrations
Should your pull request require a migration, please exclude the migration from the Rock.Migration project, but submit it in your pull request. Please add a note to your pull request that provides a heads up that a migration file is present.
