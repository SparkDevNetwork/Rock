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
import { computed, defineComponent, ref, SetupContext, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./campusesField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { areEqual } from "@Obsidian/Utility/guid";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { updateRefValue } from "@Obsidian/Utility/component";

type CampusItem = {
    guid: Guid,
    name: string,
    type?: Guid | null,
    status?: Guid | null,
    isActive: boolean
};

export const EditComponent = defineComponent({
    name: "CampusesField.Edit",

    components: {
        CheckBoxList,
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, context: SetupContext) {
        const internalValue = ref(props.modelValue ? props.modelValue.split(",") : []);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        const enhance = computed(() => {
            return props.configurationValues[ConfigurationValueKey.EnhancedSelection] == "True";
        });

        const repeatColumns = computed(() => {
            const repeatColumnsConfig = props.configurationValues[ConfigurationValueKey.RepeatColumns];

            return toNumberOrNull(repeatColumnsConfig) ?? 4;
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? props.modelValue.split(",") : []);
        });

        watch(internalValue, () => {
            context.emit("update:modelValue", internalValue.value.join(","));
        });

        return {
            internalValue,
            options,
            repeatColumns,
            enhance
        };
    },

    template: `
<DropDownList v-if="enhance" v-model="internalValue" enhanceForLongLists multiple :items="options" />
<CheckBoxList v-else v-model="internalValue" horizontal :items="options" :repeatColumns="repeatColumns" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "CampusesField.Configuration",

    components: {
        CheckBox,
        CheckBoxList,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emit: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_k: string, _v: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const enhancedSelection = ref(false);
        const numberOfColumns = ref<number | null>(null);
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
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueFalseOrNull(enhancedSelection.value) ?? "False";
            newValue[ConfigurationValueKey.RepeatColumns] = numberOfColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueFalseOrNull(includeInactive.value) ?? "False";
            newValue[ConfigurationValueKey.FilterCampusTypes] = filterCampusTypes.value.join(",");
            newValue[ConfigurationValueKey.FilterCampusStatus] = filterCampusStatus.value.join(",");
            newValue[ConfigurationValueKey.SelectableCampuses] = selectableCampuses.value.join(",");
            newValue[ConfigurationValueKey.Values] = JSON.stringify(campusOptions.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection] ?? "False")
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "")
                || newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive] ?? "False")
                || newValue[ConfigurationValueKey.FilterCampusTypes] !== (props.modelValue[ConfigurationValueKey.FilterCampusTypes] ?? "")
                || newValue[ConfigurationValueKey.FilterCampusStatus] !== (props.modelValue[ConfigurationValueKey.FilterCampusStatus] ?? "")
                || newValue[ConfigurationValueKey.SelectableCampuses] !== (props.modelValue[ConfigurationValueKey.SelectableCampuses] ?? "")
                || newValue[ConfigurationValueKey.Values] !== (props.modelValue[ConfigurationValueKey.Values] ?? "");

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

            enhancedSelection.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedSelection]);
            numberOfColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
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
        watch(enhancedSelection, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueFalseOrNull(enhancedSelection.value) ?? "False"));
        watch(numberOfColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, numberOfColumns.value?.toString() ?? ""));
        watch(includeInactive, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeInactive, asTrueFalseOrNull(includeInactive.value) ?? "False"));
        watch(filterCampusTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.FilterCampusTypes, filterCampusTypes.value.join(",")));
        watch(filterCampusStatus, () => maybeUpdateConfiguration(ConfigurationValueKey.FilterCampusStatus, filterCampusStatus.value.join(",")));
        watch(selectableCampuses, () => maybeUpdateConfiguration(ConfigurationValueKey.SelectableCampuses, selectableCampuses.value.join(",")));
        watch(campusOptions, () => maybeUpdateConfiguration(ConfigurationValueKey.Values, JSON.stringify(campusOptions.value)));

        return {
            allCampusOptions,
            campusStatusOptions,
            campusTypeOptions,
            enhancedSelection,
            filterCampusStatus,
            filterCampusTypes,
            includeInactive,
            numberOfColumns,
            selectableCampuses
        };
    },

    template: `
<div>
    <CheckBox v-model="enhancedSelection"
        label="Enhanced For Long Lists"
        help="When set, will render a searchable selection of options." />

    <NumberBox v-if="!enhancedSelection"
        v-model="numberOfColumns"
        label="Number of Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />

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
