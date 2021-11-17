System.register(["vue", "./helpBlock"], function (exports_1, context_1) {
    "use strict";
    var vue_1, helpBlock_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (helpBlock_1_1) {
                helpBlock_1 = helpBlock_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "RockLabel",
                components: {
                    HelpBlock: helpBlock_1.default
                },
                props: {
                    help: {
                        type: String,
                        default: ""
                    }
                },
                template: `
<label class="control-label">
    <slot />
    <HelpBlock v-if="help" :text="help" />
</label>`
            }));
        }
    };
});
//# sourceMappingURL=rockLabel.js.map