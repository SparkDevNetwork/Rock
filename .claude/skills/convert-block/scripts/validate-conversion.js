#!/usr/bin/env node

// Validates that all expected files exist (and WebForms files are deleted) after an Obsidian block conversion.
// Usage: node validate-conversion.js <Category> <BlockName> <detail|list|custom>

const fs = require( "fs" );
const path = require( "path" );

const args = process.argv.slice( 2 );

if ( args.length < 3 )
{
    console.error( "Usage: node validate-conversion.js <Category> <BlockName> <detail|list|custom>" );
    console.error( "Example: node validate-conversion.js Core ExceptionDetail detail" );
    process.exit( 1 );
}

const category = args[0];
const blockName = args[1];
const blockType = args[2].toLowerCase();

if ( !["detail", "list", "custom"].includes( blockType ) )
{
    console.error( `Invalid block type: "${args[2]}". Must be one of: detail, list, custom` );
    process.exit( 1 );
}

const { execSync } = require( "child_process" );
const repoRoot = process.cwd();

// camelCase helper: lowercase first character only (matches codebase convention).
function toCamelCase( str )
{
    return str.charAt( 0 ).toLowerCase() + str.slice( 1 );
}

// Tracking counters.
let passed = 0;
let failed = 0;
let warnings = 0;
const partials = [];

/**
 * Checks whether a file exists and reports the result.
 * @param {string} relativePath - Path relative to repo root.
 * @param {"required"|"expected"|"deleted"} expectation
 */
function checkFile( relativePath, expectation )
{
    const fullPath = path.join( repoRoot, relativePath );
    const exists = fs.existsSync( fullPath );

    if ( expectation === "deleted" )
    {
        if ( !exists )
        {
            console.log( `PASS  ${relativePath} (deleted)` );
            passed++;
        }
        else
        {
            console.log( `FAIL  ${relativePath} (should be deleted but still exists)` );
            failed++;
        }
    }
    else if ( expectation === "required" )
    {
        if ( exists )
        {
            console.log( `PASS  ${relativePath}` );
            passed++;
        }
        else
        {
            console.log( `FAIL  ${relativePath} (required but missing)` );
            failed++;
        }
    }
    else if ( expectation === "expected" )
    {
        if ( exists )
        {
            console.log( `PASS  ${relativePath}` );
            passed++;
        }
        else
        {
            console.log( `WARN  ${relativePath} (expected but missing — may be intentional)` );
            warnings++;
        }
    }
}

// Print header.
console.log( `Validating: ${category}/${blockName} (${blockType})` );
console.log( "-----------------------------------------------------" );

// --- Branch check ---
try
{
    const currentBranch = execSync( "git branch --show-current", { encoding: "utf8" } ).trim();
    const forbiddenBranches = ["develop", "main", "master"];

    if ( forbiddenBranches.includes( currentBranch ) )
    {
        console.log( `FAIL  Current branch is "${currentBranch}" — must be on a feature branch` );
        failed++;
    }
    else
    {
        console.log( `PASS  Branch: ${currentBranch}` );
        passed++;
    }
}
catch
{
    console.log( "WARN  Could not determine current git branch" );
    warnings++;
}

// --- Always required ---
checkFile( `Rock.ViewModels/Blocks/${category}/${blockName}/${blockName}OptionsBag.cs`, "required" );
checkFile( `Rock.Blocks/${category}/${blockName}.cs`, "required" );
checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${toCamelCase( blockName )}.obs`, "required" );

// At least one .d.ts placeholder.
const dtsDir = path.join( repoRoot, `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}` );
let hasDts = false;

if ( fs.existsSync( dtsDir ) )
{
    const dtsFiles = fs.readdirSync( dtsDir ).filter( f => f.endsWith( ".d.ts" ) );
    hasDts = dtsFiles.length > 0;
}

if ( hasDts )
{
    console.log( `PASS  Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}/ (has .d.ts files)` );
    passed++;
}
else
{
    console.log( `FAIL  Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}/ (no .d.ts files found)` );
    failed++;
}

// WebForms deleted.
checkFile( `RockWeb/Blocks/${category}/${blockName}.ascx`, "deleted" );
checkFile( `RockWeb/Blocks/${category}/${blockName}.ascx.cs`, "deleted" );

// --- Page parameter resolution check ---
// Obsidian's standard is IdKey. A page parameter resolved with PageParameter(...).AsInteger()
// is almost always a carried-over WebForms integer lookup that will fail for IdKeys and Guids.
// This is a heuristic WARN (a parameter that is genuinely a number is a valid exception), so it
// does not fail the run — but each hit must be reviewed and fixed unless it is truly numeric.
const blockCsPath = path.join( repoRoot, `Rock.Blocks/${category}/${blockName}.cs` );

if ( fs.existsSync( blockCsPath ) )
{
    const blockSource = fs.readFileSync( blockCsPath, "utf8" );
    const lines = blockSource.split( /\r?\n/ );
    const pageParamIntegerPattern = /PageParameter\s*\([^)]*\)\s*\.\s*AsInteger(OrNull)?\s*\(/;
    const offendingLines = [];

    lines.forEach( ( line, index ) =>
    {
        if ( pageParamIntegerPattern.test( line ) )
        {
            offendingLines.push( index + 1 );
        }
    } );

    if ( offendingLines.length > 0 )
    {
        console.log( `WARN  ${blockName}.cs resolves a page parameter with PageParameter(...).AsInteger() at line(s) ${offendingLines.join( ", " )}` );
        console.log( "       Obsidian's standard is IdKey. Unless the value is genuinely a number (page index, count, year)," );
        console.log( "       resolve it as Id/IdKey/Guid: Get( key, !PageCache.Layout.Site.DisablePredictableIds )." );
        console.log( "       See references/common-patterns.md § \"Page Parameter Resolution\"." );
        warnings++;
    }
    else
    {
        console.log( `PASS  ${blockName}.cs has no integer-only page parameter lookups` );
        passed++;
    }
}

// --- Type-specific checks ---
if ( blockType === "detail" )
{
    checkFile( `Rock.ViewModels/Blocks/${category}/${blockName}/${blockName}Bag.cs`, "required" );
    checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${blockName}/viewPanel.partial.obs`, "required" );
    checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${blockName}/editPanel.partial.obs`, "expected" );
}
else if ( blockType === "list" )
{
    checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${blockName}/types.partial.ts`, "expected" );
    checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${blockName}/gridSettingsModal.partial.obs`, "expected" );
}

// --- Partial file discovery ---
// Exclude files already checked above so INFO only shows additional partials.
const knownPartials = new Set( ["viewPanel.partial.obs", "editPanel.partial.obs", "gridSettingsModal.partial.obs", "types.partial.ts"] );
const partialsDir = path.join( repoRoot, `Rock.JavaScript.Obsidian.Blocks/src/${category}/${blockName}` );

if ( fs.existsSync( partialsDir ) )
{
    const allFiles = fs.readdirSync( partialsDir );
    const partialFiles = allFiles.filter( f => f.includes( ".partial." ) && !knownPartials.has( f ) );

    if ( partialFiles.length > 0 )
    {
        console.log( "" );
        console.log( "Partials found:" );

        for ( const file of partialFiles )
        {
            console.log( ` INFO  ${file}` );
            partials.push( file );
        }
    }
}

// --- Summary ---
console.log( "-----------------------------------------------------" );
console.log( `Result: ${passed} passed, ${failed} failed, ${warnings} warnings, ${partials.length} partials found` );

process.exit( failed > 0 ? 1 : 0 );
