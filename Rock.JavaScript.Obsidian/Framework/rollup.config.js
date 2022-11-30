import vue from "rollup-plugin-vue";
import typescript from "rollup-plugin-typescript2";
import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
//import babel from "@rollup/plugin-babel";
import postcss from "rollup-plugin-postcss";
import { terser } from "rollup-plugin-terser";
import cssnano from "cssnano";
import ttypescript from "ttypescript";
import copy from "rollup-plugin-copy";
import * as process from "process";
import * as path from "path";
import * as glob from "glob";

function fixStyleInjectPlugin() {
    return {
        name: "fix-style-external",
        resolveId(source) {
            if (source.indexOf("node_modules/style-inject") !== -1) {
                return { id: "style-inject", external: true };
            }

            return null;
        }
    };
}

function generateConfig(files, srcPath, outPath, tsConfig) {
    return {
        input: files,

        output: {
            format: "system",
            dir: outPath,
            entryFileNames: (chunk) => {
                if (chunk.facadeModuleId.indexOf(srcPath) === 0) {
                    return path.join(path.dirname(chunk.facadeModuleId.replace(srcPath, "").substring(1)), "[name].js");
                }

                return "[name].js";
            }
        },

        external: (target, source, c) => {
            if (["vue", "luxon", "axios", "mitt", "tslib"].includes(target)) {
                return true;
            }

            if (target.startsWith("@")) {
                return true;
            }

            // Check if this is a primary bundle.
            if (source === undefined) {
                return false;
            }

            // Check if it is a .vue file reference to itself.
            if (target.indexOf(".vue?vue&") !== -1) {
                return false;
            }

            // Check if it is a reference to a partial file.
            if (target.endsWith(".partial.ts") || target.endsWith(".partial.vue")) {
                return false;
            }

            if (target.startsWith(".")) {
                const targetPath = path.normalize(path.join(path.dirname(source), target));

                if (!targetPath.startsWith(srcPath)) {
                    return true;
                }

                return false;
            }
            else {
                if (!target.startsWith(srcPath)) {
                    return true;
                }

                console.log(target, source, c, srcPath);
            }

            throw `Unexpected target '${target}'`;
        },

        plugins: [
            vue(),

            fixStyleInjectPlugin(),

            resolve({
                extensions: [".js", ".ts", ".vue"],
                resolveOnly: ["style-inject"]
            }),

            // Process only `<style module>` blocks.
            postcss({
                modules: {
                    generateScopedName: "[local]___[hash:base64:5]",
                },
                include: /&module=.*\.css$/,
            }),

            // Process all `<style>` blocks except `<style module>`.
            postcss({ include: /(?<!&module=.*)\.css$/ }),

            commonjs(),

            typescript({
                typescript: ttypescript,
                tsconfig: tsConfig,
                tsconfigOverride: {
                    compilerOptions: {
                        module: "ESNext"
                    }
                },
                useTsconfigDeclarationDir: true
            }),

            copy({
                targets: [
                    {
                        src: "dist/Framework/*",
                        dest: "../RockWeb/Obsidian/"
                    }
                ],
                hook: "closeBundle"
            }),
        ]
    };

}

function generateBundle(srcPath, outPath) {
    return {
        input: [path.join(srcPath, "index.js")],

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
            resolve({
                extensions: [".js", ".ts"]
            }),

            commonjs()
        ]
    };
}

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

const libsConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "Libs"), path.join(cwd, "dist", "FrameworkBundled", "Libs"), true, true);
const utilityConfig = generateBundle(path.join(cwd, "dist", "Framework", "Utility"), path.join(cwd, "dist", "FrameworkBundled", "Utility.js"));
const validationRulesConfig = generateBundle(path.join(cwd, "dist", "Framework", "ValidationRules"), path.join(cwd, "dist", "FrameworkBundled", "ValidationRules.js"));
const pageStateConfig = generateBundle(path.join(cwd, "dist", "Framework", "PageState"), path.join(cwd, "dist", "FrameworkBundled", "PageState.js"));
const coreConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "Core"), path.join(cwd, "dist", "FrameworkBundled", "Core"));
const directivesConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "Directives"), path.join(cwd, "dist", "FrameworkBundled", "Directives"));
const controlsConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "Controls"), path.join(cwd, "dist", "FrameworkBundled", "Controls"));
const fieldTypesConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "FieldTypes"), path.join(cwd, "dist", "FrameworkBundled", "FieldTypes"));
const templatesConfig = generateAutoBundles(path.join(cwd, "dist", "Framework", "Templates"), path.join(cwd, "dist", "FrameworkBundled", "Templates"));

export default [libsConfig, utilityConfig, validationRulesConfig, pageStateConfig, coreConfig, directivesConfig, controlsConfig, fieldTypesConfig, templatesConfig];
