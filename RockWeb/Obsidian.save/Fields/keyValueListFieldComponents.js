System.register(["../Services/boolean", "vue", "../Elements/dropDownList", "../Elements/rockFormField", "../Elements/textBox", "./utils"], function (exports_1, context_1) {
    "use strict";
    var boolean_1, vue_1, dropDownList_1, rockFormField_1, textBox_1, utils_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    function parseModelValue(modelValue) {
        try {
            return JSON.parse(modelValue !== null && modelValue !== void 0 ? modelValue : "[]");
        }
        catch (_a) {
            return [];
        }
    }
    return {
        setters: [
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "KeyValueListField.Edit",
                components: {
                    RockFormField: rockFormField_1.default,
                    DropDownList: dropDownList_1.default,
                    TextBox: textBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup(props, { emit }) {
                    const internalValues = vue_1.ref(parseModelValue(props.modelValue));
                    const valueOptions = vue_1.computed(() => {
                        var _a;
                        try {
                            return JSON.parse((_a = props.configurationValues["values"]) !== null && _a !== void 0 ? _a : "[]");
                        }
                        catch (_b) {
                            return [];
                        }
                    });
                    const options = vue_1.computed(() => {
                        const providedOptions = valueOptions.value.map(v => {
                            return {
                                text: v.text,
                                value: v.value
                            };
                        });
                        return providedOptions;
                    });
                    const hasValues = vue_1.computed(() => valueOptions.value.length > 0);
                    const keyPlaceholder = vue_1.computed(() => {
                        var _a;
                        return (_a = props.configurationValues["keyprompt"]) !== null && _a !== void 0 ? _a : "";
                    });
                    const valuePlaceholder = vue_1.computed(() => {
                        var _a;
                        return (_a = props.configurationValues["valueprompt"]) !== null && _a !== void 0 ? _a : "";
                    });
                    const displayValueFirst = vue_1.computed(() => {
                        var _a;
                        return boolean_1.asBoolean((_a = props.configurationValues["displayvaluefirst"]) !== null && _a !== void 0 ? _a : "");
                    });
                    vue_1.watch(() => props.modelValue, () => {
                        internalValues.value = parseModelValue(props.modelValue);
                    });
                    vue_1.watch(() => internalValues.value, () => {
                        emit("update:modelValue", JSON.stringify(internalValues.value));
                    }, {
                        deep: true
                    });
                    const onAddClick = () => {
                        let defaultValue = "";
                        if (hasValues.value) {
                            defaultValue = valueOptions.value[0].value;
                        }
                        internalValues.value.push({ key: "", value: defaultValue });
                    };
                    const onRemoveClick = (index) => {
                        internalValues.value.splice(index, 1);
                    };
                    return {
                        internalValues,
                        hasValues,
                        displayValueFirst,
                        options,
                        keyPlaceholder,
                        valuePlaceholder,
                        onAddClick,
                        onRemoveClick
                    };
                },
                template: `
<RockFormField
    :modelValue="internalValues"
    formGroupClasses="key-value-list"
    name="key-value-list">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
<span :id="uniqueId" class="key-value-list">
    <span class="key-value-rows">
        <div v-for="(value, valueIndex) in internalValues" class="controls controls-row form-control-group">
            <template v-if="!displayValueFirst">
                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">

                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">
            </template>
            <template v-else>
                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">

                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">
            </template>

            <a href="#" @click.prevent="onRemoveClick(valueIndex)" class="btn btn-sm btn-danger"><i class="fa fa-times"></i></a>
        </div>
    </span>
    <div class="control-actions">
        <a class="btn btn-action btn-square btn-xs" href="#" @click.prevent="onAddClick"><i class="fa fa-plus-circle"></i></a>
    </div>
</span>
        </div>
    </template>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=keyValueListFieldComponents.js.map