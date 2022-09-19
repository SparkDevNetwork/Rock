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

import { computed, defineComponent, PropType, ref } from "vue";
import InlineSwitch from "@Obsidian/Controls/inlineSwitch";
import Modal from "@Obsidian/Controls/modal";
import Panel from "@Obsidian/Controls/panel";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import RockButton from "@Obsidian/Controls/rockButton";
import SectionHeader from "@Obsidian/Controls/sectionHeader";
import TextBox from "@Obsidian/Controls/textBox";
import { ContentCollectionBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ContentCollectionFilterControl } from "@Obsidian/Enums/Cms/contentCollectionFilterControl";
import { AttributeFilterBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/attributeFilterBag";
import SearchFilter from "./searchFilter.partial";
import AttributeSearchFilter from "./attributeSearchFilter.partial";
import { areEqual } from "@Obsidian/Utility/guid";
import { FieldType } from "@Obsidian/SystemGuids";
import { FilterSettingsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/filterSettingsBag";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { ContentCollectionDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionDetailOptionsBag";
import { alert } from "@Obsidian/Utility/dialogs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

/**
 * The items available for selection in the filter type radio button list.
 */
const editFilterTypeItems: ListItemBag[] = [
    {
        "value": "0",
        "text": "Single-Select"
    },
    {
        "value": "1",
        "text": "Multi-Select"
    }
];

/**
 * The items availalbe for selection in the filter control radio button list
 * when the field type is not a boolean.
 */
const editFilterControlStandardItems: ListItemBag[] = [
    {
        "value": ContentCollectionFilterControl.Pills.toString(),
        "text": "Pills"
    },
    {
        "value": ContentCollectionFilterControl.Dropdown.toString(),
        "text": "Dropdown"
    }
];

/**
 * The items availalbe for selection in the filter control radio button list
 * when the field type is a boolean.
 */
 const editFilterControlBooleanItems: ListItemBag[] = [
    {
        "value": ContentCollectionFilterControl.Boolean.toString(),
        "text": "Boolean"
    }
];

export default defineComponent({
    name: "Cms.ContentCollectionDetail.SearchFilters",

    components: {
        AttributeSearchFilter,
        InlineSwitch,
        Modal,
        Panel,
        RadioButtonList,
        RockButton,
        SearchFilter,
        SectionHeader,
        TextBox
    },

    props: {
        /** The content collection that contains the search filters to display and edit. */
        modelValue: {
            type: Object as PropType<ContentCollectionBag>,
            required: true
        }
    },

    emits: {
        "update:modelValue": (_value: ContentCollectionBag) => true
    },

    setup(props, { emit }) {
        console.log("setup");
        // #region Values

        const invokeBlockAction = useInvokeBlockAction();

        const editFilterKey = ref("");
        const editFilterName = ref("");
        const editFilterControl = ref("");
        const editFilterControlItems = ref<ListItemBag[]>([]);
        const editFilterEnabled = ref(false);
        const editFilterLabel = ref("");
        const editFilterType = ref("");
        const editShowFilterType = ref(false);
        const isEditModalOpen = ref(false);
        const isEditFullTextSearch = ref(false);

        // #endregion

        // #region Computed Values

        const fullTextSearchEnabled = computed((): boolean => {
            return props.modelValue.filterSettings?.fullTextSearchEnabled ?? false;
        });

        const yearSearchEnabled = computed((): boolean => {
            return props.modelValue.filterSettings?.yearSearchEnabled ?? false;
        });

        const yearSearchLabel = computed((): string => {
            return props.modelValue.filterSettings?.yearSearchLabel || "Year";
        });

        const yearSearchFilterControl = computed((): ContentCollectionFilterControl => {
            return props.modelValue.filterSettings?.yearSearchFilterControl ?? ContentCollectionFilterControl.Pills;
        });

        const yearSearchFilterIsMultipleSelection = computed((): boolean => {
            return props.modelValue.filterSettings?.yearSearchFilterIsMultipleSelection ?? false;
        });

        const yearSearchValues = computed((): ListItemBag[] => {
            return [
                {
                    text: "Filter Label",
                    value: yearSearchLabel.value
                },
                {
                    text: "Filter Control",
                    value: yearSearchFilterControl.value === ContentCollectionFilterControl.Dropdown ? "Dropdown" : "Pills"
                },
                {
                    text: "Filter Mode",
                    value: yearSearchFilterIsMultipleSelection.value ? "Multi-Select" : "Single-Select"
                }
            ];
        });

        const attributeFilters = computed((): AttributeFilterBag[] => {
            return props.modelValue.filterSettings?.attributeFilters ?? [];
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the person clicks the edit button on one of
         * the attribute search filters.
         * 
         * @param filter The attribute filter that should be edited.
         */
        const onEditAttributeFilter = (filter: AttributeFilterBag): void => {
            if (!filter.attributeKey) {
                return;
            }

            // Initialize the standard values that should be edited.
            editFilterKey.value = filter.attributeKey;
            editFilterName.value = filter.attributeName ?? "";
            editFilterEnabled.value = filter.isEnabled;
            editFilterLabel.value = filter.filterLabel ?? filter.attributeName ?? "";
            
            // Special logic for Boolean field types since they are hard coded
            // to a Boolean control and don't show a filter type.
            if (areEqual(filter.fieldTypeGuid, FieldType.Boolean)) {
                editFilterControl.value = ContentCollectionFilterControl.Boolean.toString();
                editFilterControlItems.value = editFilterControlBooleanItems;
                editFilterType.value = "0";
                editShowFilterType.value = false;
            }
            else {
                editFilterControl.value = filter.filterControl.toString();
                editFilterControlItems.value = editFilterControlStandardItems;
                editFilterType.value = filter.isMultipleSelection ? "1" : "0";
                editShowFilterType.value = true;
            }

            isEditFullTextSearch.value = false;
            isEditModalOpen.value = true;
        };

        /**
         * Event handler for when the person clicks the edit button on the
         * full text search filter.
         */
        const onEditFullTextSearch = (): void => {
            editFilterKey.value = "";
            editFilterName.value = "Full Text Search";
            editFilterEnabled.value = fullTextSearchEnabled.value;
            editShowFilterType.value = false;

            isEditFullTextSearch.value = true;
            isEditModalOpen.value = true;
        };

        /**
         * Event handler for when the person clicks the edit button on the
         * year search filter.
         */
        const onEditYearFilter = (): void => {
            editFilterKey.value = "";
            editFilterName.value = "Year";
            editFilterEnabled.value = yearSearchEnabled.value;
            editFilterLabel.value = yearSearchLabel.value;
            editFilterControl.value = yearSearchFilterControl.value.toString();
            editFilterControlItems.value = editFilterControlStandardItems;
            editFilterType.value = yearSearchFilterIsMultipleSelection.value ? "1" : "0";
            editShowFilterType.value = true;

            isEditFullTextSearch.value = false;
            isEditModalOpen.value = true;
        };

        /**
         * Event handler for when the person clicks the save button on the
         * modal while editing a search filter.
         */
        const onModalSave = async (): Promise<void> => {
            if (!props.modelValue.filterSettings) {
                return;
            }

            // Create a new bag so we don't modify the live data.
            const bag: FilterSettingsBag = {
                ...props.modelValue.filterSettings
            };

            let validProperties: string[];

            if (isEditFullTextSearch.value) {
                // Update just the full text search filter settings.
                bag.fullTextSearchEnabled = editFilterEnabled.value;
                validProperties = ["fullTextSearchEnabled"];
            }
            else if (!editFilterKey.value) {
                // Update just the year search filter settings.
                bag.yearSearchEnabled = editFilterEnabled.value;
                bag.yearSearchFilterControl = toNumberOrNull(editFilterControl.value) ?? ContentCollectionFilterControl.Pills;
                bag.yearSearchFilterIsMultipleSelection = editFilterType.value === "1";
                bag.yearSearchLabel = editFilterLabel.value;
                validProperties = ["yearSearchEnabled", "yearSearchFilterControl", "yearSearchFilterIsMultipleSelection", "yearSearchLabel"];
            }
            else {
                // Update the attribute search filter settings.

                // Create a new array so we don't modify the live data.
                bag.attributeFilters = [...(bag.attributeFilters ?? [])];

                // Find the specific attribute filter that is currently
                // being edited in the modal.
                const filterIndex = bag.attributeFilters.findIndex(f => f.attributeKey === editFilterKey.value);

                if (filterIndex === -1) {
                    return;
                }

                // Make a copy of the filter so we don't modify the live data.
                const filter = {
                    ...bag.attributeFilters[filterIndex]
                };

                filter.isEnabled = editFilterEnabled.value;
                filter.filterLabel = editFilterLabel.value;

                // More special logic around the Boolean field type.
                if (areEqual(filter.fieldTypeGuid, FieldType.Boolean)) {
                    filter.filterControl = ContentCollectionFilterControl.Boolean;
                    filter.isMultipleSelection = false;
                }
                else {
                    filter.filterControl = toNumberOrNull(editFilterControl.value) ?? ContentCollectionFilterControl.Pills;
                    filter.isMultipleSelection = editFilterType.value === "1";
                }

                bag.attributeFilters.splice(filterIndex, 1, filter);

                validProperties = ["attributeFilters"];
            }

            const box: DetailBlockBox<FilterSettingsBag, undefined> = {
                entity: bag,
                validProperties,
                isEditable: true
            };
            const data = {
                key: props.modelValue.idKey,
                box
            };

            const result = await invokeBlockAction<DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>>("SaveFilterSettings", data);

            if (!result.isSuccess || !result.data?.entity) {
                alert(result.errorMessage || "Unable to save filter settings.");
                return;
            }

            emit("update:modelValue", result.data.entity);
            isEditModalOpen.value = false;
        };

        // #endregion

        return {
            attributeFilters,
            editFilterControl,
            editFilterControlItems,
            editFilterEnabled,
            editFilterLabel,
            editFilterName,
            editFilterType,
            editFilterTypeItems,
            editShowFilterType,
            fullTextSearchEnabled,
            isEditFullTextSearch,
            isEditModalOpen,
            onEditAttributeFilter,
            onEditFullTextSearch,
            onEditYearFilter,
            onModalSave,
            yearSearchEnabled,
            yearSearchValues
        };
    },

    template: `
<Panel title="Search Filters">
    <SectionHeader title="Search Filters"
        description="The configuration below allows you to set various ways your collection can be filtered." />

    <SearchFilter :isEnabled="fullTextSearchEnabled"
        title="Full Text Search"
        description="Uses the content field of the content channel item or description of an Event Item."
        @edit="onEditFullTextSearch" />

    <SearchFilter :isEnabled="yearSearchEnabled"
        title="Year"
        description="Uses the content channel item's start date to determine the year of the content."
        :values="yearSearchValues"
        @edit="onEditYearFilter" />
    
    <SectionHeader title="Attribute Filters"
        description="The settings below allow you to provide filters for attributes that you have configured to add to your content collection."
        class="margin-t-lg" />

    <AttributeSearchFilter v-for="attribute in attributeFilters"
        :modelValue="attribute"
        @edit="onEditAttributeFilter" />
</Panel>

<Modal v-model="isEditModalOpen"
    title="Edit Search Filter"
    class="search-filter-modal"
    saveText="Save"
    @save="onModalSave">
    <h1>{{ editFilterName }}</h1>

    <div class="row">
        <div class="col-md-6">
            <InlineSwitch v-model="editFilterEnabled"
                label="Enable Filter" />

            <TextBox v-if="!isEditFullTextSearch"
                v-model="editFilterLabel"
                label="Filter Label"
                rules="required" />
        </div>
    </div>

    <div v-if="!isEditFullTextSearch" class="row">
        <div class="col-md-6">
            <RadioButtonList v-model="editFilterControl"
                label="Filter Control"
                horizontal
                :items="editFilterControlItems" />
        </div>

        <div class="col-md-6">
            <RadioButtonList v-if="editShowFilterType"
                v-model="editFilterType"
                label="Filter Type"
                horizontal
                :items="editFilterTypeItems" />
        </div>
    </div>
</Modal>
`
});
