const webpack = require("webpack");
const path = require("path");
const LodashModuleReplacementPlugin = require("lodash-webpack-plugin");
// const NpmInstallPlugin = require("npm-install-webpack-plugin");
const { getIfUtils, removeEmpty } = require("webpack-config-utils");

if (!process.env.NODE_ENV) process.env.NODE_ENV = "development";
const { ifProduction, ifNotProduction } = getIfUtils(process.env.NODE_ENV);

module.exports = {
  devtool: "cheap-module-source-map",
  performance: {
    hints: false, 
  },
  entry: [
    "react-hot-loader/patch",
    path.resolve(__dirname, "./React/index.js"),
  ],
  output: {
    filename: "bundle.js",
    path: path.resolve(__dirname, "./"),
    publicPath: "http://localhost:8080/RockWeb/",
  },
  // resolve: {
  //   alias: {
  //     react: "preact-compat",
  //     "react-dom": "preact-compat",
  //   },
  // },
  module: {
    rules: [
      // {
      //   enforce: "pre",
      //   test: /\.(js)$/,
      //   loader: "eslint-loader",
      // },
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
    new webpack.optimize.OccurrenceOrderPlugin(),
    ifProduction(new webpack.optimize.UglifyJsPlugin({
      compress: {
        screw_ie8: true,
        warnings: false,
      }
    })),
    ifNotProduction(new webpack.HotModuleReplacementPlugin()),
    // ifNotProduction(new NpmInstallPlugin()),
    new webpack.NoErrorsPlugin(),
    new webpack.NamedModulesPlugin(),
  ]),
  devServer: {
    // clientLogLevel: "none",
    compress: true,
    inline: true,
    publicPath: "http://localhost:8080/RockWeb/",
    hot: true,
    // quiet: true,
    headers: {
      "Access-Control-Allow-Origin": "http://localhost:6229",
      "Access-Control-Allow-Credentials": "true",
    }
  }
}
