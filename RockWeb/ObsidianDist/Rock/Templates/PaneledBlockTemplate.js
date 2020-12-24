define(["require", "exports", "../Vendor/Vue/vue.js"], function (require, exports, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    // Provides a generic Rock Block structure
    exports.default = vue_js_1.defineComponent({
        name: 'PaneledBlockTemplate',
        template: "<div class=\"panel panel-block\">\n    <div class=\"panel-heading\">\n        <h1 class=\"panel-title\">\n            <slot name=\"title\" />\n        </h1>\n    </div>\n    <div class=\"panel-body\">\n        <div class=\"block-content\">\n            <slot />\n        </div>\n    </div>\n</div>"
    });
});
//# sourceMappingURL=PaneledBlockTemplate.js.map