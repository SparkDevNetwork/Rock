const { pathsToModuleNameMapper } = require("ts-jest");
const { compilerOptions } = require("./tsconfig.base.json");

const paths = { ...compilerOptions.paths };

for (const key of Object.keys(paths)) {
    const values = paths[key];

    paths[key] = values.map(v => {
        return v.replace("/dist/", "/");
    });
}

/** @type {import('ts-jest/dist/types').TsJestCompileOptions} */
module.exports = {
    preset: "ts-jest",
    testEnvironment: "jsdom",
    testEnvironmentOptions: {
        customExportConditions: ["node", "node-addons"]
    },
    testMatch: [
        "./**/?(*.)+(spec|test).ts"
    ],
    transform: {
        "^.+\\.ts$": [
            "ts-jest",
            {
                tsconfig: "./Tests/tsconfig.json",
                isolatedModules: true,
            }
        ],
        "^.+\\.obs$": [
            "@vue/vue3-jest",
            {
            }
        ]
    },
    moduleFileExtensions: ["js", "jsx", "ts", "tsx", "json", "node", "d.ts", "obs"],
    moduleNameMapper: {
        "^vue$": __dirname + "/node_modules/vue/index",
        ...pathsToModuleNameMapper(paths, { prefix: __dirname })
    }
};
