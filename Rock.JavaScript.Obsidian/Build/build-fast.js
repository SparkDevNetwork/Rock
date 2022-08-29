const glob = require("glob");
const fs = require("fs");
const { exit, stdout, stderr } = require("process");
const { exec } = require("child_process");

function performBuild() {
    const process = exec("npm run build");
    process.stdout.pipe(stdout);
    process.stderr.pipe(stderr);

    process.on("exit", (exitCode) => exit(exitCode));
}

function main() {
    // If the file doesn't exist, build is required.
    if (!fs.existsSync("dist/.buildstamp")) {
        performBuild();
        return;
    }

    const buildstamp = fs.statSync("dist/.buildstamp");

    let newestFileStamp = 0;

    glob("{Framework,System,Build}/**/*", (err, files) => {
        if (err) {
            return;
        }

        for (const file of files) {
            const st = fs.statSync(file);
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

main();
