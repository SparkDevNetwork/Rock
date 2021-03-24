System.register(["vue", "./ComponentFromUrl"], function (exports_1, context_1) {
    "use strict";
    var vue_1, ComponentFromUrl_1, ValidationField;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (ComponentFromUrl_1_1) {
                ComponentFromUrl_1 = ComponentFromUrl_1_1;
            }
        ],
        execute: function () {
            (function (ValidationField) {
                ValidationField[ValidationField["CardNumber"] = 0] = "CardNumber";
                ValidationField[ValidationField["Expiry"] = 1] = "Expiry";
                ValidationField[ValidationField["SecurityCode"] = 2] = "SecurityCode";
            })(ValidationField || (ValidationField = {}));
            exports_1("ValidationField", ValidationField);
            exports_1("default", vue_1.defineComponent({
                name: 'GatewayControl',
                components: {
                    ComponentFromUrl: ComponentFromUrl_1.default
                },
                props: {
                    gatewayControlModel: {
                        type: Object,
                        required: true
                    }
                },
                computed: {
                    url: function () {
                        return this.gatewayControlModel.FileUrl;
                    },
                    settings: function () {
                        return this.gatewayControlModel.Settings;
                    }
                },
                methods: {
                    /**
                     * This method transforms the enum values into human friendly validation messages.
                     * @param validationFields
                     */
                    transformValidation: function (validationFields) {
                        var errors = {};
                        var foundError = false;
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.CardNumber)) {
                            errors['Card Number'] = 'is not valid.';
                            foundError = true;
                        }
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.Expiry)) {
                            errors['Expiration Date'] = 'is not valid.';
                            foundError = true;
                        }
                        if (validationFields === null || validationFields === void 0 ? void 0 : validationFields.includes(ValidationField.SecurityCode)) {
                            errors['Security Code'] = 'is not valid.';
                            foundError = true;
                        }
                        if (!foundError) {
                            errors['Payment Info'] = 'is not valid.';
                        }
                        this.$emit('validation', errors);
                        return;
                    }
                },
                template: "\n<ComponentFromUrl :url=\"url\" :settings=\"settings\" @validationRaw=\"transformValidation\" />"
            }));
        }
    };
});
//# sourceMappingURL=GatewayControl.js.map