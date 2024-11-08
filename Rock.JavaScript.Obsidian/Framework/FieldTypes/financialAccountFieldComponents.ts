import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import AccountPicker from "@Obsidian/Controls/accountPicker.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./financialAccountField.partial";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";

export const EditComponent = defineComponent({
    name: "FinancialAccountField.Edit",

    components: {
        AccountPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as ListItemBag);

        const displayPublicName = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.DisplayPublicName]));
        const displayChildItemCounts = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.DisplayChildItemCounts]));
        const displayActiveItemsOnly = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.DisplayActiveItemsOnly]));
        const enhancedForLongLists = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.EnhancedForLongLists]));

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            displayPublicName,
            displayChildItemCounts,
            displayActiveItemsOnly,
            enhancedForLongLists
        };
    },

    template: `
<AccountPicker v-model="internalValue" :displayPublicName="displayPublicName" :activeOnly="displayActiveItemsOnly" :displayChildItemCountLabel="displayChildItemCounts" :enhanceForLongLists="enhancedForLongLists"  />
`
});


export const ConfigurationComponent = defineComponent({
    name: "FinancialAccountField.Configuration",

    components: {
        CheckBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const displayPublicName = ref(true);
        const displayChildItemCounts = ref(false);
        const displayActiveItemsOnly = ref(false);
        const enhanceForLongLists = ref(true);

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
            newValue[ConfigurationValueKey.DisplayPublicName] = asTrueFalseOrNull(displayPublicName.value) ?? "True";
            newValue[ConfigurationValueKey.DisplayChildItemCounts] = asTrueFalseOrNull(displayChildItemCounts.value) ?? "False";
            newValue[ConfigurationValueKey.DisplayActiveItemsOnly] = asTrueFalseOrNull(displayActiveItemsOnly.value) ?? "False";
            newValue[ConfigurationValueKey.EnhancedForLongLists] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "True";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.DisplayPublicName] !== (props.modelValue[ConfigurationValueKey.DisplayPublicName] ?? "True")
                || newValue[ConfigurationValueKey.DisplayChildItemCounts] !== (props.modelValue[ConfigurationValueKey.DisplayChildItemCounts] ?? "False")
                || newValue[ConfigurationValueKey.DisplayActiveItemsOnly] !== (props.modelValue[ConfigurationValueKey.DisplayActiveItemsOnly] ?? "False")
                || newValue[ConfigurationValueKey.EnhancedForLongLists] !== (props.modelValue[ConfigurationValueKey.EnhancedForLongLists] ?? "True");

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

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            displayPublicName.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayPublicName]);
            displayChildItemCounts.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayChildItemCounts]);
            displayActiveItemsOnly.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayActiveItemsOnly]);
            enhanceForLongLists.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedForLongLists]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([displayPublicName, displayChildItemCounts, displayActiveItemsOnly, enhanceForLongLists], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(displayPublicName, () => maybeUpdateConfiguration(ConfigurationValueKey.DisplayPublicName, asTrueFalseOrNull(displayPublicName.value) ?? "True"));
        watch(displayChildItemCounts, () => maybeUpdateConfiguration(ConfigurationValueKey.DisplayChildItemCounts, asTrueFalseOrNull(displayChildItemCounts.value) ?? "False"));
        watch(displayActiveItemsOnly, () => maybeUpdateConfiguration(ConfigurationValueKey.DisplayActiveItemsOnly, asTrueFalseOrNull(displayActiveItemsOnly.value) ?? "False"));
        watch(enhanceForLongLists, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedForLongLists, asTrueFalseOrNull(enhanceForLongLists.value) ?? "True"));

        return {
            displayPublicName,
            displayChildItemCounts,
            displayActiveItemsOnly,
            enhanceForLongLists,
        };
    },

    template: `
<div>
    <CheckBox v-model="displayPublicName" label="Display Public Name" help="When set, public name will be displayed." />
    <CheckBox v-model="displayChildItemCounts" label="Display Child Item Counts" help="When set, child item counts will be displayed." />
    <CheckBox v-model="displayActiveItemsOnly" label="Display Active Items Only" help="When set, only active item will be displayed." />
    <CheckBox v-model="enhanceForLongLists" label="Enhance For Long Lists" help="When set, allows a searching for items." />
</div>
`
});
