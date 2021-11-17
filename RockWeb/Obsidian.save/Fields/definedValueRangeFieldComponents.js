System.register(["vue", "./utils", "../Elements/rockFormField", "../Services/boolean", "../Util/linq"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, rockFormField_1, boolean_1, linq_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    function parseModelValue(modelValue) {
        var _a;
        try {
            const clientValue = JSON.parse(modelValue !== null && modelValue !== void 0 ? modelValue : "");
            const splitValue = ((_a = clientValue.value) !== null && _a !== void 0 ? _a : "").split(",");
            if (splitValue.length === 1) {
                return [splitValue[0], ""];
            }
            return splitValue;
        }
        catch (_b) {
            return ["", ""];
        }
    }
    function getClientValue(lowerValue, upperValue, valueOptions, showDescription) {
        var _a, _b, _c, _d, _e, _f;
        const options = new linq_1.List(valueOptions);
        const lv = options.firstOrUndefined(v => v.value === lowerValue);
        const uv = options.firstOrUndefined(v => v.value === upperValue);
        if (!lv && !uv) {
            return {
                value: "",
                text: "",
                description: ""
            };
        }
        return {
            value: `${(_a = lv === null || lv === void 0 ? void 0 : lv.value) !== null && _a !== void 0 ? _a : ""},${(_b = uv === null || uv === void 0 ? void 0 : uv.value) !== null && _b !== void 0 ? _b : ""}`,
            text: `${(_c = lv === null || lv === void 0 ? void 0 : lv.text) !== null && _c !== void 0 ? _c : ""} to ${(_d = uv === null || uv === void 0 ? void 0 : uv.text) !== null && _d !== void 0 ? _d : ""}`,
            description: showDescription ? `${(_e = lv === null || lv === void 0 ? void 0 : lv.description) !== null && _e !== void 0 ? _e : ""} to ${(_f = uv === null || uv === void 0 ? void 0 : uv.description) !== null && _f !== void 0 ? _f : ""}` : ""
        };
    }
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (linq_1_1) {
                linq_1 = linq_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DefinedValueRangeField.Edit",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup(props, { emit }) {
                    const internalValues = parseModelValue(props.modelValue);
                    const internalValue = vue_1.ref(props.modelValue);
                    const lowerValue = vue_1.ref(internalValues[0]);
                    const upperValue = vue_1.ref(internalValues[1]);
                    const valueOptions = vue_1.computed(() => {
                        var _a;
                        try {
                            return JSON.parse((_a = props.configurationValues["values"]) !== null && _a !== void 0 ? _a : "[]");
                        }
                        catch (_b) {
                            return [];
                        }
                    });
                    const showDescription = vue_1.computed(() => {
                        return boolean_1.asBoolean(props.configurationValues["displaydescription"]);
                    });
                    const options = vue_1.computed(() => {
                        const providedOptions = valueOptions.value.map(v => {
                            return {
                                text: showDescription.value ? v.description : v.text,
                                value: v.value
                            };
                        });
                        return providedOptions;
                    });
                    vue_1.watch(() => props.modelValue, () => {
                        const internalValues = parseModelValue(props.modelValue);
                        lowerValue.value = internalValues[0];
                        upperValue.value = internalValues[1];
                    });
                    vue_1.watch(() => [lowerValue.value, upperValue.value], () => {
                        const clientValue = getClientValue(lowerValue.value, upperValue.value, valueOptions.value, showDescription.value);
                        emit("update:modelValue", JSON.stringify(clientValue));
                    });
                    return {
                        internalValue,
                        lowerValue,
                        upperValue,
                        isRequired: vue_1.inject("isRequired"),
                        options,
                        getKeyForOption(option) {
                            return option.value;
                        },
                        getTextForOption(option) {
                            return option.text;
                        }
                    };
                },
                template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-defined-value-range"
    name="definedvaluerange"
    #default="{uniqueId}"
    :rules="computedRules">
    <div :id="uniqueId" class="form-control-group">
        <select class="input-width-md form-control" v-model="lowerValue">
            <option v-if="!isRequired" value=""></option>
            <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
        </select>
        <span class="to"> to </span>
        <select class="input-width-md form-control" v-model="upperValue">
            <option v-if="!isRequired" value=""></option>
            <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
        </select>
    </div>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=definedValueRangeFieldComponents.js.map