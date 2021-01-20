System.register(["../Vendor/Vue/vue.js", "./AttributeValuesContainer"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, AttributeValuesContainer_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (AttributeValuesContainer_1_1) {
                AttributeValuesContainer_1 = AttributeValuesContainer_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'EntityAttributeValuesContainer',
                components: {
                    AttributeValuesContainer: AttributeValuesContainer_1.default
                },
                props: {
                    entity: {
                        type: Object,
                        required: true
                    },
                    categoryName: {
                        type: String,
                        default: ''
                    }
                },
                computed: {
                    attributeValues: function () {
                        var attributes = this.entity.Attributes || {};
                        var attributeValues = [];
                        for (var key in attributes) {
                            var attributeValue = attributes[key];
                            var attribute = attributeValue.Attribute;
                            if (this.categoryName && !attribute) {
                                continue;
                            }
                            if (this.categoryName && !(attribute === null || attribute === void 0 ? void 0 : attribute.CategoryNames.includes(this.categoryName))) {
                                continue;
                            }
                            attributeValues.push(attributeValue);
                        }
                        attributeValues.sort(function (a, b) { var _a, _b; return (((_a = a.Attribute) === null || _a === void 0 ? void 0 : _a.Order) || 0) - (((_b = b.Attribute) === null || _b === void 0 ? void 0 : _b.Order) || 0); });
                        return attributeValues;
                    }
                },
                template: "\n<AttributeValuesContainer :attributeValues=\"attributeValues\">"
            }));
        }
    };
});
//# sourceMappingURL=EntityAttributeValuesContainer.js.map