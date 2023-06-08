const path = require("path");
const webpack = require("webpack");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    mode: "production",
    devtool: process.env.CONFIGURATION === "Debug" ? "source-map" : false,
    entry: {
        "obsidian-core": {
            import: "./System/core.ts",
            library: {
                type: "window",
                name: "Obsidian",
                export: "default"
            }
        },
        "obsidian-vendor": {
            import: "./System/vendor.ts",
            library: {
                type: "system"
            }
        }
    },
    output: {
        path: path.resolve(__dirname, path.join("dist", "System")),
        filename: "[name].js",
    },
    resolve: {
        alias: {
            vue: path.resolve(__dirname, "node_modules/vue/dist/vue.esm-bundler.js"),
            mitt: path.resolve(__dirname, "node_modules/mitt/dist/mitt.mjs")
        },
        extensions: [".ts", ".js"]
    },
    /* Enable caching so rebuilds are faster. */
    cache: {
        type: "filesystem",
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
        new webpack.DefinePlugin({
            __VUE_OPTIONS_API__: true,
            __VUE_PROD_DEVTOOLS__: true
        })
    ],
    /* Warn if any file goes over 250KB. */
    performance: {
        maxEntrypointSize: 250000,
        maxAssetSize: 250000
    }
};
