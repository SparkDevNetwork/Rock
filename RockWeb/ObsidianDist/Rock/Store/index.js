var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
define(["require", "exports", "../Vendor/Vuex/index.js", "./commonEntities.js"], function (require, exports, index_js_1, commonEntities_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    // Declare the Vuex store
    exports.default = index_js_1.createStore({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            contextEntities: {},
            pageId: 0,
            pageGuid: '',
        },
        getters: {
            isAuthenticated: function (state) {
                return !!state.currentPerson;
            },
            contextEntity: function (state) {
                return function (type) { return state.contextEntities[type]; };
            },
            personContext: function (state, getters) {
                return getters.contextEntity('Person');
            },
            groupContext: function (state, getters) {
                return getters.contextEntity('Group');
            }
        },
        mutations: {
            setAreSecondaryBlocksShown: function (state, _a) {
                var areSecondaryBlocksShown = _a.areSecondaryBlocksShown;
                state.areSecondaryBlocksShown = areSecondaryBlocksShown;
            },
            setPageInitializationData: function (state, pageConfig) {
                state.currentPerson = pageConfig.currentPerson || null;
                state.pageParameters = pageConfig.pageParameters || {};
                state.contextEntities = pageConfig.contextEntities || {};
                state.pageId = pageConfig.pageId || 0;
                state.pageGuid = pageConfig.pageGuid || '';
            }
        },
        actions: {
            initialize: function (context, _a) {
                var pageConfig = _a.pageConfig;
                context.commit('setPageInitializationData', pageConfig);
                // Initialize each common entity module
                for (var _i = 0, commonEntities_1 = commonEntities_js_1.commonEntities; _i < commonEntities_1.length; _i++) {
                    var commonEntity = commonEntities_1[_i];
                    context.dispatch(commonEntity.namespace + "/initialize");
                }
            }
        },
        modules: __assign({}, commonEntities_js_1.commonEntityModules)
    });
});
//# sourceMappingURL=index.js.map