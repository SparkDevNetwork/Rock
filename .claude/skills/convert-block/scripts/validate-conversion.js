#!/usr/bin/env node

// Validates an Obsidian block conversion: branch is a feature branch (not develop/main/master),
// all required block files exist, .d.ts placeholders match bag .cs files 1:1, and the WebForms
// .ascx/.ascx.cs files have been deleted. Surfaces additional partials as INFO.
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
    else if ( !currentBranch )
    {
        console.log( "FAIL  Could not determine current git branch (empty result)" );
        failed++;
    }
    else
    {
        console.log( `PASS  Branch: ${currentBranch}` );
        passed++;
    }
}
catch ( err )
{
    console.log( `FAIL  git branch --show-current failed: ${err.message}` );
    failed++;
}

// --- Always required ---
checkFile( `Rock.ViewModels/Blocks/${category}/${blockName}/${blockName}OptionsBag.cs`, "required" );
checkFile( `Rock.Blocks/${category}/${blockName}.cs`, "required" );
checkFile( `Rock.JavaScript.Obsidian.Blocks/src/${category}/${toCamelCase( blockName )}.obs`, "required" );

// .d.ts files: must match bags 1:1.
const bagsDir = path.join( repoRoot, `Rock.ViewModels/Blocks/${category}/${blockName}` );
const dtsDir = path.join( repoRoot, `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}` );

const bagFiles = fs.existsSync( bagsDir )
    ? fs.readdirSync( bagsDir ).filter( f => f.endsWith( ".cs" ) )
    : [];
const dtsFiles = fs.existsSync( dtsDir )
    ? fs.readdirSync( dtsDir ).filter( f => f.endsWith( ".d.ts" ) )
    : [];

if ( bagFiles.length === 0 )
{
    console.log( `FAIL  Rock.ViewModels/Blocks/${category}/${blockName}/ (no bag files found)` );
    failed++;
}
else if ( dtsFiles.length === 0 )
{
    console.log( `FAIL  Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}/ (no .d.ts files found)` );
    failed++;
}
else
{
    // Each FooBag.cs must have a matching fooBag.d.ts (camelCase).
    const expectedDtsFromBags = bagFiles.map( bag =>
    {
        const stem = bag.replace( /\.cs$/, "" );
        return `${toCamelCase( stem )}.d.ts`;
    } );

    const dtsSet = new Set( dtsFiles );
    const missing = expectedDtsFromBags.filter( d => !dtsSet.has( d ) );
    const orphan = dtsFiles.filter( d => !expectedDtsFromBags.includes( d ) );

    if ( missing.length === 0 && orphan.length === 0 )
    {
        console.log( `PASS  Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/${category}/${blockName}/ (${bagFiles.length} bags / ${dtsFiles.length} .d.ts, 1:1 match)` );
        passed++;
    }
    else
    {
        if ( missing.length > 0 )
        {
            console.log( `FAIL  .d.ts files missing for bags: ${missing.join( ", " )}` );
            failed++;
        }
        if ( orphan.length > 0 )
        {
            console.log( `WARN  .d.ts files with no matching bag (may need cleanup): ${orphan.join( ", " )}` );
            warnings++;
        }
    }
}

// WebForms deleted.
checkFile( `RockWeb/Blocks/${category}/${blockName}.ascx`, "deleted" );
checkFile( `RockWeb/Blocks/${category}/${blockName}.ascx.cs`, "deleted" );

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
