System.register(["vue", "../Rules/index", "../Services/number", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, number_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "NumberRangeBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Object,
                        default: { lower: null, upper: null }
                    },
                    decimalCount: {
                        type: Number,
                        default: null
                    },
                    inputClasses: {
                        type: String,
                        default: ""
                    },
                    rules: {
                        type: String,
                        default: ""
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        internalValue: {
                            lower: "",
                            upper: ""
                        },
                        validationValue: ""
                    };
                },
                methods: {
                    onChange() {
                        var _a, _b;
                        this.internalValue = {
                            lower: number_1.asFormattedString(this.modelValue.lower, (_a = this.internalDecimalCount) !== null && _a !== void 0 ? _a : undefined),
                            upper: number_1.asFormattedString(this.modelValue.upper, (_b = this.internalDecimalCount) !== null && _b !== void 0 ? _b : undefined)
                        };
                    }
                },
                computed: {
                    computedValue() {
                        return {
                            lower: number_1.toNumberOrNull(this.internalValue.lower),
                            upper: number_1.toNumberOrNull(this.internalValue.upper)
                        };
                    },
                    internalDecimalCount() {
                        return this.decimalCount;
                    },
                    internalStep() {
                        return this.internalDecimalCount === null ? "any" : (1 / Math.pow(10, this.internalDecimalCount)).toString();
                    },
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        return index_1.ruleArrayToString(rules);
                    },
                },
                watch: {
                    computedValue() {
                        this.$emit("update:modelValue", this.computedValue);
                    },
                    internalValue() {
                        var _a, _b;
                        const value = `${(_a = this.internalValue.lower) !== null && _a !== void 0 ? _a : ""},${(_b = this.internalValue.upper) !== null && _b !== void 0 ? _b : ""}`;
                        this.validationValue = value;
                        const emitValue = {
                            lower: number_1.toNumberOrNull(this.internalValue.lower),
                            upper: number_1.toNumberOrNull(this.internalValue.upper)
                        };
                        this.$emit("update:modelValue", emitValue);
                    },
                    internalStep() {
                        return this.decimalCount === null ? "any" : (1 / Math.pow(10, this.decimalCount)).toString();
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            const lower = this.modelValue.lower !== null ? this.modelValue.lower.toString() : "";
                            const upper = this.modelValue.upper !== null ? this.modelValue.upper.toString() : "";
                            if (this.internalValue.lower !== lower || this.internalValue.upper !== upper) {
                                this.internalValue = {
                                    lower: lower,
                                    upper: upper
                                };
                            }
                        }
                    }
                },
                template: `
<RockFormField
    v-model="validationValue"
    formGroupClasses="number-range-editor"
    name="number-range-box"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled, tabIndex}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <input
                    :id="uniqueId + '_lower'"
                    @change="onChange"
                    type="number"
                    class="input-width-md form-control"
                    :class="inputClasses"
                    v-model="internalValue.lower"
                    :disabled="disabled"
                    :tabindex="tabIndex"
                    :step="internalStep" />
                <span class="to">to</span>
                <input
                    :id="uniqueId + '_upper'"
                    @change="onChange"
                    type="number"
                    class="input-width-md form-control"
                    :class="inputClasses"
                    v-model="internalValue.upper"
                    :disabled="disabled"
                    :tabindex="tabIndex"
                    :step="internalStep" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=numberRangeBox.js.map