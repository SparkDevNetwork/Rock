System.register(["vue", "./numberBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, numberBox_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (numberBox_1_1) {
                numberBox_1 = numberBox_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "CurrencyBox",
                components: {
                    NumberBox: numberBox_1.default
                },
                props: {
                    modelValue: {
                        type: Number,
                        default: null
                    },
                    minimumValue: {
                        type: Number
                    },
                    maximumValue: {
                        type: Number
                    },
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        internalValue: null
                    };
                },
                computed: {
                    placeholder() {
                        return "0.00";
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            if (this.modelValue !== this.internalValue) {
                                this.internalValue = this.modelValue;
                            }
                        }
                    }
                },
                template: `
<NumberBox v-model="internalValue"
    :placeholder="placeholder"
    :minimum-value="minimumValue"
    :maximum-value="maximumValue"
    :decimal-count="2"
    rules="decimal">
    <template v-slot:prepend>
        <span class="input-group-addon">$</span>
    </template>
</NumberBox>
`
            }));
        }
    };
});
//# sourceMappingURL=currencyBox.js.map