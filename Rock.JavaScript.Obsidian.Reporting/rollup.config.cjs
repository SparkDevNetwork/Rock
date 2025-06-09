/* eslint-disable */
const path = require("path");
const { defineConfigs } = require("../Rock.JavaScript.Obsidian/Build/build-tools");

const workspacePath = path.resolve(__dirname);
const srcPath = path.join(workspacePath, "src");
const outPath = path.join(workspacePath, "dist");
const reportingPath = path.join(workspacePath, "..", "RockWeb", "Obsidian", "Reporting");

const configs = [
    ...defineConfigs(srcPath, outPath, {
        copy: reportingPath
    })
];

module.exports = configs;
