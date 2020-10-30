// vue.config.js
const path = require("path");

let newPath = path.resolve(__dirname, "../public/")

console.log(newPath)

module.exports = {
  publicPath:process.env.NODE_ENV === 'production' ? '/Plugins/com_bemaservices/CustomBlocks/CVCH/ChristmasTags/' : '/',
  
  outputDir: 'dist/Plugins/com_bemaservices/CustomBlocks/CVCH/ChristmasTags/',
  assetsDir: './assets/',
  filenameHashing:false,

  "transpileDependencies": [
    "vuetify"
  ]
}