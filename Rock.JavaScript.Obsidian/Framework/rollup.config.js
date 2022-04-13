import vue from "rollup-plugin-vue";
import typescript from "rollup-plugin-typescript2";
import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
//import babel from "@rollup/plugin-babel";
import PostCSS from "rollup-plugin-postcss";
import ttypescript from "ttypescript";
import copy from "rollup-plugin-copy";
import * as process from "process";
import * as path from "path";
import * as glob from "glob";

const cwd = process.cwd();
const frameworkPath = path.join(cwd, "Framework");
let files = glob.sync(frameworkPath + "/**/*.@(ts|vue)")
    .filter(f => !f.endsWith(".d.ts"))
    .filter(f => f.indexOf(".partial.ts") === -1 && f.indexOf(".partial.vue") === -1)
    .map(f => path.normalize(f).substring(cwd.length + 1));

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

export default {
    input: files,

    output: {
        format: "system",
        dir: "dist/Framework",
        entryFileNames: (chunk) => {
            if (chunk.facadeModuleId.indexOf(frameworkPath) === 0) {
                return path.join(path.dirname(chunk.facadeModuleId.replace(frameworkPath, "").substring(1)), "[name].js");
            }

            return "[name].js";
        }
    },

    external: (target) => {
        if (["vue", "luxon", "axios", "mitt", "tslib"].includes(target)) {
            return true;
        }

        if (target[0] === ".") {
            return false;
        }

        return false;
    },

    plugins: [
        vue(),

        fixStyleInjectPlugin(),

        resolve({
            extensions: [".js", ".ts", ".vue"],
            resolveOnly: ["style-inject"]
        }),

        // Process only `<style module>` blocks.
        PostCSS({
            modules: {
                generateScopedName: "[local]___[hash:base64:5]",
            },
            include: /&module=.*\.css$/,
        }),

        // Process all `<style>` blocks except `<style module>`.
        PostCSS({ include: /(?<!&module=.*)\.css$/ }),

        commonjs(),

        typescript({
            typescript: ttypescript,
            tsconfig: "./Framework/tsconfig.json",
            tsconfigOverride: {
                compilerOptions: {
                    module: "ESNext"
                }
            },
            useTsconfigDeclarationDir: true,
            emitDeclarationOnly: true
        }),

    //    babel({
    //        extensions: [".js", ".ts"],
    //        exclude: "node_modules/**",
    //        babelHelpers: "runtime",
    //        presets: [
    //            ["@babel/preset-env", {
    //                targets: "edge >= 13 or chrome >= 50 or and_chr >= 50 or safari >= 10 or ios_saf >= 10 or firefox >= 43 or and_ff >= 43"
    //            }],
    //            "@babel/preset-typescript"
    //        ],
    //        "plugins": [
    //            ["@babel/plugin-transform-runtime"]
    //        ]
    //    }),

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
