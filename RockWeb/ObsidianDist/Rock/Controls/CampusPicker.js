define(["require", "exports", "../Store/generators.js", "../Store/index.js"], function (require, exports, generators_js_1, index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = generators_js_1.createCommonEntityPicker('Campus', function () { return index_js_1.default.getters['campuses/all'].map(function (c) { return ({
        key: c.Guid,
        value: c.Guid,
        text: c.Name
    }); }); });
});
//# sourceMappingURL=CampusPicker.js.map