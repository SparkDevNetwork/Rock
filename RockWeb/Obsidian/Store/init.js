Obsidian.Store = (function () {
    const _store = new Vuex.Store({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            pageId: 0,
            pageGuid: ''
        },
        mutations: {
            setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
                state.areSecondaryBlocksShown = areSecondaryBlocksShown
            },
            setPageInitializationData(state, data) {
                state.currentPerson = data.currentPerson;
                state.pageParameters = data.pageParameters;
                state.pageId = data.pageId;
                state.pageGuid = data.pageGuid;
            }
        }
    });

    return _store;
})();
