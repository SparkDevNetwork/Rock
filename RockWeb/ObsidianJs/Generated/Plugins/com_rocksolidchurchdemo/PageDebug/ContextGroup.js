System.register(["vue", "../../../Templates/PaneledBlockTemplate", "../../../Store/Index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, PaneledBlockTemplate_1, Index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'com_rocksolidchurchdemo.PageDebug.ContextGroup',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default
                },
                computed: {
                    contextGroup: function () {
                        return Index_1.default.getters.groupContext || {};
                    }
                },
                template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-grin-tongue-squint\"></i>\n        Context Group (TS Plugin)\n    </template>\n    <template v-slot:default>\n        <dl>\n            <dt>Group</dt>\n            <dd>{{contextGroup.Name || '<none>'}}</dd>\n        </dl>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=ContextGroup.js.map