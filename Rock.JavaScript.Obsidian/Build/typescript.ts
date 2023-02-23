import * as ts from "typescript";
import { translateVue } from "./sfc";

// The format diagnostics function helper to access host information.
const formatHost: ts.FormatDiagnosticsHost = {
    getCanonicalFileName: path => path,
    getCurrentDirectory: ts.sys.getCurrentDirectory,
    getNewLine: () => ts.sys.newLine
};

// Inform TypeScript that .vue files are allowed.
(ts as unknown as { supportedTSExtensions: string[][] }).supportedTSExtensions[0].push(".vue", ".obs");
(ts as unknown as { supportedTSExtensionsFlat: string[] }).supportedTSExtensionsFlat.push(".vue", ".obs");

/**
 * Report a diagnostic message.
 *
 * @param diagnostic The diagnostic message to be reported.
 */
export function reportDiagnostic(diagnostic: ts.Diagnostic): void {
    ts.sys.write(ts.formatDiagnostic(diagnostic, formatHost));
}

/**
 * Gets the System object to use when building a project.
 *
 * @returns A custom TypeScript System object.
 */
function getBuilderSystem(): ts.System {
    const origReadFile = ts.sys.readFile;

    const sys: ts.System = {
        ...ts.sys,

        readFile(path: string, encoding?: string | undefined): string | undefined {
            // If this is a vue file then translate it into pure TypeScript/JavaScript.
            if (path.endsWith(".vue") || path.endsWith(".obs")) {
                const content = origReadFile(path, encoding);

                return content ? translateVue(content, path) : undefined;
            }
            else {
                return origReadFile(path, encoding);
            }
        }
    };

    return sys;
}

/**
 * Creates a new builder host that can be used for building a solution.
 *
 * @returns A host that can be used to build a solution.
 */
export function createSolutionBuilderHost(): ts.SolutionBuilderHost<ts.EmitAndSemanticDiagnosticsBuilderProgram> {
    const host = ts.createSolutionBuilderHost(getBuilderSystem(), undefined, reportDiagnostic);

    // Override the writeFile function so we can modify the final output to
    // remove out temporary @ts-ignore markers.
    const origWriteFile = host.writeFile;
    host.writeFile = (path: string, data: string, writeByteOrderMark?: boolean | undefined): void => {
        if (path.endsWith(".vue.js") || path.endsWith(".obs.js")) {
            data = data.split("\n").filter(l => !l.match(/\s*\/\/ @ts-ignore/)).join("\n");
        }

        if (origWriteFile) {
            origWriteFile(path, data, writeByteOrderMark);
        }
    };

    return host;
}

/**
 * Creates a new builder host that can be used for building a solution.
 *
 * @returns A host that can be used to build a solution.
 */
export function createSolutionBuilderWithWatchHost(reportWatch: ts.WatchStatusReporter): ts.SolutionBuilderWithWatchHost<ts.EmitAndSemanticDiagnosticsBuilderProgram> {
    const host = ts.createSolutionBuilderWithWatchHost(getBuilderSystem(), undefined, reportDiagnostic, undefined, reportWatch);

    // Override the writeFile function so we can modify the final output to
    // remove out temporary @ts-ignore markers.
    const origWriteFile = host.writeFile;
    host.writeFile = (path: string, data: string, writeByteOrderMark?: boolean | undefined): void => {
        if (path.endsWith(".vue.js") || path.endsWith(".obs.js")) {
            data = data.split("\n").filter(l => !l.match(/\s*\/\/ @ts-ignore/)).join("\n");
        }

        if (origWriteFile) {
            origWriteFile(path, data, writeByteOrderMark);
        }
    };

    return host;
}
