System.register(["vue", "../Elements/LoadingIndicator", "./GatewayControl"], function (exports_1, context_1) {
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
    var vue_1, LoadingIndicator_1, GatewayControl_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (LoadingIndicator_1_1) {
                LoadingIndicator_1 = LoadingIndicator_1_1;
            },
            function (GatewayControl_1_1) {
                GatewayControl_1 = GatewayControl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'MyWellGatewayControl',
                components: {
                    LoadingIndicator: LoadingIndicator_1.default
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
                        tokenizer: null,
                        loading: true
                    };
                },
                methods: {
                    mountControl: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var globalVarName, script, sleep, settings;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        globalVarName = 'Tokenizer';
                                        if (!!window[globalVarName]) return [3, 3];
                                        script = document.createElement('script');
                                        script.type = 'text/javascript';
                                        script.src = 'https://sandbox.gotnpgateway.com/tokenizer/tokenizer.js';
                                        document.getElementsByTagName('head')[0].appendChild(script);
                                        sleep = function () { return new Promise(function (resolve) { return setTimeout(resolve, 20); }); };
                                        _a.label = 1;
                                    case 1:
                                        if (!!window[globalVarName]) return [3, 3];
                                        return [4, sleep()];
                                    case 2:
                                        _a.sent();
                                        return [3, 1];
                                    case 3:
                                        settings = this.getTokenizerSettings();
                                        this.tokenizer = new window[globalVarName](settings);
                                        this.tokenizer.create();
                                        return [2];
                                }
                            });
                        });
                    },
                    handleResponse: function (response) {
                        var _a;
                        this.loading = false;
                        if (!(response === null || response === void 0 ? void 0 : response.status) || response.status === 'error') {
                            var errorResponse = response || null;
                            this.$emit('error', (errorResponse === null || errorResponse === void 0 ? void 0 : errorResponse.message) || 'There was an unexpected problem communicating with the gateway.');
                            console.error('MyWell response was errored:', JSON.stringify(response));
                            return;
                        }
                        if (response.status === 'validation') {
                            var validationResponse = response || null;
                            if (!((_a = validationResponse === null || validationResponse === void 0 ? void 0 : validationResponse.invalid) === null || _a === void 0 ? void 0 : _a.length)) {
                                this.$emit('error', 'There was a validation issue, but the invalid field was not specified.');
                                console.error('MyWell response was errored:', JSON.stringify(response));
                                return;
                            }
                            var validationFields = [];
                            for (var _i = 0, _b = validationResponse.invalid; _i < _b.length; _i++) {
                                var myWellField = _b[_i];
                                switch (myWellField) {
                                    case 'cc':
                                        validationFields.push(GatewayControl_1.ValidationField.CardNumber);
                                        break;
                                    case 'exp':
                                        validationFields.push(GatewayControl_1.ValidationField.Expiry);
                                        break;
                                    default:
                                        console.error('Unknown MyWell validation field', myWellField);
                                        break;
                                }
                            }
                            if (!validationFields.length) {
                                this.$emit('error', 'There was a validation issue, but the invalid field could not be inferred.');
                                console.error('MyWell response contained unexpected values:', JSON.stringify(response));
                                return;
                            }
                            this.$emit('validationRaw', validationFields);
                            return;
                        }
                        if (response.status === 'success') {
                            var successResponse = response || null;
                            if (!(successResponse === null || successResponse === void 0 ? void 0 : successResponse.token)) {
                                this.$emit('error', 'There was an unexpected problem communicating with the gateway.');
                                console.error('MyWell response does not have the expected token:', JSON.stringify(response));
                                return;
                            }
                            this.$emit('successRaw', successResponse.token);
                            return;
                        }
                        this.$emit('error', 'There was an unexpected problem communicating with the gateway.');
                        console.error('MyWell response has invalid status:', JSON.stringify(response));
                    },
                    getTokenizerSettings: function () {
                        var _this = this;
                        return {
                            onLoad: function () { _this.loading = false; },
                            apikey: this.publicApiKey,
                            url: this.gatewayUrl,
                            container: this.$refs['container'],
                            submission: function (resp) {
                                _this.handleResponse(resp);
                            },
                            settings: {
                                payment: {
                                    types: ['card'],
                                    ach: {
                                        'sec_code': 'web'
                                    }
                                },
                                styles: {
                                    body: {
                                        color: 'rgb(51, 51, 51)'
                                    },
                                    '#app': {
                                        padding: '5px 15px'
                                    },
                                    'input,select': {
                                        'color': 'rgb(85, 85, 85)',
                                        'border-radius': '4px',
                                        'background-color': 'rgb(255, 255, 255)',
                                        'border': '1px solid rgb(204, 204, 204)',
                                        'box-shadow': 'rgba(0, 0, 0, 0.075) 0px 1px 1px 0px inset',
                                        'padding': '6px 12px',
                                        'font-size': '14px',
                                        'height': '34px',
                                        'font-family': 'OpenSans, \'Helvetica Neue\', Helvetica, Arial, sans-serif'
                                    },
                                    'input:focus,select:focus': {
                                        'border': '1px solid #66afe9',
                                        'box-shadow': '0 0 0 3px rgba(102,175,233,0.6)'
                                    },
                                    'select': {
                                        'padding': '6px 4px'
                                    },
                                    '.fieldsetrow': {
                                        'margin-left': '-2.5px',
                                        'margin-right': '-2.5px'
                                    },
                                    '.card > .fieldset': {
                                        'padding': '0 !important',
                                        'margin': '0 2.5px 5px !important'
                                    },
                                    'input[type=number]::-webkit-inner-spin-button,input[type=number]::-webkit-outer-spin-button': {
                                        '-webkit-appearance': 'none',
                                        'margin': '0'
                                    }
                                }
                            }
                        };
                    }
                },
                computed: {
                    publicApiKey: function () {
                        return this.settings.PublicApiKey;
                    },
                    gatewayUrl: function () {
                        return this.settings.GatewayUrl;
                    }
                },
                watch: {
                    submit: function () {
                        if (this.submit && this.tokenizer) {
                            this.loading = true;
                            this.tokenizer.submit();
                        }
                    }
                },
                mounted: function () {
                    return __awaiter(this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4, this.mountControl()];
                                case 1:
                                    _a.sent();
                                    return [2];
                            }
                        });
                    });
                },
                template: "\n<div>\n    <div ref=\"container\" style=\"min-height: 49px;\"></div>\n    <div v-if=\"loading\" class=\"text-center\">\n        <LoadingIndicator />\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=MyWellGatewayControl.js.map