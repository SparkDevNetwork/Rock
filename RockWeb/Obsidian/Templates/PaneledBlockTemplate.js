System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, PaneledBlockTemplate;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            PaneledBlockTemplate = vue_1.defineComponent({
                name: 'PaneledBlockTemplate',
                data: function () {
                    return {
                        isDrawerOpen: false
                    };
                },
                methods: {
                    onDrawerPullClick: function () {
                        this.isDrawerOpen = !this.isDrawerOpen;
                    }
                },
                template: "\n<div class=\"panel panel-block\">\n    <div class=\"panel-heading rollover-container\">\n        <h1 class=\"panel-title pull-left\">\n            <slot name=\"title\" />\n        </h1>\n        <slot name=\"titleAside\" />\n    </div>\n    <div v-if=\"$slots.drawer\" class=\"panel-drawer rock-panel-drawer\" :class=\"isDrawerOpen ? 'open' : ''\">\n        <div class=\"drawer-content\" v-show=\"isDrawerOpen\">\n            <slot name=\"drawer\" />\n        </div>\n        <div class=\"drawer-pull\" @click=\"onDrawerPullClick\">\n            <i :class=\"isDrawerOpen ? 'fa fa-chevron-up' : 'fa fa-chevron-down'\"></i>\n        </div>\n    </div>\n    <div class=\"panel-body\">\n        <div class=\"block-content\">\n            <slot />\n        </div>\n    </div>\n</div>"
            });
            exports_1("default", PaneledBlockTemplate);
        }
    };
});
//# sourceMappingURL=PaneledBlockTemplate.js.map