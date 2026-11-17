// Checks the hotfix plugin migrations a PR adds (Rock/Plugin/HotFixes):
//   1. The MinimumRockVersion in [MigrationNumber( n, "x.y" )] is the release
//      of the branch the PR targets: "19.9" on hotfix-19.9, "20.0" on
//      release-20.0. The release comes from the branch NAME, because the
//      version files are sometimes bumped late (hotfix-19.9 could still say
//      19.8 for weeks). Only develop has no release in its name, so for it
//      the major.minor of its version file is used.
//   2. The migration number isn't already used on any branch in MERGE_CHAIN,
//      or twice in the PR.
// Only ADDED files are checked. Files that came in from another chain branch
// (the PR branch merged it, same path and same content) and exact renames are
// not new migrations, so they are skipped. A new .cs file that mentions
// MigrationNumber but can't be read fails the check with a clear message.
// A hotfix or release branch whose version file lags its name (hotfix-19.9
// still saying 19.8) only gets a WARNING, never a failure: it is always in the
// job summary, and in the PR comment on PRs that touch Rock/Plugin/HotFixes
// (every PR into the branch would make the comment noise).
// Exits 1 on a problem. With PR set, it keeps ONE comment on the PR up to date
// (marker below) and removes it once the PR is clean.
//
// Env: BASE (target branch), HEAD (commit with the PR's changes, default HEAD),
// MERGE_CHAIN (the repo variable), PR + GITHUB_REPOSITORY + GH_TOKEN (optional,
// for the comment). Branches are read as origin/<branch>. Reads git objects
// only; it never builds or runs anything from the files it checks.

import { execFileSync } from "node:child_process";
import { appendFileSync } from "node:fs";

const DIR = "Rock/Plugin/HotFixes";
// A hidden HTML comment, <!--migration-version-check-->, built from parts.
const MARKER = ["<!", "migration-version-check", ">"].join("-" + "-");
const { BASE, PR, GITHUB_REPOSITORY: REPO } = process.env;
const HEAD = process.env.HEAD || "HEAD";

const run = (cmd, args, ok = [0]) => {
  try {
    return execFileSync(cmd, args, { encoding: "utf8", maxBuffer: 1 << 28, stdio: ["ignore", "pipe", "pipe"] });
  } catch (e) {
    if (ok.includes(e.status)) return e.stdout || "";
    throw e;
  }
};
const git = (...args) => run("git", args);
const gitOrNull = (...args) => {
  try {
    return git(...args);
  } catch {
    return null;
  }
};
const lines = (out) => out.split("\n").map((l) => l.trim()).filter(Boolean);

// Text that came from the PR, made safe for a comment (inside a code span) and
// for the job log.
const clean = (s) => String(s).replace(/[`\r\n]/g, " ").slice(0, 80);

// The release a branch is: major.minor, from the name where it has one. A
// version file that disagrees with the name is added to warnings.
function branchRelease(branch, warnings) {
  const named = branch.match(/^(?:hotfix|release)-(\d+)\.(\d+)$/);
  const file = versionFile(branch);
  if (named) {
    const fromName = `${named[1]}.${named[2]}`;
    if (file && file.version !== fromName) {
      warnings.push(`**\`${branch}\`** still reports version ${file.version} in \`${file.path}\`. ` +
        `Bump the version files to ${fromName} so builds of this branch run migrations marked ${fromName}.`);
    }
    return fromName;
  }
  if (file) return file.version;
  throw new Error(`Couldn't read the Rock version of ${branch}: its name has no release and no version file was found.`);
}

// { version: major.minor, path } from the branch's version file, or null.
function versionFile(branch) {
  const path = "Directory.Build.props";
  const props = gitOrNull("show", `origin/${branch}:${path}`)?.match(/<Version>(\d+)\.(\d+)\./);
  if (props) return { version: `${props[1]}.${props[2]}`, path };
  // hotfix-17.13 and hotfix-18.9 still keep it in AssemblySharedInfo.cs.
  const oldPath = "Rock.Version/AssemblySharedInfo.cs";
  const old = gitOrNull("show", `origin/${branch}:${oldPath}`)?.match(/AssemblyFileVersion\(\s*"(\d+)\.(\d+)\./);
  return old ? { version: `${old[1]}.${old[2]}`, path: oldPath } : null;
}

// The source without comments, so a commented-out attribute doesn't count.
const stripComments = (s) => s.replace(/\/\*[\s\S]*?\*\//g, "").replace(/\/\/[^\n]*/g, "");
const attr = /\[\s*MigrationNumber\s*\(\s*(\d+)\s*,\s*"([^"]*)"\s*\)\s*\]/g;

function check() {
  const problems = [];
  const warnings = [];
  let touched = false;
  const result = () => ({ problems, warnings, touched });
  let chain;
  try {
    chain = JSON.parse(process.env.MERGE_CHAIN || "{}");
  } catch {
    throw new Error("MERGE_CHAIN isn't valid JSON.");
  }
  const active = [...new Set(Object.entries(chain).flat(2))];
  if (!BASE) throw new Error("BASE isn't set.");
  if (!active.length) {
    console.log("MERGE_CHAIN isn't set, so the migrations weren't checked.");
    return result();
  }
  if (!active.includes(BASE)) {
    console.log(`${BASE} isn't a branch in the merge-up chain, so there's nothing to check.`);
    return result();
  }
  const missing = active.filter((b) => gitOrNull("rev-parse", "--verify", "-q", `origin/${b}`) === null);
  if (missing.length) throw new Error(`These merge-up branches aren't in the checkout: ${missing.join(", ")}.`);

  // Only an error when a migration needs the release (develop's file is unreadable).
  let expected, expectedError;
  try {
    expected = branchRelease(BASE, warnings);
  } catch (e) {
    expectedError = e;
  }

  // Changes the PR makes since it left BASE. Exact renames (-M100%) are the
  // same migration, not a new one; a rename with edits shows as add + delete.
  const diff = (filter) => run("git", ["diff", "-z", "-M100%", `--diff-filter=${filter}`, "--name-only", `origin/${BASE}...${HEAD}`, "--", DIR])
    .split("\0").filter(Boolean);
  touched = diff("ADMRT").length > 0;
  const added = diff("A").filter((f) => f.endsWith(".cs"));
  const deleted = new Set(diff("D"));
  if (!added.length) {
    console.log("The PR adds no migration files.");
    return result();
  }

  if (expectedError) throw expectedError;
  const seen = {};
  let checked = 0;

  for (const file of added) {
    const name = file.split("/").pop();
    const blob = git("rev-parse", `${HEAD}:${file}`).trim();

    // Already on a chain branch with the same content: it came in by merging
    // that branch (or the PR was already merged up), so it isn't this PR's.
    const inherited = active.filter((b) => gitOrNull("rev-parse", `origin/${b}:${file}`)?.trim() === blob);
    if (inherited.length) {
      console.log(`Skipped ${name}: it is already on ${inherited.join(", ")}.`);
      continue;
    }

    const source = stripComments(git("show", `${HEAD}:${file}`));
    const found = [...source.matchAll(attr)];
    if (!found.length) {
      if (/MigrationNumber/.test(source)) {
        problems.push(`**\`${name}\`** mentions MigrationNumber, but the check couldn't read it. ` +
          `Write it as \`[MigrationNumber( 351, "${expected}" )]\`, a number and a quoted version.`);
      } else {
        console.log(`Skipped ${name}: no MigrationNumber attribute, so it isn't a migration.`);
      }
      continue;
    }
    checked++;

    for (const [, rawNumber, version] of found) {
      // C# reads 0351 and 351 as the same number, so compare and report the parsed value.
      const number = Number(rawNumber);
      if (version !== expected) {
        problems.push(`**\`${name}\`** has version \`"${clean(version)}"\`, but ${BASE} is Rock ${expected}. ` +
          `Change it to \`[MigrationNumber( ${number}, "${expected}" )]\`.`);
      }

      if (seen[number] && seen[number] !== name) {
        problems.push(`**\`${name}\`** uses migration number ${number}, which \`${seen[number]}\` in this PR already uses.`);
      }
      seen[number] = name;

      const used = [];
      for (const b of active) {
        const out = run("git", ["grep", "-l", "-E", `MigrationNumber[[:space:]]*\\([[:space:]]*0*${number}[[:space:]]*,`, `origin/${b}`, "--", DIR], [0, 1]);
        for (const hit of lines(out)) {
          const path = hit.slice(`origin/${b}:`.length);
          // A file this PR deletes (the old name of an edited rename) isn't a clash on any
          // branch: later branches only have it because it was merged up from here.
          if (deleted.has(path)) continue;
          used.push(`\`${b}\` (${path.split("/").pop()})`);
        }
      }
      if (used.length) {
        problems.push(`**\`${name}\`** uses migration number ${number}, which already exists on ${used.join(", ")}. ` +
          `Pick an unused number and reserve it.`);
      }
    }
  }
  console.log(`Checked ${checked} added migration(s) against ${BASE} (Rock ${expected}) and ${active.join(", ")}.`);
  return result();
}

// One comment, edited in place; removed once the PR is clean. Only comments
// by the Actions bot count, so someone quoting the marker is never touched.
function setComment(body) {
  const api = `repos/${REPO}/issues/${PR}/comments`;
  const ids = lines(run("gh", ["api", "--paginate", `${api}?per_page=100`, "-q",
    `.[] | select(.user.login == "github-actions[bot]" and (.body | contains("${MARKER}"))) | .id`]));
  const [keep, ...extra] = ids;
  for (const id of body ? extra : ids) run("gh", ["api", "-X", "DELETE", `repos/${REPO}/issues/comments/${id}`]);
  if (!body) return;
  if (keep) run("gh", ["api", "-X", "PATCH", `repos/${REPO}/issues/comments/${keep}`, "-f", `body=${body}`]);
  else run("gh", ["api", "-X", "POST", api, "-f", `body=${body}`]);
}

let problems, warnings, touched;
try {
  ({ problems, warnings, touched } = check());
} catch (e) {
  console.log(`The migration check couldn't run: ${e.stderr?.toString().trim() || e.message}`);
  process.exit(1);
}

const plain = (t) => t.replaceAll("`", "").replaceAll("**", "");
for (const w of warnings) console.log(`Warning: ${plain(w)}`);
if (warnings.length && process.env.GITHUB_STEP_SUMMARY) {
  try {
    appendFileSync(process.env.GITHUB_STEP_SUMMARY, `### Migration check warning\n\n${warnings.map((w) => `- ${w}`).join("\n")}\n`);
  } catch { /* the log line above is enough */ }
}

// The warning goes in the comment only when the PR touches the hotfix folder.
const shown = touched ? warnings : [];
let body = "";
if (problems.length || shown.length) {
  body = `${MARKER}\n`;
  if (problems.length) {
    body += `**Migration check:** this PR adds hotfix migrations that need fixing.\n\n` +
      problems.map((p) => `- ${p}`).join("\n") +
      `\n\nThe version is the Rock release of the branch the PR targets, never copied from the previous file. ` +
      `The number must be reserved and unused on every branch.`;
  }
  if (shown.length) {
    body += `${problems.length ? "\n\n" : ""}**Warning (does not fail the check):**\n\n` + shown.map((w) => `- ${w}`).join("\n");
  }
}

if (PR && REPO) {
  try {
    setComment(body);
  } catch (e) {
    // A read-only token (a PR from a fork) can't comment; the result still counts.
    console.log(`Couldn't update the PR comment: ${e.stderr?.toString().trim() || e.message}`);
  }
}
if (problems.length) {
  console.log(problems.map((p) => `- ${plain(p)}`).join("\n"));
  process.exit(1);
}
console.log("OK");
