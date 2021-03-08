System.register(["vue", "./Index.js", "../Controls/DefinedValuePicker.js", "../Services/Number.js"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_js_1, DefinedValuePicker_js_1, Number_js_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (DefinedValuePicker_js_1_1) {
                DefinedValuePicker_js_1 = DefinedValuePicker_js_1_1;
            },
            function (Number_js_1_1) {
                Number_js_1 = Number_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '59D5A94C-94A0-4630-B80A-BB25697D74C7';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["DefinedType"] = "definedtype";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'DefinedValueField',
                components: {
                    DefinedValuePicker: DefinedValuePicker_js_1.default
                },
                props: Index_js_1.getFieldTypeProps(),
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
                            var definedTypeId = Number_js_1.toNumberOrNull(definedTypeConfig.Value);
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