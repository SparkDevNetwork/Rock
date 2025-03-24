const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const EolPlugin = require("./webpack-eol-plugin");
const MonacoWebpackPlugin = require("monaco-editor-webpack-plugin");

module.exports = {
    mode: "production",
    devtool: "source-map",
    entry: {
        Monaco: {
            import: "./src/monaco.js",
            filename: "monaco.js"
        }
    },
    output: {
        path: path.resolve(__dirname, "../RockWeb/Scripts/Rock/Monaco"),
    },
    /* Enable caching so rebuilds are faster. */
    cache: {
        type: "filesystem",
        buildDependencies: {
            config: [__filename],
        },
    },
    resolve: {
        /* WebPack will process `.ts` and then if not found try `.js`. */
        extensions: [".ts", ".js"]
    },
    module: {
        rules: [
            /* all files with a `.ts` extension will be handled by `ts-loader`. */
            {
                test: /\.ts$/,
                loader: "ts-loader"
            },
            /* `.css` files are plain CSS, configure standard loader. */
            {
                test: /\.css$/,
                use: ["style-loader", "css-loader"]
            },
            {
                test: /\.ttf$/,
                use: ["file-loader"]
            },
        ],
    },
    optimization: {
        minimizer: [
            new TerserPlugin({
                /* Disable the LICENSE.txt file that normally gets generated. */
                extractComments: false,
            }),
        ],
    },
    plugins: [
        new MonacoWebpackPlugin({
            languages: ["typescript", "javascript", "css", "html", "csharp", "liquid", "json", "less", "markdown", "powershell", "sql", "xml"],
        }),
        new EolPlugin(),
    ]
};
