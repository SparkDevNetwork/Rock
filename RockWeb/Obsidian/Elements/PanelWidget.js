System.register(["vue", "./RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'PanelWidget',
                components: {
                    RockButton: RockButton_1.default
                },
                props: {
                    isDefaultOpen: {
                        type: Boolean,
                        default: false
                    }
                },
                data: function () {
                    return {
                        isOpen: this.isDefaultOpen
                    };
                },
                methods: {
                    toggle: function () {
                        this.isOpen = !this.isOpen;
                    }
                },
                template: "\n<section class=\"panel panel-widget rock-panel-widget\">\n    <header class=\"panel-heading clearfix clickable\" @click=\"toggle\">\n        <div class=\"pull-left\">\n            <slot name=\"header\" />\n        </div>\n        <div class=\"pull-right\">\n            <RockButton btnType=\"link\" btnSize=\"xs\">\n                <i v-if=\"isOpen\" class=\"fa fa-chevron-up\"></i>\n                <i v-else class=\"fa fa-chevron-down\"></i>\n            </RockButton>\n        </div>\n    </header>\n    <div v-if=\"isOpen\" class=\"panel-body\">\n        <slot />\n    </div>\n</section>"
            }));
        }
    };
});
//# sourceMappingURL=PanelWidget.js.map