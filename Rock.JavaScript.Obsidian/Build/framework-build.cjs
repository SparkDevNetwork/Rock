/* eslint-disable */
//
// Driver that reproduces Framework/rollup.config.cjs using the external
// @sparkdevnetwork/obsidian-build-tools *library* directly, bypassing the
// obsidian-build executable (whose obsidian.config.json schema does not expose
// minify / lib / nested / bundled).
//
// The library intentionally hides rollup, so this driver never imports or
// references anything rollup-specific. It only constructs BundleBuilder objects
// and hands them to the library's run loop.
//
// Run once:   node Build/framework-build.cjs           (parallel by default)
// Serial:     node Build/framework-build.cjs --jobs 1
// Watch mode: node Build/framework-build.cjs --watch
// Job count:  node Build/framework-build.cjs --jobs N
//
const path = require("path");
const { readdirSync, statSync } = require("fs");

// The package defines only a "bin" (the executable) - no "main"/"exports" - so
// the library is reached through its compiled dist path.
const {
    buildAll,
    defineBuilders,
    defineScriptFileBuilder,
    watchAll
} = require("@sparkdevnetwork/obsidian-build-tools/dist/lib.js");

const useWatch = process.argv.includes("--watch");
const jobsIndex = process.argv.indexOf("--jobs");
const jobs = jobsIndex !== -1 && jobsIndex + 1 < process.argv.length
    ? parseInt(process.argv[jobsIndex + 1], 10)
    : undefined;

const workspacePath = path.resolve(__dirname, "..");
const srcPath = path.join(workspacePath, "Framework");
const outPath = path.join(workspacePath, "dist", "Framework");
const obsidianPath = path.join(workspacePath, "..", "RockWeb", "Obsidian");

// Mapping from rollup.config.cjs primitives to the library API:
//
//   defineConfigs(src, out, { minify, lib, copy })
//       -> defineBuilders(src, out, { minify, copy, script: { lib } })
//   defineFileConfig(dir,      out.js, { nested,  copy })
//       -> defineScriptFileBuilder(dir,      out.js, { copy, script: { nested } })
//   defineFileConfig(index.ts, out.js, { bundled, copy })
//       -> defineScriptFileBuilder(index.ts, out.js, { copy, script: { bundled } })
//
// (Remove the `copy:` fields if you only want dist/ output for a diff run.)
const builders = [
    // Libs: bundle every dependency into each output and minify.
    ...defineBuilders(path.join(srcPath, "Libs"), path.join(outPath, "Libs"), {
        minify: true,
        copy: path.join(obsidianPath, "Libs"),
        script: { lib: true }
    }),

    // Utility: build the whole directory into one nested export object.
    defineScriptFileBuilder(path.join(srcPath, "Utility"), path.join(outPath, "Utility.js"), {
        copy: obsidianPath,
        script: { nested: true }
    }),

    // ValidationRules: bundle the index and its relative imports into one file.
    defineScriptFileBuilder(path.join(srcPath, "ValidationRules", "index.ts"), path.join(outPath, "ValidationRules.js"), {
        copy: obsidianPath,
        script: { bundled: true }
    }),

    // PageState: bundle the index and its relative imports into one file.
    defineScriptFileBuilder(path.join(srcPath, "PageState", "index.ts"), path.join(outPath, "PageState.js"), {
        copy: obsidianPath,
        script: { bundled: true }
    }),

    // Standard per-file directories (scripts + stylesheets + static assets).
    ...defineBuilders(path.join(srcPath, "Core"), path.join(outPath, "Core"), {
        copy: path.join(obsidianPath, "Core")
    }),

    ...defineBuilders(path.join(srcPath, "Directives"), path.join(outPath, "Directives"), {
        copy: path.join(obsidianPath, "Directives")
    }),

    ...defineBuilders(path.join(srcPath, "Controls"), path.join(outPath, "Controls"), {
        copy: path.join(obsidianPath, "Controls")
    }),

    ...defineBuilders(path.join(srcPath, "FieldTypes"), path.join(outPath, "FieldTypes"), {
        copy: path.join(obsidianPath, "FieldTypes")
    }),

    ...defineBuilders(path.join(srcPath, "Templates"), path.join(outPath, "Templates"), {
        copy: path.join(obsidianPath, "Templates")
    }),

    // SystemGuids: minified, one file per source.
    ...defineBuilders(path.join(srcPath, "SystemGuids"), path.join(outPath, "SystemGuids"), {
        minify: true,
        copy: path.join(obsidianPath, "SystemGuids")
    })
];

// Enums: each domain directory becomes a single nested-export file.
const enumsPath = path.join(srcPath, "Enums");
readdirSync(enumsPath)
    .filter(d => statSync(path.join(enumsPath, d)).isDirectory())
    .forEach(d => {
        builders.push(defineScriptFileBuilder(
            path.join(enumsPath, d),
            path.join(outPath, "Enums", `${d}.js`),
            {
                copy: path.join(obsidianPath, "Enums"),
                script: { nested: true }
            }
        ));
    });

// The library owns the run loop, progress reporting, and error formatting, so
// the driver only has to define the builders and hand them off.
if (useWatch) {
    // Watch parallelizes the initial (and any multi-file) rebuild; --jobs 1
    // forces serial. parallel defaults to true inside watchAll.
    watchAll(builders, { jobs }).catch(() => {
        process.exitCode = 1;
    });
}
else {
    // The framework build runs in parallel by default; pass --jobs 1 to force
    // serial execution (buildAll treats parallel + jobs:1 as sequential).
    buildAll(builders, { parallel: true, jobs }).catch(() => {
        process.exitCode = 1;
    });
}
