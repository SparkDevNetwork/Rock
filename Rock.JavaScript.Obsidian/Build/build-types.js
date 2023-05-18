const execSync = require("child_process").execSync;
const path = require("path");
const { exit } = require("process");

const projects = [
    "Framework/Libs",
    "Framework/Enums",
    "Framework/SystemGuids",
    "Framework/Utility",
    "Framework/PageState",
    "Framework/ValidationRules",
    "Framework/Core",
    "Framework/Directives",
    "Framework/Controls",
    "Framework/FieldTypes",
    "Framework/Templates"
];

const execPath = path.join(__dirname, "obs-tsc.js");

for (const project of projects) {
    try {
        execSync(`node "${execPath}" --declaration --emitDeclarationOnly -p "${project}"`, {
            stdio: "inherit"
        });
    }
    catch (error) {
        exit(1);
    }
}
