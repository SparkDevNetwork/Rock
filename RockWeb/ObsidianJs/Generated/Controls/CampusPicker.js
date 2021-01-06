System.register(["../Store/Generators.js", "../Store/index.js"], function (exports_1, context_1) {
    "use strict";
    var Generators_js_1, index_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Generators_js_1_1) {
                Generators_js_1 = Generators_js_1_1;
            },
            function (index_js_1_1) {
                index_js_1 = index_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", Generators_js_1.createCommonEntityPicker('Campus', function () { return index_js_1.default.getters['campuses/all'].map(function (c) { return ({
                Guid: c.Guid,
                Id: c.Id,
                Text: c.Name
            }); }); }));
        }
    };
});
//# sourceMappingURL=CampusPicker.js.map