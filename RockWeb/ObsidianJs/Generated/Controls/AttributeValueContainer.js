System.register(["../Filters/String.js", "../Vendor/Vue/vue.js", "./RockField.js"], function (exports_1, context_1) {
    "use strict";
    var String_js_1, vue_js_1, RockField_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (String_js_1_1) {
                String_js_1 = String_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (RockField_js_1_1) {
                RockField_js_1 = RockField_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'AttributeValueContainer',
                components: {
                    RockField: RockField_js_1.default
                },
                props: {
                    isEditMode: {
                        type: Boolean,
                        default: false
                    },
                    attributeWithValues: {
                        type: Array,
                        required: true
                    },
                    showEmptyValues: {
                        type: Boolean,
                        default: true
                    },
                    showAbbreviatedName: {
                        type: Boolean,
                        default: false
                    }
                },
                methods: {
                    getAttributeLabel: function (attributeWithValue) {
                        if (this.showAbbreviatedName) {
                            return attributeWithValue.Attribute.AbbreviatedName || attributeWithValue.Attribute.Name;
                        }
                        return attributeWithValue.Attribute.Name;
                    }
                },
                computed: {
                    valuesToShow: function () {
                        if (this.showEmptyValues) {
                            return this.attributeWithValues;
                        }
                        return this.attributeWithValues.filter(function (av) { return !String_js_1.isNullOrWhitespace(av.Value); });
                    }
                },
                template: "\n<div v-if=\"!isEditMode\" v-for=\"a in valuesToShow\" class=\"form-group static-control\">\n    <label class=\"control-label\">\n        {{ getAttributeLabel(a) }}\n    </label>\n    <div class=\"control-wrapper\">\n        <div class=\"form-control-static\">\n            <RockField :fieldTypeGuid=\"a.AttributeFieldTypeGuid\" v-model=\"a.Value\" />\n        </div>\n    </div>\n</div>\n<template v-else>\n    <template v-for=\"a in attributeValues\">\n        <RockField\n            isEditMode\n            :fieldTypeGuid=\"a.Attribute.FieldTypeGuid\"\n            v-model=\"a.Value\"\n            :label=\"getAttributeLabel(a)\"\n            :help=\"a.Attribute.Description\"\n            :rules=\"a.Attribute.IsRequired ? 'required' : ''\"\n            :configurationValues=\"a.Attribute.QualifierValues\"  />\n    </template>\n</template>"
            }));
        }
    };
});
//# sourceMappingURL=AttributeValueContainer.js.map