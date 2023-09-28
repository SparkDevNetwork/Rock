const { pathsToModuleNameMapper } = require("ts-jest");
const { compilerOptions } = require("./tsconfig.json");

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
    moduleFileExtensions: ["js", "jsx", "ts", "tsx", "json", "node", "d.ts"],
    moduleNameMapper: {
        "^vue$": __dirname + "/node_modules/vue/index",
        ...pathsToModuleNameMapper(compilerOptions.paths, { prefix: __dirname })
    }
};
