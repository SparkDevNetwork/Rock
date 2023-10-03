/* eslint-disable */
const glob = require("glob");
const { existsSync, statSync, readdirSync } = require("fs");
const path = require("path");
const { exit, stdout, stderr } = require("process");
const { exec } = require("child_process");
const defineRollupConfig = require("rollup").defineConfig;
const vue = require("rollup-plugin-vue");
const babel = require("@rollup/plugin-babel").default;
const commonjs = require("@rollup/plugin-commonjs");
const resolve = require("@rollup/plugin-node-resolve").default;
const postcss = require("rollup-plugin-postcss");
const { terser } = require("rollup-plugin-terser");
const copy = require("rollup-plugin-copy");
const cssnano = require("cssnano");
const { cwd } = require("process");

/**
 * @typedef {Object} ConfigOptions
 *
 * The configuration options to use when generating the rollup configuration
 * object(s). There are four build modes.
 *
 * Normal compiles the input file and
 * writes it to the output and keeps anything except partial files as external.
 *
 * The second mode is "lib" mode. This mode does the opposite, it bundles
 * everything into the output file.
 *
 * The third mode is "bundled". This will bundle everything in the same folder
 * or underneath the folder containing the input file. The input should point
 * to an index.ts file. This is used by the framework to build certain directories
 * as a single file rather than a bunch of separate micro files.
 *
 * The final mode is "nested". This is similar to "bundled" but it uses
 * automatically generated index files for each directory and then exports a
 * single object that contains all the files in that directory as child objects.
 * This is useless to blocks and plugin developers. It is used by the framework
 * to build special directories that are then handled by the loader.
 *
 * @property {Boolean | "auto"} minify If enabled the output file will be minified. Set to "auto" to use the environment variable TODO to determine if minification should be used.
 * @property {String} copy The directory to copy the output file(s) to. (optional)
 * @property {Boolean} bundled If enabled the entire directory tree will be bundled into a single file. The outputPath should specify a filename instead of a directory. This is used by the internal build system for certain folders.
 * @property {Boolean} nested Similar to bundled, but the directory tree will be re-exported in a nested format. Special option used by Enums and Utility folders of framework.
 * @property {Boolean} lib If enabled, all references will be compiled into a single library including any node modules. Useful for adding references to external libraries.
 */

// #region Fast Build

/**
 * Executes the "npm run build" command to perform an actual build.
 */
function performBuild() {
    const process = exec("npm run build");
    process.stdout.pipe(stdout);
    process.stderr.pipe(stderr);

    process.on("exit", (exitCode) => exit(exitCode));
}

/**
 * Checks if any source file has been modified since the build stamp. If so
 * then a full build is performed via the "npm run build" script. Otherwise
 * no action will be taken.
 *
 * @param {string} pattern The glob pattern to use when checking for files modified since the buildstamp.
 */
function fastBuild(pattern) {
    // If the file doesn't exist, build is required.
    if (!existsSync("dist/.buildstamp")) {
        performBuild();
        return;
    }

    const buildstamp = statSync("dist/.buildstamp");

    let newestFileStamp = 0;

    glob(pattern, (err, files) => {
        if (err) {
            return;
        }

        for (const file of files) {
            const st = statSync(file);
            if (st.mtime.getTime() > newestFileStamp) {
                newestFileStamp = st.mtime.getTime();
            }
        }

        if (newestFileStamp > buildstamp.mtime.getTime()) {
            // Newer file sources exist, build required.
            performBuild();
        }
        else {
            // Dist is up to date, no build required.
            exit(0);
        }
    });
}

// #endregion

// #region Rollup Plugins

/**
 * Simple plugin that allows for virtual files to be resolved and loaded by
 * rollup. This is used by the fakeIndex process to bundle all files in a
 * directory tree.
 *
 * @param {Record<string, string>} modules An object whose keys are filenames and values are the contents of the virtual files.
 */
function virtual(modules) {
    const resolvedIds = new Map();

    Object.keys(modules).forEach(id => {
        resolvedIds.set(path.resolve(id), modules[id]);
    });

    return {
        name: "virtual",

        resolveId(id, importer) {
            if (id in modules) {
                return id;
            }

            if (importer) {
                const resolved = path.resolve(path.dirname(importer), id);

                if (resolvedIds.has(resolved)) {
                    return resolved;
                }
            }
            else {
                const resolved = path.resolve(cwd(), id);

                if (resolvedIds.has(resolved)) {
                    return resolved;
                }
            }
        },

        load(id) {
            if (id in modules) {
                return modules[id];
            }
            else {
                return resolvedIds.get(id);
            }
        }
    };
}

// #endregion

// #region Functions

/**
 * Scans a directory tree and creates virtual index files for use in the virtual
 * plugin. These can then be compiled without having to keep an index.ts file
 * up to date by hand.
 *
 * @param {Record<string, string>} indexes An object to store the virtual index file contents into.
 * @param {string} sourcePath The source path to use when generating virtual indexes to be compiled.
 *
 * @returns {string} The index filename to be used for this directory.
 */
function createVirtualNestedIndex(indexes, sourcePath) {
    const entries = readdirSync(sourcePath);
    let indexContent = "";

    entries.forEach(f => {
        const filePath = path.join(sourcePath, f);

        if (statSync(filePath).isDirectory()) {
            indexContent += `export * as ${f} from "./${f}/virtual-index.js";\n`;
            createVirtualNestedIndex(indexes, filePath);
        }
        else if (f.endsWith(".ts")) {
            indexContent += `export * as ${f.split(".")[0]} from "./${f.split(".")[0]}";\n`;
        }
    });

    const virtualIndexPath = path.join(sourcePath, "virtual-index.js").replace(/\\/g, "/");
    indexes[virtualIndexPath] = indexContent;

    return virtualIndexPath;
}

// #endregion

// #region Rollup Configs

/**
 * Compiles all the files, including those in sub directories, for a given
 * directory. Any filename ending with .lib.ts will be automatically compiled
 * with ConfigOptions.lib option enabled.
 *
 * @param {String} sourcePath The base path to use when searching for files to compile.
 * @param {String} outputPath The base output path to use when searching for files to compile. The relative paths to the source files will be maintained when compiled to this location.
 * @param {ConfigOptions} options The options to pass to defineFileConfig.
 *
 * @returns {Object[]} An array of rollup configuration objects.
 */
function defineConfigs(sourcePath, outputPath, options) {
    options = options || {};

    const files = glob.sync(sourcePath + "/**/*.@(ts|obs)")
        .map(f => path.normalize(f).substring(sourcePath.length + 1))
        .filter(f => !f.endsWith(".d.ts") && !f.endsWith(".partial.ts") && !f.endsWith(".partial.obs"));

    return files.map(file => {
        const fileOptions = Object.assign({}, options);
        let outFile = file;

        // If the caller requested a copy operation, append the path to the
        // source file to the copy destination. If sourcePath is "/src" and
        // outputPath is "/dist" and file is "/src/a/b/c.js" then the new
        // copy path becomes "/dist/a/b".
        if (fileOptions.copy) {
            fileOptions.copy = path.join(fileOptions.copy, path.dirname(file));
        }

        // If the filename indicates it should be compiled as a library then
        // enable that option in the file options.
        if (file.endsWith(".lib.ts") || file.endsWith(".lib.obs")) {
            fileOptions.lib = true;
        }

        // Fix extension names for the output file.
        if (outFile.endsWith(".obs")) {
            outFile = `${outFile}.js`;
        }
        else if (outFile.endsWith(".ts")) {
            outFile = outFile.replace(/\.ts$/, ".js");
        }

        return defineFileConfig(path.join(sourcePath, file), path.join(outputPath, outFile), fileOptions);
    });
}

/**
 * Defines the rollup configuration object for a single file.
 *
 * @param {String} input The path to the input file or directory to be built.
 * @param {String} output The path to the output file or directory to write compiled files.
 * @param {ConfigOptions} options The configuration options to use when compiling the source.
 *
 * @returns A rollup configuration object.
 */
function defineFileConfig(input, output, options) {
    let virtualPlugin = void 0;
    let inputFile = input;
    const absoluteSrcPath = path.resolve(input);

    options = options || {};

    // If they requested the special nested structure, we need to generate
    // a special index file that exports all the files and folders
    // recursively.
    if (options.nested) {
        const virtualData = {};
        inputFile = createVirtualNestedIndex(virtualData, input);
        virtualPlugin = virtual(virtualData);
    }

    // Not really needed, but makes the build log much cleaner.
    inputFile = path.relative(cwd(), inputFile).replace(/\\/g, "/");

    const config = defineRollupConfig({
        input: inputFile,

        output: {
            format: "system",
            file: output,
            sourcemap: true
        },

        external: (target, source) => {
            // Check if this is a primary bundle.
            if (source === undefined) {
                return false;
            }

            // If we are building a library, always bundle all externals.
            if (options.lib) {
                // Except these special cases that are handled by our global
                // imports and need to be standard.
                if (target === "vue") {
                    return true;
                }

                return false;
            }

            // Check if it is a reference to a partial file, which is included.
            if (target.endsWith(".partial") || target.endsWith(".partial.ts") || target.endsWith(".partial.obs")) {
                return false;
            }

            // Always include vue extracted files.
            if (target.includes("?vue&type")) {
                return false;
            }

            // Always keep the CSS style inejector internal.
            if (target.includes("style-inject.es.js")) {
                return false;
            }

            // If we are building a bundled file then include any relative imports.
            if (options.bundled || options.nested) {
                if (target.startsWith("./") || target.startsWith(absoluteSrcPath)) {
                    return false;
                }
            }

            return true;
        },

        plugins: [
            virtualPlugin,

            resolve({
                browser: true,
                extensions: [".js", ".ts"]
            }),

            commonjs(),

            vue({
                include: [/\.obs$/i],
                preprocessStyles: true,
                needMap: false
            }),

            postcss({
                plugins: [
                    cssnano()
                ]
            }),

            babel({
                babelHelpers: "bundled",
                presets: [
                    ["@babel/preset-env", { targets: "edge >= 13, chrome >= 50, chromeandroid >= 50, firefox >= 53, safari >= 10, ios >= 10" }],
                    "@babel/typescript"
                ],
                extensions: [".js", ".jsx", ".ts", ".tsx", ".obs"],
                comments: false,
                sourceMaps: true
            })
        ]
    });

    // If they requested minification, then do so.
    if (options.minify) {
        config.plugins.push(terser());
    }

    // If they wanted to copy it, do that after the bundle is closed.
    if (options.copy) {
        const copySource = path.relative(cwd(), output).replace(/\\/g, "/");
        const copyDestination = path.relative(cwd(), options.copy).replace(/\\/g, "/");

        config.plugins.push(copy({
            targets: [
                {
                    src: copySource,
                    dest: copyDestination,
                },
                {
                    src: `${copySource}.map`,
                    dest: copyDestination,
                }
            ],
            hook: "closeBundle"
        }));
    }

    return config;
}

// #endregion

module.exports = {
    fastBuild,
    defineFileConfig,
    defineConfigs
};
