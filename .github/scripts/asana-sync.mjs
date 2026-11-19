// Moves the Asana task(s) of a shipped mirror PR through the end of the Core
// Pipeline, and adds the "Fixed in" label to the public issue. Run by
// private-asana-sync.yml after Ship to Public pushes. See that file for the
// whole flow.
//
// Every step checks before it acts, so running it again on the same PR
// changes nothing that is already done. It only ever moves a task forward,
// and leaves a task alone when it is anywhere unexpected.
//
// Logs never print task names or notes: security task names can be sensitive.
//
// Environment:
//   PR                 Mirror PR number.
//   GITHUB_REPOSITORY  The mirror (owner/name).
//   GH_TOKEN           Token for the mirror: reads the PR, posts the comment.
//   PUBLIC_TOKEN       Token for SparkDevNetwork/Rock: adds the label. Optional.
//   ASANA_TOKEN        Asana token. Optional; without it only the label is done.
//   ASANA_CONFIG       JSON with the Asana GIDs (the ASANA_PIPELINE variable):
//                      { "workspace", "issuesProject" (Rock RMS Issues),
//                        "pipelineProject" (Core Pipeline), "devField" (the DEV
//                        custom field), "sections": { "toDo", "inProgress",
//                        "uxReview", "peerReview", "mergeReady", "risQa",
//                        "completed" } }
//   GITHUB_API, ASANA_API  Overridable for tests.
//   REPORT_FILE        Where to write the PR comment (markdown).

import { writeFileSync } from "node:fs";

const env = process.env;
const GITHUB_API = env.GITHUB_API || "https://api.github.com";
const ASANA_API = env.ASANA_API || "https://app.asana.com/api/1.0";
const PUBLIC_REPO = "SparkDevNetwork/Rock";
const MIRROR = env.GITHUB_REPOSITORY;
const PR = env.PR;

// Core Pipeline sections in the order a task moves through them. A task in
// any other section (QA Kickback, Needs Attention) is left alone.
const PIPELINE_ORDER = ["toDo", "inProgress", "uxReview", "peerReview", "mergeReady", "risQa", "completed"];

const COMMIT_LINK_SUBTASK = "Final GitHub Commit Link";
const ISSUE_SUBTASK = "Update GitHub Issue";

// How long to wait for the Asana rule that creates the two subtasks when a
// task enters Merge Ready. It took about 2 seconds on 2026-11-13.
const SUBTASK_WAIT_SECONDS = 60;

const report = [];
let problems = 0;

function note(line) {
    report.push(line);
    console.log(line);
}

function problem(line) {
    problems++;
    note(`- :warning: ${line}`);
}

const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

async function request(base, token, method, path, body) {
    for (let attempt = 1; ; attempt++) {
        const response = await fetch(base + path, {
            method,
            headers: {
                Authorization: `Bearer ${token}`,
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: body === undefined ? undefined : JSON.stringify(body),
        });

        // Asana and GitHub both send Retry-After when rate limited.
        if (response.status === 429 && attempt < 4) {
            await sleep(1000 * Number(response.headers.get("retry-after") || 5));
            continue;
        }

        // Checked before parsing, so an HTML error page reports its status.
        if (!response.ok) {
            const error = new Error(`${method} ${path.split("?")[0]} returned ${response.status}`);
            error.status = response.status;
            throw error;
        }
        const text = await response.text();
        return text ? JSON.parse(text) : null;
    }
}

const github = (token, method, path, body) => request(GITHUB_API, token, method, path, body);
const asana = async (method, path, body) => (await request(ASANA_API, env.ASANA_TOKEN, method, path, body)).data;

// #region Reading the PR

// One task reference from the PR. `kind` is "gid" (a task URL), "dev" (a DEV
// ID) or "issue" (a public issue number).
function parseReferences(title, body, branch) {
    const asanaLines = body
        .split(/\r?\n/)
        .map(line => line.match(/^\s*[-*]?\s*Asana:\s*(.+)$/i))
        .filter(Boolean)
        .map(match => match[1].trim());

    const fixes = issuesFixedBy(title);

    // An explicit Asana line wins over everything else.
    if (asanaLines.length > 0) {
        if (asanaLines.every(line => /^none\b/i.test(line))) {
            return { none: true, references: [] };
        }
        const references = [];
        for (const line of asanaLines) {
            // Every task URL and DEV ID on the line. A URL and a DEV ID for the
            // same task are merged once both are looked up.
            const partial = /\(partial\)/i.test(line);
            const gids = new Set([...line.matchAll(/app\.asana\.com\/(?:\S*?\/task\/|0\/\d+\/)(\d+)/g)].map(match => match[1]));
            const devs = new Set([...line.replace(/https?:\/\/\S+/g, "").matchAll(/\bDEV-?(\d+)\b/gi)].map(match => `DEV-${match[1]}`));
            for (const gid of gids) {
                references.push({ kind: "gid", value: gid, label: `task ${gid}`, partial });
            }
            for (const dev of devs) {
                references.push({ kind: "dev", value: dev, label: dev, partial });
            }
            if (gids.size === 0 && devs.size === 0 && !/^none\b/i.test(line)) {
                references.push({ kind: "unreadable", value: line, label: "an `Asana:` line", partial });
            }
        }
        return { none: false, references };
    }

    const references = [];
    const dev = branch.match(/(?:^|[-_/])dev-?(\d+)(?=$|[-_/])/i);
    if (dev) {
        references.push({ kind: "dev", value: `DEV-${dev[1]}`, label: `DEV-${dev[1]}`, partial: false });
    }
    for (const issue of fixes) {
        references.push({ kind: "issue", value: issue, label: `ISSUE #${issue}`, partial: false });
    }
    return { none: false, references };
}

// Issue numbers in the title's "(Fixes #1234)" or "(Fixes #1, #2)".
function issuesFixedBy(title) {
    const issues = [];
    for (const group of title.matchAll(/\(\s*(?:fixes|fixed|closes|resolves)\b([^)]*)\)/gi)) {
        for (const number of group[1].matchAll(/#(\d+)/g)) {
            issues.push(number[1]);
        }
    }
    return [...new Set(issues)];
}

// The commits Ship to Public recorded in its closing comment.
function shippedCommits(comments) {
    const all = comments.map(comment => comment.body || "").join("\n");
    const last = name => [...all.matchAll(new RegExp(`<!-- ${name}: ([0-9a-f]{40}) -->`, "g"))].pop()?.[1];
    return { fix: last("shipped-sha"), developMerge: last("develop-merge-sha") };
}

// #endregion

// #region Finding the task

async function resolveReference(config, reference) {
    if (reference.kind === "unreadable") {
        return { error: `Couldn't read the \`Asana:\` line "${reference.value}". Use \`Asana: DEV-1234\` or a task URL.` };
    }

    if (reference.kind === "gid") {
        try {
            await asana("GET", `/tasks/${reference.value}?opt_fields=gid`);
            return { gid: reference.value };
        }
        catch (error) {
            return { error: `Couldn't open ${reference.label} (${error.status}). Check the URL, and that the Asana bot user can see the task.` };
        }
    }

    let matches;
    if (reference.kind === "dev") {
        const query = new URLSearchParams({
            [`custom_fields.${config.devField}.value`]: reference.value,
            is_subtask: "false",
            opt_fields: "name",
        });
        matches = await asana("GET", `/workspaces/${config.workspace}/tasks/search?${query}`);
    }
    else {
        const query = new URLSearchParams({
            text: `ISSUE #${reference.value}`,
            "projects.any": config.issuesProject,
            is_subtask: "false",
            opt_fields: "name",
        });
        const pattern = new RegExp(`^ISSUE #${reference.value}(?![0-9])`);
        matches = (await asana("GET", `/workspaces/${config.workspace}/tasks/search?${query}`)).filter(task => pattern.test(task.name));
    }

    if (matches.length === 1) {
        return { gid: matches[0].gid };
    }
    if (matches.length === 0) {
        return { error: `No Asana task found for ${reference.label}.` };
    }
    return { error: `${matches.length} Asana tasks match ${reference.label}, so none was changed. Add \`Asana:\` with the right task's URL.` };
}

// #endregion

// #region Updating a task

function taskLink(gid) {
    return `https://app.asana.com/0/0/${gid}`;
}

function pipelineStage(config, task) {
    const membership = task.memberships.find(m => m.project?.gid === config.pipelineProject);
    if (!membership) {
        return { onPipeline: false };
    }
    const stage = PIPELINE_ORDER.find(key => config.sections[key] === membership.section?.gid);
    return { onPipeline: true, stage, rank: stage ? PIPELINE_ORDER.indexOf(stage) : -1 };
}

async function moveTo(config, gid, stage) {
    await asana("POST", `/sections/${config.sections[stage]}/addTask`, { data: { task: gid } });
}

async function subtasks(gid) {
    return asana("GET", `/tasks/${gid}/subtasks?opt_fields=name,completed,notes`);
}

async function waitForSubtasks(gid) {
    for (let waited = 0; ; waited += 3) {
        const list = await subtasks(gid);
        const commitLink = list.find(t => t.name === COMMIT_LINK_SUBTASK);
        const issue = list.find(t => t.name === ISSUE_SUBTASK);
        if ((commitLink && issue) || waited >= SUBTASK_WAIT_SECONDS) {
            return { commitLink, issue };
        }
        await sleep(3000);
    }
}

// Appends text to a subtask's notes unless `marker` is already there, then
// completes it. Keeps the template text the Asana rule put there.
async function fillSubtask(subtask, marker, text, complete) {
    const notes = subtask.notes || "";
    const data = {};
    if (!notes.includes(marker)) {
        data.notes = `${notes.replace(/\s+$/, "")}\n\n${text}`;
    }
    if (complete && !subtask.completed) {
        data.completed = true;
    }
    if (Object.keys(data).length > 0) {
        await asana("PUT", `/tasks/${subtask.gid}`, { data });
    }
}

function commitLinksText(base, commits) {
    const lines = [`Fix Commit (${base})`, `https://github.com/${PUBLIC_REPO}/commit/${commits.fix}`];
    if (commits.developMerge) {
        lines.push("", "Merge Commit (develop)", `https://github.com/${PUBLIC_REPO}/commit/${commits.developMerge}`);
    }
    return lines.join("\n");
}

// One part of work split across several PRs: record the commit on the task as
// a comment, and leave its section alone until the last part ships.
async function recordPartial(gid, label, base, commits) {
    const stories = await asana("GET", `/tasks/${gid}/stories?opt_fields=text`);
    if (!stories.some(story => (story.text || "").includes(commits.fix))) {
        await asana("POST", `/tasks/${gid}/stories`, {
            data: { text: `Shipped part of this task from Rock-Mirror PR #${PR} (https://github.com/${MIRROR}/pull/${PR}).\n\n${commitLinksText(base, commits)}` },
        });
    }
    note(`- ${label}: [task](${taskLink(gid)}) marked \`(partial)\`, so it stays where it is. The commit links were added as a comment.`);
}

async function updateTask(config, gid, label, base, commits, labelResults) {
    const task = await asana("GET", `/tasks/${gid}?opt_fields=name,memberships.project.gid,memberships.section.gid`);
    const link = `[task](${taskLink(gid)})`;
    const stage = pipelineStage(config, task);

    if (!stage.onPipeline) {
        problem(`${label}: the ${link} isn't on Core Pipeline, so it was left alone. Move it through the pipeline by hand.`);
        return;
    }
    if (stage.rank === -1) {
        problem(`${label}: the ${link} is in a Core Pipeline section the automation doesn't move tasks out of (for example QA Kickback), so it was left alone.`);
        return;
    }

    const done = [];
    const mergeReady = PIPELINE_ORDER.indexOf("mergeReady");
    const risQa = PIPELINE_ORDER.indexOf("risQa");

    if (stage.rank < mergeReady) {
        await moveTo(config, gid, "mergeReady");
        done.push("moved to Merge Ready");
    }

    const { commitLink, issue } = await waitForSubtasks(gid);
    if (!commitLink) {
        problem(`${label}: the ${link} has no "${COMMIT_LINK_SUBTASK}" subtask${done.length ? " after moving to Merge Ready" : ""}, so it stopped there. Add the commit links and move it to RIS/QA by hand.`);
        return;
    }
    await fillSubtask(commitLink, commits.fix, commitLinksText(base, commits), true);
    done.push("added the commit links");

    // The issue this task tracks, from its name ("ISSUE #1234 ⁃ ...").
    const issueNumber = task.name.match(/^ISSUE #(\d+)(?![0-9])/)?.[1];
    if (issue) {
        if (!issueNumber) {
            await fillSubtask(issue, "No public GitHub issue", "No public GitHub issue for this task.", true);
        }
        else {
            const result = labelResults.get(issueNumber);
            if (result?.label) {
                const url = `https://github.com/${PUBLIC_REPO}/issues/${issueNumber}`;
                await fillSubtask(issue, result.label, `${url}\nAdded label: ${result.label}`, true);
                done.push(`noted the "${result.label}" label`);
            }
            else {
                note(`- ${label}: left "${ISSUE_SUBTASK}" open on the ${link}, because ${result?.reason || `the PR title doesn't say it fixes #${issueNumber}`}.`);
            }
        }
    }

    if (stage.rank < risQa) {
        await moveTo(config, gid, "risQa");
        done.push("moved to RIS/QA");
    }

    note(`- ${label}: ${link} ${done.join(", ")}.`);
}

// #endregion

// #region Public issue label

// "Fixed in v20.1" for hotfix-20.1 or release-20.1. Nothing for develop: no
// label is used for fixes that only reach the next major version.
function fixedInLabel(base) {
    const version = base.match(/^(?:hotfix|release)-(\d+\.\d+)$/)?.[1];
    return version ? `Fixed in v${version}` : null;
}

async function addFixedInLabels(base, issues) {
    const results = new Map();
    if (issues.length === 0) {
        return results;
    }

    const name = fixedInLabel(base);
    if (!name) {
        for (const issue of issues) {
            results.set(issue, { reason: `no "Fixed in" label is added for fixes to \`${base}\`` });
        }
        note(`- No "Fixed in" label added: fixes to \`${base}\` don't get one.`);
        return results;
    }

    if (!env.PUBLIC_TOKEN) {
        for (const issue of issues) {
            results.set(issue, { reason: "the public repo token wasn't available" });
        }
        problem(`Couldn't add "${name}" to the public issue(s): the token for ${PUBLIC_REPO} wasn't available.`);
        return results;
    }

    try {
        await github(env.PUBLIC_TOKEN, "GET", `/repos/${PUBLIC_REPO}/labels/${encodeURIComponent(name)}`);
    }
    catch (error) {
        const missing = error.status === 404;
        const reason = missing
            ? `the "${name}" label doesn't exist on ${PUBLIC_REPO}`
            : `checking the "${name}" label failed (${error.status || error.message})`;
        for (const issue of issues) {
            results.set(issue, { reason });
        }
        problem(missing
            ? `The "${name}" label doesn't exist on ${PUBLIC_REPO}, so it wasn't added. Labels are never created automatically.`
            : `Couldn't check the "${name}" label on ${PUBLIC_REPO} (${error.status || error.message}), so no label was added. Re-run the **asana / sync** job to retry.`);
        return results;
    }

    for (const issue of issues) {
        try {
            const found = await github(env.PUBLIC_TOKEN, "GET", `/repos/${PUBLIC_REPO}/issues/${issue}`);
            if (found.pull_request) {
                results.set(issue, { reason: `#${issue} is a pull request, not an issue` });
                problem(`#${issue} on ${PUBLIC_REPO} is a pull request, not an issue, so no label was added.`);
                continue;
            }
            if (!found.labels.some(existing => existing.name === name)) {
                await github(env.PUBLIC_TOKEN, "POST", `/repos/${PUBLIC_REPO}/issues/${issue}/labels`, { labels: [name] });
            }
            results.set(issue, { label: name });
            note(`- Added "${name}" to ${PUBLIC_REPO}#${issue}.`);
        }
        catch (error) {
            results.set(issue, { reason: `adding the label failed (${error.status})` });
            problem(`Couldn't add "${name}" to ${PUBLIC_REPO}#${issue} (${error.status}).`);
        }
    }
    return results;
}

// #endregion

// Every comment on the PR. The shipped-sha marker is in the closing comment,
// near the end, so on a busy PR it can be past the first page.
async function allComments() {
    const comments = [];
    for (let page = 1; ; page++) {
        const batch = await github(env.GH_TOKEN, "GET", `/repos/${MIRROR}/issues/${PR}/comments?per_page=100&page=${page}`);
        comments.push(...batch);
        if (batch.length < 100) {
            return comments;
        }
    }
}

async function main() {
    const pr = await github(env.GH_TOKEN, "GET", `/repos/${MIRROR}/pulls/${PR}`);
    const comments = await allComments();
    const base = pr.base.ref;
    const commits = shippedCommits(comments);

    if (!commits.fix) {
        console.log(`PR #${PR} hasn't shipped (no shipped-sha marker). Nothing to do.`);
        return;
    }

    const { none, references } = parseReferences(pr.title, pr.body || "", pr.head.ref);
    const labelResults = await addFixedInLabels(base, issuesFixedBy(pr.title));

    if (!env.ASANA_TOKEN) {
        console.log("ASANA_PIPELINE_TOKEN isn't set, so Asana was skipped.");
    }
    else if (none) {
        note("- `Asana: none`, so no Asana task was changed.");
    }
    else if (references.length === 0) {
        note("- No Asana task is linked to this PR. If it has one, add `Asana: DEV-1234` (or the task URL) to the PR description, then re-run the **asana / sync** job of this PR's Ship to Public run.");
    }
    else {
        const config = JSON.parse(env.ASANA_CONFIG || "null");
        if (!config) {
            problem("The `ASANA_PIPELINE` variable isn't set on this repo, so Asana was skipped.");
        }
        else {
            const seen = new Set();
            for (const reference of references) {
                try {
                    const resolved = await resolveReference(config, reference);
                    if (resolved.error) {
                        problem(resolved.error);
                        continue;
                    }
                    // Several references (a DEV ID and an issue) can be the same task.
                    if (seen.has(resolved.gid)) {
                        continue;
                    }
                    seen.add(resolved.gid);

                    if (reference.partial) {
                        await recordPartial(resolved.gid, reference.label, base, commits);
                    }
                    else {
                        await updateTask(config, resolved.gid, reference.label, base, commits, labelResults);
                    }
                }
                catch (error) {
                    problem(`${reference.label}: an Asana request failed (${error.message}). Re-run the **asana / sync** job to retry; finished steps are skipped.`);
                }
            }
        }
    }

    if (report.length > 0 && env.REPORT_FILE) {
        const run = `${env.GITHUB_SERVER_URL || "https://github.com"}/${MIRROR}/actions/runs/${env.GITHUB_RUN_ID || ""}`;
        writeFileSync(env.REPORT_FILE, `**Asana and issue updates**\n\n${report.join("\n")}\n\n[Workflow log](${run})\n`);
    }

    if (problems > 0) {
        process.exitCode = 1;
    }
}

main().catch(error => {
    console.error(error.message);
    process.exitCode = 1;
});
