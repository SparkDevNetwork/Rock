System.register(["vue", "./fieldType", "../Services/boolean", "../Util/linq"], function (exports_1, context_1) {
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
    var vue_1, fieldType_1, boolean_1, linq_1, editComponent, DefinedValueRangeFieldType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (fieldType_1_1) {
                fieldType_1 = fieldType_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (linq_1_1) {
                linq_1 = linq_1_1;
            }
        ],
        execute: function () {
            editComponent = vue_1.defineAsyncComponent(() => __awaiter(void 0, void 0, void 0, function* () {
                return (yield context_1.import("./definedValueRangeFieldComponents")).EditComponent;
            }));
            DefinedValueRangeFieldType = class DefinedValueRangeFieldType extends fieldType_1.FieldTypeBase {
                getTextValue(value) {
                    var _a, _b;
                    try {
                        const clientValue = JSON.parse((_a = value.value) !== null && _a !== void 0 ? _a : "");
                        return (_b = (clientValue.description || clientValue.text)) !== null && _b !== void 0 ? _b : "";
                    }
                    catch (_c) {
                        return super.getTextValue(value);
                    }
                }
                getCondensedTextValue(value) {
                    var _a, _b, _c;
                    try {
                        const clientValue = JSON.parse((_a = value.value) !== null && _a !== void 0 ? _a : "");
                        return (_b = clientValue.text) !== null && _b !== void 0 ? _b : "";
                    }
                    catch (_d) {
                        return (_c = value.value) !== null && _c !== void 0 ? _c : "";
                    }
                }
                updateTextValue(value) {
                    var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k;
                    try {
                        const clientValue = JSON.parse((_a = value.value) !== null && _a !== void 0 ? _a : "");
                        try {
                            const values = new linq_1.List(JSON.parse((_c = (_b = value.configurationValues) === null || _b === void 0 ? void 0 : _b["values"]) !== null && _c !== void 0 ? _c : "[]"));
                            const displayDescription = boolean_1.asBoolean((_d = value.configurationValues) === null || _d === void 0 ? void 0 : _d["displaydescription"]);
                            const rawValues = ((_e = clientValue.value) !== null && _e !== void 0 ? _e : "").split(",");
                            if (rawValues.length !== 2) {
                                value.textValue = value.value;
                                return;
                            }
                            const lowerValue = values.firstOrUndefined(v => (v === null || v === void 0 ? void 0 : v.value) === rawValues[0]);
                            const upperValue = values.firstOrUndefined(v => (v === null || v === void 0 ? void 0 : v.value) === rawValues[1]);
                            if (lowerValue === undefined && upperValue === undefined) {
                                value.textValue = "";
                                return;
                            }
                            if (displayDescription) {
                                value.textValue = `${(_f = lowerValue === null || lowerValue === void 0 ? void 0 : lowerValue.description) !== null && _f !== void 0 ? _f : ""} to ${(_g = upperValue === null || upperValue === void 0 ? void 0 : upperValue.description) !== null && _g !== void 0 ? _g : ""}`;
                            }
                            else {
                                value.textValue = `${(_h = lowerValue === null || lowerValue === void 0 ? void 0 : lowerValue.text) !== null && _h !== void 0 ? _h : ""} to ${(_j = upperValue === null || upperValue === void 0 ? void 0 : upperValue.text) !== null && _j !== void 0 ? _j : ""}`;
                            }
                        }
                        catch (_l) {
                            value.textValue = (_k = clientValue.value) !== null && _k !== void 0 ? _k : "";
                        }
                    }
                    catch (_m) {
                        value.textValue = value.value;
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("DefinedValueRangeFieldType", DefinedValueRangeFieldType);
        }
    };
});
//# sourceMappingURL=definedValueRangeField.js.map