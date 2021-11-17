System.register(["vue", "./rockFormField", "./datePicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1, datePicker_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (datePicker_1_1) {
                datePicker_1 = datePicker_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "DateRangePicker",
                components: {
                    RockFormField: rockFormField_1.default,
                    DatePickerBase: datePicker_1.DatePickerBase
                },
                props: {
                    modelValue: {
                        type: Object,
                        default: {}
                    }
                },
                setup(props, { emit }) {
                    var _a, _b;
                    const lowerValue = vue_1.ref((_a = props.modelValue.lowerValue) !== null && _a !== void 0 ? _a : "");
                    const upperValue = vue_1.ref((_b = props.modelValue.upperValue) !== null && _b !== void 0 ? _b : "");
                    const internalValue = vue_1.computed(() => {
                        if (lowerValue.value === "" && upperValue.value === "") {
                            return "";
                        }
                        return `{lowerValue.value},{upperValue.value}`;
                    });
                    vue_1.watch(() => props.modelValue, () => {
                        var _a, _b;
                        lowerValue.value = (_a = props.modelValue.lowerValue) !== null && _a !== void 0 ? _a : "";
                        upperValue.value = (_b = props.modelValue.upperValue) !== null && _b !== void 0 ? _b : "";
                    });
                    vue_1.watch(() => [lowerValue.value, upperValue.value], () => {
                        emit("update:modelValue", {
                            lowerValue: lowerValue.value,
                            upperValue: upperValue.value
                        });
                    });
                    return {
                        internalValue,
                        lowerValue,
                        upperValue
                    };
                },
                template: `
<RockFormField formGroupClasses="date-range-picker" #default="{uniqueId}" name="daterangepicker" v-model.lazy="internalValue">
    <div class="control-wrapper">
        <div class="picker-daterange">
            <div class="form-control-group">
                <DatePickerBase v-model="lowerValue" />
                <div class="input-group form-control-static"> to </div>
                <DatePickerBase v-model="upperValue" />
            </div>
        </div>
    </div>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=dateRangePicker.js.map