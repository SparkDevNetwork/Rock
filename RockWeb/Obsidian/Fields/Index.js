System.register(["../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var Guid_1, fieldTypeComponentPaths;
    var __moduleName = context_1 && context_1.id;
    function getConfigurationValue(key, configurationValues) {
        key = (key || '').toLowerCase().trim();
        if (!configurationValues || !key) {
            return '';
        }
        var objectKey = Object.keys(configurationValues).find(function (k) { return k.toLowerCase().trim() === key; });
        if (!objectKey) {
            return '';
        }
        var configObject = configurationValues[objectKey];
        return (configObject === null || configObject === void 0 ? void 0 : configObject.Value) || '';
    }
    exports_1("getConfigurationValue", getConfigurationValue);
    function getFieldTypeProps() {
        return {
            modelValue: {
                type: String,
                required: true
            },
            isEditMode: {
                type: Boolean,
                default: false
            },
            configurationValues: {
                type: Object,
                default: function () { return ({}); }
            }
        };
    }
    exports_1("getFieldTypeProps", getFieldTypeProps);
    function registerFieldType(fieldTypeGuid, component) {
        var normalizedGuid = Guid_1.normalize(fieldTypeGuid);
        var dataToExport = {
            fieldTypeGuid: normalizedGuid,
            component: component
        };
        if (fieldTypeComponentPaths[normalizedGuid]) {
            console.error("Field type \"" + fieldTypeGuid + "\" is already registered");
        }
        else {
            fieldTypeComponentPaths[normalizedGuid] = component;
        }
        return dataToExport;
    }
    exports_1("registerFieldType", registerFieldType);
    function getFieldTypeComponent(fieldTypeGuid) {
        var field = fieldTypeComponentPaths[Guid_1.normalize(fieldTypeGuid)];
        if (field) {
            return field;
        }
        console.error("Field type \"" + fieldTypeGuid + "\" was not found");
        return null;
    }
    exports_1("getFieldTypeComponent", getFieldTypeComponent);
    return {
        setters: [
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            }
        ],
        execute: function () {
            fieldTypeComponentPaths = {};
        }
    };
});
//# sourceMappingURL=Index.js.map