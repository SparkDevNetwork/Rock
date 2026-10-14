#!/usr/bin/env node

// Sentinel cross-check. Sends the developer's review request to Codex and Grok for independent
// security reviews, each with its own focus, and prints their answers for the Sentinel agent.
// Usage: node .claude/skills/sentinel/scripts/sentinel.js cross-check   (request on stdin)
//
// Vendor detection, sign-in checks, and running the CLIs all live in the shared
// .claude/scripts/vendor-cli.js. This file only builds the briefs.

const fs = require( "fs" );
const path = require( "path" );
const vendorCli = require( "../../../scripts/vendor-cli.js" );

const briefTemplatePath = path.join( __dirname, "..", "references", "vendor-brief.md" );

// Total time per vendor, retries included. Kept under Claude Code's 10-minute Bash limit.
const timeoutSeconds = 540;

// The brief tells each vendor to end with this line, so an answer that stops early can be caught and retried.
const completionMarker = "END OF REVIEW";

const focusAreas = {
    authz: "Authorization and data exposure. Missing or inconsistent authorization checks, IDOR "
        + "(a record loaded by an Id, IdKey, or Guid from the client without checking the person is "
        + "authorized for it), missing [Authenticate] or [Secured] attributes, fields or records "
        + "returned beyond what the caller may see, and operations checked at one entry point but "
        + "not another.",
    injection: "Injection and input handling. SQL injection, Lava injection (user input rendered as "
        + "a Lava template, or overly broad EnabledLavaCommands), XSS, unsafe deserialization, file "
        + "upload and path handling, and secrets or cryptography misuse."
};

// Each vendor gets a different focus so the second opinions add coverage instead of repeating each other.
// Vendors use the "deep" tier from .claude/model-tiers.json. Effort "high" is allowed by both deep tiers.
const crossCheckJobs = [
    { label: "codex-authz", vendor: "openai", tier: "deep", effort: "high", focus: "authz" },
    { label: "grok-injection", vendor: "grok", tier: "deep", effort: "high", focus: "injection" }
];

const defaultRequest = "No specific request. Review the uncommitted changes, including untracked files. "
    + "If there are none, review the last commit.";

/**
 * Fills in the brief template for one vendor. A replacer function is used so "$" in the request
 * is never treated as a replacement pattern.
 */
function buildBrief( template, request, focus ) {
    return template
        .replace( "{{request}}", () => request )
        .replace( "{{focus}}", () => focus );
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

async function crossCheck() {
    const request = ( await readStdin() ).trim() || defaultRequest;
    const template = fs.readFileSync( briefTemplatePath, "utf8" );

    const jobs = crossCheckJobs.map( job => ( {
        label: job.label,
        vendor: job.vendor,
        tier: job.tier,
        effort: job.effort,
        prompt: buildBrief( template, request, focusAreas[job.focus] ),
        completionMarker,
        retries: 1
    } ) );

    const runResult = await vendorCli.run( { readOnly: true, timeoutSeconds, jobs } );
    console.log( vendorCli.formatRunResults( runResult ) );
}

async function main() {
    const command = process.argv[2];

    if ( command === "cross-check" ) {
        await crossCheck();
        return;
    }

    console.log( "Usage: node .claude/skills/sentinel/scripts/sentinel.js cross-check   (request on stdin)" );
}

main()
    .catch( error => console.log( `Sentinel cross-check failed: ${error.message}` ) )
    .finally( () => {
        process.exitCode = 0;
    } );
