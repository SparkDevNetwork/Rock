define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.getFieldTypeComponent = exports.registerFieldType = void 0;
    var fieldTypeComponentPaths = {};
    function registerFieldType(fieldTypeGuid, component) {
        var dataToExport = {
            fieldTypeGuid: fieldTypeGuid,
            component: component
        };
        if (fieldTypeComponentPaths[fieldTypeGuid]) {
            console.error("Field type \"" + fieldTypeGuid + "\" is already registered");
        }
        else {
            fieldTypeComponentPaths[fieldTypeGuid] = component;
        }
        return dataToExport;
    }
    exports.registerFieldType = registerFieldType;
    function getFieldTypeComponent(fieldTypeGuid) {
        return fieldTypeComponentPaths[fieldTypeGuid];
    }
    exports.getFieldTypeComponent = getFieldTypeComponent;
});
//# sourceMappingURL=Index.js.map