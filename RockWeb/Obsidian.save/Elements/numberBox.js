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
                name: "NumberBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Number,
                        default: null
                    },
                    placeholder: {
                        type: String,
                        default: ""
                    },
                    minimumValue: {
                        type: Number
                    },
                    maximumValue: {
                        type: Number
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
                        internalValue: "",
                    };
                },
                methods: {
                    onChange() {
                        var _a;
                        this.internalValue = number_1.asFormattedString(this.modelValue, (_a = this.internalDecimalCount) !== null && _a !== void 0 ? _a : undefined);
                    }
                },
                computed: {
                    internalNumberValue() {
                        return number_1.toNumberOrNull(this.internalValue);
                    },
                    internalDecimalCount() {
                        return this.decimalCount;
                    },
                    internalStep() {
                        return this.internalDecimalCount === null ? "any" : (1 / Math.pow(10, this.internalDecimalCount)).toString();
                    },
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        if (this.maximumValue !== null && this.maximumValue !== undefined) {
                            rules.push(`lte:${this.maximumValue}`);
                        }
                        if (this.minimumValue !== null && this.minimumValue !== undefined) {
                            rules.push(`gte:${this.minimumValue}`);
                        }
                        return index_1.ruleArrayToString(rules);
                    },
                    isGrouped() {
                        return this.$slots.prepend !== undefined || this.$slots.append !== undefined;
                    }
                },
                watch: {
                    internalNumberValue() {
                        this.$emit("update:modelValue", this.internalNumberValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            var _a;
                            if (this.modelValue !== this.internalNumberValue) {
                                this.internalValue = number_1.asFormattedString(this.modelValue, (_a = this.internalDecimalCount) !== null && _a !== void 0 ? _a : undefined);
                            }
                        }
                    }
                },
                template: `
<RockFormField
    v-model="internalValue"
    @change="onChange"
    formGroupClasses="rock-number-box"
    name="numberbox"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled, tabIndex}">
        <div class="control-wrapper">
            <div class="input-group" v-if="isGrouped">
                <slot name="prepend"></slot>
                <input
                    :id="uniqueId"
                    type="number"
                    class="form-control"
                    :class="inputClasses"
                    v-bind="field"
                    :disabled="disabled"
                    :placeholder="placeholder"
                    :tabindex="tabIndex"
                    :step="internalStep"
                    :min="minimumValue"
                    :max="maximumValue" />
                <slot name="append"></slot>
            </div>

            <input v-else
                :id="uniqueId"
                type="number"
                class="form-control"
                :class="inputClasses"
                v-bind="field"
                :disabled="disabled"
                :placeholder="placeholder"
                :tabindex="tabIndex"
                :step="internalStep"
                :min="minimumValue"
                :max="maximumValue" />
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=numberBox.js.map