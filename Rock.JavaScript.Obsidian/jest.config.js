const { compilerOptions } = require("./Tests/tsconfig.json");

/** @type {import('ts-jest/dist/types').InitialOptionsTsJest} */
module.exports = {
    globals: {
        "ts-jest": {
            tsconfig: "./Tests/tsconfig.json",
            isolatedModules: true,
        }
    },
    preset: "ts-jest",
    testEnvironment: "jsdom",
    testMatch: [
        "**/?(*.)+(spec|test).ts"
    ],
    transform: {
        "^.+\\.obs$": [
            "@vue/vue3-jest",
            {
            }
        ]
    },
    moduleFileExtensions: ["js", "jsx", "ts", "tsx", "json", "node", "d.ts"],
    moduleNameMapper: {
        "^@Obsidian/(.*)$": `${__dirname}/Framework/$1`
    }
};
