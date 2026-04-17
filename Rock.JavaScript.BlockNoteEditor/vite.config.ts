import { defineConfig } from "vite";
import path from "path";
import fs from "fs";
import { EOL } from "os";
import cssInjectedByJsPlugin from "vite-plugin-css-injected-by-js";

export default defineConfig({
    plugins: [
        cssInjectedByJsPlugin(),
        {
            name: 'move-dist-folder',
            closeBundle() {
                const src = path.resolve(__dirname, "dist", "blocknoteeditor.esm.js")
                const dest = path.resolve(__dirname, "..", "RockWeb", "Scripts", "Rock", "blocknoteeditor.esm.js")

                let data = fs.readFileSync(src);

                // Normalize line endings to match the OS to prevent git on Windows
                // thinking the file has changed.
                if (EOL === "\r\n") {
                    data = Buffer.from(data.toString().replace(/\n/g, EOL), "utf-8");
                }

                fs.writeFileSync(dest, data);
            }
        }
    ],
    build: {
        lib: {
            entry: path.resolve(__dirname, "src/main.ts"),
            name: "BlockNoteEditor",
            fileName: "blocknoteeditor.esm",
            formats: ["es"]
        },
        rollupOptions: {
            external: [],
            output: {
                inlineDynamicImports: true,
            }
        },
        minify: "terser",
    },
});
