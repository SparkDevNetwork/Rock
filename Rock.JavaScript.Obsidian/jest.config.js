/** @type {import('ts-jest/dist/types').InitialOptionsTsJest} */
module.exports = {
    globals: {
        "ts-jest": {
            tsconfig: "./Tests/tsconfig.json",
            isolatedModules: true,
        }
    },
    preset: 'ts-jest',
    testEnvironment: 'jsdom',
    testMatch: [
        "**/?(*.)+(spec|test).ts"
    ]
};
