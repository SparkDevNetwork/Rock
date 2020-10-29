(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Templates = {};
    Obsidian.Elements = {};
    Obsidian.Controls = {};
    Obsidian.Blocks = {};
    Obsidian.Fields = {};

    Obsidian.initializePage = function (pageData) {
        Obsidian.Store.commit('setPageInitializationData', pageData);
    };

    Obsidian.initializeBlock = function (config) {
        return new Vue({
            el: config.rootElement,
            name: `Root.${config.blockFileIdentifier}`,
            store: Obsidian.Store,
            components: {
                RockBlock: Obsidian.Controls.RockBlock
            },
            data() {
                return {
                    config: config
                };
            },
            template: `<RockBlock :config="config" />`
        });
    };
})();
