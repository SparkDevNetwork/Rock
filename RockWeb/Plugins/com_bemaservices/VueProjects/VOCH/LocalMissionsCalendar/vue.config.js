// vue.config.js
const path = require('path');

const newPath = path.resolve(__dirname).replace('VueProjects', 'CustomBlocks');

module.exports = {
  lintOnSave: true,
  publicPath:
        process.env.NODE_ENV === 'production'
          ? newPath.split('RockWeb')[1]
          : '/',

  outputDir: newPath,
  assetsDir: './assets/',
  filenameHashing: false,

  transpileDependencies: ['vuetify'],
};

