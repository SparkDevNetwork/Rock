#!/usr/bin/env node

// Generates two uppercase GUIDs for Obsidian block conversion.
// Usage: node generate-guids.js

const crypto = require( "crypto" );

const entityTypeGuid = crypto.randomUUID().toUpperCase();
const blockTypeGuid = crypto.randomUUID().toUpperCase();

console.log( `EntityTypeGuid: ${entityTypeGuid}` );
console.log( `BlockTypeGuid:  ${blockTypeGuid}` );
