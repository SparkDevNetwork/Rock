System.register(["vue", "./JavaScriptAnchor"], function (exports_1, context_1) {
    "use strict";
    var vue_1, JavaScriptAnchor_1, HelpBlock;
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
            HelpBlock = vue_1.defineComponent({
                name: 'HelpBlock',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    text: {
                        type: String,
                        required: true
                    }
                },
                mounted: function () {
                    var jquery = window['$'];
                    jquery(this.$el).tooltip();
                },
                template: "\n<JavaScriptAnchor class=\"help\" tabindex=\"-1\" data-toggle=\"tooltip\" data-placement=\"auto\" data-container=\"body\" data-html=\"true\" title=\"\" :data-original-title=\"text\">\n    <i class=\"fa fa-info-circle\"></i>\n</JavaScriptAnchor>"
            });
            exports_1("default", HelpBlock);
        }
    };
});
//# sourceMappingURL=HelpBlock.js.map