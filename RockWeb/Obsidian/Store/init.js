Obsidian.Store = (function () {
    const _store = new Vuex.Store({
        state: {
            wasInitialized: false,
            areSecondaryBlocksShown: true,
            currentPerson: null
        },
        mutations: {
            setWasInitialized(state, payload) {
                state.wasInitialized = true;
            },
            applyInitializationData(state, { data }) {
                state.currentPerson = data.CurrentPerson;
            },
            setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
                state.areSecondaryBlocksShown = areSecondaryBlocksShown
            }
        },
        actions: {
            async initializePage(context, { pageGuid }) {
                if (context.state.wasInitialized) {
                    return;
                }

                context.commit('setWasInitialized', {});
                const response = await axios.get(`/api/obsidian/v1/page/initialization/${pageGuid}`);
                context.commit('applyInitializationData', { data: response.data });
            }
        }
    });

    return _store;
})();
