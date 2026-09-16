# chat-ci

The gates for the chat platform code on this branch. `.github/workflows/chat-ci.yml` runs them on a
pull request; `ci.mjs` runs the same ones locally, in the same order, and stops at the first
failure.

```
npm ci
node ci.mjs                  every gate, with a wall time per gate
npm run check:references     just the reference gate
npm test                     the gates' own tests
```

`check-references.mjs` refuses a chat file that cites planning material kept outside this
repository: a decision or work-item number, a planning file name, a section sign. A reader here
cannot open any of those, so the reason belongs in the comment itself. It reads a named list of
chat paths rather than everything tracked, because this is a branch of Rock and almost none of the
tree is subject to that rule. Three folders are shared with the chat that already shipped in Rock;
they are deliberately absent from the list, and a chat file landing in one of them is named there
individually.

`ci.mjs` builds the unit test project and what it depends on, rather than the solution: the point is
to prove the chat code compiles and its own tests run. It uses MSBuild when it is on the path and
the .NET SDK otherwise.

`assert-tests-ran.mjs` reads a test run's own results file and refuses a run that executed fewer
tests than asked for. It exists because `dotnet test --filter` prints "No test matches the given
testcase filter" and exits zero, so a renamed namespace or a moved folder would otherwise leave a
pipeline green having run nothing. Neither `TreatNoTestsAsError` nor `VSTestTreatNoTestsAsError`
changes that exit code here, and one of this folder's own tests measures it rather than trusting
it.

The integration suite is not part of either run. It needs a database and takes minutes, so it waits
for the `run-integration` label on a pull request.
