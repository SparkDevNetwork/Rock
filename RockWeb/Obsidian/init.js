(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Templates = {};
    Obsidian.Elements = {};
    Obsidian.Controls = {};
    Obsidian.Blocks = {};
    Obsidian.Fields = {};

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
            created() {
                this.$store.dispatch('initializePage', { pageGuid: this.config.pageGuid });
            },
            template: `<RockBlock :config="config" />`
        });
    };
})();
