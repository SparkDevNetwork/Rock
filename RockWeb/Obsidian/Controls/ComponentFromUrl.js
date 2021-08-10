System.register(["vue", "../Elements/Alert", "../Elements/LoadingIndicator"], function (exports_1, context_1) {
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
    var vue_1, Alert_1, LoadingIndicator_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (LoadingIndicator_1_1) {
                LoadingIndicator_1 = LoadingIndicator_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'ComponentFromUrl',
                components: {
                    LoadingIndicator: LoadingIndicator_1.default,
                    Alert: Alert_1.default
                },
                props: {
                    url: {
                        type: String,
                        required: true
                    }
                },
                data: function () {
                    return {
                        control: null,
                        loading: true,
                        error: ''
                    };
                },
                created: function () {
                    return __awaiter(this, void 0, void 0, function () {
                        var controlComponentModule, control, e_1;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    if (!this.url) {
                                        this.error = "Could not load the control because no URL was provided";
                                        this.loading = false;
                                        return [2];
                                    }
                                    _a.label = 1;
                                case 1:
                                    _a.trys.push([1, 3, 4, 5]);
                                    return [4, context_1.import(this.url)];
                                case 2:
                                    controlComponentModule = _a.sent();
                                    control = controlComponentModule ?
                                        (controlComponentModule.default || controlComponentModule) :
                                        null;
                                    if (control) {
                                        this.control = vue_1.markRaw(control);
                                    }
                                    return [3, 5];
                                case 3:
                                    e_1 = _a.sent();
                                    console.error(e_1);
                                    this.error = "Could not load the control for '" + this.url + "'";
                                    return [3, 5];
                                case 4:
                                    this.loading = false;
                                    if (!this.control) {
                                        this.error = "Could not load the control for '" + this.url + "'";
                                    }
                                    return [7];
                                case 5: return [2];
                            }
                        });
                    });
                },
                template: "\n<Alert v-if=\"error\" alertType=\"danger\">{{error}}</Alert>\n<LoadingIndicator v-else-if=\"loading\" />\n<component v-else :is=\"control\" />"
            }));
        }
    };
});
//# sourceMappingURL=ComponentFromUrl.js.map