import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
import postcss from "rollup-plugin-postcss";
import { terser } from "rollup-plugin-terser";
import cssnano from "cssnano";
import * as process from "process";
import * as path from "path";
import * as glob from "glob";
import * as fs from "fs";

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

// eslint-disable-next-line
function createFakeIndexes(indexes, srcPath) {
    const entries = fs.readdirSync(srcPath);
    let indexContent = "";

    entries.forEach(f => {
        const filePath = path.join(srcPath, f);

        if (fs.statSync(filePath).isDirectory()) {
            indexContent += `export * as ${f} from "./${f}/fake-generated-index.js";\n`;
            createFakeIndexes(indexes, filePath);
        }
        else if (f.endsWith(".js")) {
            indexContent += `export * as ${f.split(".")[0]} from "./${f}";\n`;
        }
    });

    const fakeIndexPath = path.join(srcPath, "fake-generated-index.js").substring(cwd.length + 1).replace(/\\/g, "/");
    indexes[fakeIndexPath] = indexContent;

    return fakeIndexPath;
}

// eslint-disable-next-line
function generateBundle(srcPath, outPath, autoIndex) {
    let virtualPlugin = void 0;
    let indexFile = path.join(srcPath, "index.js");

    if (autoIndex) {
        const virtualData = {};
        indexFile = createFakeIndexes(virtualData, srcPath);
        virtualPlugin = virtual(virtualData);
    }

    return {
        input: [indexFile],

        output: {
            format: "system",
            file: outPath
        },

        external: (target, source) => {
            // Check if this is a primary bundle.
            if (source === undefined) {
                return false;
            }

            // Check if it is a reference to a partial file.
            if (target.endsWith(".partial.ts")) {
                return false;
            }

            // If the file is a same directory relative or within the same path
            // then inline it so it's included in the bundle.
            if (target.startsWith("./")) {
                return false;
            }
            else {
                if (target.startsWith(srcPath)) {
                    return false;
                }
            }

            return true;
        },

        plugins: [
            virtualPlugin,

            resolve({
                extensions: [".js", ".ts"]
            }),

            commonjs()
        ]
    };
}

// eslint-disable-next-line
function generateAutoBundles(srcPath, outPath, alwaysBundleExternals, minify) {
    const files = glob.sync(srcPath + "/**/*.@(js)")
        .map(f => path.normalize(f).substring(cwd.length + 1))
        .filter(f => !f.endsWith(".d.ts") && !f.endsWith(".partial.js"));

    const config = {
        input: files,

        output: {
            format: "system",
            dir: outPath,
            entryFileNames: (chunk) => {
                if (chunk.facadeModuleId.indexOf(srcPath) === 0) {
                    return path.join(path.dirname(chunk.facadeModuleId.replace(srcPath, "").substring(1)), "[name].js");
                }

                return "[name].js";
            },
            assetFileNames: "[name].css"
        },

        external: (target, source) => {
            // Check if this is a primary bundle.
            if (source === undefined) {
                return false;
            }

            // Check if it is a reference to a partial file.
            if (target.endsWith(".partial.ts")) {
                return false;
            }

            // If the file is a same directory relative or within the same path
            // then inline it so it's included in the bundle.
            if (target.startsWith("./")) {
                return false;
            }
            else {
                if (target.startsWith(srcPath)) {
                    return false;
                }
            }

            // TSLib is always an external which is handled by System.
            if (target === "tslib") {
                return false;
            }

            // If the bundle generator is requesting all externals be bundled
            // then do so. This is primarily used by Libs.
            if (alwaysBundleExternals === true) {
                return false;
            }

            return true;
        },

        plugins: [
            postcss({
                plugins: [
                    cssnano()
                ]
            }),

            resolve({
                extensions: [".js", ".ts"]
            }),

            commonjs()
        ]
    };

    if (minify) {
        config.plugins.push(terser());
    }

    return config;
}


const cwd = process.cwd();
const frameworkPath = path.join(cwd, "dist", "Framework");
const bundledPath = path.join(cwd, "dist", "FrameworkBundled");
const configs = [];

configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "Libs"), path.join(cwd, "dist", "FrameworkBundled", "Libs"), true, true));
configs.push(generateBundle(path.join(cwd, "dist", "Framework", "Utility"), path.join(cwd, "dist", "FrameworkBundled", "Utility.js"), true));
configs.push(generateBundle(path.join(cwd, "dist", "Framework", "ValidationRules"), path.join(cwd, "dist", "FrameworkBundled", "ValidationRules.js")));
configs.push(generateBundle(path.join(cwd, "dist", "Framework", "PageState"), path.join(cwd, "dist", "FrameworkBundled", "PageState.js")));
configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "Core"), path.join(cwd, "dist", "FrameworkBundled", "Core")));
configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "Directives"), path.join(cwd, "dist", "FrameworkBundled", "Directives")));
configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "Controls"), path.join(cwd, "dist", "FrameworkBundled", "Controls")));
configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "FieldTypes"), path.join(cwd, "dist", "FrameworkBundled", "FieldTypes")));
configs.push(generateAutoBundles(path.join(cwd, "dist", "Framework", "Templates"), path.join(cwd, "dist", "FrameworkBundled", "Templates")));
configs.push(generateAutoBundles(path.join(frameworkPath, "SystemGuids"), path.join(bundledPath, "SystemGuids"), false, true));

const enumsPath = path.join(frameworkPath, "Enums");
fs.readdirSync(enumsPath).filter(d => fs.statSync(path.join(enumsPath, d)).isDirectory()).forEach(d => {
    configs.push(generateBundle(path.join(enumsPath, d), path.join(bundledPath, "Enums", `${d}.js`), true));
});

export default configs;
