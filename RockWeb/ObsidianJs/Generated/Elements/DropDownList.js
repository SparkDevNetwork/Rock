define(["require", "exports", "../Vendor/Vue/vue.js", "../Util/Guid.js"], function (require, exports, vue_js_1, Guid_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'DropDownList',
        props: {
            modelValue: {
                type: String,
                required: true
            },
            label: {
                type: String,
                required: true
            },
            required: {
                type: Boolean,
                default: false
            },
            disabled: {
                type: Boolean,
                default: false
            },
            options: {
                type: Array,
                required: true
            }
        },
        emits: [
            'update:modelValue'
        ],
        data: function () {
            return {
                uniqueId: "rock-dropdownlist-" + Guid_js_1.newGuid(),
                internalValue: this.modelValue
            };
        },
        methods: {
            onChange: function () {
                this.$emit('update:modelValue', this.internalValue);
            }
        },
        watch: {
            value: function () {
                this.internalValue = this.modelValue;
            }
        },
        template: "<div class=\"form-group rock-drop-down-list\" :class=\"{required: required}\">\n    <label class=\"control-label\" :for=\"uniqueId\">{{label}}</label>\n    <div class=\"control-wrapper\">\n        <select :id=\"uniqueId\" class=\"form-control\" v-model=\"internalValue\" @change=\"onChange\" :disabled=\"disabled\">\n            <option value=\"\"></option>\n            <option v-for=\"o in options\" :key=\"o.key\" :value=\"o.value\">{{o.text}}</option>\n        </select>\n    </div>\n</div>"
    });
});
//# sourceMappingURL=DropDownList.js.map