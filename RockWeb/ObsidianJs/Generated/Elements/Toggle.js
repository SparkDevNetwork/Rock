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
                name: 'Toggle',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    }
                },
                data: function () {
                    return {
                        selectedClasses: 'active btn btn-primary',
                        unselectedClasses: 'btn btn-default'
                    };
                },
                methods: {
                    onClick: function (isOn) {
                        this.$emit('update:modelValue', isOn);
                    }
                },
                template: "\n<div class=\"btn-group btn-toggle btn-group-justified\">\n    <JavaScriptAnchor :class=\"modelValue ? selectedClasses : unselectedClasses\" @click=\"onClick(true)\">\n        <slot name=\"on\">On</slot>\n    </JavaScriptAnchor>\n    <JavaScriptAnchor :class=\"modelValue ? unselectedClasses : selectedClasses\" @click=\"onClick(false)\">\n        <slot name=\"off\">Off</slot>\n    </JavaScriptAnchor>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Toggle.js.map