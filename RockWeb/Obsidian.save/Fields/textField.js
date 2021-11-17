System.register(["./fieldType"], function (exports_1, context_1) {
    "use strict";
    var fieldType_1, TextFieldType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (fieldType_1_1) {
                fieldType_1 = fieldType_1_1;
            }
        ],
        execute: function () {
            TextFieldType = class TextFieldType extends fieldType_1.FieldTypeBase {
            };
            exports_1("TextFieldType", TextFieldType);
        }
    };
});
//# sourceMappingURL=textField.js.map