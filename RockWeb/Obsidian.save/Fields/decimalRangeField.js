System.register(["vue", "./fieldType", "../Services/number"], function (exports_1, context_1) {
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
    var vue_1, fieldType_1, number_1, editComponent, DecimalRangeFieldType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (fieldType_1_1) {
                fieldType_1 = fieldType_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            editComponent = vue_1.defineAsyncComponent(() => __awaiter(void 0, void 0, void 0, function* () {
                return (yield context_1.import("./decimalRangeFieldComponents")).EditComponent;
            }));
            DecimalRangeFieldType = class DecimalRangeFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    if (value.value === null || value.value === undefined || value.value === "" || value.value === ",") {
                        value.textValue = "";
                        return;
                    }
                    const numbers = value.value.split(",").map(v => number_1.toNumberOrNull(v));
                    if (numbers.length !== 2 || (numbers[0] === null && numbers[1] === null)) {
                        value.textValue = "";
                        return;
                    }
                    if (numbers[0] === null) {
                        value.textValue = `through ${numbers[1]}`;
                    }
                    else if (numbers[1] === null) {
                        value.textValue = `from ${numbers[0]}`;
                    }
                    else {
                        value.textValue = `${numbers[0]} to ${numbers[1]}`;
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("DecimalRangeFieldType", DecimalRangeFieldType);
        }
    };
});
//# sourceMappingURL=decimalRangeField.js.map