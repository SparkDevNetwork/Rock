System.register(["../Vendor/Vue/vue.js", "../Util/Guid.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Guid_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'CheckBox',
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    },
                    label: {
                        type: String,
                        required: true
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-checkbox-" + Guid_js_1.newGuid(),
                        internalValue: this.modelValue
                    };
                },
                methods: {
                    handleInput: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                watch: {
                    value: function () {
                        this.internalValue = this.modelValue;
                    }
                },
                template: "<div class=\"checkbox\">\n    <label title=\"\">\n        <input type=\"checkbox\" v-model=\"internalValue\" />\n        <span class=\"label-text \">{{label}}</span>\n    </label>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=CheckBox.js.map