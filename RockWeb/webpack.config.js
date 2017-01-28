const webpack = require("webpack");
const path = require("path");
const LodashModuleReplacementPlugin = require("lodash-webpack-plugin");
const BundleAnalyzerPlugin = require("webpack-bundle-analyzer").BundleAnalyzerPlugin;
const { getIfUtils, removeEmpty } = require("webpack-config-utils");

if (!process.env.NODE_ENV) process.env.NODE_ENV = "development";
const { ifProduction, ifNotProduction } = getIfUtils(process.env.NODE_ENV);

let publicPath = "http://localhost:8080/RockWeb/Scripts/React/dist/";
if (ifProduction(true)) publicPath = "/Scripts/React/dist/";

const entry = [
  path.resolve(__dirname, "./Scripts/React/index.js"),
];

if (ifProduction(true)) entry.unshift("react-hot-loader/patch");
  
module.exports = {
  devtool: "cheap-module-source-map",
  performance: {
    hints: ifNotProduction(false), 
  },
  entry,
  output: {
    filename: "[name].blocks.bundle.js",
    chunkFilename: "[id].block.bundle.js",
    path: path.resolve(__dirname, "./Scripts/React/dist/"),
    publicPath: publicPath, 
  },
  resolve: removeEmpty({
    alias: ifProduction({
      react: "preact-compat/dist/preact-compat",
      "react-dom": "preact-compat/dist/preact-compat",
    }),
  }),
  module: {
    rules: [
      {
        enforce: "pre",
        test: /\.(js)$/,
        loader: "eslint-loader",
      },
      {
        test: /\.(js)$/,
        loader: "babel-loader",
        exclude: /node_modules/,
        options: {
          cacheDirectory: true,
        }
      },
    ]
  },
  plugins: removeEmpty([
    new LodashModuleReplacementPlugin,
    // new webpack.optimize.AggressiveMergingPlugin(),
    new webpack.optimize.OccurrenceOrderPlugin(),
    ifProduction(new webpack.DefinePlugin({
      "process.env": {
        "NODE_ENV": JSON.stringify("production"),
        "__DEV__": false,
      },
    })),
    ifProduction(new webpack.optimize.UglifyJsPlugin({
      compress: {
        screw_ie8: true,
        warnings: false,
        dead_code: true,
        unused: true,
      },
    })),
    ifNotProduction(new webpack.HotModuleReplacementPlugin()),
    new webpack.NoEmitOnErrorsPlugin(),
    new webpack.NamedModulesPlugin(),
    new BundleAnalyzerPlugin(),
  ]),
  devServer: {
    // clientLogLevel: "none",
    compress: true,
    inline: true,
    publicPath: "http://localhost:8080/RockWeb/Scripts/React/dist/",
    hot: true,
    // quiet: true,
    headers: {
      "Access-Control-Allow-Origin": "http://localhost:6229",
      "Access-Control-Allow-Credentials": "true",
    }
  }
}
