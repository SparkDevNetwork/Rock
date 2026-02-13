import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import DataViewPicker from "@Obsidian/Controls/dataViewPicker.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./dataViewsField.partial";

export const EditComponent = defineComponent({
    name: "DataViewsField.Edit",

    components: {
        DataViewPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref([] as ListItemBag[]);

        // The selected Entity Type.
        const entityTypeGuid = computed((): string | null | undefined => {
            const entityType = JSON.parse(props.configurationValues[ConfigurationValueKey.EntityType] ?? "{}") as ListItemBag;
            return entityType?.value;
        });

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "[]");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            entityTypeGuid
        };
    },

    template: `
    <DataViewPicker v-model="internalValue" multiple :entityTypeGuid="entityTypeGuid" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DataViewsField.Configuration",

    components: {
        EntityTypePicker
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const entityType = ref<ListItemBag>();

        // Compute a consistent entity type string value to compare against.
        const entityTypeStringFromProps = computed((): string => {
            const propsString = props.modelValue[ConfigurationValueKey.EntityType];
            if (!propsString) {
                return "{}";
            }

            try {
                // It's possible for propsString to be "null", in which case we
                // want to default to an empty object for consistent comparison.
                return JSON.stringify(
                    JSON.parse(propsString) ?? {}
                );
            }
            catch {
                return "{}";
            }
        });

        // Compute a consistent entity type object value to compare against.
        const entityTypeFromProps = computed((): ListItemBag => {
            return JSON.parse(entityTypeStringFromProps.value);
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
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.EntityType] = JSON.stringify(entityType.value ?? {});

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EntityType] !== entityTypeStringFromProps.value;

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
            entityType.value = entityTypeFromProps.value;
        }, {
            immediate: true
        });

        watch(entityType, () => maybeUpdateConfiguration(ConfigurationValueKey.EntityType, JSON.stringify(entityType.value ?? {})));

        return {
            entityType
        };
    },

    template: `
<div>
    <EntityTypePicker label="Entity Type" v-model="entityType" help="The type of entity to display data views for." />
</div>
    `
});
