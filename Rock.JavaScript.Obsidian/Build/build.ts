import { dirname, relative, resolve } from "path";
import { argv } from "process";
import { glob } from "glob";
import * as ts from "typescript";
import { copyFileSync, mkdirSync, Stats, statSync } from "fs";

const formatHost: ts.FormatDiagnosticsHost = {
    getCanonicalFileName: path => path,
    getCurrentDirectory: ts.sys.getCurrentDirectory,
    getNewLine: () => ts.sys.newLine
};

/**
 * Perform a one-time build of the project.
 *
 * @param project The path to the project to be built.
 */
function buildProject(project: string): void {
    const host = ts.createSolutionBuilderHost(ts.sys, undefined, reportDiagnostic);
    const builder = ts.createSolutionBuilder(host, [project], {});
    const startTime = Date.now();

    const result = builder.build();

    const duration = Date.now() - startTime;
    logMessage(`Build finished in ${duration / 1000}s.`);

    if (result === ts.ExitStatus.Success || result === ts.ExitStatus.DiagnosticsPresent_OutputsGenerated) {
        copyFiles();
    }

    ts.sys.exit(result);
}

/**
 * Perform a one-time build and then watch for changes and continue
 * to rebuild the project.
 * 
 * @param project The project to be built and watched.
 */
function watchProject(project: string): void {
    const createProgram = ts.createSemanticDiagnosticsBuilderProgram;
    let startTime = 0;

    // Create the compiler host that will let us customize the process.
    const host = ts.createWatchCompilerHost(
        project,
        {
            skipLibCheck: true
        },
        ts.sys,
        createProgram,
        reportDiagnostic,
        reportWatchStatusChanged
    );

    // You can technically override any given hook on the host, though you probably
    // don't need to.
    // Note that we're assuming `origCreateProgram` and `origPostProgramCreate`
    // doesn't use `this` at all.
    const origCreateProgram = host.createProgram;
    host.createProgram = (rootNames: ReadonlyArray<string> | undefined, options, host, oldProgram) => {
        startTime = Date.now();

        const result = origCreateProgram(rootNames, options, host, oldProgram);

        return result;
    };

    const origPostProgramCreate = host.afterProgramCreate;
    host.afterProgramCreate = program => {
        if (origPostProgramCreate) {
            origPostProgramCreate(program);
        }

        const duration = Date.now() - startTime;
        logMessage(`Build finished in ${duration / 1000}s.`);
    };

    // `createWatchProgram` creates an initial program, watches files, and updates
    // the program over time.
    ts.createWatchProgram(host);
}

/**
 * Log a message to the console along with the current time.
 * 
 * @param message The message to be logged.
 */
function logMessage(message: string): void {
    const date = new Date();
    const formatter = new Intl.DateTimeFormat(undefined, {
        hour12: true,
        hour: "numeric",
        minute: "2-digit",
        second: "2-digit"
    });
    const formattedDate = formatter.format(date);

    console.log(`[${formattedDate}] ${message}`);
}

/**
 * Report a diagnostic message.
 * 
 * @param diagnostic The diagnostic message to be reported.
 */
function reportDiagnostic(diagnostic: ts.Diagnostic): void {
    if (diagnostic.file && diagnostic.start) {
        const { line, character } = ts.getLineAndCharacterOfPosition(diagnostic.file, diagnostic.start);
        const message = ts.flattenDiagnosticMessageText(diagnostic.messageText, formatHost.getNewLine());
        logMessage(`${diagnostic.file.fileName} (${line + 1},${character + 1}): ${message}`);
    }
    else {
        logMessage(ts.flattenDiagnosticMessageText(diagnostic.messageText, formatHost.getNewLine()));
    }
}

/**
 * Prints a diagnostic every time the watch status changes.
 * This is mainly for messages like "Starting compilation" or "Compilation completed".
 */
function reportWatchStatusChanged(diagnostic: ts.Diagnostic): void {
    if (diagnostic.code === 6032 || diagnostic.code === 6031) {
        console.clear();
    }

    const messageText = ts.flattenDiagnosticMessageText(diagnostic.messageText, formatHost.getNewLine());

    logMessage(`TS${diagnostic.code}: ${messageText}`);

    if (diagnostic.code === 6194 && diagnostic.messageText === "Found 0 errors. Watching for file changes.") {
        copyFiles();
    }
}

type FileCopyPath = {
    path: string;
    relativePath: string;
};

/**
 * Copy the files from the build artifact directory to the RockWeb folder so
 * they can be used.
 */
function copyFiles(): void {
    const projectPath = resolve(__dirname, "..");
    const solutionPath = resolve(projectPath, "..");
    let srcPath = resolve(projectPath, "Framework");
    let distPath = resolve(projectPath, "dist", "Framework");

    // Glob doesn't work with windows \ in path names.
    if (process.platform === "win32") {
        srcPath = srcPath.replace(/\\/g, "/");
        distPath = distPath.replace(/\\/g, "/");
    }

    logMessage("Copying files...");

    let copiedCount = 0;
    let files: FileCopyPath[] = [];

    // Include all compiled JavaScript files.
    files = files.concat(glob.sync(`${distPath}/**/*.js`).map(f => {
        return {
            path: f,
            relativePath: relative(distPath, f)
        };
    }));

    // Include all source type definition files.
    // NOTE: Not yet, they aren't needed until we open up to third party plugins.
    //files = files.concat(glob.sync(`${srcPath}/**/*.d.ts`).map(f => {
    //    return {
    //        path: f,
    //        relativePath: relative(srcPath, f)
    //    };
    //}));

    // If we are not in release mode, then copy the map files too.
    if (process.env.CONFIGURATION !== "Release") {
        files = files.concat(glob.sync(`${distPath}/**/*.js.map`).map(f => {
            return {
                path: f,
                relativePath: relative(distPath, f)
            };
        }));
    }

    // Loop over each file to be copied.
    files.forEach(source => {
        const relativeName = source.relativePath;
        const destination = resolve(solutionPath, "RockWeb", "Obsidian", relativeName);

        mkdirSync(dirname(destination), { recursive: true });

        const sourceStat = statSync(source.path);
        let destinationStat: Stats | null;

        try {
            destinationStat = statSync(destination);
        }
        catch {
            destinationStat = null;
        }

        // Check if the file has changed, if so copy it.
        if (destinationStat === null || sourceStat.size !== destinationStat.size || sourceStat.mtimeMs > destinationStat.mtimeMs) {
            copyFileSync(source.path, destination);
            copiedCount++;
        }
    });

    logMessage(`Copied ${copiedCount} files.`);
}

/**
 * The main logic.
 */
function main(): void {
    const configPath = resolve(__dirname, "..", "Framework", "tsconfig.json");

    if (argv.includes("--watch")) {
        watchProject(configPath);
    }
    else {
        buildProject(configPath);
    }
}

main();
