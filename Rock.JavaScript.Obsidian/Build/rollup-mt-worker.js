const { loadConfigFile } = require("rollup/loadConfigFile");
const rollup = require("rollup");

/**
 * @typedef CompileResult
 * @param source {string} The source filename that was compiled.
 * @param outputs {string[]} The output filename.
 * @param duration {number} The number of milliseconds the operation took.
 * @param errors {string[]} Any errors during compilation.
 */

/**
 * Compiles a single options bundle.
 *
 * @param {import("rollup").RollupOptions} option
 *
 * @returns {CompileResult} The result of the compile operation.
 */
async function compile(option) {
    const compileStartTime = performance.now();

    try {
        const bundle = await rollup.rollup(option);
        const writeResult = await bundle.write(option.output[0]);
        await bundle.close();
        const duration = Math.floor(performance.now() - compileStartTime);

        return {
            source: option.input,
            outputs: writeResult.output.filter(c => c.type === "chunk").map(c => c.fileName),
            duration,
            errors: []
        };
    }
    catch (error) {
        let message = error instanceof Error ? error.message : `${error}`;
        const duration = Math.floor(performance.now() - compileStartTime);

        return {
            source: option.input,
            outputs: [],
            duration,
            errors: [message]
        };
    }
}

async function main() {
    const config = await loadConfigFile(process.argv[2]);

    config.warnings.flush();

    /** @type {import("rollup").RollupOptions[]} */
    const options = config.options
        .filter(o => typeof o.input === "string" && o.output && o.output.length === 1);

    process.on("message", command => {
        if (command.type === "exit") {
            process.exit(0);
        }
        else if (command.type === "compile") {
            compileFile(command.data);
        }
    });

    process.send({ type: "ready" });

    async function compileFile(file) {
        const option = options.find(o => o.input === file);

        if (!option) {
            process.send({
                type: "result",
                data: {
                    source: file,
                    outputs: [],
                    duraiton: 0,
                    errors: ["File was not found."]
                }
            });

            process.send({ type: "ready" });
        }
        else {
            const result = await compile(option);

            process.send({
                type: "result",
                data: result
            });

            process.send({ type: "ready" });
        }
    }
}

main();
