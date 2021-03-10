System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'PaneledBlockTemplate',
                template: "<div class=\"panel panel-block\">\n    <div class=\"panel-heading rollover-container\">\n        <h1 class=\"panel-title pull-left\">\n            <slot name=\"title\" />\n        </h1>\n        <slot name=\"titleAside\" />\n    </div>\n    <div class=\"panel-body\">\n        <div class=\"block-content\">\n            <slot />\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=PaneledBlockTemplate.js.map