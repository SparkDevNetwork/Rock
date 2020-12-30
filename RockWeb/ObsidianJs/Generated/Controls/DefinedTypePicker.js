define(["require", "exports", "../Store/Generators.js", "../Store/index.js"], function (require, exports, Generators_js_1, index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Generators_js_1.createCommonEntityPicker('DefinedType', function () { return index_js_1.default.getters['definedTypes/all'].map(function (dt) { return ({
        key: dt.Guid,
        value: dt.Guid,
        text: dt.Name
    }); }); });
});
//# sourceMappingURL=DefinedTypePicker.js.map