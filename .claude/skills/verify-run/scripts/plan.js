// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

// Helper for verification plan files used by the verify-plan and verify-run skills.
// Edits only what each command says it edits, so agents never hand-edit large tables.
//
// Usage:
//   node plan.js validate <plan> [--new]
//   node plan.js list <plan> [--group <name>] [--status blank|failed|pass|blocked]
//   node plan.js set <plan> <TestId> <PASS|FAIL|blank> [notes]
//   node plan.js merge <plan> <stagingDir>
//   node plan.js summary <plan>
//   node plan.js assemble <out> <headerFile> <fragmentsDir>

"use strict";

const fs = require("fs");
const path = require("path");

const kinds = ["Golden", "Regression", "Edge", "Security", "Data", "Automated"];
const statuses = ["", "PASS", "FAIL", "BLOCKED"];
const testIdPattern = /^[A-Za-z][A-Za-z0-9.]*-\d{2,}$/;

/**
 * Splits a markdown table row into trimmed cells, honoring escaped pipes.
 */
function splitRow(line) {
    const inner = line.trim().replace(/^\|/, "").replace(/\|$/, "");
    return inner.split(/(?<!\\)\|/).map(c => c.trim());
}

/**
 * Builds a table row from cells.
 */
function joinRow(cells) {
    return "| " + cells.join(" | ") + " |";
}

/**
 * Makes a value safe for a single table cell.
 */
function cellText(value) {
    return (value || "").replace(/\r?\n/g, " ").replace(/(?<!\\)\|/g, "\\|").trim();
}

/**
 * Parses the plan into its lines plus the table rows and instruction sections it contains.
 */
function parse(file) {
    const text = fs.readFileSync(file, "utf8");
    const lines = text.split(/\r?\n/);
    const tablesStart = lines.findIndex(l => l.trim() === "# Test Tables");
    const instructionsStart = lines.findIndex(l => l.trim() === "# Test Instructions");
    const rows = [];
    const sections = [];
    const tableGroups = [];
    const instructionGroups = [];

    if (tablesStart >= 0) {
        const end = instructionsStart > tablesStart ? instructionsStart : lines.length;
        let group = null;
        for (let i = tablesStart + 1; i < end; i++) {
            const line = lines[i];
            if (line.startsWith("## ")) {
                group = line.substring(3).trim();
                tableGroups.push(group);
            }
            else if (line.startsWith("|") && !/^\|\s*Test ID\s*\|/.test(line) && !/^\|[\s|:-]+\|$/.test(line.trim())) {
                const cells = splitRow(line);
                rows.push({ index: i, group, cells, id: cells[0] });
            }
        }
    }

    if (instructionsStart >= 0) {
        let group = null;
        let current = null;
        for (let i = instructionsStart + 1; i < lines.length; i++) {
            const line = lines[i];
            if (line.startsWith("# ")) {
                break;
            }
            if (line.startsWith("## ")) {
                group = line.substring(3).trim();
                instructionGroups.push(group);
                current = null;
            }
            else if (line.startsWith("### ")) {
                const heading = line.substring(4).trim();
                current = { index: i, group, id: heading.split(/\s+/)[0], body: [] };
                sections.push(current);
            }
            else if (current) {
                current.body.push(line);
            }
        }
    }

    return { text, lines, tablesStart, instructionsStart, rows, sections, tableGroups, instructionGroups };
}

/**
 * Writes lines back, keeping the file's original line endings.
 */
function save(file, plan) {
    const eol = plan.text.includes("\r\n") ? "\r\n" : "\n";
    fs.writeFileSync(file, plan.lines.join(eol), "utf8");
}

/**
 * Checks prose lines (outside code fences and inline code) for dash characters the
 * organization style forbids.
 */
function findDashProblems(lines) {
    const problems = [];
    let inFence = false;
    lines.forEach((line, i) => {
        if (/^\s*(```|~~~)/.test(line)) {
            inFence = !inFence;
            return;
        }
        if (inFence || /^\|[\s|:-]+\|$/.test(line.trim())) {
            return;
        }
        const prose = line.replace(/`[^`]*`/g, "");
        if (/[\u2013\u2014]/.test(prose) || /(^|[^-])--([^-]|$)/.test(prose)) {
            problems.push(`line ${i + 1}: em-dash, en-dash or double hyphen in prose`);
        }
    });
    return problems;
}

function validate(file, isNew) {
    const plan = parse(file);
    const errors = [];

    if (plan.tablesStart < 0) {
        errors.push("missing '# Test Tables' heading");
    }
    if (plan.instructionsStart < 0) {
        errors.push("missing '# Test Instructions' heading");
    }
    if (plan.tableGroups.join("\n") !== plan.instructionGroups.join("\n")) {
        errors.push(`group headings differ between tables (${plan.tableGroups.join(", ")}) and instructions (${plan.instructionGroups.join(", ")})`);
    }

    const seen = new Set();
    for (const row of plan.rows) {
        const where = `line ${row.index + 1} (${row.id})`;
        if (row.cells.length !== 6) {
            errors.push(`${where}: expected 6 cells, found ${row.cells.length}`);
            continue;
        }
        if (!testIdPattern.test(row.id)) {
            errors.push(`${where}: Test ID does not match <Subject>-<NN>`);
        }
        if (seen.has(row.id)) {
            errors.push(`${where}: duplicate Test ID`);
        }
        seen.add(row.id);
        if (!kinds.includes(row.cells[2])) {
            errors.push(`${where}: Kind '${row.cells[2]}' is not one of ${kinds.join(", ")}`);
        }
        if (!statuses.includes(row.cells[3])) {
            errors.push(`${where}: Status '${row.cells[3]}' is not PASS, FAIL, BLOCKED or blank`);
        }
        if (isNew && (row.cells[3] !== "" || row.cells[5] !== "")) {
            errors.push(`${where}: a new plan must have empty Status and Notes`);
        }
    }

    const sectionIds = new Map();
    for (const section of plan.sections) {
        if (sectionIds.has(section.id)) {
            errors.push(`line ${section.index + 1}: duplicate instruction section ${section.id}`);
        }
        sectionIds.set(section.id, section);
        const body = section.body.join("\n");
        if (!seen.has(section.id)) {
            errors.push(`line ${section.index + 1}: instruction section ${section.id} has no table row`);
        }
        if (!/\*\*Expected/.test(body)) {
            errors.push(`line ${section.index + 1}: section ${section.id} has no **Expected** result`);
        }
        if (!/^Covers:/m.test(body)) {
            errors.push(`line ${section.index + 1}: section ${section.id} has no Covers line`);
        }
        if (body.replace(/\s/g, "").length < 80) {
            errors.push(`line ${section.index + 1}: section ${section.id} looks too short`);
        }
    }
    for (const row of plan.rows) {
        const section = sectionIds.get(row.id);
        if (!section) {
            errors.push(`line ${row.index + 1}: ${row.id} has no instruction section`);
        }
        else if (section.group !== row.group) {
            errors.push(`${row.id}: table group '${row.group}' differs from instruction group '${section.group}'`);
        }
    }

    errors.push(...findDashProblems(plan.lines));

    if (errors.length) {
        console.log(`${errors.length} problem(s):`);
        errors.forEach(e => console.log(`- ${e}`));
        process.exitCode = 1;
    }
    else {
        console.log(`OK: ${plan.rows.length} tests in ${plan.tableGroups.length} group(s).`);
    }
}

function list(file, options) {
    const plan = parse(file);
    const statusFilter = {
        blank: s => s === "",
        failed: s => s === "FAIL",
        pass: s => s === "PASS",
        blocked: s => s === "BLOCKED"
    }[options.status || ""] || (() => true);

    plan.rows
        .filter(r => !options.group || r.group === options.group)
        .filter(r => statusFilter(r.cells[3]))
        .forEach(r => console.log(`${r.id}\t${r.cells[3] || "-"}\t${r.group}`));
}

/**
 * Sets the Status and Notes cells of one row. Returns false if the row was not found.
 */
function setResult(plan, testId, status, notes) {
    const row = plan.rows.find(r => r.id === testId);
    if (!row) {
        return false;
    }
    const cells = row.cells.slice();
    cells[3] = status;
    cells[5] = cellText(notes);
    plan.lines[row.index] = joinRow(cells);
    row.cells = cells;
    return true;
}

function normalizeStatus(value, allowBlocked) {
    const status = (value || "").trim().toUpperCase();
    if (status === "BLANK" || status === "") {
        return "";
    }
    if (status === "BLOCKED" && !allowBlocked) {
        throw new Error("BLOCKED is set only by a human. Pass --human if a human asked for it.");
    }
    if (!statuses.includes(status)) {
        throw new Error(`Unknown status '${value}'.`);
    }
    return status;
}

function set(file, testId, status, notes, allowBlocked) {
    const plan = parse(file);
    if (!setResult(plan, testId, normalizeStatus(status, allowBlocked), notes)) {
        throw new Error(`Test ID ${testId} not found in the tables.`);
    }
    save(file, plan);
    console.log(`${testId}: ${status || "blank"}`);
}

function merge(file, stagingDir) {
    const plan = parse(file);
    const applied = new Map();
    const conflicts = [];

    for (const name of fs.readdirSync(stagingDir).filter(n => n.endsWith(".md")).sort()) {
        const lines = fs.readFileSync(path.join(stagingDir, name), "utf8").split(/\r?\n/);
        for (const line of lines) {
            if (!line.startsWith("|") || /^\|\s*Test ID\s*\|/.test(line) || /^\|[\s|:-]+\|$/.test(line.trim())) {
                continue;
            }
            // Workers write raw notes, so any extra cells are pipes inside the note.
            const [id, status, ...noteCells] = splitRow(line);
            const result = { status: normalizeStatus(status, false), notes: noteCells.join(" | ").replace(/\\\|/g, "|"), source: name };
            const previous = applied.get(id);
            if (previous) {
                conflicts.push(`${id} appears in ${previous.source} and ${name}`);
                if (previous.status === "FAIL") {
                    continue;
                }
            }
            applied.set(id, result);
        }
    }

    const missing = [];
    for (const [id, result] of applied) {
        if (!setResult(plan, id, result.status, result.notes)) {
            missing.push(id);
        }
    }
    save(file, plan);

    console.log(`Merged ${applied.size - missing.length} result(s).`);
    if (missing.length) {
        console.log(`Not found in the plan: ${missing.join(", ")}`);
    }
    if (conflicts.length) {
        console.log("Duplicates (FAIL kept where results disagree):");
        conflicts.forEach(c => console.log(`- ${c}`));
    }
}

function summary(file) {
    const plan = parse(file);
    const groups = new Map();
    for (const row of plan.rows) {
        const counts = groups.get(row.group) || { rows: 0, PASS: 0, FAIL: 0, BLOCKED: 0, blank: 0 };
        counts.rows++;
        counts[row.cells[3] || "blank"]++;
        groups.set(row.group, counts);
    }

    console.log("| Group | Rows | PASS | FAIL | BLOCKED | Blank |");
    console.log("|---|---|---|---|---|---|");
    const total = { rows: 0, PASS: 0, FAIL: 0, BLOCKED: 0, blank: 0 };
    for (const [group, c] of groups) {
        console.log(`| ${group} | ${c.rows} | ${c.PASS} | ${c.FAIL} | ${c.BLOCKED} | ${c.blank} |`);
        Object.keys(total).forEach(k => total[k] += c[k]);
    }
    console.log(`| **Total** | ${total.rows} | ${total.PASS} | ${total.FAIL} | ${total.BLOCKED} | ${total.blank} |`);
}

/**
 * Builds a plan from a header file (everything before '# Test Tables') and group fragments.
 * Each fragment starts with '<!-- GROUP: name -->', then '<!-- TABLE -->' with table rows
 * (no header), then '<!-- INSTRUCTIONS -->' with the '###' sections. Fragments are used in
 * file name order.
 */
function assemble(out, headerFile, fragmentsDir) {
    const header = fs.readFileSync(headerFile, "utf8").trimEnd();
    const fragments = fs.readdirSync(fragmentsDir).filter(n => n.endsWith(".md")).sort().map(name => {
        const text = fs.readFileSync(path.join(fragmentsDir, name), "utf8");
        const group = (text.match(/<!--\s*GROUP:\s*(.+?)\s*-->/) || [])[1];
        const table = (text.split(/<!--\s*TABLE\s*-->/)[1] || "").split(/<!--\s*INSTRUCTIONS\s*-->/)[0].trim();
        const instructions = (text.split(/<!--\s*INSTRUCTIONS\s*-->/)[1] || "").trim();
        if (!group || !table || !instructions) {
            throw new Error(`Fragment ${name} is missing its GROUP, TABLE or INSTRUCTIONS marker.`);
        }
        return { group, table, instructions };
    });

    const parts = [header, "", "# Test Tables", ""];
    for (const f of fragments) {
        parts.push(`## ${f.group}`, "", "| Test ID | Subject | Kind | Status | Description | Notes |", "|---|---|---|---|---|---|", f.table, "");
    }
    parts.push("# Test Instructions", "");
    for (const f of fragments) {
        parts.push(`## ${f.group}`, "", f.instructions, "");
    }
    fs.writeFileSync(out, parts.join("\n"), "utf8");
    console.log(`Wrote ${out} from ${fragments.length} fragment(s). Run validate --new next.`);
}

function main() {
    const args = process.argv.slice(2);
    const flags = {};
    const positional = [];
    for (let i = 0; i < args.length; i++) {
        if (args[i] === "--new" || args[i] === "--human") {
            flags[args[i].substring(2)] = true;
        }
        else if (args[i].startsWith("--")) {
            flags[args[i].substring(2)] = args[++i];
        }
        else {
            positional.push(args[i]);
        }
    }

    const [command, ...rest] = positional;
    try {
        switch (command) {
            case "validate": validate(rest[0], !!flags.new); break;
            case "list": list(rest[0], flags); break;
            case "set": set(rest[0], rest[1], rest[2], rest[3], !!flags.human); break;
            case "merge": merge(rest[0], rest[1]); break;
            case "summary": summary(rest[0]); break;
            case "assemble": assemble(rest[0], rest[1], rest[2]); break;
            default:
                console.log("Commands: validate, list, set, merge, summary, assemble. See the comment at the top of plan.js.");
                process.exitCode = 1;
        }
    }
    catch (error) {
        console.error(error.message);
        process.exitCode = 1;
    }
}

main();
