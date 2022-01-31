const rimraf = require("rimraf");
const process = require("process");

if (process.env.CONFIGURATION === "Release") {
    console.log("Removing *.js.map files.");
    rimraf.sync("dist/**/*.js.map");
}
