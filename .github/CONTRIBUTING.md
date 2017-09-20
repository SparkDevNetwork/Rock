# Contributing to Rock

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

The following is a set of guidelines for contributing to [Rock](https://github.com/SparkDevNetwork/Rock) and its packages, which are hosted in the [Spark Development Network organization](https://github.com/SparkDevNetwork) on GitHub.
These are just guidelines, not rules, use your best judgment and feel free to propose changes to this document in a pull request.

#### Table Of Contents

[I don't want to read this whole thing, I just have a question!!!](#i-dont-want-to-read-this-whole-thing-i-just-have-a-question)

[What should I know before I get started?](#what-should-i-know-before-i-get-started)

  * [License](#license)
  * [Rock and Plugins](#rock-and-plugins)

[How Can I Contribute?](#how-can-i-contribute)

  * [Reporting Bugs](#reporting-bugs)
  * [Suggesting Enhancements](#suggesting-enhancements)
  * [Your First Code Contribution](#your-first-code-contribution)
  * [Pull Requests](#pull-requests)

[Styleguides](#styleguides)

  * [Git Commit Messages](#git-commit-messages)
  * [JavaScript Styleguide](#javascript-styleguide)
  * [C# and ASPX Styleguide](#c-and-aspx-styleguide)
  * [CSS and Less Styleguide](#css-and-less-styleguide)
  * [SQL Styleguide](#sql-styleguide)

[Additional Notes](#additional-notes)

  * [Issue and Pull Request Labels](#issue-and-pull-request-labels)

## I don't want to read this whole thing I just have a question!!!

> **Note:** Please don't file an issue to ask a question. You'll get faster results by using the resources below.

Rock has an official [Q&A site](https://www.rockrms.com/Rock/Ask), where the community chimes in with helpful advice and best practices.

* [Ask Rock, Q&A](https://www.rockrms.com/Rock/Ask)
* [Rock Documentation](https://www.rockrms.com/Learn)
* [RockU Video Training](https://www.rockrms.com/rocku)

If chat is more your speed, you can join the Rock community on Slack:

* [Join the Rock Conversation on Slack!](https://www.rockrms.com/slack)
    * Even though Slack is a chat service, sometimes it takes several hours for community members to respond &mdash; please be patient!
    * Use the `#general` channel for general questions or discussion about Rock
    * Use the `#troubleshooting` channel for help with specific issues
    * There are many other channels available, check the channel list

## What should I know before I get started?

### License

By contributing your code, you agree to license your contribution under the [Rock Community License Agreement](https://www.rockrms.com/license).  We also need you to sign and send a scanned copy of the [Spark Contributor Agreement](http://www.rockrms.com/Content/RockExternal/Misc/Contributor%20Agreement.pdf) back to the Spark Secretary (Secretary @ sparkdevnetwork.org ) to keep the lawyer off our backs. 

### Rock and Plugins

The power of Rock is that it isn't limited by what the core team provides. Instead the community is able to extend Rock to the furthest reaches of their creativity and abilities. Rock [can be extended](https://www.rockrms.com/Rock/Developer/BookContent/26/26) through plugins, themes, workflow actions, jobs, and external applications added through the Rock Shop or manually.
Because Rock is so extensible, it's possible that a feature you've become accustomed to in Rock, or an issue you're encountering isn’t coming from a bundled feature at all, but rather a plugin you’ve installed.
Contact information for products purchased in the Rock Shop are available on the item page in the store.

## How Can I Contribute?

### Reporting Bugs

This section guides you through submitting a bug report for Rock. Following these guidelines helps maintainers and the community understand your report :pencil:, reproduce the behavior :computer: :computer:, and find related reports :mag_right:.

Before creating bug reports, please check [this list](#before-submitting-a-bug-report) as you might find out that you don't need to create one. When you are creating a bug report, please [include as many details as possible](#how-do-i-submit-a-good-bug-report) and be sure to fill out the provided template.

#### Before Submitting A Bug Report

* **Check the [Rock Developer Q & A site](http://www.rockrms.com/Rock/Ask/Developing)** for a list of common questions and problems.
* **Determine [which repository the problem should be reported in](#rock-and-packages)**.
* **Perform a [cursory search](https://github.com/issues?q=+is%3Aissue+user%3ASparkDevNetwork)** to see if the problem has already been reported. If it has, add a reaction to the existing issue instead of opening a new one or commenting.

#### How Do I Submit A (Good) Bug Report?

Bugs are tracked as [GitHub issues](https://guides.github.com/features/issues/). After you've determined [which repository](#rock-and-plugins) your bug is related to, create an issue on that repository and provide the following information.

Explain the problem and include additional details to help maintainers reproduce the problem:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the exact steps which reproduce the problem** in as many details as possible. For example, start by explaining how you started Rock, e.g. which command exactly you used in the terminal, or how you started Rock otherwise. When listing steps, **don't just say what you did, but explain how you did it**. For example, if you moved the cursor to the end of a line, explain if you used the mouse, or a keyboard shortcut or an Rock command, and if so which one?
* **Provide specific examples to demonstrate the steps**. Include links to files or GitHub projects, or copy/pasteable snippets, which you use in those examples. If you're providing snippets in the issue, use [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines).
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
* **Explain which behavior you expected to see instead and why.**
* **Include screenshots and animated GIFs** which show you following the described steps and clearly demonstrate the problem. You can use [this tool](http://www.cockos.com/licecap/) to record GIFs on macOS and Windows.
* **If the problem wasn't triggered by a specific action**, describe what you were doing before the problem happened and share more information using the guidelines below.

Provide more context by answering these questions:

* **Can you reproduce the problem on a fresh install or the [demo site](http://rock.rocksolidchurchdemo.com/)?**
* **Did the problem start happening recently** (e.g. after updating to a new version of Rock) or was this always a problem?
* If the problem started happening recently, **can you reproduce the problem in an older version of Rock?** What's the most recent version in which the problem doesn't happen?
* **Can you reliably reproduce the issue?** If not, provide details about how often the problem happens and under which conditions it normally happens.
* Include [details about your configuration and environment](#environment-and-diagnostics-information) including **Rock Version**, **Client Culture Setting** and which (if any) [plugins](#rock-and-plugins) you have installed?


### Suggesting Enhancements

The core team is not short on ideas -- we're short on people who can execute ideas. But, if you have an idea
for enhancing Rock that is absolutely amazing we would like to know about it, and if you are a developer, 
we would love to guide you toward successfully executing that idea.

This section guides you through submitting an enhancement suggestion for Rock, including completely new features and  improvements to existing functionality. Following these guidelines helps maintainers and the community understand your suggestion :pencil: and find related suggestions :mag_right:.

Before creating enhancement suggestions, please check [this list](#before-submitting-an-enhancement-suggestion) as you might find out that you don't need to create one. When you are creating an enhancement suggestion, please [include as many details as possible](#how-do-i-submit-a-good-enhancement-suggestion). If you'd like, you can use [this template](#template-for-submitting-enhancement-suggestions) to structure the information.

#### Before Submitting An Enhancement Suggestion

* **Check if there's already a Rock Shop plugin which provides that enhancement.**
* **Check the [Community Projects List](https://www.rockrms.com/CommunityProjects) to see if developers are already working on that idea.**
* **Check the [plugin-discussion Slack channel](https://rockrms.slack.com/archives/plugin-discussion) and discuss your idea there.**

Based on the outcome of the above the core team may help shape your idea and ask you to implement it as a [Pull Request](#pull-request) provided you've already worked through [Your First Code Contribution](#your-first-code-contribution). 

#### How Do I Submit A (Good) Enhancement Suggestion?

If you are not a developer or cannot hire one to help you execute your idea, we'd still like to know about your enhancement suggestion.  We use a [black book](https://www.rockrms.com/Rock/Ideas) to capture your idea and ask that provide the following details:

* **Use a clear and descriptive title** to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Provide specific examples to demonstrate the idea**. Include copy/pasteable snippets which you use in those examples, as [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines).
* **Describe the current behavior** and **explain which behavior you expected to see instead** and why.
* **Include screenshots and animated GIFs** which help you demonstrate the steps or point out the part of Rock which the suggestion is related to. You can use [this tool](http://www.cockos.com/licecap/) to record GIFs on macOS and Windows.
* **Explain why this enhancement would be useful** to most Rock users and isn't something that can or should be implemented as a [plugin](#rock-and-plugins).
* **List some other sites or applications where this enhancement exists.**
* **Specify which version of Rock you're using.**

#### Template For Submitting Enhancement Suggestions

    [Short description of suggestion]

    **Steps which explain the enhancement**
    
    1. [First Step]
    2. [Second Step]
    3. [Other Steps...]

    **Current and suggested behavior**
    
    [Describe current and suggested behavior here]

    **Why would the enhancement be useful to most users**
    
    [Explain why the enhancement would be useful to most users]

    [List some other text editors or applications where this enhancement exists]
    
    **Screenshots and GIFs**

    ![Screenshots and GIFs which demonstrate the steps or part of Rock the enhancement suggestion is related to](url)

    **Rock Version:** [Enter Rock version here]

### Your First Code Contribution

Unsure where to begin contributing to Rock? You can start by looking through these `Claim It` issues:

* [Claim It][claim-it] - issues which should only require a small amount of code, and a test or two.

If you want to read about using Rock or developing plugins for Rock, the [Rock Documentation](https://www.rockrms.com/Learn/Documentation) and  [Developer Guides](https://www.rockrms.com/Developer) are free and available online.

### Pull Requests
The Rock Community loves Pull Requests! In an effort to 'row in the same direction' and minimize wasted development time
on your part and review time on ours, we have implemented the following guidelines for PRs:
1. If you are submitting a PR for a logged Issue / Enchancement request please reference it in your commit (Fixes #1234 or Closes #2445)
2. If your PR is for an enchancment that has not been discussed and approved by the core team please get that approval BEFORE submitting
the request. In fact, this approval should be recieved before writing the code to limit rework on your part. This will ensure that all code is working into the same vision and direction of the core project.

#### Keep In Mind The Following
* When making changes keep the changes simple (i.e. don't refactor boldly) otherwise the git diff will obscure your change and make it difficult to quickly see the actual change.
* Include screenshots and animated GIFs in your pull request whenever possible.
* Follow the [ASPX](#c-and-aspx-styleguide), [C#](#c-and-aspx-styleguide), [CSS](#css-styleguide), [JavaScript](#javascript-styleguide), and [SQL](#sql-styleguide) styleguides.
* Remember these [Committing Your Code standards](https://github.com/SparkDevNetwork/Rock/wiki/Committing-Your-Code)
* Use a tool like [GhostDoc](http://submain.com/download/ghostdoc/) to *help* document your methods.
* Try a tool like [CodeMaid](http://www.codemaid.net/) (free, in the Visual Studio store). It will auto-align your code, add brackets to single-line if statements, remove unnecessary whitespace and more.
* Should your pull request require a migration, please exclude the migration from the Rock.Migration project, but submit it in your
pull request. Please add a note to your pull request that provides a heads up that a migration file is present. 

## Styleguides

### Git Commit Messages

[Adhere to these standards.](https://github.com/SparkDevNetwork/Rock/wiki/Committing-Your-Code)

* Prefix each commit with a plus (+) or minus (-) sign.
* Use the past tense ("Added feature" not "Add feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally via the `(Fixes #0000)` notation as your commit suffix.

### C# and ASPX Styleguide

[Adhere to the Rock Coding Standards.](https://github.com/SparkDevNetwork/Rock/wiki/Coding-standards)

Use the Rock [vssettings file](https://github.com/sparkdevnetwork/rock/blob/master/Dev%20Tools/Env/rock_vs2015.vssettings)

- Use braces, even for single line cases (for, if/else, etc.).
- 4 spaces (no tabs)
- Insert space within parenthesis `( a > b )`
- Insert space after colon for base of interface in type declaration, after commas, after semicolons in "for" statement,
and before colon for base or interface in type declaration
- Use the Organize Usings | Remove and Sort tools when editing .cs files. Sort your usings putting 'System' directives first.

### CSS and LESS Styleguide

[Adhere to the Code Guide.](http://codeguide.co/#css)

- When feasible, default color palettes should comply with [WCAG color contrast guidelines](http://www.w3.org/TR/WCAG20/#visual-audio-contrast).
- Except in rare cases, don't remove default `:focus` styles (via e.g. `outline: none;`) without providing alternative styles. See [this A11Y Project post](http://a11yproject.com/posts/never-remove-css-outlines) for more details.

### JavaScript Styleguide

[Adhere to the Code Guide.](https://github.com/SparkDevNetwork/Rock/wiki/JavaScript-Coding-Standards)

- Use IDs, not classes as selectors `$('#hfItemId_' + this.options.controlId);`
- `CamelCasing` for all variable declarations
- `PascalCasing` for class declarations
- 2 spaces (no tabs)
- strict mode

### SQL Styleguide

[Adhere to the Code Guide.](https://github.com/SparkDevNetwork/Rock/wiki/Coding-standards#sql-coding-standards)

- Capitalize reserved words
- Main keywords on new line
- Use brackets around table and column names
- Use proper capitalization for table and column names

## Additional Notes

### Issue and Pull Request Labels

This section lists the labels we use to help us track and manage issues and pull requests. Most labels are used across all `SparkDevNetwork` repositories, but some are specific to `SparkDevNetwork/Rock`.

[GitHub search](https://help.github.com/articles/searching-issues/) makes it easy to use labels for finding groups of issues or pull requests you're interested in. For example, you might be interested in [open issues across `SparkDevNetwork/Rock` and all Spark Development Network-owned packages which are labeled as bugs, but still need to be reliably reproduced][search-rock-repo-label-status-needs-info] or perhaps [open pull requests in `SparkDevNetwork/Rock` which haven't been reviewed yet][search-rock-repo-label-status-review-needed]. To help you find issues and pull requests, each label is listed with search links for finding open items with that label in `SparkDevNetwork/Rock` only and also across all Rock repositories. We  encourage you to read about [other search filters](https://help.github.com/articles/searching-issues/) which will help you write more focused queries.

The labels are loosely grouped by their purpose, but it's not required that every issue have a label from every group or that an issue can't have more than one label from the same group.

Please open an issue on `SparkDevNetwork/Rock` if you have suggestions for new labels, and if you notice some labels are missing on some repositories, then please open an issue on that repository.


#### Type of Issue and Issue State

#### Priority

| Priority Label name | `SparkDevNetwork/Rock` :mag_right: | `SparkDevNetwork`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `Priority: Critical` | [search][search-rock-repo-label-priority-critical] | [search][search-SparkDevNetwork-org-label-priority-critical] | Affects all production Rock installs in a way that impacts external users, impacts critical features like finances, registrations, etc or is a source for data corruption. |
| `Priority: High` | [search][search-rock-repo-label-priority-high] | [search][search-SparkDevNetwork-org-label-priority-high] | Affects most production Rock installs in a way that will be noticed. |
| `Priority: Low` | [search][search-rock-repo-label-priority-low] | [search][search-SparkDevNetwork-org-label-priority-low] | Affects a small number of Rock installations and will not be noticed by most users.|

#### Status

| Status Label name | `SparkDevNetwork/Rock` :mag_right: | `SparkDevNetwork`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `Status: Available` | [search][search-rock-repo-label-status-available] | [search][search-SparkDevNetwork-org-label-status-available] | No one has claimed responsibility for resolving this issue. Generally this will be applied to bugs and enhancement issues, but may be applied to others. |
| `Status: Confirmed` | [search][search-rock-repo-label-status-confirmed] | [search][search-SparkDevNetwork-org-label-status-confirmed] | It's clear what the subject of the issue is about, and what the resolution should be. |
| `Status: In Progress` | [search][search-rock-repo-label-status-in-progress] | [search][search-SparkDevNetwork-org-label-status-in-progress] | This issue is being worked on, and has someone assigned. |
| `Status: On Hold` | [search][search-rock-repo-label-status-on-hold] | [search][search-SparkDevNetwork-org-label-status-on-hold] | Similar to blocked, but is assigned to someone. May also be assigned to someone because of their experience, but it's recognized they are unable to process the issue at this time. |
| `Status: Completed` | [search][search-rock-repo-label-status-completed] | [search][search-SparkDevNetwork-org-label-status-completed] | Nothing further to be done with this issue. Awaiting to be closed by the requestor out of politeness, or can be closed by a project member. |
| `Status: Needs Info` | [search][search-rock-repo-label-status-needs-info] | [search][search-SparkDevNetwork-org-label-status-needs-info] | Issues that are unclear or cannot be reproduced as described. |
| `Status: Review Needed` | [search][search-rock-repo-label-status-review-needed] | [search][search-SparkDevNetwork-org-label-status-review-needed] | The issue has a PR attached to it which needs to be reviewed. Should receive review by others in the community, and at least one member / committer. Specifics on when merging PRs is allowed is still up for debate. |
| `Status: Revision Needed` | [search][search-rock-repo-label-status-revision-needed] | [search][search-SparkDevNetwork-org-label-status-revision-needed] | At least two people have seen issues in the PR that makes them uneasy. Submitter of PR needs to revise the PR related to the issue. |
| `Status: Abandoned` | [search][search-rock-repo-label-status-abandoned] | [search][search-SparkDevNetwork-org-label-status-abandoned] | It's believed that this issue is no longer important to the requestor and no one else has shown an interest in it. |
| `Status: Won't Fix` | [search][search-rock-repo-label-status-wont-fix] | [search][search-SparkDevNetwork-org-label-status-wont-fix] | The Rock core team has decided not to fix these issues for now, either because they're working as intended or for some other reason. |

#### Type

| Type Label name | `SparkDevNetwork/Rock` :mag_right: | `SparkDevNetwork`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `Type: Bug` | [search][search-rock-repo-label-type-bug] | [search][search-SparkDevNetwork-org-label-type-bug] | Confirmed bugs or reports that are very likely to be bugs. |
| `Type: Documentation` | [search][search-rock-repo-label-type-documentation] | [search][search-SparkDevNetwork-org-label-type-documentation] | Related to any type of documentation. |
| `Type: Enhancement` | [search][search-rock-repo-label-type-enhancement] | [search][search-SparkDevNetwork-org-label-type-enhancement] | Feature requests. |
| `Type: Feedback` | [search][search-rock-repo-label-type-feedback] | [search][search-SparkDevNetwork-org-label-type-feedback] | General feedback more than bug reports or feature requests. |
| `Type: Plugin Idea` | [search][search-rock-repo-label-type-plugin-idea] | [search][search-SparkDevNetwork-org-label-type-plugin-idea] | Feature request which might be good candidates for new plugins, instead of extending Rock. |
| `Type: Duplicate` | [search][search-rock-repo-label-type-duplicate] | [search][search-SparkDevNetwork-org-label-type-duplicate] | Issues which are duplicates of other issues, i.e. they have been reported before. |

#### Topic Categories

| Label name | `SparkDevNetwork/Rock` :mag_right: | `SparkDevNetwork`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `Topic: API` | [search][search-rock-repo-label-topic-api] | [search][search-SparkDevNetwork-org-label-topic-api] | Related to Rock’s public REST APIs. |
| `Topic: Check-in` | [search][search-rock-repo-label-topic-checkin] | [search][search-SparkDevNetwork-org-label-topic-checkin] | Related to Check-in and Attendance. |
| `Topic: Communications` | [search][search-rock-repo-label-topic-communications] | [search][search-SparkDevNetwork-org-label-topic-communications] | Related to Rock Communications: Email, SMS, MMS, Push Notifications, etc. |
| `Topic: Event Registration` | [search][search-rock-repo-label-topic-event-registration] | [search][search-SparkDevNetwork-org-label-topic-event-registration] | Related to Rock Event Registration. |
| `Topic: External App` | [search][search-rock-repo-label-topic-external-app] | [search][search-SparkDevNetwork-org-label-topic-external-app] | Related to Rock's external apps: Check Scanner, Statement Generator, Job Scheduler Service, and Windows Check-in Client. |
| `Topic: Finance` | [search][search-rock-repo-label-topic-finance] | [search][search-SparkDevNetwork-org-label-topic-finance] | Related to finance. |
| `Topic: Lava` | [search][search-rock-repo-label-topic-lava] | [search][search-SparkDevNetwork-org-label-topic-lava] | Related to the Lava templating language. |
| `Topic: Metrics` | [search][search-rock-repo-label-topic-metrics] | [search][search-SparkDevNetwork-org-label-topic-metrics] | Related to Metrics. |
| `Topic: Reporting` | [search][search-rock-repo-label-topic-reporting] | [search][search-SparkDevNetwork-org-label-topic-reporting] | Related to Reporting. |
| `Topic: Rock Internals` | [search][search-rock-repo-label-topic-rock-internals] | [search][search-SparkDevNetwork-org-label-topic-rock-internals] | Related to Rock Internals. |
| `Topic: Security` | [search][search-rock-repo-label-topic-security] | [search][search-SparkDevNetwork-org-label-topic-security] | Related to security. |
| `Topic: UI` | [search][search-rock-repo-label-topic-ui] | [search][search-SparkDevNetwork-org-label-topic-ui] | Related to visual design. |
| `Topic: Workflows` | [search][search-rock-repo-label-topic-workflows] | [search][search-SparkDevNetwork-org-label-topic-workflows] | Related to Workflows. |
| `Topic: Performance` | [search][search-rock-repo-label-topic-performance] | [search][search-SparkDevNetwork-org-label-topic-performance] | Related to performance. |
| `Topic: uncaught-exception` | [search][search-rock-repo-label-topic-uncaught-exception] | [search][search-SparkDevNetwork-org-label-topic-uncaught-exception] | Issues about uncaught exceptions. |
| `Topic: Crash` | [search][search-rock-repo-label-topic-crash] | [search][search-SparkDevNetwork-org-label-topic-crash] | Reports of Rock completely crashing. |
| `Topic: git` | [search][search-rock-repo-label-topic-git] | [search][search-SparkDevNetwork-org-label-topic-git] | Related to Git functionality. |

#### `SparkDevNetwork/Rock` Topic Categories

| Label name | `SparkDevNetwork/Rock` :mag_right: | `SparkDevNetwork`‑org :mag_right: | Description |
| --- | --- | --- | --- |
| `Topic: Installer` | [search][search-rock-repo-label-topic-installer] |  | Related to the Rock installer. |
| `Topic: Updater` | [search][search-rock-repo-label-topic-updater] |  | Related to the Rock updater. |




[search-rock-repo-label-priority-critical]: https://github.com/SparkDevNetwork/Rock/labels/Priority%3A%20Critical
[search-SparkDevNetwork-org-label-priority-critical]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Priority%3A+Critical"
[search-rock-repo-label-priority-high]: https://github.com/SparkDevNetwork/Rock/labels/Priority%3A%20High"
[search-SparkDevNetwork-org-label-priority-high]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Priority%3A+High"
[search-rock-repo-label-priority-low]: https://github.com/SparkDevNetwork/Rock/labels/Priority%3A%20Low
[search-SparkDevNetwork-org-label-priority-low]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Priority%3A+Low"


[search-rock-repo-label-status-available]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Available
[search-SparkDevNetwork-org-label-status-available]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Available"
[search-rock-repo-label-status-confirmed]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Confirmed
[search-SparkDevNetwork-org-label-status-confirmed]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Confirmed"
[search-rock-repo-label-status-in-progress]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20In%20Progress
[search-SparkDevNetwork-org-label-status-in-progress]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+In+Progress"
[search-rock-repo-label-status-on-hold]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20On%20Hold
[search-SparkDevNetwork-org-label-status-on-hold]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+On+Hold"
[search-rock-repo-label-status-completed]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Completed
[search-SparkDevNetwork-org-label-status-completed]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Completed"
[search-rock-repo-label-status-needs-info]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Needs%20Info
[search-SparkDevNetwork-org-label-status-needs-info]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Needs+Info"
[search-rock-repo-label-status-review-needed]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Review%20Needed
[search-SparkDevNetwork-org-label-status-review-needed]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Review+Needed"
[search-rock-repo-label-status-revision-needed]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Revision%20Needed
[search-SparkDevNetwork-org-label-status-revision-needed]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Revision+Needed"
[search-rock-repo-label-status-abandoned]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Abandoned
[search-SparkDevNetwork-org-label-status-abandoned]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Abandoned"
[search-rock-repo-label-status-wont-fix]: https://github.com/SparkDevNetwork/Rock/labels/Status%3A%20Won%27t%20Fix
[search-SparkDevNetwork-org-label-status-wont-fix]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Status%3A+Won%27t+Fix"


[search-rock-repo-label-type-bug]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Bug
[search-SparkDevNetwork-org-label-type-bug]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3AType%3A+Bug
[search-rock-repo-label-type-documentation]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Documentation
[search-SparkDevNetwork-org-label-type-documentation]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Type%3A+Documentation"
[search-rock-repo-label-type-enhancement]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Enhancement
[search-SparkDevNetwork-org-label-type-enhancement]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Type%3A+Enhancement"
[search-rock-repo-label-type-feedback]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Feedback
[search-SparkDevNetwork-org-label-type-feedback]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Type%3A+Feedback"
[search-rock-repo-label-type-plugin-idea]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Plugin%20Idea
[search-SparkDevNetwork-org-label-type-plugin-idea]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Type%3A%20Plugin%20Idea"
[search-rock-repo-label-type-duplicate]: https://github.com/SparkDevNetwork/Rock/labels/Type%3A%20Duplicate
[search-SparkDevNetwork-org-label-type-duplicate]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Type%3A%20Duplicate"


[search-rock-repo-label-topic-api]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20API
[search-SparkDevNetwork-org-label-topic-api]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20API"
[search-rock-repo-label-topic-checkin]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Check-in
[search-SparkDevNetwork-org-label-topic-checkin]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Check-in"
[search-rock-repo-label-topic-communications]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Communications
[search-SparkDevNetwork-org-label-topic-communications]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Communications"

[search-rock-repo-label-topic-event-registration]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Event%20Registration
[search-SparkDevNetwork-org-label-topic-event-registration]:https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Event%20Registration" 

[search-rock-repo-label-topic-external-app]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20External%20App
[search-SparkDevNetwork-org-label-topic-external-app]:https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20External%20App" 

[search-rock-repo-label-topic-finance]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Finance
[search-SparkDevNetwork-org-label-topic-finance]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Finance"

[search-rock-repo-label-topic-lava]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Lava
[search-SparkDevNetwork-org-label-topic-lava]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Lava"

[search-rock-repo-label-topic-metrics]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Metrics
[search-SparkDevNetwork-org-label-topic-metrics]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Metrics"

[search-rock-repo-label-topic-reporting]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Reporting
[search-SparkDevNetwork-org-label-topic-reporting]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Reporting"

[search-rock-repo-label-topic-rock-internals]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Rock%20Internals
[search-SparkDevNetwork-org-label-topic-rock-internals]:https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Rock%20Internals" 

[search-rock-repo-label-topic-security]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Security
[search-SparkDevNetwork-org-label-topic-security]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Security"

[search-rock-repo-label-topic-ui]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20UI
[search-SparkDevNetwork-org-label-topic-ui]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20UI"

[search-rock-repo-label-topic-workflows]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Workflows
[search-SparkDevNetwork-org-label-topic-workflows]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Workflows"

[search-rock-repo-label-topic-performance]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Performance
[search-SparkDevNetwork-org-label-topic-performance]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Performance"

[search-rock-repo-label-topic-uncaught-exception]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Uncaught%20Exception
[search-SparkDevNetwork-org-label-topic-uncaught-exception]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A20Uncaught%20Exception"
[search-rock-repo-label-topic-crash]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Crash
[search-SparkDevNetwork-org-label-topic-crash]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Crash"
[search-rock-repo-label-topic-git]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Git
[search-SparkDevNetwork-org-label-topic-git]: https://github.com/issues?q=is%3Aopen+is%3Aissue+user%3ASparkDevNetwork+label%3A"Topic%3A%20Git"


[search-rock-repo-label-topic-installer]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Installer
[search-rock-repo-label-topic-updater]: https://github.com/SparkDevNetwork/Rock/labels/Topic%3A%20Updater

