System.register(["vue", "../../Templates/PaneledBlockTemplate", "../../Controls/Loading", "../../Store/Index", "../../Util/Guid", "../../Elements/JavaScriptAnchor", "../../Controls/RockForm", "../../Elements/TextBox", "../../Elements/RockButton", "../../Controls/AttributeValuesContainer"], function (exports_1, context_1) {
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
    var vue_1, PaneledBlockTemplate_1, Loading_1, Index_1, Guid_1, JavaScriptAnchor_1, RockForm_1, TextBox_1, RockButton_1, AttributeValuesContainer_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (Loading_1_1) {
                Loading_1 = Loading_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (AttributeValuesContainer_1_1) {
                AttributeValuesContainer_1 = AttributeValuesContainer_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Crm.AttributeValues',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    Loading: Loading_1.default,
                    JavaScriptAnchor: JavaScriptAnchor_1.default,
                    RockForm: RockForm_1.default,
                    TextBox: TextBox_1.default,
                    RockButton: RockButton_1.default,
                    AttributeValuesContainer: AttributeValuesContainer_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction'),
                        configurationValues: vue_1.inject('configurationValues')
                    };
                },
                data: function () {
                    return {
                        isLoading: false,
                        isEditMode: false
                    };
                },
                computed: {
                    person: function () {
                        return (Index_1.default.getters.personContext || null);
                    },
                    personGuid: function () {
                        var _a;
                        return ((_a = this.person) === null || _a === void 0 ? void 0 : _a.guid) || null;
                    },
                    categoryGuids: function () {
                        return this.configurationValues.CategoryGuids || [];
                    },
                    useAbbreviatedNames: function () {
                        return this.configurationValues.UseAbbreviatedNames;
                    },
                    attributeValues: function () {
                        var _this = this;
                        var _a;
                        var attributes = ((_a = this.person) === null || _a === void 0 ? void 0 : _a.attributes) || {};
                        var attributeValues = [];
                        for (var key in attributes) {
                            var attributeValue = attributes[key];
                            var attribute = attributeValue.attribute;
                            if (this.categoryGuids && !attribute) {
                                continue;
                            }
                            if (this.categoryGuids && !(attribute === null || attribute === void 0 ? void 0 : attribute.categoryGuids.some(function (g1) { return _this.categoryGuids.some(function (g2) { return Guid_1.areEqual(g1, g2); }); }))) {
                                continue;
                            }
                            attributeValues.push(attributeValue);
                        }
                        attributeValues.sort(function (a, b) {
                            var _a, _b, _c, _d;
                            var aOrder = ((_a = a.attribute) === null || _a === void 0 ? void 0 : _a.order) || 0;
                            var bOrder = ((_b = b.attribute) === null || _b === void 0 ? void 0 : _b.order) || 0;
                            if (aOrder === bOrder) {
                                var aName = ((_c = a.attribute) === null || _c === void 0 ? void 0 : _c.name) || '';
                                var bName = ((_d = b.attribute) === null || _d === void 0 ? void 0 : _d.name) || '';
                                if (aName > bName) {
                                    return 1;
                                }
                                if (aName < bName) {
                                    return -1;
                                }
                            }
                            return aOrder - bOrder;
                        });
                        return attributeValues;
                    }
                },
                methods: {
                    goToViewMode: function () {
                        this.isEditMode = false;
                    },
                    goToEditMode: function () {
                        this.isEditMode = true;
                    },
                    doSave: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var keyValueMap, _i, _a, a;
                            return __generator(this, function (_b) {
                                switch (_b.label) {
                                    case 0:
                                        this.isLoading = true;
                                        keyValueMap = {};
                                        for (_i = 0, _a = this.attributeValues; _i < _a.length; _i++) {
                                            a = _a[_i];
                                            if (a.attribute) {
                                                keyValueMap[a.attribute.key] = a.value;
                                            }
                                        }
                                        return [4, this.invokeBlockAction('SaveAttributeValues', {
                                                personGuid: this.personGuid,
                                                keyValueMap: keyValueMap
                                            })];
                                    case 1:
                                        _b.sent();
                                        this.goToViewMode();
                                        this.isLoading = false;
                                        return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<PaneledBlockTemplate class=\"panel-persondetails\">\n    <template v-slot:title>\n        <i :class=\"configurationValues.BlockIconCssClass\"></i>\n        {{ configurationValues.BlockTitle }}\n    </template>\n    <template v-slot:titleAside>\n        <div class=\"actions rollover-item pull-right\">\n            <JavaScriptAnchor title=\"Order Attributes\" class=\"btn-link edit\">\n                <i class=\"fa fa-bars\"></i>\n            </JavaScriptAnchor>\n            <JavaScriptAnchor title=\"Edit Attributes\" class=\"btn-link edit\" @click=\"goToEditMode\">\n                <i class=\"fa fa-pencil\"></i>\n            </JavaScriptAnchor>\n        </div>\n    </template>\n    <template v-slot:default>\n        <Loading :isLoading=\"isLoading\">\n            <AttributeValuesContainer v-if=\"!isEditMode\" :attributeValues=\"attributeValues\" :showEmptyValues=\"false\" />\n            <RockForm v-else @submit=\"doSave\">\n                <AttributeValuesContainer :attributeValues=\"attributeValues\" isEditMode :showAbbreviatedName=\"useAbbreviatedNames\" />\n                <div class=\"actions\">\n                    <RockButton btnType=\"primary\" btnSize=\"xs\" type=\"submit\">Save</RockButton>\n                    <RockButton btnType=\"link\" btnSize=\"xs\" @click=\"goToViewMode\">Cancel</RockButton>\n                </div>\n            </RockForm>\n        </Loading>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=AttributeValues.js.map