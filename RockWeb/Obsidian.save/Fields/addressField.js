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
    var vue_1, fieldType_1, editComponent, AddressFieldType;
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
                return (yield context_1.import("./addressFieldComponents")).EditComponent;
            }));
            AddressFieldType = class AddressFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    var _a, _b, _c, _d, _e;
                    try {
                        const addressValue = JSON.parse(value.value || "{}");
                        let textValue = `${(_a = addressValue.street1) !== null && _a !== void 0 ? _a : ""} ${(_b = addressValue.street2) !== null && _b !== void 0 ? _b : ""} ${(_c = addressValue.city) !== null && _c !== void 0 ? _c : ""}, ${(_d = addressValue.state) !== null && _d !== void 0 ? _d : ""} ${(_e = addressValue.postalCode) !== null && _e !== void 0 ? _e : ""}`;
                        textValue = textValue.replace(/  +/, " ");
                        textValue = textValue.replace(/^ +/, "");
                        textValue = textValue.replace(/ +$/, "");
                        if (textValue === ",") {
                            value.textValue = "";
                        }
                        else {
                            value.textValue = textValue;
                        }
                    }
                    catch (_f) {
                        value.textValue = value.value;
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("AddressFieldType", AddressFieldType);
        }
    };
});
//# sourceMappingURL=addressField.js.map