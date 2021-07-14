System.register(["vue", "../../../Elements/Alert", "../../../Elements/CheckBox", "../../../Elements/DropDownList", "../../../Elements/NumberUpDown", "../../../Elements/NumberUpDownGroup", "../../../Services/Number", "../../../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Alert_1, CheckBox_1, DropDownList_1, NumberUpDown_1, NumberUpDownGroup_1, Number_1, Guid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (NumberUpDownGroup_1_1) {
                NumberUpDownGroup_1 = NumberUpDownGroup_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.FeeField',
                components: {
                    NumberUpDown: NumberUpDown_1.default,
                    NumberUpDownGroup: NumberUpDownGroup_1.default,
                    DropDownList: DropDownList_1.default,
                    CheckBox: CheckBox_1.default,
                    Alert: Alert_1.default
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
                data: function () {
                    return {
                        dropDownValue: '',
                        checkboxValue: false
                    };
                },
                methods: {
                    getItemLabel: function (item) {
                        var formattedCost = Number_1.default.asFormattedString(item.cost);
                        if (item.countRemaining) {
                            var formattedRemaining = Number_1.default.asFormattedString(item.countRemaining, 0);
                            return item.name + " ($" + formattedCost + ") (" + formattedRemaining + " remaining)";
                        }
                        return item.name + " ($" + formattedCost + ")";
                    }
                },
                computed: {
                    label: function () {
                        if (this.singleItem) {
                            var formattedCost = Number_1.default.asFormattedString(this.singleItem.cost);
                            return this.fee.name + " ($" + formattedCost + ")";
                        }
                        return this.fee.name;
                    },
                    singleItem: function () {
                        if (this.fee.items.length !== 1) {
                            return null;
                        }
                        return this.fee.items[0];
                    },
                    isHidden: function () {
                        return !this.fee.items.length;
                    },
                    isCheckbox: function () {
                        return !!this.singleItem && !this.fee.allowMultiple;
                    },
                    isNumberUpDown: function () {
                        return !!this.singleItem && this.fee.allowMultiple;
                    },
                    isNumberUpDownGroup: function () {
                        return this.fee.items.length > 1 && this.fee.allowMultiple;
                    },
                    isDropDown: function () {
                        return this.fee.items.length > 1 && !this.fee.allowMultiple;
                    },
                    dropDownListOptions: function () {
                        var _this = this;
                        return this.fee.items.map(function (i) { return ({
                            key: i.guid,
                            text: _this.getItemLabel(i),
                            value: i.guid
                        }); });
                    },
                    numberUpDownGroupOptions: function () {
                        var _this = this;
                        return this.fee.items.map(function (i) { return ({
                            key: i.guid,
                            label: _this.getItemLabel(i),
                            max: i.countRemaining || 100,
                            min: 0
                        }); });
                    },
                    rules: function () {
                        return this.fee.isRequired ? 'required' : '';
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        deep: true,
                        handler: function () {
                            if (this.isDropDown) {
                                this.dropDownValue = '';
                                for (var _i = 0, _a = this.fee.items; _i < _a.length; _i++) {
                                    var item = _a[_i];
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
                        handler: function () {
                            for (var _i = 0, _a = this.fee.items; _i < _a.length; _i++) {
                                var item = _a[_i];
                                this.modelValue[item.guid] = this.modelValue[item.guid] || 0;
                            }
                        }
                    },
                    dropDownValue: function () {
                        for (var _i = 0, _a = this.fee.items; _i < _a.length; _i++) {
                            var item = _a[_i];
                            var isSelected = Guid_1.default.areEqual(this.dropDownValue, item.guid);
                            this.modelValue[item.guid] = isSelected ? 1 : 0;
                        }
                    },
                    checkboxValue: function () {
                        if (this.singleItem) {
                            this.modelValue[this.singleItem.guid] = this.checkboxValue ? 1 : 0;
                        }
                    }
                },
                template: "\n<template v-if=\"!isHidden\">\n    <CheckBox v-if=\"isCheckbox\" :label=\"label\" v-model=\"checkboxValue\" :inline=\"false\" :rules=\"rules\" />\n    <NumberUpDown v-else-if=\"isNumberUpDown\" :label=\"label\" :min=\"0\" :max=\"singleItem.countRemaining || 100\" v-model=\"modelValue[singleItem.guid]\" :rules=\"rules\" />\n    <DropDownList v-else-if=\"isDropDown\" :label=\"label\" :options=\"dropDownListOptions\" v-model=\"dropDownValue\" :rules=\"rules\" formControlClasses=\"input-width-md\" />\n    <NumberUpDownGroup v-else-if=\"isNumberUpDownGroup\" :label=\"label\" :options=\"numberUpDownGroupOptions\" v-model=\"modelValue\" :rules=\"rules\" />\n    <Alert v-else alertType=\"danger\">This fee configuration is not supported</Alert>\n</template>"
            }));
        }
    };
});
//# sourceMappingURL=FeeField.js.map