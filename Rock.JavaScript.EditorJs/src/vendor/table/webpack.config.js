const path = require('path');

module.exports = {
  entry: './src/plugin.js',
  output: {
    path: path.join(__dirname, '/dist'),
    publicPath: '/',
    filename: 'bundle.js',
    library: 'Table',
    libraryExport: 'default',
    libraryTarget: 'umd'
  },
  module: {
    rules: [
      {
        test: /\.pcss$/,
        use: ['style-loader', 'css-loader', 'postcss-loader']
      },
      {
        test: /\.svg$/,
        loader: 'svg-inline-loader?removeSVGTagAttrs=false'
      }
    ]
  }
};