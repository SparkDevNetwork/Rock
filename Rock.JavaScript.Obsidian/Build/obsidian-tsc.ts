import { argv } from "process";
import * as ts from "typescript";
import { spawn, spawnSync } from "child_process";
import { createSolutionBuilderHost, createSolutionBuilderWithWatchHost, reportDiagnostic } from "./typescript";

/**
 * Perform a one-time build of the project.
 *
 * @param project The path to the project to be built.
 */
function buildProject(project: string, postBuildScript: string): void {
    const builder = ts.createSolutionBuilder(createSolutionBuilderHost(), [project], {});
    const result = builder.build();

    if (result !== 0) {
        ts.sys.exit(result);
    }

    if (postBuildScript) {
        runPostBuild();
    }
    else {
        ts.sys.exit(0);
    }

    function runPostBuild(): void {
        if (!postBuildScript) {
            return;
        }

        var scriptProc = spawn("npm", ["run", "--silent", postBuildScript], {
            shell: true
        });

        scriptProc.on("exit", code => ts.sys.exit(code));
        scriptProc.stdout.on("data", data => ts.sys.write(data));
        scriptProc.stderr.on("data", data => ts.sys.write(data));
    }
}

/**
 * Perform a one-time build of the project.
 *
 * @param project The path to the project to be built.
 */
function watchProject(project: string, postBuildScript: string): void {
    const host = createSolutionBuilderWithWatchHost(reportWatch);
    const origWatchFile = host.watchFile;
    const origWatchDirectory = host.watchDirectory;
    let postBuildQueue: (() => void)[] = [];

    // Override watchFile so we can intercept the callback. This allows us
    // to delay additional builds until the post-build script has completed.
    host.watchFile = (path: string, callback: ts.FileWatcherCallback, pollingInterval?: number, options?: ts.WatchOptions): ts.FileWatcher => {
        return origWatchFile(path, (fileName: string, eventKind: ts.FileWatcherEventKind) => {
            if (buildStart === 0) {
                callback(fileName, eventKind);
            }
            else {
                postBuildQueue.push(() => callback(fileName, eventKind));
            }
        }, pollingInterval, options);
    };

    // Override watchDirectory so we can intercept the callback. This allows us
    // to delay additional builds until the post-build script has completed.
    host.watchDirectory = (path: string, callback: ts.DirectoryWatcherCallback, recursive?: boolean, options?: ts.WatchOptions): ts.FileWatcher => {
        return origWatchDirectory(path, (fileName: string) => {
            if (buildStart === 0) {
                callback(fileName);
            }
            else {
                postBuildQueue.push(() => callback(fileName));
            }
        }, recursive, options);
    };

    const builder = ts.createSolutionBuilderWithWatch(host, [project], {});
    let buildStart: number = 0;

    initiateFirstBuild();

    /** Initiate the first build if required and then start the watcher. */
    function initiateFirstBuild() {
        var firstBuild = builder.getNextInvalidatedProject();
        if (firstBuild) {
            startBuild(firstBuild);
        }
        else {
            buildStart = Date.now();
        }

        // Start the watcher process.
        builder.build();
    }

    /** Start building the specified project. */
    function startBuild(proj: ts.InvalidatedProject<ts.EmitAndSemanticDiagnosticsBuilderProgram>, watchDiag?: ts.Diagnostic) {
        // Clear screen.
        ts.sys.write("\x1b[H\x1b[J");
        ts.sys.write("[    0.000s] ");

        if (watchDiag) {
            reportDiagnostic(watchDiag);
        }
        else {
            ts.sys.write("Initial build started.\n");
        }

        buildStart = Date.now();

        if (proj && proj.kind === ts.InvalidatedProjectKind.Build) {
            proj.emit();
        }
    }

    /** Get the build time as a left-padded string. */
    function paddedBuildTime(): string {
        const duration = Date.now() - buildStart;

        return `${((duration / 1000).toFixed(3) + "s").padStart(10)}`;
    }

    /** Called when the watch process has detected changes. */
    function reportWatch(diag: ts.Diagnostic) {
        var proj = builder.getNextInvalidatedProject();

        if (proj) {
            startBuild(proj, diag);
        }
        else {
            ts.sys.write(`[${paddedBuildTime()}] `);
            reportDiagnostic(diag);

            if (diag.code === 6194 && diag.messageText.toString().includes("Found 0 errors")) {
                runPostBuild();
            }
            else {
                ts.sys.write(`[${paddedBuildTime()}] Build failed.\n`);
                completeBuild();
            }
        }
    }

    /** Called by our code when the build has completely finished.  */
    function completeBuild() {
        buildStart = 0;

        const queue = postBuildQueue;
        postBuildQueue = [];

        queue.forEach(cb => cb());
    }

    /** Executes the post-build script if it was specified. */
    function runPostBuild(): void {
        if (!postBuildScript) {
            return completeBuild();
        }

        let lastLineData = "";

        var rollupProc = spawn("npm", ["run", "--silent", postBuildScript], {
            shell: true
        });

        rollupProc.on("exit", code => {
            if (code === 0) {
                ts.sys.write(`[${paddedBuildTime()}] Build completed.\n`);
            }
            else {
                ts.sys.write(`[${paddedBuildTime()}] Build failed.\n`);
            }

            completeBuild();
        });

        // Buffer output so we can display it one line at a time with the
        // timestamp prefixed to it.
        function dataReceived(data: string): void {
            const fullData = lastLineData + data;

            if (fullData.indexOf("\n") === -1) {
                lastLineData = fullData;
                return;
            }

            var lines = fullData.split("\n");
            for (var i = 0; i < lines.length - 1; i++) {
                ts.sys.write(`[${paddedBuildTime()}] ${lines[i]}\n`);
            }

            lastLineData = lines[lines.length - 1];
        }

        rollupProc.stdout.on("data", dataReceived);
        rollupProc.stderr.on("data", dataReceived);
    }
}

let isWatch = false;
let project = "";
let postBuildScript = "";
let args = argv.slice(2);

while (args.length > 0) {
    if (args[0] === "--watch") {
        isWatch = true;
        args = args.slice(1);
    }
    else if (args[0] === "--post-build" && args.length > 1) {
        postBuildScript = args[1];
        args = args.slice(2);
    }
    else {
        project = args[0];
        args = args.slice(1);
    }
}

if (!project) {
    console.error("Must provide path to project to build.");
    process.exit(1);
}

if (isWatch) {
    watchProject(project, postBuildScript);
}
else {
    buildProject(project, postBuildScript);
}
