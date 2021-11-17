System.register(["vue", "./fieldType", "../Services/string"], function (exports_1, context_1) {
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
    var vue_1, fieldType_1, string_1, editComponent, PhoneNumberFieldType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (fieldType_1_1) {
                fieldType_1 = fieldType_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            }
        ],
        execute: function () {
            editComponent = vue_1.defineAsyncComponent(() => __awaiter(void 0, void 0, void 0, function* () {
                return (yield context_1.import("./phoneNumberFieldComponents")).EditComponent;
            }));
            PhoneNumberFieldType = class PhoneNumberFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    value.textValue = string_1.formatPhoneNumber(value.value || "");
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("PhoneNumberFieldType", PhoneNumberFieldType);
        }
    };
});
//# sourceMappingURL=phoneNumberField.js.map