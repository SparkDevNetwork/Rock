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
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define(["require", "exports", "../Vendor/Vuex/index.js", "../Util/cache.js", "../Util/http.js"], function (require, exports, index_js_1, cache_js_1, http_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    /**
    * Generate a Vuex module that fetches, caches, and stores common entities for use across all controls and blocks.
    * Provide the namespace (ex: campuses) that will serve as the Vuex namespace.
    * Also provide the apiUrl (ex: api/campuses) that allows the module to hydrate its items when needed.
    * @param {{namespace: string, apiUrl: string}} config
    */
    var generateCommonEntityModule = function (_a) {
        var namespace = _a.namespace, apiUrl = _a.apiUrl;
        return {
            namespaced: true,
            state: {
                items: []
            },
            mutations: {
                setItems: function (state, _a) {
                    var items = _a.items;
                    state.items = items;
                }
            },
            getters: {
                all: function (state) {
                    return state.items;
                },
                getByGuid: function (state) {
                    return function (guid) { return state.items.find(function (i) { return i.Guid === guid; }); };
                }
            },
            actions: {
                initialize: function (context) {
                    return __awaiter(this, void 0, void 0, function () {
                        var cacheKey, items, response;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    cacheKey = "common-entity-" + namespace;
                                    items = cache_js_1.default.get(cacheKey);
                                    if (!(!items || !items.length)) return [3 /*break*/, 2];
                                    return [4 /*yield*/, http_js_1.default.get(apiUrl)];
                                case 1:
                                    response = _a.sent();
                                    items = response.data;
                                    cache_js_1.default.set(cacheKey, items);
                                    _a.label = 2;
                                case 2:
                                    context.commit('setItems', { items: items });
                                    return [2 /*return*/];
                            }
                        });
                    });
                }
            }
        };
    };
    // The common entity configs that will be used with generateCommonEntityModule to create store modules
    var commonEntities = [
        { namespace: 'campuses', apiUrl: '/api/obsidian/v1/commonentities/campuses' },
        { namespace: 'definedTypes', apiUrl: '/api/obsidian/v1/commonentities/definedTypes' }
    ];
    var commonEntityModules = {};
    // Generate a module for each config
    for (var _i = 0, commonEntities_1 = commonEntities; _i < commonEntities_1.length; _i++) {
        var commonEntity = commonEntities_1[_i];
        commonEntityModules[commonEntity.namespace] = generateCommonEntityModule(commonEntity);
    }
    // Declare the Vuex store
    exports.default = index_js_1.createStore({
        state: {
            areSecondaryBlocksShown: true,
            currentPerson: null,
            pageParameters: {},
            contextEntities: {},
            pageId: 0,
            pageGuid: ''
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
            setPageInitializationData: function (state, data) {
                state.currentPerson = data.currentPerson || null;
                state.pageParameters = data.pageParameters || {};
                state.contextEntities = data.contextEntities || {};
                state.pageId = data.pageId || 0;
                state.pageGuid = data.pageGuid || '';
            }
        },
        actions: {
            initialize: function (context, payload) {
                context.commit('setPageInitializationData', payload.pageData);
                // Initialize each common entity module
                for (var _i = 0, commonEntities_2 = commonEntities; _i < commonEntities_2.length; _i++) {
                    var commonEntity = commonEntities_2[_i];
                    context.dispatch(commonEntity.namespace + "/initialize");
                }
            }
        },
        modules: __assign({}, commonEntityModules)
    });
});
//# sourceMappingURL=index.js.map