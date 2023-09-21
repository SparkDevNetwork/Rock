// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import { PropType, computed, defineComponent, inject, ref, watch } from "vue";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { getFieldEditorProps } from "./utils";
import RockFormField from "@Obsidian/Controls/rockFormField.obs";
import { ClientValue, ConfigurationPropertyKey, ConfigurationValueKey, ValueItem } from "./definedValueRangeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { List } from "@Obsidian/Utility/linq";

function parseModelValue(modelValue: string | undefined): string[] {
    try {
        const clientValue = JSON.parse(modelValue ?? "") as ClientValue;
        const splitValue = (clientValue.value ?? "").split(",");

        if (splitValue.length === 1) {
            return [splitValue[0], ""];
        }

        return splitValue;
    }
    catch {
        return ["", ""];
    }
}

function getClientValue(lowerValue: string, upperValue: string, valueOptions: ValueItem[], showDescription: boolean): ClientValue {
    const options = new List(valueOptions);
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
        value: `${lv?.value ?? ""},${uv?.value ?? ""}`,
        text: `${lv?.text ?? ""} to ${uv?.text ?? ""}`,
        description: showDescription ? `${lv?.description ?? ""} to ${uv?.description ?? ""}` : ""
    };
}

export const EditComponent = defineComponent({
    name: "DefinedValueRangeField.Edit",

    components: {
        RockFormField
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValues = parseModelValue(props.modelValue);
        const internalValue = ref(props.modelValue);
        const lowerValue = ref(internalValues[0]);
        const upperValue = ref(internalValues[1]);

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        const showDescription = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.DisplayDescription]);
        });

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            const providedOptions: ListItemBag[] = valueOptions.value.map(v => {
                return {
                    text: showDescription.value ? v.description : v.text,
                    value: v.value
                };
            });

            return providedOptions;
        });

        watch(() => props.modelValue, () => {
            const internalValues = parseModelValue(props.modelValue);

            lowerValue.value = internalValues[0];
            upperValue.value = internalValues[1];
        });

        watch(() => [lowerValue.value, upperValue.value], () => {
            const clientValue = getClientValue(lowerValue.value, upperValue.value, valueOptions.value, showDescription.value);

            emit("update:modelValue", JSON.stringify(clientValue));
        });

        return {
            internalValue,
            lowerValue,
            upperValue,
            isRequired: inject("isRequired") as boolean,
            options,
            getKeyForOption(option: ListItemBag): string {
                return option.value ?? "";
            },
            getTextForOption(option: ListItemBag): string {
                return option.text ?? "";
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
});

export const ConfigurationComponent = defineComponent({
    name: "DefinedValueRangeField.Configuration",

    components: {
        DropDownList,
        CheckBox
    },

    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        configurationProperties: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const definedTypeValue = ref("");
        const displayDescriptions = ref(false);

        /** The defined types that are available to be selected from. */
        const definedTypeItems = ref<ListItemBag[]>([]);

        /** The options to show in the defined type picker. */
        const definedTypeOptions = computed((): ListItemBag[] => {
            return definedTypeItems.value;
        });

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.DefinedType] = definedTypeValue.value;
            newValue[ConfigurationValueKey.DisplayDescription] = asTrueFalseOrNull(displayDescriptions.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.DefinedType] !== props.modelValue[ConfigurationValueKey.DefinedType]
                || newValue[ConfigurationValueKey.DisplayDescription] !== (props.modelValue[ConfigurationValueKey.DisplayDescription] ?? "False");

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };


        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            const definedTypes = props.configurationProperties[ConfigurationPropertyKey.DefinedTypes];
            definedTypeItems.value = definedTypes ? JSON.parse(props.configurationProperties.definedTypes) as ListItemBag[] : [];
            definedTypeValue.value = props.modelValue.definedtype;
            displayDescriptions.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayDescription]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([definedTypeValue, displayDescriptions], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });


        return {
            definedTypeValue,
            definedTypeOptions,
            displayDescriptions,
        };
    },

    template: `
<div>
    <DropDownList v-model="definedTypeValue" label="Defined Type" :items="definedTypeOptions" :showBlankItem="false" />
    <CheckBox v-model="displayDescriptions" label="Display Descriptions" text="Yes" help="When set, the defined value descriptions will be displayed instead of the values." />
</div>
`
});
