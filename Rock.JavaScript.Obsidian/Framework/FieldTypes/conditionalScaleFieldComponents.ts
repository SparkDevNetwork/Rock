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
import { defineComponent, ref, watch } from "vue";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import ColorPicker from "@Obsidian/Controls/colorPicker.obs";
import RockFormField from "@Obsidian/Controls/rockFormField.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ClientValue, ConfigurationValueKey } from "./conditionalScaleField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { newGuid } from "@Obsidian/Utility/guid";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

function parseModelValue(modelValue: string | undefined): ClientValue[] {
    try {
        return JSON.parse(modelValue ?? "[]") as ClientValue[];
    }
    catch {
        return [];
    }
}

export const EditComponent = defineComponent({
    name: "ConditionalScaleField.Edit",

    components: {
        NumberBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<number | null>(null);

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = toNumberOrNull(props.modelValue || "");
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue !== null ? internalValue.value?.toString() : "");
        });

        return {
            internalValue,
        };
    },

    template: `
    <NumberBox v-model="internalValue" rules="decimal" />
    `
});

export const ConfigurationComponent = defineComponent({
    name: "ConditionalScaleField.Configuration",

    components: {
        RockFormField,
        TextBox,
        NumberBox,
        ColorPicker
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const configuration = ref<ClientValue[]>([]);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.ConfigurationJSON] = JSON.stringify(configuration.value ?? []);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ConfigurationJSON] !== (props.modelValue[ConfigurationValueKey.ConfigurationJSON] ?? []);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        const onAddClick = (): void => {
            const range = configuration.value.length;
            configuration.value.push({ label: "", lowValue: null, highValue: null, color:"", rangeIndex:range + 1 ,guid: newGuid()  });
        };

        const onRemoveClick = (index: number): void => {
            configuration.value.splice(index, 1);
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            configuration.value = parseModelValue(props.modelValue[ConfigurationValueKey.ConfigurationJSON]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(configuration, () => maybeUpdateConfiguration(ConfigurationValueKey.ConfigurationJSON, JSON.stringify(configuration.value) ?? ""), {deep : true});

        return {
            configuration,
            onAddClick,
            onRemoveClick
        };
    },

    template: `
    <RockFormField
    :modelValue="configuration"
    formGroupClasses="conditional-scale"
    name="conditional-scale">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
<span :id="uniqueId" class="conditional-scale">
    <span class="conditional-scale-rows">
        <div v-for="(value, valueIndex) in configuration" class="controls row margin-b-md">
            <div class='col-md-5'>
                <input v-model="value.label" required="true" class="conditional-scale-label form-control margin-b-md" type="text" placeholder="Label">
                <NumberBox v-model="value.highValue" class="conditional-scale-highValue form-control" placeholder="High Value" />
            </div>

            <div class='col-md-5'>
                <ColorPicker v-model="value.color" />
                <NumberBox v-model="value.lowValue" class="conditional-scale-lowValue form-control margin-t-md" placeholder="Low Value" />
            </div>
                <div class='col-md-2'>
                    <a href="#" @click.prevent="onRemoveClick(valueIndex)" class="btn btn-sm btn-danger"><i class="fa fa-times"></i></a>
                </div>

        </div>
    </span>
    <hr>
    <div class="control-actions">
        <a class="btn btn-action btn-square btn-xs" href="#" @click.prevent="onAddClick"><i class="fa fa-plus-circle"></i></a>
    </div>
</span>
        </div>
    </template>
</RockFormField>

`
});
