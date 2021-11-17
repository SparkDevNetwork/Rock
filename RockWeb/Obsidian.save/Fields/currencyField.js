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
    var vue_1, fieldType_1, number_1, editComponent, CurrencyFieldType;
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
                return (yield context_1.import("./currencyFieldComponents")).EditComponent;
            }));
            CurrencyFieldType = class CurrencyFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    var _a;
                    value.textValue = (_a = number_1.toCurrencyOrNull(value.value)) !== null && _a !== void 0 ? _a : "";
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("CurrencyFieldType", CurrencyFieldType);
        }
    };
});
//# sourceMappingURL=currencyField.js.map