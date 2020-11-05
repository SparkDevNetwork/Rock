Obsidian.Store = (function () {
    const generateCommonEntityModule = ({ namespace, apiUrl }) => {
        return {
            namespaced: true,
            state: {
                items: []
            },
            mutations: {
                setItems(state, { items }) {
                    state.items = items;
                }
            },
            actions: {
                async initialize(context) {
                    const cacheKey = `common-entity-${namespace}`;
                    let items = Obsidian.Util.getCache(cacheKey);

                    if (!items || !items.length) {
                        console.log(`Making API call for ${namespace}`);
                        const response = await Obsidian.Http.get(apiUrl);
                        items = response.data;
                        Obsidian.Util.setCache(cacheKey, items);
                    }

                    context.commit('setItems', { items });
                }
            }
        };
    };

    const commonEntities = [
        { namespace: 'campuses', apiUrl: '/api/campuses/' },
        { namespace: 'definedTypes', apiUrl: '/api/definedtypes/' }
    ];
    const commonEntityModules = {};

    for (const commonEntity of commonEntities) {
        commonEntityModules[commonEntity.namespace] = generateCommonEntityModule(commonEntity);
    }

    const _store = new Vuex.Store({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            contextEntities: {},
            pageId: 0,
            pageGuid: ''
        },
        getters: {
            contextEntity(state) {
                return (type) => state.contextEntities[type]
            },
            personContext(state, getters) {
                return getters.contextEntity('Person');
            },
            groupContext(state, getters) {
                return getters.contextEntity('Group');
            }
        },
        mutations: {
            setAreSecondaryBlocksShown(state, { areSecondaryBlocksShown }) {
                state.areSecondaryBlocksShown = areSecondaryBlocksShown;
            },
            setPageInitializationData(state, data) {
                state.currentPerson = data.currentPerson || null;
                state.pageParameters = data.pageParameters || {};
                state.contextEntities = data.contextEntities || {};
                state.pageId = data.pageId || 0;
                state.pageGuid = data.pageGuid || '';
            }
        },
        actions: {
            initialize(context, payload) {
                context.commit('setPageInitializationData', payload.pageData);

                for (const commonEntity of commonEntities) {
                    context.dispatch(`${commonEntity.namespace}/initialize`);
                }
            }
        },
        modules: {
            ...commonEntityModules
        }
    });

    return _store;
})();
