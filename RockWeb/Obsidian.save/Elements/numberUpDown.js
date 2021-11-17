System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1, NumberUpDownInternal;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("NumberUpDownInternal", NumberUpDownInternal = vue_1.defineComponent({
                name: "NumberUpDownInternal",
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    },
                    min: {
                        type: Number,
                        default: 1
                    },
                    max: {
                        type: Number,
                        default: 10
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                data() {
                    return {
                        internalValue: 0
                    };
                },
                methods: {
                    goUp() {
                        if (!this.isUpDisabled) {
                            this.internalValue++;
                        }
                    },
                    goDown() {
                        if (!this.isDownDisabled) {
                            this.internalValue--;
                        }
                    }
                },
                computed: {
                    isUpDisabled() {
                        return this.internalValue >= this.max;
                    },
                    isDownDisabled() {
                        return this.internalValue <= this.min;
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue;
                        }
                    },
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    }
                },
                template: `
<div class="numberincrement">
    <a @click="goDown" class="numberincrement-down" :class="{disabled: disabled || isDownDisabled}" :disabled="disabled || isDownDisabled">
        <i class="fa fa-minus "></i>
    </a>
    <span class="numberincrement-value">{{modelValue}}</span>
    <a @click="goUp" class="numberincrement-up" :class="{disabled: disabled || isUpDisabled}" :disabled="disabled || isUpDisabled">
        <i class="fa fa-plus "></i>
    </a>
</div>`
            }));
            exports_1("default", vue_1.defineComponent({
                name: "NumberUpDown",
                components: {
                    RockFormField: rockFormField_1.default,
                    NumberUpDownInternal
                },
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    },
                    min: {
                        type: Number,
                        default: 1
                    },
                    max: {
                        type: Number,
                        default: 10
                    },
                    numberIncrementClasses: {
                        type: String,
                        default: ""
                    }
                },
                data() {
                    return {
                        internalValue: 0
                    };
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue;
                        }
                    },
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    }
                },
                methods: {
                    additionalClasses(fieldLabel) {
                        if (fieldLabel !== "") {
                            return `margin-t-sm ${this.numberIncrementClasses}`;
                        }
                        else {
                            return this.numberIncrementClasses;
                        }
                    }
                },
                template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="number-up-down"
    name="numberupdown">
    <template #default="{uniqueId, field, errors, disabled, fieldLabel}">
        <div class="control-wrapper">
            <NumberUpDownInternal v-model="internalValue" :min="min" :max="max" :class="additionalClasses(fieldLabel)" />
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=numberUpDown.js.map