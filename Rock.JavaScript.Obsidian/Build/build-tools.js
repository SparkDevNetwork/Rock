/* eslint-disable */
const glob = require("glob");
const { existsSync, statSync, readdirSync } = require("fs");
const path = require("path");
const os = require("os");
const fs = require("fs");
const ts = require("typescript");
const { exit, stdout, stderr } = require("process");
const { exec, spawn } = require("child_process");

const MaxConcurrency = Math.max(1, Math.floor(os.cpus().length / 3));

/**
 * @typedef {Object} ConfigOptions
 *
 * The configuration options to use when generating the rollup configuration
 * object(s). There are four build modes.
 *
 * Normal compiles the input file and
 * writes it to the output and keeps anything except partial files as external.
 *
 * The second mode is "lib" mode. This mode does the opposite, it bundles
 * everything into the output file.
 *
 * The third mode is "bundled". This will bundle everything in the same folder
 * or underneath the folder containing the input file. The input should point
 * to an index.ts file. This is used by the framework to build certain directories
 * as a single file rather than a bunch of separate micro files.
 *
 * The final mode is "nested". This is similar to "bundled" but it uses
 * automatically generated index files for each directory and then exports a
 * single object that contains all the files in that directory as child objects.
 * This is useless to blocks and plugin developers. It is used by the framework
 * to build special directories that are then handled by the loader.
 *
 * @property {Boolean | "auto"} minify If enabled the output file will be minified. Set to "auto" to use the environment variable TODO to determine if minification should be used.
 * @property {String} copy The directory to copy the output file(s) to. (optional)
 * @property {Boolean} bundled If enabled the entire directory tree will be bundled into a single file. The outputPath should specify a filename instead of a directory. This is used by the internal build system for certain folders.
 * @property {Boolean} nested Similar to bundled, but the directory tree will be re-exported in a nested format. Special option used by Enums and Utility folders of framework.
 * @property {Boolean} lib If enabled, all references will be compiled into a single library including any node modules. Useful for adding references to external libraries.
 */

// #region Fast Build

/**
 * Executes the "npm run build" command to perform an actual build.
 */
function performBuild() {
    const process = exec("npm run build");
    process.stdout.pipe(stdout);
    process.stderr.pipe(stderr);

    process.on("exit", (exitCode) => exit(exitCode));
}

/**
 * Checks if any source file has been modified since the build stamp. If so
 * then a full build is performed via the "npm run build" script. Otherwise
 * no action will be taken.
 *
 * @param {string} pattern The glob pattern to use when checking for files modified since the buildstamp.
 */
function fastBuild(pattern) {
    // If the file doesn't exist, build is required.
    if (!existsSync("dist/.buildstamp")) {
        performBuild();
        return;
    }

    const buildstamp = statSync("dist/.buildstamp");

    let newestFileStamp = 0;

    const files = glob.globSync(pattern.replace(/\\/g, "/"));

    for (const file of files) {
        const st = statSync(file);

        if (st.mtime.getTime() > newestFileStamp) {
            newestFileStamp = st.mtime.getTime();
        }
    }

    if (newestFileStamp > buildstamp.mtime.getTime()) {
        // Newer file sources exist, build required.
        performBuild();
    }
    else {
        // Dist is up to date, no build required.
        exit(0);
    }
}

// #endregion

// #region Declaration Types

class DeclarationBuilder {
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

    constructor() {
        /** @type Project[] */
        this.projectsToBuild = [];

        /** @type BuildTask[] */
        this.buildTasks = [];

        /** @type string[] */
        this.arguments = ["--declaration", "--emitDeclarationOnly"];
    }

    /**
     * Takes a project path and returns the absolute path to the tsconfig.json file.
     *
     * @param {string} projectPath The unresolved project path.
     *
     * @returns {string} The full path to the tsconfig.json file.
     */
    resolveProjectFile(projectPath) {
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
     * @param {(projectFile: string) => boolean} includeReferencedProject An optional callback used to determine if a referenced project should be included.
     */
    importProject(projectFile, includeReferencedProject) {
        if (this.projectsToBuild.some(p => p.projectFile === projectFile)) {
            return;
        }

        const cfg = ts.readConfigFile(projectFile, ts.sys.readFile);
        const tsconfig = ts.parseJsonConfigFileContent(cfg.config, ts.sys, path.dirname(projectFile));
        const references = [];

        if (tsconfig.projectReferences) {
            for (const p of tsconfig.projectReferences) {
                const referencedProjectFile = this.resolveProjectFile(p.path);
                if (!includeReferencedProject || includeReferencedProject(referencedProjectFile)) {
                    references.push(referencedProjectFile);
                }
            }
        }

        // Only add this project if it isn't a reference only project.
        if (tsconfig.fileNames.length > 0) {
            this.projectsToBuild.push({
                projectFile,
                references,
                config: tsconfig,
                built: false,
                failed: false
            });
        }

        for (const r of references) {
            this.importProject(r, includeReferencedProject);
        }
    }

    /**
     * Starts any build tasks that can be started.
     *
     * @returns {boolean} true if any tasks were started.
     */
    startBuildTasks() {
        if (this.buildTasks.length >= MaxConcurrency || this.projectsToBuild.some(p => p.failed)) {
            return false;
        }

        const projects = this.projectsToBuild
            .filter(p => {
                if (p.built) {
                    return false;
                }

                // Check for references that have not been built.
                for (const r of p.references) {
                    const rp = this.projectsToBuild.find(a => a.projectFile === r);

                    if (!rp.built) {
                        return false;
                    }
                }

                // Currently being built.
                if (this.buildTasks.some(t => t.projectFile === p.projectFile)) {
                    return false;
                }

                // Project has not been built and all references have been built.
                return true;
            });

        if (projects.length === 0) {
            return false;
        }

        let tryMoreTasks = false;

        while (projects.length > 0 && this.buildTasks.length < MaxConcurrency) {
            const project = projects.shift();

            if (!this.isProjectOutOfDate(project)) {
                const relativeFile = path.relative(process.cwd(), project.projectFile);

                project.built = true;

                console.log(`Project '${relativeFile}' is up-to-date.`);

                tryMoreTasks = true;

                continue;
            }

            const proc = spawn(["npx", "vue-tsc", ...this.arguments, "-p", `"${project.projectFile}"`].join(" "), { shell: true, stdio: "inherit" });

            this.buildTasks.push({
                projectFile: project.projectFile,
                process: proc,
                start: performance.now()
            });

            proc.on("error", err => {
                project.failed = true;
                console.error(`Error while building project "${project.projectFile}".`);
                console.error(err);

                const buildIndex = this.buildTasks.findIndex(t => t.projectFile === project.projectFile);
                this.buildTasks.splice(buildIndex, 1);
            });

            proc.on("exit", code => {
                if (code === 0) {
                    project.built = true;
                }
                else {
                    project.failed = true;
                }

                const buildIndex = this.buildTasks.findIndex(t => t.projectFile === project.projectFile);
                const duration = Math.floor(performance.now() - this.buildTasks[buildIndex].start);
                const relativeFile = path.relative(process.cwd(), project.projectFile);

                console.log(`Project '${relativeFile}' ${project.failed ? "failed to build" : "built"} in ${duration.toLocaleString()}ms.`);

                this.buildTasks.splice(buildIndex, 1);

                this.startBuildTasks();
            });
        }

        if (tryMoreTasks) {
            this.startBuildTasks();
        }
    }

    /**
     * Checks if the project is out of date and in need to building.
     *
     * @param {Project} project The project to be checked.
     *
     * @returns {boolean} 'true' if the project is out of date and should be built.
     */
    isProjectOutOfDate(project) {
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

        // Check all the files referenced in the last build to see if they are
        // newer than the build info file. If they are, then we need to rebuild.
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

        // Check for any files that had compiler errors last time we ran.
        for (const fileDiagnostic of buildInfo.program.semanticDiagnosticsPerFile) {
            if (Array.isArray(fileDiagnostic)) {
                for (const diagnostic of fileDiagnostic[1]) {
                    if (diagnostic.category === 1) {
                        return true;
                    }
                }
            }
        }

        // Do a final check of all files in the source directory. If any new
        // files got added then this will pick then up and rebuild.
        const files = glob.globSync(path.dirname(project.projectFile).replace(/\\/g, "/") + "/**/*");

        for (const file of files) {
            const fileStamp = fs.statSync(file).mtimeMs;
            if (fileStamp >= buildInfoStamp) {
                return true;
            }
        }

        return false;
    }

    /**
     * Builds all the projects and returns a promise that indicates when the
     * process has finished and if it was successful.
     *
     * @returns {Promise<{success: boolean, duration: number}>} A promise that can be awaited.
     */
    build() {
        return new Promise(resolve => {
            const start = performance.now();

            this.startBuildTasks();

            const timer = setInterval(() => {
                if (this.buildTasks.length > 0) {
                    return;
                }

                const duration = performance.now() - start;
                console.log(`Build completed in ${Math.round(duration / 100) / 10}s.`);

                clearInterval(timer);

                if (this.projectsToBuild.some(p => !p.built)) {
                    const neverBuiltProjects = this.projectsToBuild
                        .filter(p => !p.built)
                        .map(p => path.relative(process.cwd(), p.projectFile))
                        .join(", ");

                    console.error(`Error: The following projects never attempted to build: ${neverBuiltProjects}`);

                    resolve({
                        success: false,
                        duration
                    });
                }
                else if (this.projectsToBuild.some(p => p.failed)) {
                    resolve({
                        success: false,
                        duration
                    });
                }
                else {
                    resolve({
                        success: true,
                        duration
                    });
                }
            }, 100);
        });
    }
}

// #endregion

module.exports = {
    DeclarationBuilder,
    fastBuild,
};
