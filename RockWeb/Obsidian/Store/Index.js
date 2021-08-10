System.register(["vuex", "./CommonEntities"], function (exports_1, context_1) {
    "use strict";
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
    var vuex_1, CommonEntities_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vuex_1_1) {
                vuex_1 = vuex_1_1;
            },
            function (CommonEntities_1_1) {
                CommonEntities_1 = CommonEntities_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vuex_1.createStore({
                state: {
                    areSecondaryBlocksShown: true,
                    currentPerson: null,
                    pageParameters: {},
                    contextEntities: {},
                    pageId: 0,
                    pageGuid: '',
                    executionStartTime: new Date(),
                    debugTimings: [],
                    loginUrlWithReturnUrl: ''
                },
                getters: {
                    isAuthenticated: function (state) {
                        return !!state.currentPerson;
                    },
                    contextEntity: function (state) {
                        return function (type) { return (state.contextEntities[type] || null); };
                    },
                    personContext: function (state, getters) {
                        return getters.contextEntity('Person');
                    },
                    groupContext: function (state, getters) {
                        return getters.contextEntity('Group');
                    },
                    pageParameter: function (state) {
                        return function (key) { return (state.pageParameters[key]); };
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
                        state.executionStartTime = pageConfig.executionStartTime;
                        state.loginUrlWithReturnUrl = pageConfig.loginUrlWithReturnUrl;
                    },
                    reportOnLoadDebugTiming: function (state, payload) {
                        var pageStartTime = state.executionStartTime.getTime();
                        var timestampMs = payload.StartTimeMs - pageStartTime;
                        var durationMs = payload.FinishTimeMs - payload.StartTimeMs;
                        state.debugTimings.push({
                            TimestampMs: timestampMs,
                            DurationMs: durationMs,
                            IndentLevel: 1,
                            IsTitleBold: false,
                            SubTitle: payload.Subtitle,
                            Title: payload.Title
                        });
                    }
                },
                actions: {
                    initialize: function (context, _a) {
                        var pageConfig = _a.pageConfig;
                        context.commit('setPageInitializationData', pageConfig);
                        for (var _i = 0, commonEntities_1 = CommonEntities_1.commonEntities; _i < commonEntities_1.length; _i++) {
                            var commonEntity = commonEntities_1[_i];
                            context.dispatch(commonEntity.namespace + "/initialize");
                        }
                    },
                    redirectToLogin: function (context) {
                        if (context.state.loginUrlWithReturnUrl) {
                            window.location.href = context.state.loginUrlWithReturnUrl;
                        }
                    }
                },
                modules: __assign({}, CommonEntities_1.commonEntityModules)
            }));
        }
    };
});
//# sourceMappingURL=Index.js.map