const eol = require("eol");
const sources = require("webpack-sources");

const pluginName = "WebpackEolPlugin";

/**
 * This plugin forces outputs from webpack to use CRLF instead of OS native.
 * Fixes issues with git thinking files are different when they are not.
 */
class WebpackEolPlugin {
    apply(compilation) {
        compilation.hooks.compilation.tap(pluginName, (compilation) => {
            compilation.hooks.afterOptimizeAssets.tap(
                {
                    name: pluginName,
                    stage: compilation.PROCESS_ASSETS_STAGE_DEV_TOOL
                },
                (assets) => {
                    for (let i in assets) {
                        if (i.endsWith(".js.map")) {
                            continue;
                        }

                        const asset = compilation.getAsset(i);  // <- standardized version of asset object
                        let source = asset.source.source(); // <- standardized way of getting asset source

                        // Special case for SVG files. We know they are text
                        // but because they are an asset, webpack treats them
                        // as a binary blob.
                        if (i.endsWith(".svg") && typeof source === "object" && source instanceof Buffer) {
                            source = source.toString();
                        }

                        if (typeof source !== "string") {
                            continue;
                        }

                        // standardized way of updating asset source
                        compilation.updateAsset(
                            i,
                            new sources.RawSource(eol.crlf(source))
                        );
                    }
                });
        });
    }
}

module.exports = WebpackEolPlugin;
