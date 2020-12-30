define(["require", "exports", "../Vendor/Vue/vue.js", "../Util/Guid.js"], function (require, exports, vue_js_1, Guid_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
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
    });
});
//# sourceMappingURL=CheckBox.js.map