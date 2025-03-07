module.exports = {
    root: true,
    extends: [
        "eslint:recommended",
    ],
    env: {
        node: true
    },
    parserOptions: {
        ecmaVersion: 6,
        sourceType: "module"
    },
    rules: {
        // Warn if tabs are used anywhere in a file.
        "no-tabs": "warn",

        // Warn if single quotes are used instead of double quotes.
        quotes: ["warn", "double", { "avoidEscape": true, "allowTemplateLiterals": true }],

        // Warn if semicolons are omitted.
        semi: "error",

        // Disable JavaScript brace-style in favor of TypeScript support. This
        // brace style matches our C# style.
        "brace-style": ["error", "stroustrup"],

        // Make unwanted white-space inside parenthesis an error.
        "space-in-parens": ["error", "never"]
    }
};
