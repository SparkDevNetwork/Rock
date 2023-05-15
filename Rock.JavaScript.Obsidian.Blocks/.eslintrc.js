module.exports = {
    root: true,
    parser: "vue-eslint-parser",
    plugins: [
        "@typescript-eslint",
    ],
    extends: [
        "eslint:recommended",
        "plugin:@typescript-eslint/recommended",
    ],
    env: {
        browser: true,
        amd: true
    },
    parserOptions: {
        parser: "@typescript-eslint/parser",
        ecmaVersion: 6,
        sourceType: "module"
    },
    rules: {
        // Warn if tabs are used anywhere in a file.
        "no-tabs": "warn",

        // Warn if single quotes are used instead of double quotes.
        quotes: ["warn", "double", { "avoidEscape": true, "allowTemplateLiterals": true }],

        // Warn if semicolons are omitted.
        semi: "off",
        "@typescript-eslint/semi": ["error", "always"],

        // Warn about unused variables, unless they are prefixed with underscore.
        "@typescript-eslint/no-unused-vars": ["warn", {
            "argsIgnorePattern": "^_",
            "varsIgnorePattern": "^_"
        }],

        // Allow things like `value: number = 5`.
        "@typescript-eslint/no-inferrable-types": "off",

        // Enforce return types be specified on functions.
        "@typescript-eslint/explicit-function-return-type": [
            1,
            {
                allowExpressions: true
            }
        ],

        // Enforce certain naming conventions on all code.
        "@typescript-eslint/naming-convention": [
            "error",
            // By default everything will use camelCase unless otherwise noted below.
            {
                selector: "default",
                format: ["camelCase"]
            },

            // Variables and parameters that are unused may also be prefixed with an underscore.
            {
                selector: ["variable", "parameter"],
                format: ["camelCase"],
                modifiers: ["unused"],
                leadingUnderscore: "allow"
            },

            // Variables that are exported may use either camelCase or PascalCase.
            {
                selector: ["variable"],
                format: ["camelCase", "PascalCase"],
                modifiers: ["exported"]
            },

            // Enum members should be PascalCase as it is similar to a type.
            {
                selector: ["enumMember"],
                format: ["PascalCase"]
            },

            // Interfaces must be prefixed with a capital I.
            {
                selector: ["interface"],
                format: ["PascalCase"],
                prefix: ["I"]
            },

            // Allow object literals {} to use any naming since we often use
            // these to pass data to 3rd party libraries and don't have a choice.
            {
                selector: ["objectLiteralProperty", "objectLiteralMethod"],
                format: null
            },

            // Any type-like definitions (types, classes, interfaces, enums, etc.)
            // must be PascalCase.
            {
                selector: "typeLike",
                format: ["PascalCase"]
            }
        ],

        // Disable JavaScript brace-style in favor of TypeScript support. This
        // brace style matches our C# style.
        "brace-style": "off",
        "@typescript-eslint/brace-style": ["warn", "stroustrup"],

        // Make unwanted white-space inside parenthesis an error.
        "space-in-parens": ["error", "never"]
    },
    overrides: [
        {
            "files": ["src/**/*.ts", "src/**/*.vue", "src/**/*.obs"],
            "rules": {
                // Disable undefined use warnings, TypeScript will tell us.
                "no-undef": 0
            }
        }
    ]
};
