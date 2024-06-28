/* eslint-disable */
const path = require("path");
const { readdirSync, statSync } = require("fs");
const { defineConfigs, defineFileConfig } = require("../Build/build-tools");

const workspacePath = path.resolve(__dirname, "..");
const srcPath = path.join(workspacePath, "Framework");
const outPath = path.join(workspacePath, "dist", "Framework");
const obsidianPath = path.join(workspacePath, "..", "RockWeb", "Obsidian");

const configs = [
    ...defineConfigs(path.join(srcPath, "Libs"), path.join(outPath, "Libs"), {
        minify: true,
        lib: true,
        copy: path.join(obsidianPath, "Libs")
    }).map(cfg => {
        if (cfg.input.endsWith("html5-qrcode.ts")) {
            // Silence warnings complaining about previous bundler.
            cfg.onwarn = (warning, warn) => {
                if (warning.code === "THIS_IS_UNDEFINED" || warning.code === "SOURCEMAP_ERROR") {
                    return;
                }
                warn(warning);
            };

            return cfg;
        }
        else {
            return cfg;
        }
    }),

    defineFileConfig(path.join(srcPath, "Utility"), path.join(outPath, "Utility.js"), {
        nested: true,
        copy: obsidianPath
    }),

    defineFileConfig(path.join(srcPath, "ValidationRules", "index.ts"), path.join(outPath, "ValidationRules.js"), {
        bundled: true,
        copy: obsidianPath
    }),

    defineFileConfig(path.join(srcPath, "PageState", "index.ts"), path.join(outPath, "PageState.js"), {
        bundled: true,
        copy: obsidianPath
    }),

    ...defineConfigs(path.join(srcPath, "Core"), path.join(outPath, "Core"), {
        copy: path.join(obsidianPath, "Core")
    }),

    ...defineConfigs(path.join(srcPath, "Directives"), path.join(outPath, "Directives"), {
        copy: path.join(obsidianPath, "Directives")
    }),

    ...defineConfigs(path.join(srcPath, "Controls"), path.join(outPath, "Controls"), {
        copy: path.join(obsidianPath, "Controls")
    }),

    ...defineConfigs(path.join(srcPath, "FieldTypes"), path.join(outPath, "FieldTypes"), {
        copy: path.join(obsidianPath, "FieldTypes")
    }),

    ...defineConfigs(path.join(srcPath, "Templates"), path.join(outPath, "Templates"), {
        copy: path.join(obsidianPath, "Templates")
    }),

    ...defineConfigs(path.join(srcPath, "SystemGuids"), path.join(outPath, "SystemGuids"), {
        minify: true,
        copy: path.join(obsidianPath, "SystemGuids")
    })
];

const enumsPath = path.join(srcPath, "Enums");
readdirSync(enumsPath).filter(d => statSync(path.join(enumsPath, d)).isDirectory()).forEach(d => {
    configs.push(defineFileConfig(path.join(enumsPath, d), path.join(outPath, "Enums", `${d}.js`), {
        nested: true,
        copy: path.join(obsidianPath, "Enums")
    }));
});

export default configs;
