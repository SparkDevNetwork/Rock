window.Obsidian = (function () {

    /**
     * Make sure this function is available at page load time so that Obsidian blocks (C# generated JS) can call.
     * @param {any} config
     */
    const initializeBlock = (config) => {
        const blockPath = `/ObsidianDist/Rock/Blocks/${config.blockFileIdentifier}.js`;

        require(
            ['/ObsidianDist/Rock/index.js'],
            ({ initializeBlock }) => {
                require(
                    [blockPath],
                    blockComponentModule => initializeBlock(config, blockComponentModule ? blockComponentModule.default : null),
                    err => {
                        initializeBlock(config, null);
                        throw err;
                    }
                );
            },
            err => {
                console.error(`Could not initialize Obsidian when trying to load ${config.blockFileIdentifier}`);
                throw err;
            }
        );
    };

    /**
     * Make sure this function is available at page load time so that the Rock Page (C# generated JS) can call.
     * @param {any} config
     */
    const initializePage = (config) => {
        require(['/ObsidianDist/Rock/index.js'], ({ initializePage }) => {
            initializePage(config);
        });
    };

    return {
        initializeBlock,
        initializePage
    };
})();


