const path = require("path");
const fs = require("fs");
const os = require("os");
const ts = require("typescript");
const { spawn } = require("child_process");

const MaxConcurrency = os.cpus().length;
const mainProject = "Framework";
const execPath = path.join(__dirname, "obs-tsc.js");

/**
 * @typedef Project
 * @prop projectFile {string} The absolute path to the project file.
 * @prop references {string[]} The list of project files referenced by this project.
 * @prop config {import("typescript").ParsedCommandLine} The tsconfig contents.
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

    const cfg = ts.readConfigFile(projectFile, ts.sys.readFile);
    const tsconfig = ts.parseJsonConfigFileContent(cfg.config, ts.sys, path.dirname(projectFile));
    const references = [];

    if (tsconfig.projectReferences) {
        for (const p of tsconfig.projectReferences) {
            references.push(resolveProjectFile(p.path));
        }
    }

    // Only add this project if it isn't a reference only project.
    if (tsconfig.fileNames.length > 0) {
        projectsToBuild.push({
            projectFile,
            references,
            config: tsconfig,
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

    let tryMoreTasks = false;

    while (projects.length > 0 && buildTasks.length < MaxConcurrency) {
        const project = projects.shift();

        if (!isProjectOutOfDate(project)) {
            const relativeFile = path.relative(process.cwd(), project.projectFile);

            project.built = true;

            console.log(`Project '${relativeFile}' is up-to-date.`);

            tryMoreTasks = true;

            continue;
        }

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

            console.log(`Project '${relativeFile}' ${project.failed ? "failed to build" : "built"} in ${duration}ms.`);

            proc.stderr.pipe(process.stderr);
            proc.stdout.pipe(process.stdout);

            buildTasks.splice(buildIndex, 1);

            startBuildTasks();
        });
    }

    if (tryMoreTasks) {
        startBuildTasks();
    }
}

/**
 * Checks if the project is out of date and in need to building.
 *
 * @param {Project} project The project to be checked.
 *
 * @returns {boolean} 'true' if the project is out of date and should be built.
 */
function isProjectOutOfDate(project) {
    if (!project.config.options || !project.config.options.outDir || !project.config.options.rootDir) {
        return true;
    }

    const relOutDir = path.relative(project.config.options.rootDir, path.dirname(project.projectFile));
    const outDir = path.join(project.config.options.outDir, relOutDir);
    const buildInfoFile = path.resolve(path.join(outDir, "tsconfig.tsbuildinfo"));
    if (!fs.existsSync(buildInfoFile)) {
        return true;
    }

    const buildInfoStamp = fs.statSync(buildInfoFile).mtimeMs;
    const buildInfo = JSON.parse(fs.readFileSync(buildInfoFile, { encoding: "utf-8" }));

    if (!buildInfo.program || !buildInfo.program.fileNames) {
        return true;
    }

    for (const filename of buildInfo.program.fileNames) {
        let resolvedFilename = path.resolve(path.dirname(buildInfoFile), filename);

        // This is what the whole process is really about. This is a virtual
        // file that doesn't really exist, so typescript compiler thinks the
        // project needs to be rebuilt.
        if (path.basename(resolvedFilename) === "__vls_types.d.ts") {
            continue;
        }

        // Not sure why, but sometimes these get referenced as .obs.ts instead
        // of .obs, which means TypeScript has added a .ts extension for some
        // odd reason.
        if (resolvedFilename.endsWith(".obs.ts")) {
            const tmpFilename = resolvedFilename.substring(0, resolvedFilename.length - 3);
            if (fs.existsSync(tmpFilename)) {
                resolvedFilename = tmpFilename;
            }
        }

        if (!fs.existsSync(resolvedFilename)) {
            return true;
        }

        const fileStamp = fs.statSync(resolvedFilename).mtimeMs;
        if (fileStamp >= buildInfoStamp) {
            return true;
        }
    }

    return false;
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

    if (projectsToBuild.some(p => p.failed)) {
        process.exit(1);
    }
    else {
        process.exit(0);
    }
}, 100);
