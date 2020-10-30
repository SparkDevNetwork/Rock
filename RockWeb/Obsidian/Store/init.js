Obsidian.Store = (function () {
    const _store = new Vuex.Store({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            contextEntities: {},
            pageId: 0,
            pageGuid: ''
        },
        mutations: {
            setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
                state.areSecondaryBlocksShown = areSecondaryBlocksShown
            },
            setPageInitializationData(state, data) {
                state.currentPerson = data.currentPerson || null;
                state.pageParameters = data.pageParameters || {};
                state.contextEntities = data.contextEntities || {};
                state.pageId = data.pageId || 0;
                state.pageGuid = data.pageGuid || '';
            }
        }
    });

    return _store;
})();
