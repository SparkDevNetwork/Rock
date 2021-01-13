System.register(["../Vendor/Vue/vue.js", "./Index.js", "../Elements/TextBox.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1, TextBox_js_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '9C204CD0-1233-41C5-818A-C5DA439445AA';
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
                name: 'TextField',
                components: {
                    TextBox: TextBox_js_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    edit: {
                        type: Boolean,
                        default: false
                    },
                    label: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    safeValue: function () {
                        return (this.modelValue || '').trim();
                    },
                    valueIsNull: function () {
                        return !this.safeValue;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<TextBox v-if=\"edit\" v-model=\"internalValue\" :label=\"label\" :help=\"help\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=TextField.js.map