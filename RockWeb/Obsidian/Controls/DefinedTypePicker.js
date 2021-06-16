System.register(["../Store/Generators", "../Store/Index"], function (exports_1, context_1) {
    "use strict";
    var Generators_1, Index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Generators_1_1) {
                Generators_1 = Generators_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", Generators_1.createCommonEntityPicker('DefinedType', function () { return Index_1.default.getters['definedTypes/all'].map(function (dt) { return ({
                Guid: dt.Guid,
                Id: dt.Id,
                Text: dt.Name
            }); }); }));
        }
    };
});
//# sourceMappingURL=DefinedTypePicker.js.map