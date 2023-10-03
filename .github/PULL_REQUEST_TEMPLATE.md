
## Notice

**In case you are submitting a non bug-fix-PR, we highly recommend you to engage in a PR discussion first.**

There are many factors we consider before accepting a pull request. This includes:
1. Whether or not the Rock system you run is a standard, main-line build. If it is not, there is a lower chance we will accept your request since it may impact some other part of the system you don't regularly use.
2. Features that would be used by less than 80% of Rock organizations, or ones that don't match the goals of Rock.

With the PR discussion we can assess your proposed changes before you start working on it so that we can come up with the best possible approach to it. This may include:
1. Coming up with an alternate approach that does not involve changes to core.
2. Advising how your proposed solution be done in a different way that is more efficient and consistent with the rest of the system.
3. Have one of our core developers make the changes for you. This may be the case if the change involves intricate tasks like an EF migration or something similar.


## Proposed Changes

<!--
Describe the big picture of your changes here to communicate to the maintainers why we should accept this pull request. If it fixes a bug or resolves a feature request, be sure to link to that issue.

Please include screenshots if your pull request either alters existing UI or provides new UI. Arrows and labels are helpful.
-->

Fixes: #

## Types of changes

What types of changes does your code introduce to Rock?
_Put an `x` in the boxes that apply_

- [ ] Bugfix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality, which has been approved by the core team)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)

## Checklist

_Put an `x` in the boxes that apply. You can also fill these out after creating the PR. If you're unsure about any of them, don't hesitate to ask. We're here to help! This is simply a reminder of what we are going to look for before merging your code._

- [ ] This is a single-commit PR. (If not, please squash your commit and re-submit it.)
- [ ] I verified my PR does not include whitespace/formatting changes -- because if it does it will be closed without merging.	
- [ ] I have read the [Contributing to Rock](https://github.com/SparkDevNetwork/Rock/blob/master/.github/CONTRIBUTING.md) doc
- [ ] By contributing code, I agree to license my contribution under the [Rock Community License Agreement](https://www.rockrms.com/license)
- [ ] Unit tests pass locally with my changes
- [ ] I have added any required [unit tests](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests.UnitTests/README.md) or [integration tests](https://github.com/SparkDevNetwork/Rock/blob/develop/Rock.Tests.Integration/README.md) that prove my fix is effective or that my feature works
- [ ] I have included updated language for the [Rock Documentation](https://www.rockrms.com/Learn/Documentation) (if appropriate)

## Further comments
<!--
If this is a relatively large or complex change, kick off the discussion by explaining why you chose the solution you did and what alternatives you considered, etc...
-->

## Documentation
<!--
If your change effects the UI or needs to be documented in one of the existing docs http://www.rockrms.com/Learn/Documentation, please provide the brief write-up here.
-->

## Migrations
If your pull request requires a migration, please *exclude the migration from the Rock.Migration project*, but submit it with your pull request. Please add a note to your pull request that provides a heads up that a migration file is present.
