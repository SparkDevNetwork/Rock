const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const EolPlugin = require("./webpack-eol-plugin");

module.exports = {
    mode: "production",
    devtool: "source-map",
    entry: {
        RealTime: {
            import: "./src/realtime/index.ts",
            filename: "realtime.js",
            library: {
                type: "assign-properties",
                name: ["Rock", "RealTime"]
            }
        }
    },
    output: {
        path: path.resolve(__dirname, "../RockWeb/Scripts/Rock"),
    },
    /* Enable caching so rebuilds are faster. */
    cache: {
        type: 'filesystem',
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
            }
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
    /* Warn if any file goes over 500KB. */
    performance: {
        maxEntrypointSize: 512000,
        maxAssetSize: 512000
    }
};
