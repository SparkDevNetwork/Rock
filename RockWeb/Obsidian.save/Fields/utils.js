System.register(["../Util/guid"], function (exports_1, context_1) {
    "use strict";
    var guid_1, fieldTypeTable;
    var __moduleName = context_1 && context_1.id;
    function getFieldEditorProps() {
        return {
            modelValue: {
                type: String,
                required: true
            },
            configurationValues: {
                type: Object,
                default: () => ({})
            }
        };
    }
    exports_1("getFieldEditorProps", getFieldEditorProps);
    function registerFieldType(fieldTypeGuid, fieldType) {
        const normalizedGuid = guid_1.normalize(fieldTypeGuid);
        if (!guid_1.isValidGuid(fieldTypeGuid) || normalizedGuid === null) {
            throw "Invalid guid specified when registering field type.";
        }
        if (fieldTypeTable[normalizedGuid] !== undefined) {
            throw "Invalid attempt to replace existing field type.";
        }
        fieldTypeTable[normalizedGuid] = fieldType;
    }
    exports_1("registerFieldType", registerFieldType);
    function getFieldType(fieldTypeGuid) {
        const normalizedGuid = guid_1.normalize(fieldTypeGuid);
        if (normalizedGuid !== null) {
            const field = fieldTypeTable[normalizedGuid];
            if (field) {
                return field;
            }
        }
        console.error(`Field type "${fieldTypeGuid}" was not found`);
        return null;
    }
    exports_1("getFieldType", getFieldType);
    return {
        setters: [
            function (guid_1_1) {
                guid_1 = guid_1_1;
            }
        ],
        execute: function () {
            fieldTypeTable = {};
        }
    };
});
//# sourceMappingURL=utils.js.map