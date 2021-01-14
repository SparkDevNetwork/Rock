System.register(["../Util/Guid.js"], function (exports_1, context_1) {
    "use strict";
    var Guid_js_1, fieldTypeComponentPaths;
    var __moduleName = context_1 && context_1.id;
    function getFieldTypeProps() {
        return {
            modelValue: {
                type: String,
                required: true
            },
            edit: {
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
        var normalizedGuid = Guid_js_1.normalize(fieldTypeGuid);
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
        var field = fieldTypeComponentPaths[Guid_js_1.normalize(fieldTypeGuid)];
        if (field) {
            return field;
        }
        console.error("Field type \"" + fieldTypeGuid + "\" was not found");
        return null;
    }
    exports_1("getFieldTypeComponent", getFieldTypeComponent);
    return {
        setters: [
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeComponentPaths = {};
        }
    };
});
//# sourceMappingURL=Index.js.map