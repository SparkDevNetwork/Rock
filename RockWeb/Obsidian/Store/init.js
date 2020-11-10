/**
 * The Obsidian Vuex Store.
 * This houses data shared across controls, blocks, and even pages. The store contains functions originally
 * provided by the RockPage in DotNet Framework such as page parameters and context entities.
 */
Obsidian.Store = (function () {

    /**
     * Generate a Vuex module that fetches, caches, and stores common entities for use across all controls and blocks.
     * Provide the namespace (ex: campuses) that will serve as the Vuex namespace.
     * Also provide the apiUrl (ex: api/campuses) that allows the module to hydrate its items when needed.
     * @param {{namespace: string, apiUrl: string}} config
     */
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
            getters: {
                all(state) {
                    return state.items;
                },
                getByGuid(state) {
                    return guid => state.items.find(i => i.Guid === guid);
                }
            },
            actions: {
                async initialize(context) {
                    const cacheKey = `common-entity-${namespace}`;
                    let items = Obsidian.Util.getCache(cacheKey);

                    if (!items || !items.length) {
                        const response = await Obsidian.Http.get(apiUrl);
                        items = response.data;
                        Obsidian.Util.setCache(cacheKey, items);
                    }

                    context.commit('setItems', { items });
                }
            }
        };
    };

    // The common entity configs that will be used with generateCommonEntityModule to create store modules
    const commonEntities = [
        { namespace: 'campuses', apiUrl: '/api/obsidian/v1/commonentities/campuses' },
        { namespace: 'definedTypes', apiUrl: '/api/obsidian/v1/commonentities/definedTypes' }
    ];
    const commonEntityModules = {};

    // Generate a module for each config
    for (const commonEntity of commonEntities) {
        commonEntityModules[commonEntity.namespace] = generateCommonEntityModule(commonEntity);
    }

    // Declare the Vuex store
    const _store = Vuex.createStore({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            contextEntities: {},
            pageId: 0,
            pageGuid: ''
        },
        getters: {
            isAuthenticated(state) {
                return !!state.currentPerson;
            },
            contextEntity(state) {
                return (type) => state.contextEntities[type];
            },
            personContext(state, getters) {
                return getters.contextEntity('Person');
            },
            groupContext(state, getters) {
                return getters.contextEntity('Group');
            }
        },
        mutations: {
            setIsAuthenticated(state, { isAuthenticated }) {
                state.isAuthenticated = isAuthenticated;
            },
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

                // Initialize each common entity module
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
