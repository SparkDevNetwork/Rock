#!/usr/bin/env node

import { spawnSync } from "child_process";
import { parse } from "node-html-parser";
import Asana from "asana";
import { env } from "process";

const missing_variables = [];

if (!env.ASANA_ACCESS_TOKEN) {
    missing_variables.push("ASANA_ACCESS_TOKEN");
}

if (!env.ASANA_WORKSPACE) {
    missing_variables.push("ASANA_WORKSPACE");
}

if (!env.ASANA_PROJECT) {
    missing_variables.push("ASANA_PROJECT");
}

if (!env.ASANA_SECTION) {
    missing_variables.push("ASANA_SECTION");
}

if (missing_variables.length > 0 && !process.argv.some(a => a === "--no-asana")) {
    console.error(`Missing one or more environment variables: ${missing_variables.join(", ")}`);
    process.exit(1);
}

// Initialize Asana client.
//
// You can get the Workspace Gid and Project Gid by inspecting the URL when
// viewing the project in the browser:
//
// https://app.asana.com/1/<workspace_gid>/project/<project_gid>/overview/<some_other_gid>
//
// Next you can get the Section Gid by calling this API from the browser while
// logged in:
//
// https://app.asana.com/api/1.0/projects/<project_gid>/sections

const client = Asana.ApiClient.instance;
const token = client.authentications["token"];
token.accessToken = env.ASANA_ACCESS_TOKEN;
const asanaWorkspaceGid = env.ASANA_WORKSPACE;
const asanaProjectGid = env.ASANA_PROJECT;
const asanaSectionGid = env.ASANA_SECTION;

const repoRoot = runCommand("git", ["rev-parse", "--show-toplevel"]).trim();

/**
 * Helper to run a command with spawnSync and return combined stdout and stderr as text.
 *
 * @param {string} cmd - Command to run (e.g., "git").
 * @param {string[]} args - Arguments for the command.
 * @param {string|undefined} cwd - Working directory.
 *
 * @returns {string} Combined output text.
 */
function runCommand(cmd, args, cwd) {
    const result = spawnSync(cmd, args, { cwd, encoding: "utf-8" });

    return ((result.stdout || "") + (result.stderr || "")).trim();
}

/**
 * Gets the list of files changed in the latest commit.
 * Returns an array of file paths (one per line).
 *
 * @returns {string[]} Array of changed file paths.
 */
function getChangedFiles() {
    const output = runCommand("git", ["diff", "--name-only", "HEAD^", "HEAD"], repoRoot);
    return output.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
}

/**
 * Gets the previous content of a file, handling renames.
 *
 * @param {string} relPath - Path to the file relative to the git root.
 *
 * @returns {string|null} Previous content of the file, or null if not found.
 */
function getPreviousFileContent(relPath) {
    // Check for renames in the last commit
    const diffOutput = runCommand("git", ["diff", "--name-status", "HEAD^", "HEAD"], repoRoot);
    const diffLines = diffOutput.split(/\r?\n/);
    let oldPath = relPath;

    for (const line of diffLines) {
        // Rename line format: R100\toldPath\tnewPath
        if (line.startsWith("R")) {
            const parts = line.split(/\t/);
            if (parts[2] === relPath) {
                oldPath = parts[1];
                break;
            }
        }
    }
    // Try to get previous content
    const prevContent = runCommand("git", ["show", `HEAD^:${oldPath}`], repoRoot);

    return prevContent || null;
}

/**
 * Gets the current content of a file.
 *
 * @param {string} relPath - Path to the file relative to the git root.
 *
 * @returns {string|null} Current content of the file, or null if not found.
 */
function getCurrentFileContent(relPath) {
    // Try to get current content
    const currContent = runCommand("git", ["show", `HEAD:${relPath}`], repoRoot);

    return currContent || null;
}

/**
 * Verifies the file to see if any style tags were modified.
 *
 * @param {string} filePath Path to the file relative to the git root.
 *
 * @returns {boolean} True if styles were modified, false otherwise.
 */
function detectStyleChanges(filePath) {
    const prevContent = getPreviousFileContent(filePath);
    const currentContent = getCurrentFileContent(filePath);

    const previousRoot = parse(prevContent ?? "");
    const currentRoot = parse(currentContent ?? "");

    const previousStyles = previousRoot.querySelectorAll("style");
    const currentStyles = currentRoot.querySelectorAll("style");

    if (previousStyles.length !== currentStyles.length) {
        console.log(`Style tag count changed in ${filePath}: previous=${previousStyles.length}, current=${currentStyles.length}`);
        return true;
    }

    for (let i = 0; i < previousStyles.length; i++) {
        // Ignore whitespace differences.
        const previousStyle = previousStyles[i].innerHTML.replace(/\s+/g, '').trim();
        const currentStyle = currentStyles[i].innerHTML.replace(/\s+/g, '').trim();

        if (previousStyle !== currentStyle) {
            console.log(`Style tag content changed in ${filePath} at index ${i}`);
            return true;
        }
    }

    return false;
}

/**
 * Creates an Asana task reporting the style changes.
 *
 * @param {string[]} changedFiles The relative paths to the changed files.
 *
 * @returns {Promise<void>} A promise that resolves when the task is created.
 */
async function createAsanaTask(changedFiles) {
    const shortCommitHash = runCommand("git", ["rev-parse", "--short", "HEAD"], repoRoot);
    const longCommitHash = runCommand("git", ["rev-parse", "HEAD"], repoRoot);
    const commitMessage = runCommand("git", ["log", "-1", "--pretty=%B"], repoRoot);

    const tasksApiInstance = new Asana.TasksApi();
    const message = `Commit <a href="https://github.com/SparkDevNetwork/Rock/commit/${longCommitHash}">${shortCommitHash}</a> made changes to Obsidian style tags.\n\n<pre>${commitMessage}</pre>`;
    const fileBullets = changedFiles.map(f => `<li>${f}</li>`).join("");
    const body = {
        data: {
            name: `Changes to Obsidian styles in ${shortCommitHash}`,
            approval_status: "pending",
            completed: false,
            html_notes: `<body>${message}\n\n<ul>${fileBullets}</ul>\n\n</body>`,
            workspace: asanaWorkspaceGid,
            memberships: [
                {
                    project: asanaProjectGid,
                    section: asanaSectionGid
                }
            ]
        },
    };
    const opts = {};

    const createResult = await tasksApiInstance.createTask(body, opts);

    console.log(`Created new asana task id=${createResult.data.gid}`);
}

async function main() {
    const changedFiles = getChangedFiles();
    const changedObsidianFiles = changedFiles.filter(f => f.endsWith(".obs"));

    /**
     * The files that should be reported for style changes.
     *
     * @type {string[]}
     */
    const reportFiles = [];

    for (const filePath of changedObsidianFiles) {
        try {
            if (detectStyleChanges(filePath)) {
                reportFiles.push(filePath);
            }
        }
        catch (error) {
            console.error(`Error processing file ${filePath}:`, error);
        }
    }

    if (process.argv.some(a => a === "--test-asana")) {
        reportFiles.push("Test file");
    }

    if (reportFiles.length > 0) {
        if (!process.argv.some(a => a === "--no-asana")) {
            try {
                await createAsanaTask(reportFiles);
            }
            catch (error) {
                console.error("Error creating Asana task:", error.message);

                if (error.response?.error?.text) {
                    console.error(error.response.error.text);
                }
            }
        }
    }
    else {
        console.log("No style changes detected.");
    }
}

main();
