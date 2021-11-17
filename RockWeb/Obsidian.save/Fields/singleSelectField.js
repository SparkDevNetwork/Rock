System.register(["vue", "./fieldType"], function (exports_1, context_1) {
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
    var vue_1, fieldType_1, editComponent, SingleSelectFieldType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (fieldType_1_1) {
                fieldType_1 = fieldType_1_1;
            }
        ],
        execute: function () {
            editComponent = vue_1.defineAsyncComponent(() => __awaiter(void 0, void 0, void 0, function* () {
                return (yield context_1.import("./singleSelectFieldComponents")).EditComponent;
            }));
            SingleSelectFieldType = class SingleSelectFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    var _a, _b;
                    if (value.value === undefined || value.value === null || value.value === "") {
                        value.textValue = "";
                        return;
                    }
                    try {
                        const values = JSON.parse((_b = (_a = value.configurationValues) === null || _a === void 0 ? void 0 : _a["values"]) !== null && _b !== void 0 ? _b : "[]");
                        const selectedValues = values.filter(v => v.value === value.value);
                        if (selectedValues.length >= 1) {
                            value.textValue = selectedValues[0].text;
                        }
                        else {
                            value.textValue = "";
                        }
                    }
                    catch (_c) {
                        value.textValue = value.value;
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("SingleSelectFieldType", SingleSelectFieldType);
        }
    };
});
//# sourceMappingURL=singleSelectField.js.map