System.register(["../Vendor/Vue/vue.js", "../Util/Guid.js", "./RockFormField.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Guid_js_1, RockFormField_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (RockFormField_js_1_1) {
                RockFormField_js_1 = RockFormField_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'DropDownList',
                components: {
                    RockFormField: RockFormField_js_1.default
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
                    }
                },
                data: function () {
                    return {
                        uniqueId: "rock-dropdownlist-" + Guid_js_1.newGuid(),
                        internalValue: this.blankValue
                    };
                },
                methods: {
                    syncValue: function () {
                        var _this = this;
                        var _a;
                        this.internalValue = this.modelValue;
                        var selectedOption = this.options.find(function (o) { return o.value === _this.internalValue; }) || null;
                        if (!selectedOption) {
                            this.internalValue = this.showBlankItem ?
                                this.blankValue :
                                (((_a = this.options[0]) === null || _a === void 0 ? void 0 : _a.value) || this.blankValue);
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
                template: "\n<RockFormField\n    :modelValue=\"internalValue\"\n    formGroupClasses=\"rock-drop-down-list\"\n    name=\"dropdownlist\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <select :id=\"uniqueId\" class=\"form-control\" :class=\"formControlClasses\" :disabled=\"disabled\" v-bind=\"field\" v-model=\"internalValue\">\n                <option v-if=\"showBlankItem\" :value=\"blankValue\"></option>\n                <option v-for=\"o in options\" :key=\"o.key\" :value=\"o.value\">{{o.text}}</option>\n            </select>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=DropDownList.js.map