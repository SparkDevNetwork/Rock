System.register(["vue", "./javaScriptAnchor"], function (exports_1, context_1) {
    "use strict";
    var vue_1, javaScriptAnchor_1, HelpBlock;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (javaScriptAnchor_1_1) {
                javaScriptAnchor_1 = javaScriptAnchor_1_1;
            }
        ],
        execute: function () {
            HelpBlock = vue_1.defineComponent({
                name: "HelpBlock",
                components: {
                    JavaScriptAnchor: javaScriptAnchor_1.default
                },
                props: {
                    text: {
                        type: String,
                        required: true
                    }
                },
                mounted() {
                    const jquery = window["$"];
                    jquery(this.$el).tooltip();
                },
                template: `
<JavaScriptAnchor class="help" tabindex="-1" data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true" title="" :data-original-title="text">
    <i class="fa fa-info-circle"></i>
</JavaScriptAnchor>`
            });
            exports_1("default", HelpBlock);
        }
    };
});
//# sourceMappingURL=helpBlock.js.map