window.Obsidian = (function () {

    /**
     * System.js requires loading the root file this way. Once that file is loaded, then
     * functions within can be used.
     */
    const getInit = async () => {
        return await System.import('/ObsidianDist/Rock/index.js');
    };

    /**
     * Make sure this function is available at page load time so that Obsidian blocks (C# generated JS) can call.
     * @param {any} config
     */
    const initializeBlock = async (config) => {
        const init = await getInit();
        let blockComponent = null;

        try {
            const module = await System.import(`/ObsidianDist/Rock/Blocks/${config.blockFileIdentifier}.js`);
            blockComponent = module.default;
        }
        catch (e) {
            console.error('Problem loading block component file', e);
        }

        init.initializeBlock(config, blockComponent);
    };

    /**
     * Make sure this function is available at page load time so that the Rock Page (C# generated JS) can call.
     * @param {any} config
     */
    const initializePage = async (config) => {
        const init = await getInit();
        init.initializePage(config);
    };

    return {
        initializeBlock,
        initializePage
    };
})();


