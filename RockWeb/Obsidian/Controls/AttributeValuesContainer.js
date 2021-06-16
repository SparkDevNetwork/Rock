System.register(["../Services/String", "vue", "./RockField"], function (exports_1, context_1) {
    "use strict";
    var String_1, vue_1, RockField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockField_1_1) {
                RockField_1 = RockField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'AttributeValuesContainer',
                components: {
                    RockField: RockField_1.default
                },
                props: {
                    isEditMode: {
                        type: Boolean,
                        default: false
                    },
                    attributeValues: {
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
                    getAttributeLabel: function (attributeValue) {
                        var _a, _b;
                        if (this.showAbbreviatedName && ((_a = attributeValue.Attribute) === null || _a === void 0 ? void 0 : _a.AbbreviatedName)) {
                            return attributeValue.Attribute.AbbreviatedName;
                        }
                        return ((_b = attributeValue.Attribute) === null || _b === void 0 ? void 0 : _b.Name) || '';
                    }
                },
                computed: {
                    validAttributeValues: function () {
                        return this.attributeValues.filter(function (av) { return av.Attribute; });
                    },
                    valuesToShow: function () {
                        if (this.showEmptyValues) {
                            return this.validAttributeValues;
                        }
                        return this.validAttributeValues.filter(function (av) { return !String_1.isNullOrWhitespace(av.Value); });
                    }
                },
                template: "\n<div v-if=\"!isEditMode\" v-for=\"a in valuesToShow\" class=\"form-group static-control\">\n    <template v-if=\"a.Value\">\n        <label class=\"control-label\">\n            {{ getAttributeLabel(a) }}\n        </label>\n        <div class=\"control-wrapper\">\n            <div class=\"form-control-static\">\n                <RockField :fieldTypeGuid=\"a.Attribute.FieldTypeGuid\" v-model=\"a.Value\" :configurationValues=\"a.Attribute.QualifierValues\" />\n            </div>\n        </div>\n    </template>\n</div>\n<template v-else>\n    <template v-for=\"a in validAttributeValues\">\n        <RockField\n            isEditMode\n            :fieldTypeGuid=\"a.Attribute.FieldTypeGuid\"\n            v-model=\"a.Value\"\n            :label=\"getAttributeLabel(a)\"\n            :help=\"a.Attribute.Description\"\n            :rules=\"a.Attribute.IsRequired ? 'required' : ''\"\n            :configurationValues=\"a.Attribute.QualifierValues\"  />\n    </template>\n</template>"
            }));
        }
    };
});
//# sourceMappingURL=AttributeValuesContainer.js.map