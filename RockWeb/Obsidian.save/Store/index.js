System.register(["vue", "../Util/rockDateTime"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockDateTime_1, state, Store, store;
    var __moduleName = context_1 && context_1.id;
    function useStore() {
        return store;
    }
    exports_1("useStore", useStore);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            state = vue_1.reactive({
                areSecondaryBlocksShown: true,
                currentPerson: null,
                pageParameters: {},
                contextEntities: {},
                pageId: 0,
                pageGuid: "",
                executionStartTime: rockDateTime_1.RockDateTime.now().toMilliseconds(),
                debugTimings: [],
                loginUrlWithReturnUrl: ""
            });
            Store = class Store {
                constructor() {
                    this.state = vue_1.shallowReadonly(state);
                }
                setAreSecondaryBlocksShown(areSecondaryBlocksShown) {
                    state.areSecondaryBlocksShown = areSecondaryBlocksShown;
                }
                initialize(pageConfig) {
                    state.currentPerson = pageConfig.currentPerson || null;
                    state.pageParameters = pageConfig.pageParameters || {};
                    state.contextEntities = pageConfig.contextEntities || {};
                    state.pageId = pageConfig.pageId || 0;
                    state.pageGuid = pageConfig.pageGuid || "";
                    state.executionStartTime = pageConfig.executionStartTime;
                    state.loginUrlWithReturnUrl = pageConfig.loginUrlWithReturnUrl;
                }
                addPageDebugTiming(timing) {
                    const pageStartTime = state.executionStartTime;
                    const timestampMs = timing.startTimeMs - pageStartTime;
                    const durationMs = timing.finishTimeMs - timing.startTimeMs;
                    state.debugTimings.push({
                        timestampMs: timestampMs,
                        durationMs: durationMs,
                        indentLevel: 1,
                        isTitleBold: false,
                        subTitle: timing.subtitle,
                        title: timing.title
                    });
                }
                redirectToLogin() {
                    if (state.loginUrlWithReturnUrl) {
                        window.location.href = state.loginUrlWithReturnUrl;
                    }
                }
                get isAuthenticated() {
                    return !!state.currentPerson;
                }
                getContextEntity(type) {
                    return state.contextEntities[type] || null;
                }
                get personContext() {
                    return this.getContextEntity("person");
                }
                get groupContext() {
                    return this.getContextEntity("group");
                }
                getPageParameter(key) {
                    return state.pageParameters[key];
                }
            };
            store = new Store();
        }
    };
});
//# sourceMappingURL=index.js.map