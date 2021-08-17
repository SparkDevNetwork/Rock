System.register(["vue", "../Elements/LoadingIndicator", "../Elements/TextBox", "../Util/Guid", "./GatewayControl"], function (exports_1, context_1) {
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
    var vue_1, LoadingIndicator_1, TextBox_1, Guid_1, GatewayControl_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (LoadingIndicator_1_1) {
                LoadingIndicator_1 = LoadingIndicator_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (GatewayControl_1_1) {
                GatewayControl_1 = GatewayControl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'TestGatewayControl',
                components: {
                    LoadingIndicator: LoadingIndicator_1.default,
                    TextBox: TextBox_1.default
                },
                props: {
                    settings: {
                        type: Object,
                        required: true
                    },
                    submit: {
                        type: Boolean,
                        required: true
                    }
                },
                data: function () {
                    return {
                        loading: false,
                        cardNumber: ''
                    };
                },
                watch: {
                    submit: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var validationFields, token;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (!this.submit || this.loading) {
                                            return [2];
                                        }
                                        this.loading = true;
                                        return [4, new Promise(function (resolve) { return setTimeout(resolve, 500); })];
                                    case 1:
                                        _a.sent();
                                        if (this.cardNumber === '0000') {
                                            this.$emit('error', 'This is a serious problem with the gateway.');
                                            this.loading = false;
                                            return [2];
                                        }
                                        if (this.cardNumber.length <= 10) {
                                            validationFields = [GatewayControl_1.ValidationField.CardNumber];
                                            this.$emit('validationRaw', validationFields);
                                            this.loading = false;
                                            return [2];
                                        }
                                        token = Guid_1.newGuid().replace(/-/g, '');
                                        this.$emit('successRaw', token);
                                        this.loading = false;
                                        return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<div>\n    <div v-if=\"loading\" class=\"text-center\">\n        <LoadingIndicator />\n    </div>\n    <div v-else>\n        <TextBox label=\"Credit Card\" v-model=\"cardNumber\" />\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=TestGatewayControl.js.map