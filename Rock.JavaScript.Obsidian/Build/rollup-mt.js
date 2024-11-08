const { loadConfigFile } = require("rollup/loadConfigFile");
const path = require("path");
const fork = require("child_process").fork;
const os = require("os");

/**
 * @typedef CompileResult
 * @param source {string} The source filename that was compiled.
 * @param outputs {string[]} The output filename.
 * @param duration {number} The number of milliseconds the operation took.
 * @param errors {string[]} Any errors during compilation.
 */

function abortCompile() {
    compileError = true;

    for (const worker of workers) {
        try {
            worker.send({ type: "exit" });
        }
        catch (error) {
            // Intentionally ignored.
        }
    }
}

/**
 *
 * @param {CompileResult} result The result of the compile operation.
 */
function handleResult(result) {
    if (result.errors.length > 0) {
        process.stdout.write(`${result.source} [${result.duration}ms]\n`);

        for (const error of result.errors) {
            process.stderr.write(`${error}\n`);
        }

        abortCompile();
    }
    else {
        if (result.outputs.length === 1) {
            process.stdout.write(`${result.source} => \u001b[32m${result.outputs[0]}\u001b[39m [${result.duration} ms]\n`);
        }
        else {
            process.stdout.write(`${result.source} [${result.duration}ms]\n`);

            for (const output of result.outputs) {
                process.stdout.write(`    => \u001b[32m${output}\u001b[39m\n`);
            }
        }
    }
}

/** @type {import("child_process").ChildProcess[]} */
let workers = [];
let compileError = false;

async function main() {
    const workerCount = Math.max(1, Math.min(4, os.cpus().length));
    process.stdout.write(`Building with ${workerCount} workers.\n\n`);

    const config = await loadConfigFile(process.argv[2]);
    config.warnings.flush();

    /** @type {import("rollup").RollupOptions[]} */
    const options = config.options
        .filter(o => typeof o.input === "string" && o.output && o.output.length === 1);

    for (let i = 0; i < workerCount; i++) {
        const worker = fork(path.join(__dirname, "./rollup-mt-worker.js"), [
            process.argv[2]
        ]);

        worker.on("message", command => {
            if (command.type === "ready") {
                if (options.length > 0) {
                    const input = options[0].input;

                    options.splice(0, 1);

                    worker.send({
                        type: "compile",
                        data: input
                    });
                }
                else {
                    worker.send({ type: "exit" });
                }
            }
            else if (command.type == "result") {
                handleResult(command.data);
            }
        });

        worker.on("exit", code => {
            const workerIndex = workers.indexOf(worker);
            workers.splice(workerIndex, 1);

            if (code !== 0) {
                abortCompile();
            }

            if (workers.length === 0) {
                process.exit(compileError ? 1 : 0);
            }
        });

        workers.push(worker);
    }
}

main();
