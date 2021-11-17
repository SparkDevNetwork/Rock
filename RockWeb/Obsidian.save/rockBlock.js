System.register(["./Util/http", "vue", "./Store/index", "./Util/rockDateTime"], function (exports_1, context_1) {
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
    var http_1, vue_1, index_1, rockDateTime_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (http_1_1) {
                http_1 = http_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "RockBlock",
                props: {
                    config: {
                        type: Object,
                        required: true
                    },
                    blockComponent: {
                        type: Object,
                        default: null
                    },
                    startTimeMs: {
                        type: Number,
                        required: true
                    }
                },
                setup(props) {
                    const log = vue_1.reactive([]);
                    const writeLog = (method, url) => {
                        log.push({
                            date: rockDateTime_1.RockDateTime.now(),
                            method,
                            url
                        });
                    };
                    const httpCall = (method, url, params = undefined, data = undefined) => __awaiter(this, void 0, void 0, function* () {
                        writeLog(method, url);
                        return yield http_1.doApiCall(method, url, params, data);
                    });
                    const get = (url, params = undefined) => __awaiter(this, void 0, void 0, function* () {
                        return yield httpCall("GET", url, params);
                    });
                    const post = (url, params = undefined, data = undefined) => __awaiter(this, void 0, void 0, function* () {
                        return yield httpCall("POST", url, params, data);
                    });
                    const invokeBlockAction = (actionName, data = undefined) => __awaiter(this, void 0, void 0, function* () {
                        return yield post(`/api/v2/BlockActions/${store.state.pageGuid}/${props.config.blockGuid}/${actionName}`, undefined, Object.assign({ __context: {
                                pageParameters: store.state.pageParameters
                            } }, data));
                    });
                    const blockHttp = { get, post };
                    vue_1.provide("http", blockHttp);
                    vue_1.provide("invokeBlockAction", invokeBlockAction);
                    vue_1.provide("configurationValues", props.config.configurationValues);
                },
                data() {
                    return {
                        blockGuid: this.config.blockGuid,
                        error: "",
                        finishTimeMs: null
                    };
                },
                methods: {
                    clearError() {
                        this.error = "";
                    }
                },
                computed: {
                    renderTimeMs() {
                        if (!this.finishTimeMs || !this.startTimeMs) {
                            return null;
                        }
                        return this.finishTimeMs - this.startTimeMs;
                    },
                    pageGuid() {
                        return store.state.pageGuid;
                    }
                },
                errorCaptured(err) {
                    const defaultMessage = "An unknown error was caught from the block.";
                    if (err instanceof Error) {
                        this.error = err.message || defaultMessage;
                    }
                    else if (err) {
                        this.error = JSON.stringify(err) || defaultMessage;
                    }
                    else {
                        this.error = defaultMessage;
                    }
                },
                mounted() {
                    var _a;
                    this.finishTimeMs = rockDateTime_1.RockDateTime.now().toMilliseconds();
                    const componentName = ((_a = this.blockComponent) === null || _a === void 0 ? void 0 : _a.name) || "";
                    const nameParts = componentName.split(".");
                    let subtitle = nameParts[0] || "";
                    if (subtitle && subtitle.indexOf("(") !== 0) {
                        subtitle = `(${subtitle})`;
                    }
                    if (nameParts.length) {
                        store.addPageDebugTiming({
                            title: nameParts[1] || "<Unnamed>",
                            subtitle: subtitle,
                            startTimeMs: this.startTimeMs,
                            finishTimeMs: this.finishTimeMs
                        });
                    }
                },
                template: `
<div class="obsidian-block">
    <div v-if="!blockComponent" class="alert alert-danger">
        <strong>Not Found</strong>
        Could not find block component: "{{this.config.blockFileUrl}}"
    </div>
    <div v-if="error" class="alert alert-danger">
        <strong>Uncaught Error</strong>
        {{error}}
    </div>
    <component :is="blockComponent" />
</div>`
            }));
        }
    };
});
//# sourceMappingURL=rockBlock.js.map