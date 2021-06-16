System.register(["vue", "../Elements/JavaScriptAnchor", "./ComponentFromUrl"], function (exports_1, context_1) {
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
    var vue_1, JavaScriptAnchor_1, ComponentFromUrl_1, ValidationField;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            },
            function (ComponentFromUrl_1_1) {
                ComponentFromUrl_1 = ComponentFromUrl_1_1;
            }
        ],
        execute: function () {
            (function (ValidationField) {
                ValidationField[ValidationField["CardNumber"] = 0] = "CardNumber";
                ValidationField[ValidationField["Expiry"] = 1] = "Expiry";
                ValidationField[ValidationField["SecurityCode"] = 2] = "SecurityCode";
            })(ValidationField || (ValidationField = {}));
            exports_1("ValidationField", ValidationField);
            exports_1("default", vue_1.defineComponent({
                name: 'GatewayControl',
                components: {
                    ComponentFromUrl: ComponentFromUrl_1.default,
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    gatewayControlModel: {
                        type: Object,
                        required: true
                    }
                },
                data: function () {
                    return {
                        isSuccess: false
                    };
                },
                computed: {
                    url: function () {
                        return this.gatewayControlModel.FileUrl;
                    },
                    settings: function () {
                        return this.gatewayControlModel.Settings;
                    }
                },
                methods: {
                    reset: function () {
                        var _this = this;
                        this.isSuccess = true;
                        this.$nextTick(function () {
                            _this.isSuccess = false;
                            _this.$emit('reset');
                        });
                    },
                    onSuccess: function (token) {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                this.isSuccess = true;
                                this.$emit('success', token);
                                return [2];
                            });
                        });
                    },
                    transformValidation: function (validationFields) {
                        var errors = {};
                        var foundError = false;
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.CardNumber)) {
                            errors['Card Number'] = 'is not valid.';
                            foundError = true;
                        }
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.Expiry)) {
                            errors['Expiration Date'] = 'is not valid.';
                            foundError = true;
                        }
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.SecurityCode)) {
                            errors['Security Code'] = 'is not valid.';
                            foundError = true;
                        }
                        if (!foundError) {
                            errors['Payment Info'] = 'is not valid.';
                        }
                        this.$emit('validation', errors);
                        return;
                    }
                },
                template: "\n<ComponentFromUrl v-if=\"!isSuccess\" :url=\"url\" :settings=\"settings\" @validationRaw=\"transformValidation\" @successRaw=\"onSuccess\" />\n<div v-else class=\"text-center\">\n    Your payment is ready.\n    <small>\n        <JavaScriptAnchor @click=\"reset\">\n            Reset Payment\n        </JavaScriptAnchor>\n    </small>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=GatewayControl.js.map