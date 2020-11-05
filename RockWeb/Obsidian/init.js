(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Templates = {};
    Obsidian.Elements = {};
    Obsidian.Controls = {};
    Obsidian.Blocks = {};
    Obsidian.Fields = {};

    Obsidian.Http = {
        doApiCall(method, url, params, data) {
            return axios({ method, url, data, params });
        },
        get(url, params) {
            return Obsidian.Http.doApiCall('GET', url, params);
        },
        post(url, params, data) {
            return Obsidian.Http.doApiCall('POST', url, params, data);
        }
    };

    Obsidian.initializePage = function (pageData) {
        Obsidian.Store.dispatch('initialize', { pageData });
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
