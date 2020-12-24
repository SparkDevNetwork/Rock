define(["require", "exports", "../Vendor/Vue/vue.js", "../Util/guid.js"], function (require, exports, vue_js_1, guid_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'TextBox',
        props: {
            modelValue: {
                type: String,
                required: true
            },
            label: {
                type: String,
                required: true
            },
            type: {
                type: String,
                default: 'text'
            }
        },
        emits: [
            'update:modelValue'
        ],
        data: function () {
            return {
                uniqueId: "rock-textbox-" + guid_js_1.newGuid(),
                internalValue: this.modelValue
            };
        },
        methods: {
            handleInput: function () {
                this.$emit('update:modelValue', this.internalValue);
            },
        },
        watch: {
            value: function () {
                this.internalValue = this.modelValue;
            }
        },
        template: "<div class=\"form-group rock-text-box\">\n    <label class=\"control-label\" :for=\"uniqueId\">\n        {{label}}\n    </label>\n    <div class=\"control-wrapper\">\n        <input :id=\"uniqueId\" :type=\"type\" class=\"form-control\" v-model=\"internalValue\" @input=\"handleInput\" />\n    </div>\n</div>"
    });
});
//# sourceMappingURL=TextBox.js.map