System.register(["vue", "../Elements/DropDownList", "../Util/Cache", "../Util/Http", "../Services/String"], function (exports_1, context_1) {
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
    var vue_1, DropDownList_1, Cache_1, Http_1, String_1;
    var __moduleName = context_1 && context_1.id;
    function createCommonEntityPicker(entityName, getOptionsFunc) {
        var entityNameForDisplay = String_1.splitCamelCase(entityName);
        return vue_1.defineComponent({
            name: entityName + "Picker",
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
                    default: entityNameForDisplay
                }
            },
            data: function () {
                return {
                    providedOptions: getOptionsFunc(),
                    selectedGuid: '',
                    isLoading: false
                };
            },
            computed: {
                options: function () {
                    return getOptionsFunc().map(function (o) { return ({
                        key: o.Guid,
                        text: o.Text,
                        value: o.Guid
                    }); });
                }
            },
            watch: {
                modelValue: {
                    immediate: true,
                    handler: function () {
                        this.selectedGuid = this.modelValue;
                    }
                },
                selectedGuid: {
                    immediate: true,
                    handler: function () {
                        this.$emit('update:modelValue', this.selectedGuid);
                    }
                }
            },
            template: "\n<DropDownList v-model=\"selectedGuid\" :disabled=\"isLoading\" :label=\"label\" :options=\"options\" />"
        });
    }
    exports_1("createCommonEntityPicker", createCommonEntityPicker);
    function generateCommonEntityModule(commonEntity) {
        return {
            namespaced: true,
            state: {
                items: []
            },
            mutations: {
                setItems: function (state, _a) {
                    var items = _a.items;
                    state.items = items;
                }
            },
            getters: {
                all: function (state) {
                    return state.items;
                },
                getByGuid: function (state) {
                    return function (guid) {
                        return state.items.find(function (i) { return i.guid === guid; }) || null;
                    };
                },
                getById: function (state) {
                    return function (id) {
                        return state.items.find(function (i) { return i.id === id; }) || null;
                    };
                }
            },
            actions: {
                initialize: function (context) {
                    return __awaiter(this, void 0, void 0, function () {
                        var cacheKey, items, response;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    cacheKey = "common-entity-" + commonEntity.namespace;
                                    items = Cache_1.default.get(cacheKey) || [];
                                    if (!(!items || !items.length)) return [3, 2];
                                    return [4, Http_1.default.get(commonEntity.apiUrl)];
                                case 1:
                                    response = _a.sent();
                                    items = response.data || [];
                                    Cache_1.default.set(cacheKey, items);
                                    _a.label = 2;
                                case 2:
                                    context.commit('setItems', { items: items });
                                    return [2];
                            }
                        });
                    });
                }
            }
        };
    }
    exports_1("generateCommonEntityModule", generateCommonEntityModule);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (Cache_1_1) {
                Cache_1 = Cache_1_1;
            },
            function (Http_1_1) {
                Http_1 = Http_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=Generators.js.map