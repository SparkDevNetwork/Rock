define(["require", "exports", "../Store/generators.js", "../Store/index.js"], function (require, exports, generators_js_1, index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = generators_js_1.createCommonEntityPicker('DefinedType', function () { return index_js_1.default.getters['definedTypes/all'].map(function (dt) { return ({
        key: dt.Guid,
        value: dt.Guid,
        text: dt.Name
    }); }); });
});
//# sourceMappingURL=DefinedTypePicker.js.map