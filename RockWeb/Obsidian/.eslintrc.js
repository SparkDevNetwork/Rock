module.exports = {
    root: false,
    parser: '@typescript-eslint/parser',
    plugins: [
        '@typescript-eslint',
    ],
    extends: [
        'eslint:recommended',
        'plugin:@typescript-eslint/recommended',
    ],
    env: {
        browser: true,
        amd: true
    },
    parserOptions: {
        ecmaVersion: 6,
        sourceType: 'module'
    },
    rules: {
        'no-tabs': 'warn',
        quotes: ['warn', 'single', { 'avoidEscape': true, 'allowTemplateLiterals': true }],
        semi: ['warn', 'always'],
        'no-unused-vars': 'off',
        '@typescript-eslint/no-unused-vars': "warn",
        '@typescript-eslint/explicit-function-return-type': 'off',
        '@typescript-eslint/explicit-module-boundary-types': 'off'
    }
  };
