# Rock PR Policy
The Rock Community loves Pull Requests! In an effort to 'row in the same direction' and minimize wasted development time
on your part and review time on ours, we have implemented the following guidelines for PRs:
1. If you are submitting a PR for a logged Issue / Enchancement request please reference it in your commit (Fixes #1234 or Closes #2445)
2. If your PR is for an enchancment that has not been discussed and approved by the core team please get that approval BEFORE submitting
the request. In fact, this approval should be recieved before writing the code to limit rework on your part. This will ensure that all code is working into the same vision and direction of the core project.


## Contributor Agreement
_Have you filled out and sent your [Spark Contributor Agreement](http://www.rockrms.com/Content/RockExternal/Misc/Contributor%20Agreement.pdf) to secretary [at] sparkdevnetwork.org?_

## Context
_What is the problem you encountered that lead to you creating this pull request?_

## Goal
_What will this pull request achieve and how will this fix the problem?_

## Strategy
_How have you implemented your solution?_

## Tests
_If your code is a new method or function (that doesn't need a mock database or SqlServerTypes library) and can be Xunit tested [see example](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests/Rock/Lava/RockFiltersTests.cs) be sure your pull request includes the corresponding unit tests in the Rock.Tests project. In all cases *you* MUST test your code before submitting a pull request._

## Possible Implications
_What could this change potentially impact? Are there any security considerations? Where could this potentially affect backwards compatibility?_

## Screenshots
_Provide us some screenshots if your pull request either alters existing UI or provides new UI. Arrows and labels are helpful._

## Documentation
_If your change effects the UI or needs to be documented in one of the existing [user guides](http://www.rockrms.com/Learn/Documentation), please provide the brief write-up here:_

## Migrations
Should your pull request require a migration, please exclude the migration from the Rock.Migration project, but submit it in your pull request. Please add a note to your pull request that provides a heads up that a migration file is present.
