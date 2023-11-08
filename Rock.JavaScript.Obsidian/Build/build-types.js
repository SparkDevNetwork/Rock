const path = require("path");
const fs = require("fs");
const os = require("os");
const { spawn } = require("child_process");

const MaxConcurrency = os.cpus().length;
const mainProject = "Framework";
const execPath = path.join(__dirname, "obs-tsc.js");

/**
 * @typedef Project
 * @prop projectFile {string} The absolute path to the project file.
 * @prop references {string[]} The list of project files referenced by this project.
 * @prop built {boolean} Determines if the project has been built.
 * @prop failed {boolean} Determines if the project has failed to build.
 *
 * @typedef BuildTask
 * @prop projectFile {string} The absolute path to the project file being built.
 * @prop process {import("child_process").ChildProcess} The child process handling the task.
 * @prop start {number} The timestamp when the process started.
 */
/** @type Project[] */
const projectsToBuild = [];

/** @type BuildTask[] */
const buildTasks = [];

/**
 * Takes a project path and returns the absolute path to the tsconfig.json file.
 *
 * @param {string} projectPath The unresolved project path.
 *
 * @returns {string} The full path to the tsconfig.json file.
 */
function resolveProjectFile(projectPath) {
    if (!path.isAbsolute(projectPath)) {
        projectPath = path.resolve(projectPath);
    }

    return fs.statSync(projectPath).isDirectory()
        ? path.join(projectPath, "tsconfig.json")
        : projectPath;
}

/**
 * Imports the project and all references into the build pipeline.
 *
 * @param {string} projectFile The absolute path to the tsconfig.json file.
 */
function importProject(projectFile) {
    if (projectsToBuild.some(p => p.projectFile === projectFile)) {
        return;
    }

    const tsconfig = JSON.parse(fs.readFileSync(projectFile, { encoding: "utf-8" }));
    const references = [];

    if (tsconfig.references) {
        for (const p of tsconfig.references) {
            const refPath = resolveProjectFile(path.resolve(path.dirname(projectFile), p.path));

            references.push(refPath);
        }
    }

    // Only add this project if it isn't a reference only project.
    if (!tsconfig.files || tsconfig.files.length > 0 || !tsconfig.include || tsconfig.include.length > 0) {
        projectsToBuild.push({
            projectFile,
            references,
            built: false,
            failed: false
        });
    }

    for (const r of references) {
        importProject(r);
    }
}

/**
 * Starts any build tasks that can be started.
 *
 * @returns {boolean} true if any tasks were started.
 */
function startBuildTasks() {
    if (buildTasks.length >= MaxConcurrency || projectsToBuild.some(p => p.failed)) {
        return false;
    }

    const projects = projectsToBuild
        .filter(p => {
            if (p.built) {
                return false;
            }

            // Check for references that have not been built.
            for (const r of p.references) {
                const rp = projectsToBuild.find(a => a.projectFile === r);

                if (!rp.built) {
                    return false;
                }
            }

            // Currently being built.
            if (buildTasks.some(t => t.projectFile === p.projectFile)) {
                return false;
            }

            // Project has not been built and all references have been built.
            return true;
        });

    if (projects.length === 0) {
        return false;
    }

    while (projects.length > 0 && buildTasks.length < MaxConcurrency) {
        const project = projects.shift();

        const proc = spawn("node", [execPath, "--declaration", "--emitDeclarationOnly", "-p", project.projectFile]);

        buildTasks.push({
            projectFile: project.projectFile,
            process: proc,
            start: performance.now()
        });

        proc.on("error", err => {
            project.failed = true;
            console.error(`Error while building project "${project.projectFile}".`);
            console.error(err);

            const buildIndex = buildTasks.findIndex(t => t.projectFile === project.projectFile);
            buildTasks.splice(buildIndex, 1);
        });

        proc.on("exit", code => {
            if (code === 0) {
                project.built = true;
            }
            else {
                project.failed = true;
            }

            const buildIndex = buildTasks.findIndex(t => t.projectFile === project.projectFile);
            const duration = Math.floor(performance.now() - buildTasks[buildIndex].start);
            const relativeFile = path.relative(process.cwd(), project.projectFile);

            console.log(`Build for '${relativeFile}' ${project.failed ? "failed" : "completed"} in ${duration}ms.`);

            proc.stderr.pipe(process.stderr);
            proc.stdout.pipe(process.stdout);

            buildTasks.splice(buildIndex, 1);

            startBuildTasks();
        });
    }
}

const start = performance.now();
importProject(resolveProjectFile(mainProject));

startBuildTasks();

const timer = setInterval(() => {
    if (buildTasks.length > 0) {
        return;
    }

    const duration = performance.now() - start;
    console.log(`Build completed in ${Math.round(duration / 100) / 10}s.`);

    clearInterval(timer);
}, 100);
