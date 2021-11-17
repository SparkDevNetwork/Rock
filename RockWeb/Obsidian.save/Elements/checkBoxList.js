System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1;
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
            exports_1("default", vue_1.defineComponent({
                name: "CheckBoxList",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Array,
                        default: []
                    },
                    options: {
                        type: Array,
                        required: true
                    },
                    repeatColumns: {
                        type: Number,
                        default: 0
                    },
                    horizontal: {
                        type: Boolean,
                        default: false
                    }
                },
                setup(props, { emit }) {
                    const internalValue = vue_1.ref([...props.modelValue]);
                    vue_1.watch(() => props.modelValue, () => internalValue.value = props.modelValue);
                    vue_1.watchEffect(() => emit("update:modelValue", internalValue.value));
                    const valueForOption = (option) => option.value;
                    const textForOption = (option) => option.text;
                    const uniqueIdForOption = (uniqueId, option) => `${uniqueId}-${option.value.replace(" ", "-")}`;
                    const containerClasses = vue_1.computed(() => {
                        const classes = [];
                        if (props.horizontal) {
                            classes.push("rockcheckboxlist-horizontal");
                            if (props.repeatColumns > 0) {
                                classes.push(`in-columns in-columns-${props.repeatColumns}`);
                            }
                        }
                        else {
                            classes.push("rockcheckboxlist-vertical");
                        }
                        return classes.join(" ");
                    });
                    return {
                        containerClasses,
                        internalValue,
                        textForOption,
                        uniqueIdForOption,
                        valueForOption
                    };
                },
                template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="check-box-list"
    name="check-box-list">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
            <div class="controls rockcheckboxlist" :class="containerClasses">
                <template v-if="horizontal">
                    <label v-for="option in options" class="checkbox-inline" :for="uniqueIdForOption(uniqueId, option)">
                        <input :id="uniqueIdForOption(uniqueId, option)" :name="uniqueId" type="checkbox" :value="valueForOption(option)" v-model="internalValue" />
                        <span class="label-text">{{textForOption(option)}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="option in options" class="checkbox">
                        <label :for="uniqueIdForOption(uniqueId, option)">
                            <input :id="uniqueIdForOption(uniqueId, option)" :name="uniqueId" type="checkbox" :value="valueForOption(option)" v-model="internalValue" />
                            <span class="label-text">{{textForOption(option)}}</span>
                        </label>
                    </div>
                </template>
            </div>
        </div>
    </template>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=checkBoxList.js.map