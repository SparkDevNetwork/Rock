const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const EolPlugin = require("./webpack-eol-plugin");

module.exports = {
    mode: "production",
    devtool: process.env.CONFIGURATION === "Debug" ? "source-map" : false,
    entry: {
        "obsidian-core": {
            import: "./Core/core.ts",
            library: {
                type: "window",
                name: "Obsidian",
                export: "default"
            }
        },
        "obsidian-vendor": {
            import: "./Core/vendor.ts",
            library: {
                type: "system"
            }
        }
    },
    output: {
        path: path.resolve(__dirname, "dist"),
        filename: "[name].js",
    },
    resolve: {
        alias: {
            vue: path.resolve(__dirname, 'node_modules/vue/dist/vue.esm-bundler.js')
        },
        extensions: [".ts", ".js"]
    },
    /* Enable caching so rebuilds are faster. */
    cache: {
        type: 'filesystem',
        buildDependencies: {
            config: [__filename],
        },
    },
    module: {
        rules: [
            /* all files with a `.ts` extension will be handled by `ts-loader`. */
            {
                test: /\.ts$/,
                loader: "ts-loader"
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
        new EolPlugin(),
    ],
    /* Warn if any file goes over 250KB. */
    performance: {
        maxEntrypointSize: 250000,
        maxAssetSize: 250000
    }
};
