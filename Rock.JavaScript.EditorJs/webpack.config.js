const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const EolPlugin = require("./webpack-eol-plugin");

module.exports = {
    mode: "production",
    devtool: "source-map",
    entry: {
        editor: "./src/editor.ts",
        "editor-tools": "./src/editor-tools.ts"
    },
    output: {
        path: path.resolve(__dirname, "../RockWeb/Scripts/Rock/UI/structuredcontenteditor"),
        filename: "[name].js",

        /* The library will export into the global namespace prefixed with Rock.UI.StructuredContent. */
        libraryTarget: "assign-properties",
        library: ["Rock", "UI", "StructuredContentEditor"]
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
            },
            /* The following are only needed because editor JS and plugins have
             * inline CSS and SVGs. Normally this stuff would go elsewhere. */
            /* `.pcss` files are PostCSS style and will by post-processed by `postcss-loader`. */
            {
                test: /\.pcss$/,
                use: ["style-loader", "css-loader", "postcss-loader"]
            },
            /* `.css` files are plain CSS, configure standard loader. */
            {
                test: /\.css$/,
                use: ["style-loader", "css-loader"]
            },
            /* `.svg` files to be converted to inline syntax. */
            {
                test: /\.svg$/,
                loader: "svg-inline-loader",
                options: {
                    removeSVGTagAttrs: false
                }
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
