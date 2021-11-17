System.register(["vue", "../../../Controls/rockField", "../../../Elements/alert"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockField_1, alert_1;
    var __moduleName = context_1 && context_1.id;
    function isRuleMet(rule, fieldValues) {
        const value = fieldValues[rule.comparedToRegistrationTemplateFormFieldGuid] || "";
        if (typeof value !== "string") {
            return false;
        }
        const strVal = value.toLowerCase().trim();
        const comparison = rule.comparedToValue.toLowerCase().trim();
        if (!strVal) {
            return false;
        }
        switch (rule.comparisonType) {
            case 1:
                return strVal === comparison;
            case 2:
                return strVal !== comparison;
            case 8:
                return strVal.includes(comparison);
            case 16:
                return !strVal.includes(comparison);
        }
        return false;
    }
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockField_1_1) {
                rockField_1 = rockField_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.RegistrantAttributeField",
                components: {
                    Alert: alert_1.default,
                    RockField: rockField_1.default
                },
                props: {
                    field: {
                        type: Object,
                        required: true
                    },
                    fieldValues: {
                        type: Object,
                        required: true
                    }
                },
                setup(props) {
                    var _a, _b, _c;
                    const isVisible = vue_1.computed(() => {
                        switch (props.field.visibilityRuleType) {
                            case 1:
                                return props.field.visibilityRules.every(vr => isRuleMet(vr, props.fieldValues));
                            case 3:
                                return props.field.visibilityRules.every(vr => !isRuleMet(vr, props.fieldValues));
                            case 2:
                                return props.field.visibilityRules.some(vr => isRuleMet(vr, props.fieldValues));
                            case 4:
                                return props.field.visibilityRules.some(vr => !isRuleMet(vr, props.fieldValues));
                        }
                        return true;
                    });
                    const attribute = vue_1.reactive(Object.assign(Object.assign({}, props.field.attribute), { value: (_c = (_a = props.fieldValues[props.field.guid]) !== null && _a !== void 0 ? _a : (_b = props.field.attribute) === null || _b === void 0 ? void 0 : _b.value) !== null && _c !== void 0 ? _c : "" }));
                    vue_1.watch(() => props.fieldValues[props.field.guid], (value) => {
                        attribute.value = value;
                    });
                    vue_1.watch(() => attribute.value, (value) => {
                        props.fieldValues[props.field.guid] = value;
                    });
                    return {
                        isVisible,
                        attribute
                    };
                },
                template: `
<template v-if="isVisible">
    <RockField v-if="attribute" isEditMode :attributeValue="attribute" />
    <Alert v-else alertType="danger">Could not resolve attribute field</Alert>
</template>`
            }));
        }
    };
});
//# sourceMappingURL=registrantAttributeField.js.map