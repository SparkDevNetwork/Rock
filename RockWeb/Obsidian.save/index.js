System.register(["vue", "./rockBlock", "./Store/index", "./Rules/index", "./Util/rockDateTime"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, rockBlock_1, index_1, rockDateTime_1, store;
    var __moduleName = context_1 && context_1.id;
    function initializeBlock(config) {
        return __awaiter(this, void 0, void 0, function* () {
            const blockPath = `${config.blockFileUrl}.js`;
            let blockComponent = null;
            let errorMessage = "";
            try {
                const blockComponentModule = yield context_1.import(blockPath);
                blockComponent = blockComponentModule ?
                    (blockComponentModule.default || blockComponentModule) :
                    null;
            }
            catch (e) {
                console.error(e);
                errorMessage = `${e}`;
            }
            const name = `Root${config.blockFileUrl.replace(/\//g, ".")}`;
            const startTimeMs = rockDateTime_1.RockDateTime.now().toMilliseconds();
            const app = vue_1.createApp({
                name,
                components: {
                    RockBlock: rockBlock_1.default
                },
                data() {
                    return {
                        config: config,
                        blockComponent: blockComponent ? vue_1.markRaw(blockComponent) : null,
                        startTimeMs,
                        errorMessage
                    };
                },
                template: `
<div v-if="errorMessage" class="alert alert-danger">
    <strong>Error Initializing Block</strong>
    <br />
    {{errorMessage}}
</div>
<RockBlock v-else :config="config" :blockComponent="blockComponent" :startTimeMs="startTimeMs" />`
            });
            app.mount(config.rootElement);
            return app;
        });
    }
    exports_1("initializeBlock", initializeBlock);
    function initializePage(pageConfig) {
        return __awaiter(this, void 0, void 0, function* () {
            yield store.initialize(pageConfig);
        });
    }
    exports_1("initializePage", initializePage);
    function initializePageTimings(config) {
        return __awaiter(this, void 0, void 0, function* () {
            const rootElement = document.getElementById(config.elementId);
            if (!rootElement) {
                console.error("Could not show Obsidian debug timings because the HTML element did not resolve.");
                return;
            }
            const pageDebugTimings = (yield context_1.import("./Controls/pageDebugTimings")).default;
            const app = vue_1.createApp({
                name: "PageDebugTimingsRoot",
                components: {
                    PageDebugTimings: pageDebugTimings
                },
                data() {
                    return {
                        viewModels: config.debugTimingViewModels
                    };
                },
                template: `<PageDebugTimings :serverViewModels="viewModels" />`
            });
            app.mount(rootElement);
        });
    }
    exports_1("initializePageTimings", initializePageTimings);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockBlock_1_1) {
                rockBlock_1 = rockBlock_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (_1) {
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
        }
    };
});
//# sourceMappingURL=index.js.map