System.register(["vue", "./Index", "../Elements/DropDownList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DropDownList_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["Values"] = "values";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'SingleSelectField',
                components: {
                    DropDownList: DropDownList_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    /** The value to display when not in edit mode */
                    safeValue: function () {
                        return (this.modelValue || '').trim();
                    },
                    /** The options to choose from in the drop down list */
                    options: function () {
                        var valuesConfig = this.configurationValues[ConfigurationValueKey.Values];
                        if (valuesConfig && valuesConfig.Value) {
                            return valuesConfig.Value.split(',').map(function (v) {
                                if (v.indexOf('^') !== -1) {
                                    var parts = v.split('^');
                                    var value = parts[0];
                                    var text = parts[1];
                                    return {
                                        key: value,
                                        text: text,
                                        value: value
                                    };
                                }
                                return {
                                    key: v,
                                    text: v,
                                    value: v
                                };
                            });
                        }
                        return [];
                    },
                    /** Any additional attributes that will be assigned to the control */
                    configAttributes: function () {
                        var attributes = {};
                        return attributes;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue || '';
                        }
                    }
                },
                template: "\n<DropDownList v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" :options=\"options\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=SingleSelect.js.map