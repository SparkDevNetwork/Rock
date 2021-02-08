System.register(["../Util/http.js", "../Vendor/Vue/vue.js", "../Store/Index.js", "../Elements/Alert.js"], function (exports_1, context_1) {
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
    var http_js_1, vue_js_1, Index_js_1, Alert_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (http_js_1_1) {
                http_js_1 = http_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (Alert_js_1_1) {
                Alert_js_1 = Alert_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'RockBlock',
                components: {
                    Alert: Alert_js_1.default
                },
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
                setup: function (props) {
                    var _this = this;
                    var log = vue_js_1.reactive([]);
                    var writeLog = function (method, url) {
                        log.push({
                            date: new Date(),
                            method: method,
                            url: url
                        });
                    };
                    var httpCall = function (method, url, params, data) {
                        if (params === void 0) { params = undefined; }
                        if (data === void 0) { data = undefined; }
                        return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        writeLog(method, url);
                                        return [4 /*yield*/, http_js_1.doApiCall(method, url, params, data)];
                                    case 1: return [2 /*return*/, _a.sent()];
                                }
                            });
                        });
                    };
                    var get = function (url, params) {
                        if (params === void 0) { params = undefined; }
                        return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, httpCall('GET', url, params)];
                                    case 1: return [2 /*return*/, _a.sent()];
                                }
                            });
                        });
                    };
                    var post = function (url, params, data) {
                        if (params === void 0) { params = undefined; }
                        if (data === void 0) { data = undefined; }
                        return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, httpCall('POST', url, params, data)];
                                    case 1: return [2 /*return*/, _a.sent()];
                                }
                            });
                        });
                    };
                    var invokeBlockAction = function (actionName, data) {
                        if (data === void 0) { data = undefined; }
                        return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, post("/api/blocks/action/" + props.config.blockGuid + "/" + actionName, undefined, __assign({ __context: {
                                                pageParameters: Index_js_1.default.state.pageParameters
                                            } }, data))];
                                    case 1: return [2 /*return*/, _a.sent()];
                                }
                            });
                        });
                    };
                    var blockHttp = { get: get, post: post };
                    vue_js_1.provide('http', blockHttp);
                    vue_js_1.provide('invokeBlockAction', invokeBlockAction);
                    vue_js_1.provide('configurationValues', props.config.configurationValues);
                },
                data: function () {
                    return {
                        blockGuid: this.config.blockGuid,
                        error: '',
                        finishTimeMs: null
                    };
                },
                methods: {
                    clearError: function () {
                        this.error = '';
                    }
                },
                computed: {
                    renderTimeMs: function () {
                        if (!this.finishTimeMs || !this.startTimeMs) {
                            return null;
                        }
                        return this.finishTimeMs - this.startTimeMs;
                    },
                    pageGuid: function () {
                        return Index_js_1.default.state.pageGuid;
                    }
                },
                errorCaptured: function (err) {
                    if (err instanceof Error) {
                        this.error = err.message;
                    }
                },
                mounted: function () {
                    var _a;
                    this.finishTimeMs = (new Date()).getTime();
                    var componentName = ((_a = this.blockComponent) === null || _a === void 0 ? void 0 : _a.name) || '';
                    var nameParts = componentName.split('.');
                    var subtitle = nameParts[0] || '';
                    if (subtitle && subtitle.indexOf('(') !== 0) {
                        subtitle = "(" + subtitle + ")";
                    }
                    if (nameParts.length) {
                        Index_js_1.default.commit('reportOnLoadDebugTiming', {
                            Title: nameParts[1] || '<Unnamed>',
                            Subtitle: subtitle,
                            StartTimeMs: this.startTimeMs,
                            FinishTimeMs: this.finishTimeMs
                        });
                    }
                },
                template: "<div class=\"obsidian-block\">\n    <Alert v-if=\"!blockComponent\" alertType=\"danger\">\n        <strong>Not Found</strong>\n        Could not find block component: \"{{this.config.blockFileUrl}}\"\n    </Alert>\n    <Alert v-if=\"error\" alertType=\"danger\" :dismissible=\"true\" @dismiss=\"clearError\">\n        <strong>Uncaught Error</strong>\n        {{error}}\n    </Alert>\n    <component :is=\"blockComponent\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RockBlock.js.map