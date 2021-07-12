System.register(["vue", "../Elements/DropDownList"], function (exports_1, context_1) {
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
    var vue_1, DropDownList_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'DefinedValuePicker',
                components: {
                    DropDownList: DropDownList_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    label: {
                        type: String,
                        default: 'Defined Value'
                    },
                    definedTypeGuid: {
                        type: String,
                        default: ''
                    },
                    displayDescriptions: {
                        type: Boolean,
                        default: false
                    },
                    show: {
                        type: Boolean,
                        default: true
                    }
                },
                setup: function () {
                    return {
                        http: vue_1.inject('http')
                    };
                },
                emits: [
                    'update:modelValue',
                    'update:model',
                    'receivedDefinedValues'
                ],
                data: function () {
                    return {
                        isInitialLoadDone: false,
                        internalValue: this.modelValue,
                        definedValues: [],
                        isLoading: false
                    };
                },
                computed: {
                    isEnabled: function () {
                        return !!this.definedTypeGuid && !this.isLoading;
                    },
                    options: function () {
                        var _this = this;
                        return this.definedValues.map(function (dv) { return ({
                            key: dv.guid,
                            value: dv.guid,
                            text: _this.displayDescriptions ? dv.description : dv.value
                        }); });
                    }
                },
                watch: {
                    modelValue: function () {
                        this.internalValue = this.modelValue;
                    },
                    definedTypeGuid: {
                        immediate: true,
                        handler: function () {
                            return __awaiter(this, void 0, void 0, function () {
                                var result;
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0:
                                            if (!!this.definedTypeGuid) return [3, 1];
                                            this.definedValues = [];
                                            return [3, 3];
                                        case 1:
                                            this.isLoading = true;
                                            return [4, this.http.get("/api/v2/controls/definedvaluepickers/" + this.definedTypeGuid)];
                                        case 2:
                                            result = _a.sent();
                                            if (result && result.data) {
                                                this.definedValues = result.data;
                                                this.$emit('receivedDefinedValues', this.definedValues);
                                            }
                                            this.isLoading = false;
                                            _a.label = 3;
                                        case 3:
                                            this.isInitialLoadDone = true;
                                            return [2];
                                    }
                                });
                            });
                        }
                    },
                    internalValue: function () {
                        var _this = this;
                        this.$emit('update:modelValue', this.internalValue);
                        var definedValue = this.definedValues.find(function (dv) { return dv.guid === _this.internalValue; }) || null;
                        this.$emit('update:model', definedValue);
                    }
                },
                template: "\n<DropDownList v-if=\"isInitialLoadDone && show\" v-model=\"internalValue\" :disabled=\"!isEnabled\" :label=\"label\" :options=\"options\" />"
            }));
        }
    };
});
//# sourceMappingURL=DefinedValuePicker.js.map