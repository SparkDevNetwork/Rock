System.register(["vue", "./HelpBlock"], function (exports_1, context_1) {
    "use strict";
    var vue_1, HelpBlock_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (HelpBlock_1_1) {
                HelpBlock_1 = HelpBlock_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockLabel',
                components: {
                    HelpBlock: HelpBlock_1.default
                },
                props: {
                    help: {
                        type: String,
                        default: ''
                    }
                },
                template: "\n<label class=\"control-label\">\n    <slot />\n    <HelpBlock v-if=\"help\" :text=\"help\" />\n</label>"
            }));
        }
    };
});
//# sourceMappingURL=RockLabel.js.map