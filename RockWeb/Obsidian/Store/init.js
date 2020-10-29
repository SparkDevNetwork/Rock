Obsidian.Store = (function () {
    const _store = new Vuex.Store({
        state: {
            wasInitialized: false,
            areSecondaryBlocksShown: true,
            currentPerson: null
        },
        mutations: {
            setWasInitialized(state) {
                state.wasInitialized = true;
            },
            setCurrentPerson(state, { currentPerson }) {
                state.currentPerson = currentPerson;
            },
            setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
                state.areSecondaryBlocksShown = areSecondaryBlocksShown
            }
        },
        actions: {
            async initialize(context) {
                if (context.state.wasInitialized) {
                    return;
                }

                context.commit('setWasInitialized');

                try {
                    const response = await axios.get('/api/People/GetCurrentPerson');
                    context.commit('setCurrentPerson', { currentPerson: response.data });
                }
                catch (e) {
                    console.log('Get current person error:', e);
                }
            }
        }
    });

    _store.dispatch('initialize');
    return _store;
})();
