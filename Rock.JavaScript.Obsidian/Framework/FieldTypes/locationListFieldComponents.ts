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
import { defineComponent, PropType, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import LocationList from "@Obsidian/Controls/locationList.obs";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import LocationItemPicker from "@Obsidian/Controls/locationItemPicker.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";
import { ConfigurationValueKey } from "./locationListField.partial";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { asListItemBagOrNull } from "@Obsidian/Utility/listItemBag";
import { Guid } from "@Obsidian/Types";
import { toGuidOrNull } from "@Obsidian/Utility/guid";


export const EditComponent = defineComponent({
    name: "LocationListField.Edit",

    components: {
        LocationList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<ListItemBag | null>(null);
        const locationTypeGuid = ref<Guid | null>(null);
        const parentGuid = ref<Guid | null>(null);
        const allowAdd = ref<boolean>(false);
        const showCityState = ref<boolean>(false);
        const addressRequired = ref<boolean>(false);

        watch(() => props.modelValue, () => {
            try {
                internalValue.value = JSON.parse(props.modelValue || "null");
            }
            catch {
                internalValue.value = null;
            }
        }, { immediate: true });

        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        watch(() => props.configurationValues, () => {
            locationTypeGuid.value = toGuidOrNull(asListItemBagOrNull(props.configurationValues[ConfigurationValueKey.LocationType])?.value);
            parentGuid.value = toGuidOrNull(asListItemBagOrNull(props.configurationValues[ConfigurationValueKey.ParentLocation])?.value);
            allowAdd.value = asBoolean(props.configurationValues[ConfigurationValueKey.AllowAddingNewLocations]);
            showCityState.value = asBoolean(props.configurationValues[ConfigurationValueKey.ShowCityState]);
            addressRequired.value = asBoolean(props.configurationValues[ConfigurationValueKey.AddressRequired]);
        }, { immediate: true });

        return {
            internalValue,
            locationTypeGuid,
            parentGuid,
            allowAdd,
            showCityState,
            addressRequired
        };
    },

    template: `
<LocationList v-model="internalValue" :allowAdd="allowAdd" :locationTypeValueGuid="locationTypeGuid" :parentLocationGuid="parentGuid" :showCityState="showCityState" :isAddressRequired="addressRequired" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "LocationListField.Configuration",

    components: {
        DefinedValuePicker,
        LocationItemPicker,
        CheckBox
    },

    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    emits: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_key: string, _value: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        console.debug("CONFIG", props.modelValue);
        const locationTypeGuid = DefinedType.LocationType;

        const locationType = ref<ListItemBag | null>(null);
        const parentLocation = ref<ListItemBag | null>(null);
        const allowAdding = ref<boolean>(false);
        const showCityState = ref<boolean>(false);
        const addressRequired = ref<boolean>(false);

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
            newValue[ConfigurationValueKey.LocationType] = locationType.value ? JSON.stringify(locationType.value) : "";
            newValue[ConfigurationValueKey.ParentLocation] = parentLocation.value ? JSON.stringify(parentLocation.value) : "";
            newValue[ConfigurationValueKey.AllowAddingNewLocations] = asTrueOrFalseString(allowAdding.value);
            newValue[ConfigurationValueKey.ShowCityState] = asTrueOrFalseString(showCityState.value);
            newValue[ConfigurationValueKey.AddressRequired] = asTrueOrFalseString(addressRequired.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.LocationType] !== props.modelValue[ConfigurationValueKey.LocationType]
                || newValue[ConfigurationValueKey.ParentLocation] !== (props.modelValue[ConfigurationValueKey.ParentLocation])
                || newValue[ConfigurationValueKey.AllowAddingNewLocations] !== (props.modelValue[ConfigurationValueKey.AllowAddingNewLocations])
                || newValue[ConfigurationValueKey.ShowCityState] !== (props.modelValue[ConfigurationValueKey.ShowCityState])
                || newValue[ConfigurationValueKey.AddressRequired] !== (props.modelValue[ConfigurationValueKey.AddressRequired]);

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

        // Watch for changes coming in from the parent component and update our data to match the new information.
        watch(() => props.modelValue, () => {
            locationType.value = asListItemBagOrNull(props.modelValue[ConfigurationValueKey.LocationType]);
            parentLocation.value = asListItemBagOrNull(props.modelValue[ConfigurationValueKey.ParentLocation]);
            allowAdding.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowAddingNewLocations]);
            showCityState.value = asBoolean(props.modelValue[ConfigurationValueKey.ShowCityState]);
            addressRequired.value = asBoolean(props.modelValue[ConfigurationValueKey.AddressRequired]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(locationType, () => maybeUpdateConfiguration(ConfigurationValueKey.LocationType, JSON.stringify(locationType.value)));
        watch(parentLocation, () => maybeUpdateConfiguration(ConfigurationValueKey.ParentLocation, JSON.stringify(parentLocation.value)));
        watch(allowAdding, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowAddingNewLocations, asTrueOrFalseString(allowAdding.value)));
        watch(showCityState, () => maybeUpdateConfiguration(ConfigurationValueKey.ShowCityState, asTrueOrFalseString(showCityState.value)));
        watch(addressRequired, () => maybeUpdateConfiguration(ConfigurationValueKey.AddressRequired, asTrueOrFalseString(addressRequired.value)));

        return {
            locationTypeGuid,
            locationType,
            parentLocation,
            allowAdding,
            showCityState,
            addressRequired,
        };
    },

    template: `
<DefinedValuePicker v-model="locationType" label="Location Type" :definedTypeGuid="locationTypeGuid" showBlankItem />
<LocationItemPicker v-model="parentLocation" label="Parent Location" :multiple="multiple" rules="required" />
<CheckBox v-model="allowAdding" label="Allow Adding New Locations" />
<CheckBox v-model="showCityState" label="Show City / State" />
<CheckBox v-model="addressRequired" label="Address Required" />
`
});
