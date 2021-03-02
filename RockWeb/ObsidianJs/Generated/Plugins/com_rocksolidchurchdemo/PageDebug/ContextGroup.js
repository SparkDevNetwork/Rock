System.register(["../../../Vendor/Vue/vue.js", "../../../Templates/PaneledBlockTemplate.js", "../../../Store/Index.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, PaneledBlockTemplate_js_1, Index_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (PaneledBlockTemplate_js_1_1) {
                PaneledBlockTemplate_js_1 = PaneledBlockTemplate_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'com_rocksolidchurchdemo.PageDebug.ContextGroup',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_js_1.default
                },
                computed: {
                    contextGroup: function () {
                        return Index_js_1.default.getters.groupContext || {};
                    }
                },
                template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-grin-tongue-squint\"></i>\n        Context Group (TS Plugin)\n    </template>\n    <template v-slot:default>\n        <dl>\n            <dt>Group</dt>\n            <dd>{{contextGroup.Name || '<none>'}}</dd>\n        </dl>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=ContextGroup.js.map