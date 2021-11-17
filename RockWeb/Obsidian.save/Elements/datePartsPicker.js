System.register(["vue", "../Rules/index", "../Services/dateKey", "../Services/number", "./rockFormField", "../Util/rockDateTime"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, dateKey_1, number_1, rockFormField_1, rockDateTime_1;
    var __moduleName = context_1 && context_1.id;
    function getDefaultDatePartsPickerModel() {
        return {
            year: 0,
            month: 0,
            day: 0
        };
    }
    exports_1("getDefaultDatePartsPickerModel", getDefaultDatePartsPickerModel);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (dateKey_1_1) {
                dateKey_1 = dateKey_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "DatePartsPicker",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ""
                    },
                    modelValue: {
                        type: Object,
                        required: true
                    },
                    requireYear: {
                        type: Boolean,
                        default: true
                    },
                    showYear: {
                        type: Boolean,
                        default: true
                    },
                    allowFutureDates: {
                        type: Boolean,
                        default: true
                    },
                    futureYearCount: {
                        type: Number,
                        default: 50
                    },
                    startYear: {
                        type: Number,
                        default: 1900
                    }
                },
                data() {
                    return {
                        internalDay: "0",
                        internalMonth: "0",
                        internalYear: "0",
                        days: []
                    };
                },
                methods: {
                    getValue() {
                        return {
                            day: number_1.toNumber(this.internalDay),
                            month: number_1.toNumber(this.internalMonth),
                            year: number_1.toNumber(this.internalYear)
                        };
                    },
                    updateDays() {
                        var _a, _b, _c, _d;
                        let dayCount = 31;
                        const year = number_1.toNumber(this.internalYear);
                        const month = number_1.toNumber(this.internalMonth);
                        if (this.showYear && year > 0 && month > 0) {
                            dayCount = (_d = (_c = (_b = (_a = rockDateTime_1.RockDateTime.fromParts(year, month, 1)) === null || _a === void 0 ? void 0 : _a.addMonths(1)) === null || _b === void 0 ? void 0 : _b.addDays(-1)) === null || _c === void 0 ? void 0 : _c.day) !== null && _d !== void 0 ? _d : 31;
                        }
                        else if ([1, 3, 5, 7, 8, 10, 12].indexOf(month) !== -1) {
                            dayCount = 31;
                        }
                        else if ([4, 6, 9, 11].indexOf(month) !== -1) {
                            dayCount = 30;
                        }
                        else if (month === 2) {
                            dayCount = 29;
                        }
                        const days = [];
                        for (let day = 1; day <= dayCount; day++) {
                            days.push(day.toString());
                        }
                        this.days = days;
                    }
                },
                computed: {
                    computedRequireYear() {
                        return this.showYear && this.requireYear;
                    },
                    internalDateKey() {
                        if (!this.modelValue.year && !this.computedRequireYear) {
                            const dateKey = dateKey_1.default.toNoYearDateKey(this.modelValue.month, this.modelValue.day);
                            return dateKey;
                        }
                        const dateKey = dateKey_1.default.toDateKey(this.modelValue.year, this.modelValue.month, this.modelValue.day);
                        return dateKey;
                    },
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        if (rules.indexOf("required") !== -1 && rules.indexOf("datekey") === -1) {
                            rules.push("datekey");
                        }
                        return index_1.ruleArrayToString(rules);
                    },
                    years() {
                        const years = [];
                        let year = rockDateTime_1.RockDateTime.now().year;
                        if (this.futureYearCount > 0 && this.allowFutureDates) {
                            year += this.futureYearCount;
                        }
                        while (year >= 1900) {
                            years.push(year.toString());
                            year--;
                        }
                        return years;
                    },
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalDay = this.modelValue.day.toString();
                            this.internalMonth = this.modelValue.month.toString();
                            this.internalYear = this.modelValue.year.toString();
                            this.updateDays();
                        }
                    },
                    showYear: {
                        immediate: true,
                        handler() {
                            this.updateDays();
                        }
                    },
                    internalDay() {
                        this.$emit("update:modelValue", this.getValue());
                    },
                    internalMonth() {
                        const day = number_1.toNumberOrNull(this.internalDay);
                        this.updateDays();
                        if (day != null && day >= this.days.length + 1) {
                            this.internalDay = this.days.length.toString();
                        }
                        else {
                            this.$emit("update:modelValue", this.getValue());
                        }
                    },
                    internalYear() {
                        const day = number_1.toNumberOrNull(this.internalDay);
                        this.updateDays();
                        if (day != null && day >= this.days.length + 1) {
                            this.internalDay = this.days.length.toString();
                        }
                        else {
                            this.$emit("update:modelValue", this.getValue());
                        }
                    },
                },
                template: `
<RockFormField
    :modelValue="internalDateKey"
    formGroupClasses="birthday-picker"
    name="birthday"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <select :id="uniqueId + '-month'" class="form-control input-width-sm" :disabled="disabled" v-model="internalMonth">
                    <option value="0"></option>
                    <option value="1">Jan</option>
                    <option value="2">Feb</option>
                    <option value="3">Mar</option>
                    <option value="4">Apr</option>
                    <option value="5">May</option>
                    <option value="6">Jun</option>
                    <option value="7">Jul</option>
                    <option value="8">Aug</option>
                    <option value="9">Sep</option>
                    <option value="10">Oct</option>
                    <option value="11">Nov</option>
                    <option value="12">Dec</option>
                </select>
                <span class="separator">/</span>
                <select :id="uniqueId + '-day'" class="form-control input-width-sm" v-model="internalDay">
                    <option value="0"></option>
                    <option v-for="day in days" :key="day" :value="day">{{day}}</option>
                </select>
                <span v-if="showYear" class="separator">/</span>
                <select v-if="showYear" :id="uniqueId + '-year'" class="form-control input-width-sm" v-model="internalYear">
                    <option value="0"></option>
                    <option v-for="year in years" :value="year">{{year}}</option>
                </select>
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=datePartsPicker.js.map