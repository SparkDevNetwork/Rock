System.register(["vue", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'DropDownList',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    options: {
                        type: Array,
                        required: true
                    },
                    showBlankItem: {
                        type: Boolean,
                        default: true
                    },
                    blankValue: {
                        type: String,
                        default: ''
                    },
                    formControlClasses: {
                        type: String,
                        default: ''
                    },
                    placeholder: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        internalValue: this.blankValue
                    };
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue;
                        }
                    },
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<RockFormField\n    :modelValue=\"internalValue\"\n    formGroupClasses=\"rock-drop-down-list\"\n    name=\"dropdownlist\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <select :id=\"uniqueId\" class=\"form-control\" :class=\"formControlClasses\" :disabled=\"disabled\" v-model=\"internalValue\">\n                <option v-if=\"showBlankItem\" :value=\"blankValue\">{{placeholder}}</option>\n                <option v-for=\"o in options\" :key=\"o.key\" :value=\"o.value\">{{o.text}}</option>\n            </select>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=DropDownList.js.map