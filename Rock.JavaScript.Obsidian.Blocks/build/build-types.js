const execSync = require("child_process").execSync;
const path = require("path");
const { exit } = require("process");

const projects = [
    "."
];

const execPath = path.join(__dirname, "..", "..", "Rock.JavaScript.Obsidian", "Build", "obs-tsc.js");

for (const project of projects) {
    try {
        execSync(`node "${execPath}" --noEmit -p "${project}"`, {
            stdio: "inherit"
        });
    }
    catch (error) {
        exit(1);
    }
}
