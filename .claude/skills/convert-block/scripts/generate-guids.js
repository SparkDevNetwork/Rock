#!/usr/bin/env node

// Generates GUIDs for Obsidian block conversion.
//
// EntityTypeGuid is the live value used on the new C# class.
// BlockTypeGuid is informational ONLY — it goes in a `// was [Rock.SystemGuid.BlockTypeGuid( "..." )]`
//   comment immediately above the active attribute. The active
//   `[Rock.SystemGuid.BlockTypeGuid(...)]` MUST reuse the WebForms block's
//   existing GUID so BlockTypeService.StagePossibleMigrateWebFormsToObsidianBlock
//   can perform the chop at startup. See references/common-patterns.md
//   § GUID Assignment Rules.
//
// Usage: node generate-guids.js

const crypto = require( "crypto" );

const entityTypeGuid = crypto.randomUUID().toUpperCase();
const blockTypeGuid = crypto.randomUUID().toUpperCase();

console.log( `EntityTypeGuid (live):                       ${entityTypeGuid}` );
console.log( `BlockTypeGuid  (commented "was" line only):  ${blockTypeGuid}` );
console.log( `` );
console.log( `Active [BlockTypeGuid] attribute MUST reuse the WebForms block's existing GUID.` );
console.log( `Read it from RockWeb/Blocks/[Category]/[BlockName].ascx.cs.` );
