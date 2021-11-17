System.register(["vue", "./utils", "../Elements/datePicker", "../Services/boolean", "../Services/number", "../Elements/datePartsPicker", "../Util/rockDateTime"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, utils_1, datePicker_1, boolean_1, number_1, datePartsPicker_1, rockDateTime_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (datePicker_1_1) {
                datePicker_1 = datePicker_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (datePartsPicker_1_1) {
                datePartsPicker_1 = datePartsPicker_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DateField.Edit",
                components: {
                    DatePicker: datePicker_1.default,
                    DatePartsPicker: datePartsPicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: "",
                        internalDateParts: datePartsPicker_1.getDefaultDatePartsPickerModel(),
                        formattedString: ""
                    };
                },
                setup() {
                    return {};
                },
                computed: {
                    datePartsAsDate() {
                        var _a;
                        if (!((_a = this.internalDateParts) === null || _a === void 0 ? void 0 : _a.day) || !this.internalDateParts.month || !this.internalDateParts.year) {
                            return null;
                        }
                        return rockDateTime_1.RockDateTime.fromParts(this.internalDateParts.year, this.internalDateParts.month, this.internalDateParts.day) || null;
                    },
                    isDatePartsPicker() {
                        const config = this.configurationValues["datePickerControlType"];
                        return (config === null || config === void 0 ? void 0 : config.toLowerCase()) === "date parts picker";
                    },
                    configAttributes() {
                        const attributes = {};
                        const displayCurrentConfig = this.configurationValues["displayCurrentOption"];
                        const displayCurrent = boolean_1.asBoolean(displayCurrentConfig);
                        attributes.displayCurrentOption = displayCurrent;
                        attributes.isCurrentDateOffset = displayCurrent;
                        const futureYearConfig = this.configurationValues["futureYearCount"];
                        const futureYears = number_1.toNumber(futureYearConfig);
                        if (futureYears > 0) {
                            attributes.futureYearCount = futureYears;
                        }
                        return attributes;
                    }
                },
                methods: {
                    syncModelValue() {
                        var _a, _b;
                        this.internalValue = (_a = this.modelValue) !== null && _a !== void 0 ? _a : "";
                        const dateParts = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec((_b = this.modelValue) !== null && _b !== void 0 ? _b : "");
                        if (dateParts != null) {
                            this.internalDateParts.year = number_1.toNumber(dateParts[1]);
                            this.internalDateParts.month = number_1.toNumber(dateParts[2]);
                            this.internalDateParts.day = number_1.toNumber(dateParts[3]);
                        }
                        else {
                            this.internalDateParts.year = 0;
                            this.internalDateParts.month = 0;
                            this.internalDateParts.day = 0;
                        }
                    }
                },
                watch: {
                    datePartsAsDate() {
                        var _a;
                        if (this.isDatePartsPicker) {
                            const d1 = this.datePartsAsDate;
                            const d2 = rockDateTime_1.RockDateTime.parseISO((_a = this.modelValue) !== null && _a !== void 0 ? _a : "");
                            if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                                this.$emit("update:modelValue", d1 !== null ? d1.toISOString().split("T")[0] : "");
                            }
                        }
                    },
                    internalValue() {
                        var _a;
                        if (!this.isDatePartsPicker) {
                            const d1 = rockDateTime_1.RockDateTime.parseISO(this.internalValue);
                            const d2 = rockDateTime_1.RockDateTime.parseISO((_a = this.modelValue) !== null && _a !== void 0 ? _a : "");
                            if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                                this.$emit("update:modelValue", this.internalValue);
                            }
                        }
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            return __awaiter(this, void 0, void 0, function* () {
                                yield this.syncModelValue();
                            });
                        }
                    }
                },
                template: `
<DatePartsPicker v-if="isDatePartsPicker" v-model="internalDateParts" v-bind="configAttributes" />
<DatePicker v-else v-model="internalValue" v-bind="configAttributes" />
`
            }));
        }
    };
});
//# sourceMappingURL=dateFieldComponents.js.map