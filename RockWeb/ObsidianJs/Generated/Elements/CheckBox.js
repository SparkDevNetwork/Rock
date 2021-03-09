System.register(["../Vendor/Vue/vue.js", "../Util/Guid.js", "../Rules/Index.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Guid_js_1, Index_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
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
                        uniqueId: "rock-checkbox-" + Guid_js_1.newGuid(),
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
                        var rules = Index_js_1.ruleStringToArray(this.rules);
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