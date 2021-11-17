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
    var vue_1, fieldType_1, editComponent, SSNFieldType;
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
                return (yield context_1.import("./ssnFieldComponents")).EditComponent;
            }));
            SSNFieldType = class SSNFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    if (value.value === null || value.value === undefined) {
                        value.textValue = "";
                        return;
                    }
                    const strippedValue = value.value.replace(/[^0-9]/g, "");
                    if (strippedValue.length !== 9) {
                        value.textValue = "";
                        return;
                    }
                    value.textValue = `xxx-xx-${value.value.substr(5, 4)}`;
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("SSNFieldType", SSNFieldType);
        }
    };
});
//# sourceMappingURL=ssnField.js.map