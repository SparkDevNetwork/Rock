define(["require", "exports", "../Util/Guid.js"], function (require, exports, Guid_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.getFieldTypeComponent = exports.registerFieldType = void 0;
    var fieldTypeComponentPaths = {};
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
    exports.registerFieldType = registerFieldType;
    function getFieldTypeComponent(fieldTypeGuid) {
        var field = fieldTypeComponentPaths[Guid_js_1.normalize(fieldTypeGuid)];
        if (field) {
            return field;
        }
        console.error("Field type \"" + fieldTypeGuid + "\" was not found");
        return null;
    }
    exports.getFieldTypeComponent = getFieldTypeComponent;
});
//# sourceMappingURL=Index.js.map