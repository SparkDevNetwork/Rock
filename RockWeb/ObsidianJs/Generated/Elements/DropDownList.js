System.register(["../Vendor/Vue/vue.js", "../Util/Guid.js", "../Vendor/VeeValidate/vee-validate.js", "./RockLabel.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Guid_js_1, vee_validate_js_1, RockLabel_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (vee_validate_js_1_1) {
                vee_validate_js_1 = vee_validate_js_1_1;
            },
            function (RockLabel_js_1_1) {
                RockLabel_js_1 = RockLabel_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'DropDownList',
                components: {
                    Field: vee_validate_js_1.Field,
                    RockLabel: RockLabel_js_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    },
                    options: {
                        type: Array,
                        required: true
                    },
                    rules: {
                        type: String,
                        default: ''
                    },
                    help: {
                        type: String,
                        default: ''
                    },
                    showBlankItem: {
                        type: Boolean,
                        default: true
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-dropdownlist-" + Guid_js_1.newGuid(),
                        internalValue: ''
                    };
                },
                computed: {
                    isRequired: function () {
                        return this.rules.includes('required');
                    }
                },
                methods: {
                    syncValue: function () {
                        this.internalValue = this.modelValue;
                        if (!this.showBlankItem && !this.internalValue && this.options.length) {
                            this.internalValue = this.options[0].value;
                            this.$emit('update:modelValue', this.internalValue);
                        }
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.syncValue();
                        }
                    },
                    options: {
                        immediate: true,
                        handler: function () {
                            this.syncValue();
                        }
                    },
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<Field\n    v-model=\"internalValue\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <div class=\"form-group rock-drop-down-list\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <RockLabel :for=\"uniqueId\" :help=\"help\">{{label}}</RockLabel>\n        <div class=\"control-wrapper\">\n            <select :id=\"uniqueId\" class=\"form-control\" :disabled=\"disabled\" v-bind=\"field\">\n                <option v-if=\"showBlankItem\" value=\"\"></option>\n                <option v-for=\"o in options\" :key=\"o.key\" :value=\"o.value\">{{o.text}}</option>\n            </select>\n        </div>\n    </div>\n</Field>"
            }));
        }
    };
});
//# sourceMappingURL=DropDownList.js.map