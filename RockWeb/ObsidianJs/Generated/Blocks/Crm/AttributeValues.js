System.register(["../../Vendor/Vue/vue.js", "../../Templates/PaneledBlockTemplate.js", "../../Controls/Loading.js", "../../Store/Index.js", "../../Elements/JavaScriptAnchor.js", "../../Controls/RockForm.js", "../../Elements/TextBox.js", "../../Elements/RockButton.js", "../../Controls/AttributeValueContainer.js"], function (exports_1, context_1) {
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
    var vue_js_1, PaneledBlockTemplate_js_1, Loading_js_1, Index_js_1, JavaScriptAnchor_js_1, RockForm_js_1, TextBox_js_1, RockButton_js_1, AttributeValueContainer_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (PaneledBlockTemplate_js_1_1) {
                PaneledBlockTemplate_js_1 = PaneledBlockTemplate_js_1_1;
            },
            function (Loading_js_1_1) {
                Loading_js_1 = Loading_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (JavaScriptAnchor_js_1_1) {
                JavaScriptAnchor_js_1 = JavaScriptAnchor_js_1_1;
            },
            function (RockForm_js_1_1) {
                RockForm_js_1 = RockForm_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            },
            function (RockButton_js_1_1) {
                RockButton_js_1 = RockButton_js_1_1;
            },
            function (AttributeValueContainer_js_1_1) {
                AttributeValueContainer_js_1 = AttributeValueContainer_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'Crm.AttributeValues',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_js_1.default,
                    Loading: Loading_js_1.default,
                    JavaScriptAnchor: JavaScriptAnchor_js_1.default,
                    RockForm: RockForm_js_1.default,
                    TextBox: TextBox_js_1.default,
                    RockButton: RockButton_js_1.default,
                    AttributeValueContainer: AttributeValueContainer_js_1.default
                },
                setup: function () {
                    return {
                        blockAction: vue_js_1.inject('blockAction'),
                        blockSettings: vue_js_1.inject('blockSettings')
                    };
                },
                data: function () {
                    return {
                        isLoading: true,
                        isEditMode: false,
                        attributeDataList: []
                    };
                },
                computed: {
                    personGuid: function () {
                        var person = (Index_js_1.default.getters.personContext || null);
                        return person ? person.Guid : null;
                    },
                    useAbbreviatedNames: function () {
                        return this.blockSettings.UseAbbreviatedNames;
                    }
                },
                methods: {
                    goToViewMode: function () {
                        this.isEditMode = false;
                    },
                    goToEditMode: function () {
                        this.isEditMode = true;
                    },
                    loadData: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (!this.personGuid) {
                                            this.attributeDataList = [];
                                            return [2 /*return*/];
                                        }
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, , 3, 4]);
                                        this.isLoading = true;
                                        return [4 /*yield*/, this.blockAction('GetAttributeValueList', {
                                                PersonGuid: this.personGuid
                                            })];
                                    case 2:
                                        result = _a.sent();
                                        if (result.data) {
                                            this.attributeDataList = result.data;
                                        }
                                        return [3 /*break*/, 4];
                                    case 3:
                                        this.isLoading = false;
                                        return [7 /*endfinally*/];
                                    case 4: return [2 /*return*/];
                                }
                            });
                        });
                    },
                    doSave: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var keyArgsMap, _i, _a, a;
                            return __generator(this, function (_b) {
                                switch (_b.label) {
                                    case 0:
                                        this.isLoading = true;
                                        keyArgsMap = {};
                                        for (_i = 0, _a = this.attributeDataList; _i < _a.length; _i++) {
                                            a = _a[_i];
                                            keyArgsMap[a.AttributeKey] = a;
                                        }
                                        return [4 /*yield*/, this.blockAction('SaveAttributeValues', {
                                                personGuid: this.personGuid,
                                                keyArgsMap: keyArgsMap
                                            })];
                                    case 1:
                                        _b.sent();
                                        this.goToViewMode();
                                        this.isLoading = false;
                                        return [2 /*return*/];
                                }
                            });
                        });
                    }
                },
                watch: {
                    personGuid: {
                        immediate: true,
                        handler: function () {
                            return __awaiter(this, void 0, void 0, function () {
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0:
                                            if (!this.personGuid) return [3 /*break*/, 2];
                                            return [4 /*yield*/, this.loadData()];
                                        case 1:
                                            _a.sent();
                                            _a.label = 2;
                                        case 2: return [2 /*return*/];
                                    }
                                });
                            });
                        }
                    }
                },
                template: "\n<PaneledBlockTemplate class=\"panel-persondetails\">\n    <template v-slot:title>\n        <i :class=\"blockSettings.BlockIconCssClass\"></i>\n        {{ blockSettings.BlockTitle }}\n    </template>\n    <template v-slot:titleAside>\n        <div class=\"actions rollover-item pull-right\">\n            <JavaScriptAnchor title=\"Order Attributes\" class=\"btn-link edit\">\n                <i class=\"fa fa-bars\"></i>\n            </JavaScriptAnchor>\n            <JavaScriptAnchor title=\"Edit Attributes\" class=\"btn-link edit\" @click=\"goToEditMode\">\n                <i class=\"fa fa-pencil\"></i>\n            </JavaScriptAnchor>\n        </div>\n    </template>\n    <template v-slot:default>\n        <Loading :isLoading=\"isLoading\">\n            <AttributeValueContainer v-if=\"!isEditMode\" :attributeValues=\"attributeDataList\" :showEmptyValues=\"false\" />\n            <RockForm v-else @submit=\"doSave\">\n                <AttributeValueContainer :attributeValues=\"attributeDataList\" isEditMode :showAbbreviatedName=\"useAbbreviatedNames\" />\n                <div class=\"actions\">\n                    <RockButton class=\"btn-primary btn-xs\" type=\"submit\">Save</RockButton>\n                    <RockButton class=\"btn-link btn-xs\" @click=\"goToViewMode\">Cancel</RockButton>\n                </div>\n            </RockForm>\n        </Loading>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=AttributeValues.js.map