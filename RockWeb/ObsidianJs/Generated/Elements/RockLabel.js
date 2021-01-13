System.register(["../Vendor/Vue/vue.js", "./JavaScriptAnchor.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, JavaScriptAnchor_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (JavaScriptAnchor_js_1_1) {
                JavaScriptAnchor_js_1 = JavaScriptAnchor_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'RockLabel',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_js_1.default
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