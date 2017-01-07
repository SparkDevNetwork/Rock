const webpack = require("webpack");
const path = require("path");

process.env.NODE_ENV = "development";
module.exports = {
  devtool: "cheap-module-source-map",
  performance: {
    hints: false, 
  },
  entry: [
    "./RockWeb/Themes/Rock/Scripts/index.js",
  ],
  output: {
    filename: "bundle.js",
    path: path.resolve(__dirname, "./Themes/Rock"),
    publicPath: "http://localhost:8080/RockWeb/Themes/Rock/",
  },
  module: {
    rules: [
      {
        test: /\.(js)$/,
        loader: "babel-loader",
        options: {
          babelrc: false,
          presets: [
            "es2015",
            "stage-0",
            "react",
          ],
          cacheDirectory: true,
          plugins: [
            "react-hot-loader/babel",
          ]
        }
      },
    ]
  },
  plugins: [
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.HotModuleReplacementPlugin(),
    new webpack.NoErrorsPlugin(),
    new webpack.NamedModulesPlugin(),
  ],
  devServer: {
    clientLogLevel: "none",
    compress: true,
    inline: true,
    publicPath: "http://localhost:8080/RockWeb/Themes/Rock/",
    hot: true,
    quiet: true,
    headers: {
      "Access-Control-Allow-Origin": "http://localhost:6229",
      "Access-Control-Allow-Credentials": "true",
    }
  }
}
