System.register(["../Services/string", "vue", "./textFieldComponents"], function (exports_1, context_1) {
    "use strict";
    var string_1, vue_1, textFieldComponents_1, FieldTypeBase;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (textFieldComponents_1_1) {
                textFieldComponents_1 = textFieldComponents_1_1;
            }
        ],
        execute: function () {
            FieldTypeBase = class FieldTypeBase {
                getTextValue(value) {
                    var _a;
                    return (_a = value.textValue) !== null && _a !== void 0 ? _a : "";
                }
                getHtmlValue(value) {
                    return `<span>${string_1.escapeHtml(this.getTextValue(value))}</span>`;
                }
                updateTextValue(value) {
                    value.textValue = value.value;
                }
                getCondensedTextValue(value) {
                    var _a;
                    return string_1.truncate((_a = value.textValue) !== null && _a !== void 0 ? _a : "", 100);
                }
                getCondensedHtmlValue(value) {
                    return this.getHtmlValue(value);
                }
                getFormattedComponent(value) {
                    return vue_1.defineComponent(() => {
                        return vue_1.compile(this.getHtmlValue(value));
                    });
                }
                getCondensedFormattedComponent(value) {
                    return vue_1.defineComponent(() => {
                        return vue_1.compile(this.getCondensedHtmlValue(value));
                    });
                }
                getEditComponent(_value) {
                    return textFieldComponents_1.EditComponent;
                }
            };
            exports_1("FieldTypeBase", FieldTypeBase);
        }
    };
});
//# sourceMappingURL=fieldType.js.map