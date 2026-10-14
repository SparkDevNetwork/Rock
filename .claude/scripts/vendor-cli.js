#!/usr/bin/env node

// Shared helper for running other vendors' AI CLIs (Codex, Grok) from Claude Code skills and agents.
// It knows how to find each CLI, check that it's signed in, and run it safely on Windows, macOS,
// and Linux. It knows nothing about what the prompts are for.
//
// Usage:
//   node .claude/scripts/vendor-cli.js detect [--json]
//   node .claude/scripts/vendor-cli.js run [--json] < jobs.json
//
// Job file format (stdin for "run"):
//   {
//     "readOnly": true,
//     "timeoutSeconds": 600,
//     "jobs": [
//       { "label": "codex-review", "vendor": "openai", "tier": "default", "effort": "high", "prompt": "...",
//         "completionMarker": "END OF REVIEW", "retries": 1 }
//     ]
//   }
//
// timeoutSeconds is the total time for each job, retries included.
// completionMarker (optional): text the prompt tells the model to end with. An answer without it
// is reported as "incomplete", and retried up to "retries" times while time remains.
//
// Vendors and tiers are resolved through .claude/model-tiers.json, so callers never name a model
// version. Always exits 0. A missing CLI, a failed sign-in, or a timeout is reported in the output.
//
// Can also be used as a module:
//   const vendorCli = require( "<path>/.claude/scripts/vendor-cli.js" );
//   const results = await vendorCli.run( { readOnly: true, jobs } );

const childProcess = require( "child_process" );
const crypto = require( "crypto" );
const fs = require( "fs" );
const os = require( "os" );
const path = require( "path" );

const repoRoot = path.resolve( __dirname, "..", ".." );
const modelTiersPath = path.join( repoRoot, ".claude", "model-tiers.json" );
const isWindows = process.platform === "win32";

const defaultTimeoutSeconds = 600;
const signInCheckTimeoutMs = 10000;
const minimumRetryMs = 60000;
const maxOutputCharacters = 200000;

// Model IDs and efforts end up on a command line, so they must be plain tokens.
const safeTokenPattern = /^[A-Za-z0-9._-]+$/;

// When a CLI is a .cmd shim on Windows it has to run through cmd.exe. Every argument is checked
// against this pattern first so nothing can be interpreted by the shell.
const safeShellArgumentPattern = /^[A-Za-z0-9._=:~\\/-]+$/;

// #region Vendor adapters

/*
    Each adapter says how to check sign-in and how to build the command line for one vendor.
    To add a vendor: add an entry here and a matching entry under "vendors" in model-tiers.json.
*/
const vendorAdapters = {
    openai: {
        displayName: "Codex",
        command: "codex",
        installHint: "Install the Codex CLI from OpenAI's documentation, then run: codex login",
        signInHint: "Run: codex login",

        isSignedIn( executable ) {
            const status = runSync( executable, [ "login", "status" ] );
            if ( status.exitCode === 0 ) {
                return true;
            }

            if ( process.env.CODEX_API_KEY || process.env.OPENAI_API_KEY ) {
                return true;
            }

            const codexHome = process.env.CODEX_HOME || path.join( os.homedir(), ".codex" );
            return isNonEmptyFile( path.join( codexHome, "auth.json" ) );
        },

        // Codex reads the prompt from stdin when the prompt argument is "-".
        buildInvocation( { model, effort, readOnly, prompt } ) {
            const args = [ "exec" ];

            if ( readOnly ) {
                args.push( "-s", "read-only" );
            }

            args.push( "-m", model, "-c", `model_reasoning_effort=${effort}`, "--ephemeral", "--color", "never", "-" );

            return { args, stdin: prompt };
        }
    },

    grok: {
        displayName: "Grok",
        command: "grok",
        installHint: "Install the Grok CLI from xAI's documentation, then run: grok login",
        signInHint: "Run: grok login (or set XAI_API_KEY)",

        isSignedIn() {
            if ( process.env.XAI_API_KEY ) {
                return true;
            }

            return isNonEmptyFile( path.join( os.homedir(), ".grok", "auth.json" ) );
        },

        // Grok's single-turn mode doesn't read stdin, so the prompt goes in a temp file.
        // Plan mode keeps Grok from making changes. JSON output tells us why it stopped,
        // how many steps it took, and what it cost.
        buildInvocation( { model, effort, readOnly, prompt } ) {
            const promptFile = path.join( os.tmpdir(), `vendor-cli-${crypto.randomUUID()}.txt` );
            fs.writeFileSync( promptFile, prompt, "utf8" );

            const args = [ "--prompt-file", promptFile, "-m", model, "--reasoning-effort", effort, "--output-format", "json" ];

            if ( readOnly ) {
                args.push( "--permission-mode", "plan" );
            }

            return { args, stdin: null, tempFiles: [ promptFile ] };
        },

        parseOutput( stdout ) {
            try {
                const response = JSON.parse( stdout );
                const details = [];

                if ( typeof response.num_turns === "number" ) {
                    details.push( `${response.num_turns} steps` );
                }

                if ( response.stopReason ) {
                    details.push( `stop reason ${response.stopReason}` );
                }

                if ( typeof response.total_cost_usd === "number" ) {
                    details.push( `cost $${response.total_cost_usd.toFixed( 2 )}` );
                }

                return { text: typeof response.text === "string" ? response.text : "", details };
            }
            catch {
                return { text: stdout, details: [] };
            }
        }
    }
};

// #endregion Vendor adapters

// #region Public API

/**
 * Reports whether each known vendor CLI is installed and signed in.
 * Never makes a model call.
 */
function detect() {
    return Object.keys( vendorAdapters ).map( vendor => detectVendor( vendor ) );
}

/**
 * Runs every job at the same time and returns one result per job, in the same order.
 * Jobs for vendors that aren't ready are returned as skipped rather than failing the run.
 */
async function run( request ) {
    const options = request || {};
    const jobs = Array.isArray( options.jobs ) ? options.jobs : [];
    const readOnly = options.readOnly === true;
    const timeoutSeconds = Number( options.timeoutSeconds ) > 0 ? Number( options.timeoutSeconds ) : defaultTimeoutSeconds;

    const tiers = loadModelTiers();
    const fingerprintBefore = readOnly ? getWorkingTreeFingerprint() : null;

    const results = await Promise.all( jobs.map( ( job, index ) => runJob( job, index, tiers, readOnly, timeoutSeconds ) ) );

    let workingTree = null;
    if ( readOnly ) {
        const fingerprintAfter = getWorkingTreeFingerprint();
        workingTree = fingerprintBefore !== null && fingerprintBefore === fingerprintAfter ? "unchanged" : "changed";
    }

    return { workingTree, results };
}

// #endregion Public API

// #region Jobs

/**
 * Runs a single job and always resolves, never rejects.
 */
async function runJob( job, index, tiers, readOnly, timeoutSeconds ) {
    const label = typeof job.label === "string" && job.label ? job.label : `${job.vendor || "job"}-${index + 1}`;
    const result = { label, vendor: job.vendor, tier: job.tier || "default", model: null, effort: null, status: "error", notes: [], output: "" };

    const adapter = vendorAdapters[job.vendor];
    if ( !adapter ) {
        result.notes.push( `Unknown vendor '${job.vendor}'. Known vendors: ${Object.keys( vendorAdapters ).join( ", " )}.` );
        return result;
    }

    const detection = detectVendor( job.vendor );
    if ( detection.state !== "ready" ) {
        result.status = "skipped";
        result.notes.push( detection.message );
        return result;
    }

    const resolved = resolveModel( tiers, job.vendor, result.tier, job.effort );
    if ( resolved.error ) {
        result.status = "skipped";
        result.notes.push( resolved.error );
        return result;
    }

    result.model = resolved.model;
    result.effort = resolved.effort;
    result.notes.push( ...resolved.notes );

    if ( typeof job.prompt !== "string" || !job.prompt.trim() ) {
        result.notes.push( "The job has no prompt." );
        return result;
    }

    const completionMarker = typeof job.completionMarker === "string" ? job.completionMarker.trim() : "";
    const retries = Number.isInteger( job.retries ) && job.retries > 0 ? job.retries : 0;
    const startedAt = Date.now();
    const deadline = startedAt + timeoutSeconds * 1000;

    /*
        9/28/26 - CLAUDE

        Grok sometimes ends a run after its opening line, exits cleanly, and returns no review.
        A clean exit alone can't be trusted, so callers can ask for a completion marker. An answer
        without it is retried while time remains, then reported as "incomplete" instead of "ok".
        All attempts share one deadline so the whole job still fits inside the caller's timeout.

        Reason: Catch vendor answers that stop early.
    */
    for ( let attempt = 1; attempt <= retries + 1; attempt++ ) {
        const remainingMs = deadline - Date.now();
        if ( attempt > 1 && remainingMs < minimumRetryMs ) {
            result.notes.push( "Not enough time left to retry." );
            break;
        }

        const attemptResult = await runAttempt( adapter, detection.executable, resolved, readOnly, job.prompt, remainingMs );
        Object.assign( result, { status: attemptResult.status, output: attemptResult.output } );
        result.notes.push( ...attemptResult.notes );

        if ( result.status !== "ok" || !completionMarker ) {
            break;
        }

        if ( result.output.includes( completionMarker ) ) {
            result.output = result.output.replace( completionMarker, "" ).trim();
            break;
        }

        result.status = "incomplete";
        result.notes.push( `Attempt ${attempt} ended without a complete answer.` );
    }

    result.seconds = Math.round( ( Date.now() - startedAt ) / 1000 );

    return result;
}

/**
 * Runs one attempt of a job and always resolves, never rejects.
 */
async function runAttempt( adapter, executable, resolved, readOnly, prompt, timeoutMs ) {
    const attempt = { status: "error", output: "", notes: [] };
    let invocation = null;

    try {
        invocation = adapter.buildInvocation( { model: resolved.model, effort: resolved.effort, readOnly, prompt } );
        const processResult = await runAsync( executable, invocation.args, invocation.stdin, timeoutMs );
        const parsed = adapter.parseOutput ? adapter.parseOutput( processResult.stdout.trim() ) : { text: processResult.stdout, details: [] };

        attempt.output = truncate( parsed.text.trim() );

        if ( parsed.details.length ) {
            attempt.notes.push( `${adapter.displayName}: ${parsed.details.join( ", " )}.` );
        }

        if ( processResult.timedOut ) {
            attempt.status = "timeout";
            attempt.notes.push( `Stopped after ${Math.round( timeoutMs / 1000 )} seconds.` );
        }
        else if ( processResult.exitCode !== 0 ) {
            attempt.notes.push( `${adapter.displayName} exited with code ${processResult.exitCode}. It may not be signed in, or a flag or model ID may be wrong.` );
            const errorTail = lastLines( processResult.stderr, 15 );
            if ( errorTail ) {
                attempt.notes.push( errorTail );
            }
        }
        else {
            attempt.status = "ok";
        }
    }
    catch ( error ) {
        attempt.notes.push( error.message );
    }
    finally {
        for ( const tempFile of ( invocation && invocation.tempFiles ) || [] ) {
            try {
                fs.unlinkSync( tempFile );
            }
            catch {
                // Intentionally ignored: the temp file lives in the OS temp folder and is harmless if left behind.
            }
        }
    }

    return attempt;
}

/**
 * Looks up the model and effort for a vendor and tier in model-tiers.json.
 * An effort the tier doesn't allow falls back to the tier's default.
 */
function resolveModel( tiers, vendor, tier, requestedEffort ) {
    const tierConfig = tiers && tiers.vendors && tiers.vendors[vendor] && tiers.vendors[vendor].tiers && tiers.vendors[vendor].tiers[tier];
    if ( !tierConfig ) {
        return { error: `Tier '${tier}' for vendor '${vendor}' was not found in .claude/model-tiers.json.` };
    }

    const notes = [];
    const allowedEfforts = Array.isArray( tierConfig.overridable_to ) ? tierConfig.overridable_to : [];
    let effort = tierConfig.default_effort;

    if ( requestedEffort ) {
        if ( allowedEfforts.includes( requestedEffort ) ) {
            effort = requestedEffort;
        }
        else {
            notes.push( `Effort '${requestedEffort}' isn't allowed for ${vendor}/${tier}, so the default '${effort}' was used.` );
        }
    }

    const model = tierConfig.model;
    if ( !safeTokenPattern.test( model || "" ) || !safeTokenPattern.test( effort || "" ) ) {
        return { error: `The model or effort for ${vendor}/${tier} in .claude/model-tiers.json contains characters that aren't allowed.` };
    }

    return { model, effort, notes };
}

// #endregion Jobs

// #region Detection

/**
 * Finds a vendor's CLI and checks whether it's signed in.
 */
function detectVendor( vendor ) {
    const adapter = vendorAdapters[vendor];
    const executable = which( adapter.command );

    if ( !executable ) {
        return { vendor, displayName: adapter.displayName, state: "not-installed", message: `${adapter.displayName} is not installed. ${adapter.installHint}` };
    }

    if ( !adapter.isSignedIn( executable ) ) {
        return { vendor, displayName: adapter.displayName, state: "not-signed-in", executable, message: `${adapter.displayName} is installed but not signed in. ${adapter.signInHint}` };
    }

    return { vendor, displayName: adapter.displayName, state: "ready", executable, message: `${adapter.displayName} is ready.` };
}

/**
 * Finds a program on PATH. On Windows only files with a PATHEXT extension count, because npm
 * also installs an extensionless bash script next to the .cmd shim.
 */
function which( command ) {
    const directories = ( process.env.PATH || "" ).split( path.delimiter ).filter( Boolean );
    const extensions = isWindows
        ? ( process.env.PATHEXT || ".COM;.EXE;.BAT;.CMD" ).split( ";" ).filter( Boolean )
        : [ "" ];

    for ( const directory of directories ) {
        for ( const extension of extensions ) {
            const candidate = path.join( directory, command + extension );
            if ( isFile( candidate ) ) {
                return candidate;
            }
        }
    }

    return null;
}

// #endregion Detection

// #region Process helpers

/**
 * Decides how to start a program. Windows .cmd and .bat files can only run through cmd.exe,
 * so their arguments are checked first and the command line is built here, with the command
 * path quoted. Node deprecates passing an argument array together with shell: true (DEP0190).
 */
function prepareSpawn( executable, args ) {
    const needsShell = isWindows && /\.(cmd|bat)$/i.test( executable );

    if ( !needsShell ) {
        return { file: executable, args, shell: false };
    }

    if ( /["%]/.test( executable ) ) {
        throw new Error( `Refusing to run '${executable}' through cmd.exe because its path contains unsafe characters.` );
    }

    const unsafeArgument = args.find( argument => !safeShellArgumentPattern.test( argument ) );
    if ( unsafeArgument !== undefined ) {
        throw new Error( `Refusing to pass '${unsafeArgument}' through cmd.exe.` );
    }

    return { file: [ `"${executable}"`, ...args ].join( " " ), args: [], shell: true };
}

/**
 * Runs a short command and waits for it. Used for sign-in checks.
 */
function runSync( executable, args ) {
    try {
        const prepared = prepareSpawn( executable, args );
        const result = childProcess.spawnSync( prepared.file, prepared.args, {
            shell: prepared.shell,
            timeout: signInCheckTimeoutMs,
            windowsHide: true,
            encoding: "utf8",
            stdio: [ "ignore", "pipe", "pipe" ]
        } );

        return { exitCode: result.status === null ? -1 : result.status };
    }
    catch {
        return { exitCode: -1 };
    }
}

/**
 * Runs a vendor CLI, feeds it stdin, and kills the whole process tree if it runs too long.
 */
function runAsync( executable, args, stdin, timeoutMs ) {
    return new Promise( resolve => {
        const prepared = prepareSpawn( executable, args );
        const child = childProcess.spawn( prepared.file, prepared.args, {
            cwd: repoRoot,
            shell: prepared.shell,
            windowsHide: true,
            detached: !isWindows,
            stdio: [ "pipe", "pipe", "pipe" ]
        } );

        let stdout = "";
        let stderr = "";
        let timedOut = false;
        let isFinished = false;

        const timer = setTimeout( () => {
            timedOut = true;
            killTree( child );
        }, timeoutMs );

        const finish = exitCode => {
            if ( isFinished ) {
                return;
            }

            isFinished = true;
            clearTimeout( timer );
            resolve( { exitCode, stdout, stderr, timedOut } );
        };

        child.stdout.on( "data", data => {
            stdout += data;
        } );

        child.stderr.on( "data", data => {
            stderr += data;
        } );

        child.on( "error", error => {
            stderr += error.message;
            finish( -1 );
        } );

        child.on( "close", code => finish( code === null ? -1 : code ) );

        child.stdin.on( "error", () => {
            // Intentionally ignored: the CLI may exit before reading stdin, and that shows up in its exit code.
        } );

        if ( stdin ) {
            child.stdin.write( stdin );
        }

        child.stdin.end();
    } );
}

/**
 * Stops a process and everything it started. Windows needs taskkill for the child processes.
 */
function killTree( child ) {
    try {
        if ( isWindows ) {
            childProcess.execFileSync( "taskkill", [ "/pid", String( child.pid ), "/T", "/F" ], { stdio: "ignore", windowsHide: true } );
        }
        else {
            process.kill( -child.pid, "SIGKILL" );
        }
    }
    catch {
        // Intentionally ignored: the process may have already exited.
    }
}

// #endregion Process helpers

// #region Working tree check

/**
 * Hashes git status and the working tree diff so a read-only run can tell if anything changed.
 * Returns null if git isn't available.
 */
function getWorkingTreeFingerprint() {
    try {
        const gitOptions = { cwd: repoRoot, encoding: "utf8", maxBuffer: 256 * 1024 * 1024, windowsHide: true };
        const status = childProcess.execFileSync( "git", [ "status", "--porcelain", "--untracked-files=all" ], gitOptions );
        const diff = childProcess.execFileSync( "git", [ "diff", "HEAD", "--no-ext-diff", "--no-textconv" ], gitOptions );

        return crypto.createHash( "sha256" ).update( status ).update( diff ).digest( "hex" );
    }
    catch {
        return null;
    }
}

// #endregion Working tree check

// #region Small helpers

function loadModelTiers() {
    try {
        return JSON.parse( fs.readFileSync( modelTiersPath, "utf8" ) );
    }
    catch {
        return null;
    }
}

function isFile( filePath ) {
    try {
        return fs.statSync( filePath ).isFile();
    }
    catch {
        return false;
    }
}

function isNonEmptyFile( filePath ) {
    try {
        const stats = fs.statSync( filePath );
        return stats.isFile() && stats.size > 0;
    }
    catch {
        return false;
    }
}

function truncate( text ) {
    if ( text.length <= maxOutputCharacters ) {
        return text;
    }

    return `${text.slice( 0, maxOutputCharacters )}\n\n[Output cut off at ${maxOutputCharacters} characters.]`;
}

function lastLines( text, count ) {
    return ( text || "" ).trim().split( /\r?\n/ ).slice( -count ).join( "\n" );
}

function readStdin() {
    return new Promise( resolve => {
        let data = "";
        process.stdin.setEncoding( "utf8" );
        process.stdin.on( "data", chunk => {
            data += chunk;
        } );
        process.stdin.on( "end", () => resolve( data ) );
        process.stdin.on( "error", () => resolve( data ) );
    } );
}

// #endregion Small helpers

// #region Formatting

/**
 * Formats the results of run() as plain text for an agent or a person to read.
 */
function formatRunResults( runResult ) {
    const sections = runResult.results.map( result => {
        const details = [ result.vendor, `tier ${result.tier}` ];
        if ( result.model ) {
            details.push( `model ${result.model}` );
        }

        if ( result.effort ) {
            details.push( `effort ${result.effort}` );
        }

        const timing = typeof result.seconds === "number" ? ` (${result.seconds}s)` : "";
        const lines = [ `## ${result.label}: ${details.join( ", " )}: ${result.status}${timing}` ];

        for ( const note of result.notes ) {
            lines.push( `Note: ${note}` );
        }

        if ( result.status === "incomplete" ) {
            lines.push( "Note: The answer never finished. Treat this vendor as not having run." );
        }

        if ( result.output ) {
            lines.push( "", result.output );
        }

        return lines.join( "\n" );
    } );

    if ( runResult.workingTree === "unchanged" ) {
        sections.push( "Working tree: unchanged during vendor calls." );
    }
    else if ( runResult.workingTree === "changed" ) {
        sections.push( "WARNING: The working tree changed while vendor CLIs were running. Check git status before trusting anything." );
    }

    return sections.join( "\n\n" );
}

function formatDetection( detections ) {
    return detections.map( detection => `${detection.vendor} (${detection.displayName}): ${detection.message}` ).join( "\n" );
}

// #endregion Formatting

// #region Command line

async function main() {
    const [ command, ...flags ] = process.argv.slice( 2 );
    const isJson = flags.includes( "--json" );

    if ( command === "detect" ) {
        const detections = detect();
        console.log( isJson ? JSON.stringify( detections, null, 2 ) : formatDetection( detections ) );
        return;
    }

    if ( command === "run" ) {
        const input = await readStdin();
        let request;

        try {
            request = JSON.parse( input );
        }
        catch ( error ) {
            console.log( `Could not read the job list from stdin as JSON: ${error.message}` );
            return;
        }

        const runResult = await run( request );
        console.log( isJson ? JSON.stringify( runResult, null, 2 ) : formatRunResults( runResult ) );
        return;
    }

    console.log( "Usage:\n  node .claude/scripts/vendor-cli.js detect [--json]\n  node .claude/scripts/vendor-cli.js run [--json] < jobs.json" );
}

if ( require.main === module ) {
    main()
        .catch( error => console.log( `vendor-cli failed: ${error.message}` ) )
        .finally( () => {
            process.exitCode = 0;
        } );
}

module.exports = { detect, run, formatRunResults, formatDetection };

// #endregion Command line
