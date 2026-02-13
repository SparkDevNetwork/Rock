#!/usr/bin/env node

import { spawnSync } from "child_process";
import { parse } from "node-html-parser";
import Asana from "asana";
import { env } from "process";
import { parseArgs } from "node:util";

/**
 * @typedef {Object} Config
 *
 * @property {boolean} asana - Whether to create Asana tasks.
 * @property {string} asanaToken - The Asana access token.
 * @property {string} asanaWorkspace - The Asana workspace GID.
 * @property {string} asanaProject - The Asana project GID.
 * @property {string} asanaSection - The Asana section GID.
 * @property {boolean} test - Whether to force a notification to be sent for testing.
 * @property {string} from - The starting commit, this is the commit before the push.
 * @property {string} to - The final commit in the push.
 */

const config = parseConfig();
const repoRoot = runCommand("git", ["rev-parse", "--show-toplevel"]).trim();

initializeAsana();
main();

/**
 * Initializes the Asana client.
 *
 * You can get the Workspace Gid and Project Gid by inspecting the URL when
 * viewing the project in the browser:
 *
 * https://app.asana.com/1/<workspace_gid>/project/<project_gid>/overview/<some_other_gid>
 *
 * Next you can get the Section Gid by calling this API from the browser while
 * logged in:
 *
 * https://app.asana.com/api/1.0/projects/<project_gid>/sections
 */
function initializeAsana() {
    const missingSettings = [];

    if (!config.asanaToken) {
        missingSettings.push({ env: "ASANA_ACCESS_TOKEN", arg: "--asana-token" });
    }

    if (!config.asanaWorkspace) {
        missingSettings.push({ env: "ASANA_WORKSPACE", arg: "--asana-workspace" });
    }

    if (!config.asanaProject) {
        missingSettings.push({ env: "ASANA_PROJECT", arg: "--asana-project" });
    }

    if (!config.asanaSection) {
        missingSettings.push({ env: "ASANA_SECTION", arg: "--asana-section" });
    }

    if (missingSettings.length > 0 && config.asana) {
        console.error(`Missing one or more environment variables: ${missingSettings.map(m => `${m.env} (${m.arg})`).join(", ")}`);
        process.exit(1);
    }

    const client = Asana.ApiClient.instance;
    const token = client.authentications["token"];
    token.accessToken = config.asanaToken;
}

/**
 * Parses command line arguments and returns a configuration object.
 *
 * @returns {Config}
 */
function parseConfig() {
    const args = parseArgs({
        options: {
            "asana": { type: "boolean", default: false },
            "asana-token": { type: "string" },
            "asana-workspace": { type: "string" },
            "asana-project": { type: "string" },
            "asana-section": { type: "string" },
            "test": { type: "boolean", default: false },
            "from": { type: "string" },
            "to": { type: "string" },
        }
    });

    if (!args.values["from"]) {
        console.error("Missing required argument: from");
        process.exit(1);
    }

    if (!args.values["to"]) {
        console.error("Missing required argument: to");
        process.exit(1);
    }

    if (!args.values["asana-token"] && env.ASANA_ACCESS_TOKEN) {
        args.values["asana-token"] = env.ASANA_ACCESS_TOKEN;
    }

    if (!args.values["asana-workspace"] && env.ASANA_WORKSPACE) {
        args.values["asana-workspace"] = env.ASANA_WORKSPACE;
    }

    if (!args.values["asana-project"] && env.ASANA_PROJECT) {
        args.values["asana-project"] = env.ASANA_PROJECT;
    }

    if (!args.values["asana-section"] && env.ASANA_SECTION) {
        args.values["asana-section"] = env.ASANA_SECTION;
    }

    return {
        asana: args.values["asana"],
        asanaToken: args.values["asana-token"],
        asanaWorkspace: args.values["asana-workspace"],
        asanaProject: args.values["asana-project"],
        asanaSection: args.values["asana-section"],
        test: args.values["test"],
        from: args.values["from"],
        to: args.values["to"],
    };
}

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

    if (result.status !== 0) {
        console.error(`Command failed: ${cmd} ${args.join(" ")}`);
        console.error(result.stderr);
        process.exit(1);
    }

    return ((result.stdout || "") + (result.stderr || "")).trim();
}

/**
 * Gets the list of files changed in the latest commit.
 * Returns an array of file paths (one per line).
 *
 * @param {string} from The previous commit.
 * @param {string} to The commit that made the change.
 *
 * @returns {string[]} Array of changed file paths.
 */
function getChangedFiles(from, to) {
    const output = runCommand("git", ["diff", "--name-only", from, to], repoRoot);
    return output.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
}

/**
 * Gets the previous content of a file, handling renames.
 *
 * @param {string} relPath - Path to the file relative to the git root.
 * @param {string} from The previous commit.
 * @param {string} to The commit that made the change.
 *
 * @returns {string|null} Previous content of the file, or null if not found.
 */
function getPreviousFileContent(relPath, from, to) {
    // Check for renames or additions in the last commit
    const diffOutput = runCommand("git", ["diff", "--name-status", from, to], repoRoot);
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
        else if (line.startsWith("A")) {
            const parts = line.split(/\t/);
            if (parts[1] === relPath) {
                return null;
            }
        }
    }

    // Try to get previous content
    const prevContent = runCommand("git", ["show", `${from}:${oldPath}`], repoRoot);

    return prevContent || null;
}

/**
 * Gets the current content of a file.
 *
 * @param {string} relPath - Path to the file relative to the git root.
 * @param {string} from The previous commit.
 * @param {string} to The commit that made the change.
 *
 * @returns {string|null} Current content of the file, or null if not found.
 */
function getCurrentFileContent(relPath, from, to) {
    // Check for deletions in this commit
    const diffOutput = runCommand("git", ["diff", "--name-status", from, to], repoRoot);
    const diffLines = diffOutput.split(/\r?\n/);
    let oldPath = relPath;

    for (const line of diffLines) {
        if (line.startsWith("D")) {
            const parts = line.split(/\t/);
            if (parts[1] === relPath) {
                return null;
            }
        }
    }

    // Try to get current content
    const currContent = runCommand("git", ["show", `${to}:${relPath}`], repoRoot);

    return currContent || null;
}

/**
 * Verifies the file to see if any style tags were modified.
 *
 * @param {string} filePath Path to the file relative to the git root.
 * @param {string} from The previous commit.
 * @param {string} to The commit that made the change.
 *
 * @returns {boolean} True if styles were modified, false otherwise.
 */
function detectStyleChanges(filePath, from, to) {
    const prevContent = getPreviousFileContent(filePath, from, to);
    const currentContent = getCurrentFileContent(filePath, from, to);

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
 * @param {string} to The commit that made the change.
 *
 * @returns {Promise<void>} A promise that resolves when the task is created.
 */
async function createAsanaTask(changedFiles, to) {
    const shortCommitHash = runCommand("git", ["rev-parse", "--short", to], repoRoot);
    const commitMessage = runCommand("git", ["log", "-1", "--pretty=%B"], repoRoot);

    const tasksApiInstance = new Asana.TasksApi();
    const message = `Commit <a href="https://github.com/SparkDevNetwork/Rock/commit/${to}">${shortCommitHash}</a> made changes to Obsidian style tags.\n\n<pre>${commitMessage}</pre>`;
    const fileBullets = changedFiles.map(f => `<li>${f}</li>`).join("");
    const body = {
        data: {
            name: `Changes to Obsidian styles in ${shortCommitHash}`,
            approval_status: "pending",
            completed: false,
            html_notes: `<body>${message}\n\n<ul>${fileBullets}</ul>\n\n</body>`,
            workspace: config.asanaWorkspace,
            memberships: [
                {
                    project: config.asanaProject,
                    section: config.asanaSection
                }
            ]
        },
    };
    const opts = {};

    const createResult = await tasksApiInstance.createTask(body, opts);

    console.log(`Created new asana task id=${createResult.data.gid}`);
}

/**
 * Processes a single commit to detect style changes and create Asana tasks if needed.
 *
 * @param {string} from The previous commit.
 * @param {string} to The commit that made the change.
 */
async function processCommit(from, to) {
    const changedFiles = getChangedFiles(from, to);
    const changedObsidianFiles = changedFiles.filter(f => f.endsWith(".obs"));

    /**
     * The files that should be reported for style changes.
     *
     * @type {string[]}
     */
    const reportFiles = [];

    for (const filePath of changedObsidianFiles) {
        try {
            if (detectStyleChanges(filePath, from, to)) {
                reportFiles.push(filePath);
            }
        }
        catch (error) {
            console.error(`Error processing file ${filePath}:`, error);
        }
    }

    if (config.test) {
        reportFiles.push("Test file");
    }

    if (reportFiles.length > 0) {
        if (config.asana) {
            try {
                await createAsanaTask(reportFiles, to);
            }
            catch (error) {
                console.error("Error creating Asana task:", error.message);

                if (error.response?.error?.text) {
                    console.error(error.response.error.text);
                }
            }
        }

        console.log(`\x1b[1;31mStyle changes detected in commit ${to}.\x1b[0m`);
    }
    else {
        console.log(`No style changes detected in commit ${to}.`);
    }
}

async function main() {
    const revList = runCommand("git", ["rev-list", "--first-parent", `${config.from}..${config.to}`], repoRoot).split(/\r?\n/);

    if (revList.length < 1) {
        console.log("No commits found.");
        process.exit(1);
    }

    revList.push(config.from);
    revList.reverse();

    for (let i = 0; i < revList.length - 1; i++) {
        await processCommit(revList[i], revList[i + 1]);
    }
}
