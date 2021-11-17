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
    var vue_1, fieldType_1, number_1, editComponent, DayOfWeekFieldType;
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
                return (yield context_1.import("./dayOfWeekFieldComponents")).EditComponent;
            }));
            DayOfWeekFieldType = class DayOfWeekFieldType extends fieldType_1.FieldTypeBase {
                updateTextValue(value) {
                    const dayValue = number_1.toNumberOrNull(value.value);
                    if (dayValue === null) {
                        value.textValue = "";
                    }
                    else {
                        switch (dayValue) {
                            case 0:
                                value.textValue = "Sunday";
                                break;
                            case 1:
                                value.textValue = "Monday";
                                break;
                            case 2:
                                value.textValue = "Tuesday";
                                break;
                            case 3:
                                value.textValue = "Wednesday";
                                break;
                            case 4:
                                value.textValue = "Thursday";
                                break;
                            case 5:
                                value.textValue = "Friday";
                                break;
                            case 6:
                                value.textValue = "Saturday";
                                break;
                            default:
                                value.textValue = "";
                                break;
                        }
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("DayOfWeekFieldType", DayOfWeekFieldType);
        }
    };
});
//# sourceMappingURL=dayOfWeekField.js.map