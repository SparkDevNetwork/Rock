define(["require", "exports", "../../../Vendor/Vue/vue.js", "../../../Templates/PaneledBlockTemplate.js", "../../../Store/Index.js"], function (require, exports, vue_js_1, PaneledBlockTemplate_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'org_rocksolidchurch.PageDebug.ContextGroup',
        components: {
            PaneledBlockTemplate: PaneledBlockTemplate_js_1.default
        },
        computed: {
            contextGroup: function () {
                return Index_js_1.default.getters.groupContext || {};
            }
        },
        template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-grin-tongue-squint\"></i>\n        Context Group (TS Plugin)\n    </template>\n    <template v-slot:default>\n        <dl>\n            <dt>Group</dt>\n            <dd>{{contextGroup.Name}}</dd>\n        </dl>\n    </template>\n</PaneledBlockTemplate>"
    });
});
//# sourceMappingURL=ContextGroup.js.map