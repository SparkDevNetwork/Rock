System.register(["vue", "../../../Elements/alert", "../../../Elements/checkBox", "../../../Elements/dropDownList", "../../../Elements/numberUpDown", "../../../Elements/numberUpDownGroup", "../../../Services/number", "../../../Util/guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, alert_1, checkBox_1, dropDownList_1, numberUpDown_1, numberUpDownGroup_1, number_1, guid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (numberUpDown_1_1) {
                numberUpDown_1 = numberUpDown_1_1;
            },
            function (numberUpDownGroup_1_1) {
                numberUpDownGroup_1 = numberUpDownGroup_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.FeeField",
                components: {
                    NumberUpDown: numberUpDown_1.default,
                    NumberUpDownGroup: numberUpDownGroup_1.default,
                    DropDownList: dropDownList_1.default,
                    CheckBox: checkBox_1.default,
                    Alert: alert_1.default
                },
                props: {
                    modelValue: {
                        type: Object,
                        required: true
                    },
                    fee: {
                        type: Object,
                        required: true
                    }
                },
                data() {
                    return {
                        dropDownValue: "",
                        checkboxValue: false
                    };
                },
                methods: {
                    getItemLabel(item) {
                        const formattedCost = number_1.default.asFormattedString(item.cost, 2);
                        if (item.countRemaining) {
                            const formattedRemaining = number_1.default.asFormattedString(item.countRemaining, 0);
                            return `${item.name} ($${formattedCost}) (${formattedRemaining} remaining)`;
                        }
                        return `${item.name} ($${formattedCost})`;
                    }
                },
                computed: {
                    label() {
                        if (this.singleItem) {
                            return this.getItemLabel(this.singleItem);
                        }
                        return this.fee.name;
                    },
                    singleItem() {
                        if (this.fee.items.length !== 1) {
                            return null;
                        }
                        return this.fee.items[0];
                    },
                    isHidden() {
                        return !this.fee.items.length;
                    },
                    isCheckbox() {
                        return !!this.singleItem && !this.fee.allowMultiple;
                    },
                    isNumberUpDown() {
                        return !!this.singleItem && this.fee.allowMultiple;
                    },
                    isNumberUpDownGroup() {
                        return this.fee.items.length > 1 && this.fee.allowMultiple;
                    },
                    isDropDown() {
                        return this.fee.items.length > 1 && !this.fee.allowMultiple;
                    },
                    dropDownListOptions() {
                        return this.fee.items.map(i => ({
                            text: this.getItemLabel(i),
                            value: i.guid
                        }));
                    },
                    numberUpDownGroupOptions() {
                        return this.fee.items.map(i => ({
                            key: i.guid,
                            label: this.getItemLabel(i),
                            max: i.countRemaining || 100,
                            min: 0
                        }));
                    },
                    rules() {
                        return this.fee.isRequired ? "required" : "";
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        deep: true,
                        handler() {
                            if (this.isDropDown) {
                                this.dropDownValue = "";
                                for (const item of this.fee.items) {
                                    if (!this.dropDownValue && this.modelValue[item.guid]) {
                                        this.modelValue[item.guid] = 1;
                                        this.dropDownValue = item.guid;
                                    }
                                    else if (this.modelValue[item.guid]) {
                                        this.modelValue[item.guid] = 0;
                                    }
                                }
                            }
                            if (this.isCheckbox && this.singleItem) {
                                this.checkboxValue = !!this.modelValue[this.singleItem.guid];
                                this.modelValue[this.singleItem.guid] = this.checkboxValue ? 1 : 0;
                            }
                        }
                    },
                    fee: {
                        immediate: true,
                        handler() {
                            for (const item of this.fee.items) {
                                this.modelValue[item.guid] = this.modelValue[item.guid] || 0;
                            }
                        }
                    },
                    dropDownValue() {
                        for (const item of this.fee.items) {
                            const isSelected = guid_1.default.areEqual(this.dropDownValue, item.guid);
                            this.modelValue[item.guid] = isSelected ? 1 : 0;
                        }
                    },
                    checkboxValue() {
                        if (this.singleItem) {
                            this.modelValue[this.singleItem.guid] = this.checkboxValue ? 1 : 0;
                        }
                    }
                },
                template: `
<template v-if="!isHidden">
    <CheckBox v-if="isCheckbox" :label="label" v-model="checkboxValue" :inline="false" :rules="rules" />
    <NumberUpDown v-else-if="isNumberUpDown" :label="label" :min="0" :max="singleItem.countRemaining || 100" v-model="modelValue[singleItem.guid]" :rules="rules" />
    <DropDownList v-else-if="isDropDown" :label="label" :options="dropDownListOptions" v-model="dropDownValue" :rules="rules" formControlClasses="input-width-md" />
    <NumberUpDownGroup v-else-if="isNumberUpDownGroup" :label="label" :options="numberUpDownGroupOptions" v-model="modelValue" :rules="rules" />
    <Alert v-else alertType="danger">This fee configuration is not supported</Alert>
</template>`
            }));
        }
    };
});
//# sourceMappingURL=feeField.js.map