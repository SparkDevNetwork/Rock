System.register(["vue", "../Rules/index", "./basicTimePicker", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, basicTimePicker_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (basicTimePicker_1_1) {
                basicTimePicker_1 = basicTimePicker_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "TimePicker",
                components: {
                    RockFormField: rockFormField_1.default,
                    BasicTimePicker: basicTimePicker_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ""
                    },
                    modelValue: {
                        type: Object,
                        default: {}
                    }
                },
                data() {
                    return {
                        internalValue: {}
                    };
                },
                methods: {},
                computed: {
                    computedRules() {
                        const rules = index_1.ruleStringToArray(this.rules);
                        return index_1.ruleArrayToString(rules);
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
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="timepicker-input"
    name="time-picker"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="timepicker-input">
                <BasicTimePicker v-model="internalValue" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=timePicker.js.map