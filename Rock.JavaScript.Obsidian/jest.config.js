const { compilerOptions } = require("./Tests/tsconfig.json");

/** @type {import('ts-jest/dist/types').InitialOptionsTsJest} */
module.exports = {
    preset: "ts-jest",
    testEnvironment: "jsdom",
    testEnvironmentOptions: {
        customExportConditions: ["node", "node-addons"]
    },
    testMatch: [
        "**/?(*.)+(spec|test).ts"
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
        "^@Obsidian/(.*)$": `${__dirname}/Framework/$1`
    }
};
