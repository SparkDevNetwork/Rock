System.register(["vue", "./Index", "../Controls/DefinedValuePicker", "../Services/Number"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DefinedValuePicker_1, Number_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DefinedValuePicker_1_1) {
                DefinedValuePicker_1 = DefinedValuePicker_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '59D5A94C-94A0-4630-B80A-BB25697D74C7';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["DefinedType"] = "definedtype";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'DefinedValueField',
                components: {
                    DefinedValuePicker: DefinedValuePicker_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    safeValue: function () {
                        return (this.modelValue || '').trim();
                    },
                    configAttributes: function () {
                        var attributes = {};
                        var definedTypeConfig = this.configurationValues[ConfigurationValueKey.DefinedType];
                        if (definedTypeConfig && definedTypeConfig.Value) {
                            var definedTypeId = Number_1.toNumberOrNull(definedTypeConfig.Value);
                            if (definedTypeId) {
                                var definedType = this.$store.getters['definedTypes/getById'](definedTypeId);
                                attributes.definedTypeGuid = (definedType === null || definedType === void 0 ? void 0 : definedType.Guid) || '';
                            }
                        }
                        return attributes;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<DefinedValuePicker v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=DefinedValueField.js.map