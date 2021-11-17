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
    var vue_1, fieldType_1, editComponent, RatingFieldType;
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
                return (yield context_1.import("./ratingFieldComponents")).EditComponent;
            }));
            RatingFieldType = class RatingFieldType extends fieldType_1.FieldTypeBase {
                getTextValue(value) {
                    return value.textValue || "0";
                }
                getHtmlValue(value) {
                    var _a, _b, _c;
                    let ratingValue;
                    try {
                        ratingValue = JSON.parse((_a = value.value) !== null && _a !== void 0 ? _a : "");
                    }
                    catch (_d) {
                        ratingValue = null;
                    }
                    const rating = (_b = ratingValue === null || ratingValue === void 0 ? void 0 : ratingValue.value) !== null && _b !== void 0 ? _b : 0;
                    const maxRating = (_c = ratingValue === null || ratingValue === void 0 ? void 0 : ratingValue.maxValue) !== null && _c !== void 0 ? _c : 5;
                    let html = "";
                    for (let i = 0; i < rating && i < maxRating; i++) {
                        html += `<i class="fa fa-rating-selected"></i>`;
                    }
                    for (let i = rating; i < maxRating; i++) {
                        html += `<i class="fa fa-rating-unselected"></i>`;
                    }
                    return html;
                }
                updateTextValue(value) {
                    var _a, _b, _c;
                    try {
                        const ratingValue = JSON.parse((_a = value.value) !== null && _a !== void 0 ? _a : "");
                        value.textValue = (_c = (_b = ratingValue === null || ratingValue === void 0 ? void 0 : ratingValue.value) === null || _b === void 0 ? void 0 : _b.toString()) !== null && _c !== void 0 ? _c : "";
                    }
                    catch (_d) {
                        value.textValue = "";
                    }
                }
                getEditComponent(_value) {
                    return editComponent;
                }
            };
            exports_1("RatingFieldType", RatingFieldType);
        }
    };
});
//# sourceMappingURL=ratingField.js.map