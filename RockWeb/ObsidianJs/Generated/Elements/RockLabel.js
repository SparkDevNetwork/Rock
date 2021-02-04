System.register(["vue", "./JavaScriptAnchor"], function (exports_1, context_1) {
    "use strict";
    var vue_1, JavaScriptAnchor_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockLabel',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    help: {
                        type: String,
                        default: ''
                    }
                },
                mounted: function () {
                    if (this.help) {
                        var helpAnchor = this.$refs.help;
                        var jQuery = window['$'];
                        jQuery(helpAnchor.$el).tooltip();
                    }
                },
                template: "\n<label class=\"control-label\">\n    <slot />\n    <JavaScriptAnchor v-if=\"help\" ref=\"help\" class=\"help\" tabindex=\"-1\" data-toggle=\"tooltip\" data-placement=\"auto\" data-container=\"body\" data-html=\"true\" title=\"\" :data-original-title=\"help\">\n        <i class=\"fa fa-info-circle\"></i>\n    </JavaScriptAnchor>\n</label>"
            }));
        }
    };
});
//# sourceMappingURL=RockLabel.js.map