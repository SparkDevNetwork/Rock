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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./campusField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { areEqual } from "@Obsidian/Utility/guid";

type CampusItem = {
    guid: Guid,
    name: string,
    type?: Guid | null,
    status?: Guid | null,
    isActive: boolean
};

export const EditComponent = defineComponent({
    name: "CampusField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ?? "");

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                const optionsListItems = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
                const isIncludeInactive = asBoolean(!props.configurationValues[ConfigurationValueKey.IncludeInactive]);
                const isValueFoundOnActiveItems = optionsListItems.find(x => x.value == internalValue.value);
                if (!isIncludeInactive && !isValueFoundOnActiveItems) {
                    const inactiveListItem = JSON.parse(props.configurationValues[ConfigurationValueKey.ValuesInactive] ?? "[]") as ListItemBag[];
                    const selectedValue = inactiveListItem.find(x => x.value == internalValue.value);
                    if (selectedValue) {
                        optionsListItems.push(selectedValue);
                    }
                }
                return optionsListItems;
            }
            catch {
                return [];
            }
        });

        watch(() => props.modelValue, () => internalValue.value = props.modelValue ?? "");

        watch(internalValue, () => emit("update:modelValue", internalValue.value));

        const shouldHidePicker = computed((): boolean => {
            return asBoolean(!props.configurationValues[ConfigurationValueKey.ForceVisible])
            && options.value.length <= 1
            && props.configurationValues[ConfigurationValueKey.FilterCampusTypes] == ""
            && props.configurationValues[ConfigurationValueKey.FilterCampusStatus] == "";
        });

        return {
            internalValue,
            options,
            shouldHidePicker
        };
    },

    template: `
<DropDownList v-if="!shouldHidePicker" v-model="internalValue" :items="options" />
`
});

export const FilterComponent = defineComponent({
    name: "CampusField.Filter",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue.split(",").filter(s => s !== ""));

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        watch(() => props.modelValue, () => internalValue.value = props.modelValue.split(",").filter(s => s !== ""));

        watch(internalValue, () => emit("update:modelValue", internalValue.value.join(",")));

        return {
            internalValue,
            options
        };
    },

    template: `
<CheckBoxList v-model="internalValue" :items="options" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "CampusField.Configuration",

    components: {
        CheckBoxList,
        CheckBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const includeInactive = ref(false);
        const filterCampusTypes = ref<string[]>([]);
        const filterCampusStatus = ref<string[]>([]);
        const selectableCampuses = ref<string[]>([]);

        /** The campus types that are available to be selected from. */
        const campusTypeOptions = ref<ListItemBag[]>([]);

        /** The campus statuses that are available to be selected from. */
        const campusStatusOptions = ref<ListItemBag[]>([]);

        /** The campuses that are available to be selected from. */
        const allCampusItems = ref<CampusItem[]>([]);

        const allCampusOptions = computed((): ListItemBag[] => {
            return allCampusItems.value.map((c): ListItemBag => {
                return {
                    value: c.guid,
                    text: c.name
                };
            });
        });

        /**
         * The campuses that are available to be selected from, these values
         * get emitted as the options the default value control can pick from.
         */
        const campusOptions = computed((): ListItemBag[] => {
            return allCampusItems.value.filter(c => {
                if (!includeInactive.value && !c.isActive) {
                    return false;
                }

                if (filterCampusTypes.value.length) {
                    if (filterCampusTypes.value.filter(o => areEqual(o, c.type)).length === 0) {
                        return false;
                    }
                }

                if (filterCampusStatus.value.length) {
                    if (filterCampusStatus.value.filter(o => areEqual(o, c.status)).length === 0) {
                        return false;
                    }
                }

                if (selectableCampuses.value.length) {
                    if (selectableCampuses.value.filter(o => areEqual(o, c.guid)).length === 0) {
                        return false;
                    }
                }

                return true;
            }).map(c => {
                return {
                    value: c.guid,
                    text: c.name
                };
            });
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
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueFalseOrNull(includeInactive.value) ?? "False";
            newValue[ConfigurationValueKey.FilterCampusTypes] = filterCampusTypes.value.join(",");
            newValue[ConfigurationValueKey.FilterCampusStatus] = filterCampusStatus.value.join(",");
            newValue[ConfigurationValueKey.SelectableCampuses] = selectableCampuses.value.join(",");
            newValue[ConfigurationValueKey.Values] = JSON.stringify(campusOptions.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive] ?? "False")
                || newValue[ConfigurationValueKey.FilterCampusTypes] !== (props.modelValue[ConfigurationValueKey.FilterCampusTypes] ?? "")
                || newValue[ConfigurationValueKey.FilterCampusStatus] !== (props.modelValue[ConfigurationValueKey.FilterCampusStatus] ?? "")
                || newValue[ConfigurationValueKey.SelectableCampuses] !== (props.modelValue[ConfigurationValueKey.SelectableCampuses] ?? "")
                || newValue[ConfigurationValueKey.Values] !== (props.modelValue[ConfigurationValueKey.Values] ?? "[]");

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
            const campuses = props.configurationProperties[ConfigurationPropertyKey.Campuses];
            const campusTypes = props.configurationProperties[ConfigurationPropertyKey.CampusTypes];
            const campusStatuses = props.configurationProperties[ConfigurationPropertyKey.CampusStatuses];

            allCampusItems.value = campuses ? JSON.parse(campuses) as CampusItem[] : [];
            campusTypeOptions.value = campusTypes ? JSON.parse(campusTypes) as ListItemBag[] : [];
            campusStatusOptions.value = campusStatuses ? JSON.parse(campusStatuses) as ListItemBag[] : [];

            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
            filterCampusTypes.value = (props.modelValue[ConfigurationValueKey.FilterCampusTypes]?.split(",") ?? []).filter(s => s !== "");
            filterCampusStatus.value = (props.modelValue[ConfigurationValueKey.FilterCampusStatus]?.split(",") ?? []).filter(s => s !== "");
            selectableCampuses.value = (props.modelValue[ConfigurationValueKey.SelectableCampuses]?.split(",") ?? []).filter(s => s !== "");
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(includeInactive, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeInactive, asTrueFalseOrNull(includeInactive.value) ?? "False"));
        watch(filterCampusTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.FilterCampusTypes, filterCampusTypes.value.join(",")));
        watch(filterCampusStatus, () => maybeUpdateConfiguration(ConfigurationValueKey.FilterCampusStatus, filterCampusStatus.value.join(",")));
        watch(selectableCampuses, () => maybeUpdateConfiguration(ConfigurationValueKey.SelectableCampuses, selectableCampuses.value.join(",")));
        watch(campusOptions, () => emit("updateConfigurationValue", ConfigurationValueKey.Values, JSON.stringify(campusOptions.value)));

        return {
            allCampusOptions,
            campusStatusOptions,
            campusTypeOptions,
            filterCampusStatus,
            filterCampusTypes,
            includeInactive,
            selectableCampuses
        };
    },

    template: `
<div>
    <CheckBox v-model="includeInactive"
        label="Include Inactive"
        help="When set, inactive campuses will be included in the list." />

    <CheckBoxList v-model="filterCampusTypes"
        label="Filter Campus Types"
        help="When set this will filter the campuses displayed in the list to the selected Types. Setting a filter will cause the campus picker to display even if 0 campuses are in the list."
        :items="campusTypeOptions"
        horizontal />

    <CheckBoxList v-model="filterCampusStatus"
        label="Filter Campus Status"
        help="When set this will filter the campuses displayed in the list to the selected Status. Setting a filter will cause the campus picker to display even if 0 campuses are in the list."
        :items="campusStatusOptions"
        horizontal />

    <CheckBoxList v-model="selectableCampuses"
        label="Selectable Campuses"
        :items="allCampusOptions"
        horizontal />
</div>
`
});
