import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
import * as process from "process";
import * as path from "path";
import * as glob from "glob";

function generateAutoBundles(srcPath, outPath) {
    const files = glob.sync(srcPath + "/**/*.@(js)")
        .map(f => path.normalize(f).substring(cwd.length + 1))
        .filter(f => !f.endsWith(".d.ts") && !f.endsWith(".partial.js") && !f.endsWith(".partial.vue.js") && !f.endsWith(".partial.obs.js"));

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


const cwd = process.cwd();

const blocksConfig = generateAutoBundles(path.join(cwd, "dist", "Rock.JavaScript.Obsidian.Blocks", "src"), path.join("dist", "BlocksBundled"));

export default [blocksConfig];
