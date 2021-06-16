System.register(["vue", "../Util/Guid", "../Rules/Index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Guid_1, Index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'CheckBox',
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    inline: {
                        type: Boolean,
                        default: true
                    },
                    rules: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        uniqueId: "rock-checkbox-" + Guid_1.newGuid(),
                        internalValue: this.modelValue
                    };
                },
                methods: {
                    toggle: function () {
                        if (!this.isRequired) {
                            this.internalValue = !this.internalValue;
                        }
                        else {
                            this.internalValue = true;
                        }
                    }
                },
                computed: {
                    isRequired: function () {
                        var rules = Index_1.ruleStringToArray(this.rules);
                        return rules.indexOf('required') !== -1;
                    }
                },
                watch: {
                    modelValue: function () {
                        this.internalValue = this.modelValue;
                    },
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    isRequired: {
                        immediate: true,
                        handler: function () {
                            if (this.isRequired) {
                                this.internalValue = true;
                            }
                        }
                    }
                },
                template: "\n<div v-if=\"inline\" class=\"checkbox\">\n    <label title=\"\">\n        <input type=\"checkbox\" v-model=\"internalValue\" />\n        <span class=\"label-text \">{{label}}</span>\n    </label>\n</div>\n<div v-else class=\"form-group rock-check-box\" :class=\"isRequired ? 'required' : ''\">\n    <label class=\"control-label\" :for=\"uniqueId\">{{label}}</label>\n    <div class=\"control-wrapper\">\n        <div class=\"rock-checkbox-icon\" @click=\"toggle\" :class=\"isRequired ? 'text-muted' : ''\">\n            <i v-if=\"modelValue\" class=\"fa fa-check-square-o fa-lg\"></i>\n            <i v-else class=\"fa fa-square-o fa-lg\"></i>\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=CheckBox.js.map