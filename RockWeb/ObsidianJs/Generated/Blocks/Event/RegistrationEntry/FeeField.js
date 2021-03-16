// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
                        var formattedCost = Number_1.default.asFormattedString(item.Cost);
                        if (item.CountRemaining) {
                            var formattedRemaining = Number_1.default.asFormattedString(item.CountRemaining, 0);
                            return item.Name + " ($" + formattedCost + ") (" + formattedRemaining + " remaining)";
                        }
                        return item.Name + " ($" + formattedCost + ")";
                    }
                },
                computed: {
                    label: function () {
                        if (this.singleItem) {
                            var formattedCost = Number_1.default.asFormattedString(this.singleItem.Cost);
                            return this.fee.Name + " ($" + formattedCost + ")";
                        }
                        return this.fee.Name;
                    },
                    singleItem: function () {
                        if (this.fee.Items.length !== 1) {
                            return null;
                        }
                        return this.fee.Items[0];
                    },
                    isHidden: function () {
                        return !this.fee.Items.length;
                    },
                    isCheckbox: function () {
                        return !!this.singleItem && !this.fee.AllowMultiple;
                    },
                    isNumberUpDown: function () {
                        return !!this.singleItem && this.fee.AllowMultiple;
                    },
                    isNumberUpDownGroup: function () {
                        return this.fee.Items.length > 1 && this.fee.AllowMultiple;
                    },
                    isDropDown: function () {
                        return this.fee.Items.length > 1 && !this.fee.AllowMultiple;
                    },
                    dropDownListOptions: function () {
                        var _this = this;
                        return this.fee.Items.map(function (i) { return ({
                            key: i.Guid,
                            text: _this.getItemLabel(i),
                            value: i.Guid
                        }); });
                    },
                    numberUpDownGroupOptions: function () {
                        var _this = this;
                        return this.fee.Items.map(function (i) { return ({
                            key: i.Guid,
                            label: _this.getItemLabel(i),
                            max: i.CountRemaining || 100,
                            min: 0
                        }); });
                    },
                    rules: function () {
                        return this.fee.IsRequired ? 'required' : '';
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        deep: true,
                        handler: function () {
                            // Set the drop down selecton
                            if (this.isDropDown) {
                                this.dropDownValue = '';
                                for (var _i = 0, _a = this.fee.Items; _i < _a.length; _i++) {
                                    var item = _a[_i];
                                    if (!this.dropDownValue && this.modelValue[item.Guid]) {
                                        // Pick the first option that has a qty
                                        this.modelValue[item.Guid] = 1;
                                        this.dropDownValue = item.Guid;
                                    }
                                    else if (this.modelValue[item.Guid]) {
                                        // Any other quantities need to be zeroed since only one can be picked
                                        this.modelValue[item.Guid] = 0;
                                    }
                                }
                            }
                            // Set the checkbox selection
                            if (this.isCheckbox && this.singleItem) {
                                this.checkboxValue = !!this.modelValue[this.singleItem.Guid];
                                this.modelValue[this.singleItem.Guid] = this.checkboxValue ? 1 : 0;
                            }
                        }
                    },
                    fee: {
                        immediate: true,
                        handler: function () {
                            for (var _i = 0, _a = this.fee.Items; _i < _a.length; _i++) {
                                var item = _a[_i];
                                this.modelValue[item.Guid] = this.modelValue[item.Guid] || 0;
                            }
                        }
                    },
                    dropDownValue: function () {
                        // Drop down implies a quantity of 1. Zero out all items except for the one selected.
                        for (var _i = 0, _a = this.fee.Items; _i < _a.length; _i++) {
                            var item = _a[_i];
                            var isSelected = Guid_1.default.areEqual(this.dropDownValue, item.Guid);
                            this.modelValue[item.Guid] = isSelected ? 1 : 0;
                        }
                    },
                    checkboxValue: function () {
                        if (this.singleItem) {
                            // Drop down implies a quantity of 1.
                            this.modelValue[this.singleItem.Guid] = this.checkboxValue ? 1 : 0;
                        }
                    }
                },
                template: "\n<template v-if=\"!isHidden\">\n    <CheckBox v-if=\"isCheckbox\" :label=\"label\" v-model=\"checkboxValue\" :inline=\"false\" :rules=\"rules\" />\n    <NumberUpDown v-else-if=\"isNumberUpDown\" :label=\"label\" :min=\"0\" :max=\"singleItem.CountRemaining || 100\" v-model=\"modelValue[singleItem.Guid]\" :rules=\"rules\" />\n    <DropDownList v-else-if=\"isDropDown\" :label=\"label\" :options=\"dropDownListOptions\" v-model=\"dropDownValue\" :rules=\"rules\" formControlClasses=\"input-width-md\" />\n    <NumberUpDownGroup v-else-if=\"isNumberUpDownGroup\" :label=\"label\" :options=\"numberUpDownGroupOptions\" v-model=\"modelValue\" :rules=\"rules\" />\n    <Alert v-else alertType=\"danger\">This fee configuration is not supported</Alert>\n</template>"
            }));
        }
    };
});
//# sourceMappingURL=FeeField.js.map