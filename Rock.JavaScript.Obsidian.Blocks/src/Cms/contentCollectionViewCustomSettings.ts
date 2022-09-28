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

import { computed, defineComponent, ref, watch } from "vue";
import Alert from "@Obsidian/Controls/alert";
import CheckBox from "@Obsidian/Controls/checkBox";
import CheckBoxList from "@Obsidian/Controls/checkBoxList";
import CodeEditor from "@Obsidian/Controls/codeEditor";
import DropDownList from "@Obsidian/Controls/dropDownList";
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox";
import LoadingIndicator from "@Obsidian/Controls/loadingIndicator";
import Modal from "@Obsidian/Controls/modal";
import NumberBox from "@Obsidian/Controls/numberBox";
import RockButton from "@Obsidian/Controls/rockButton";
import RockFormFieldError from "@Obsidian/Controls/rockFormFieldError";
import SectionHeader from "@Obsidian/Controls/sectionHeader";
import TextBox from "@Obsidian/Controls/textBox";
import { getSecurityGrant, provideSecurityGrant, setCustomSettingsBoxValue, useInvokeBlockAction, useReloadBlock } from "@Obsidian/Utility/block";
import { CustomSettingsBox } from "@Obsidian/ViewModels/Blocks/customSettingsBox";
import { CustomSettingsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/customSettingsBag";
import { FilterOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/filterOptionsBag";
import { CustomSettingsOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/customSettingsOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import FilterGrid from "./ContentCollectionView/filterGrid.partial";
import { SortOrdersKey } from "./ContentCollectionView/types";
import { alert } from "@Obsidian/Utility/dialogs";
import { areEqual } from "@Obsidian/Utility/guid";
import { ContentCollectionListItemBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/contentCollectionListItemBag";


/** The items that can be picked from the Enabled Sort Orders list. */
const enabledSortOrdersItems: ListItemBag[] = [
    {
        value: SortOrdersKey.Relevance,
        text: "Relevance"
    },
    {
        value: SortOrdersKey.Newest,
        text: "Newest"
    },
    {
        value: SortOrdersKey.Oldest,
        text: "Oldest"
    },
    {
        value: SortOrdersKey.Trending,
        text: "Trending"
    },
    {
        value: SortOrdersKey.Alphabetical,
        text: "Alphabetical"
    }
];

export default defineComponent({
    name: "Cms.ContentCollectionView.CustomSettings",

    components: {
        Alert,
        CheckBox,
        CheckBoxList,
        CodeEditor,
        DropDownList,
        FilterGrid,
        InlineCheckBox,
        LoadingIndicator,
        Modal,
        NumberBox,
        RockButton,
        RockFormFieldError,
        SectionHeader,
        TextBox
    },

    emits: {
        "close": () => true
    },

    setup(props, { emit }) {
        const invokeBlockAction = useInvokeBlockAction();
        const securityGrant = getSecurityGrant(null);
        const reloadBlock = useReloadBlock();

        // #region Values

        const errorMessage = ref("");

        const isLoading = ref(true);
        const isModalOpen = ref(true);
        const contentCollection = ref("");
        const contentCollectionItems = ref<ContentCollectionListItemBag[]>([]);
        const groupResultsBySource = ref(false);
        const searchOnLoad = ref(false);
        const numberOfResults = ref<number | null>(null);
        const showFiltersPanel = ref(false);
        const showFullTextSearch = ref(false);
        const showSort = ref(false);
        const enabledSortOrders = ref<string[]>([]);
        const trendingTerm = ref("");
        const filters = ref<FilterOptionsBag[]>([]);
        const resultsTemplate = ref("");
        const itemTemplate = ref("");
        const preSearchTemplate = ref("");
        const boostMatchingSegments = ref(false);
        const boostMatchingRequestFilters = ref(false);
        const segmentBoostAmount = ref<number | null>(null);
        const requestFilterBoostAmount = ref<number | null>(null);

        const editingFilter = ref<FilterOptionsBag | null>(null);
        const editingFilterHeaderMarkup = ref("");

        // #endregion

        // #region Computed Values

        const saveButtonText = computed((): string => {
            return errorMessage.value || !isLoading.value ? "Save" : "";
        });

        const isTrendingSortEnabled = computed((): boolean => {
            return enabledSortOrders.value.includes(SortOrdersKey.Trending);
        });

        // #endregion

        // #region Functions

        /**
         * Begins loading the current settings in the background so that the UI
         * can be displayed.
         */
        const startLoading = async (): Promise<void> => {
            const result = await invokeBlockAction<CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>>("GetCustomSettings");

            if (result.isSuccess && result.data && result.data.settings && result.data.options) {
                // Set the values for the UI.
                contentCollection.value = result.data.settings.contentCollection ?? "";
                showFiltersPanel.value = result.data.settings.showFiltersPanel;
                showFullTextSearch.value = result.data.settings.showFullTextSearch;
                showSort.value = result.data.settings.showSort;
                numberOfResults.value = result.data.settings.numberOfResults ?? null;
                searchOnLoad.value = result.data.settings.searchOnLoad;
                groupResultsBySource.value = result.data.settings.groupResultsBySource;
                enabledSortOrders.value = result.data.settings.enabledSortOrders ?? [];
                trendingTerm.value = result.data.settings.trendingTerm ?? "";
                filters.value = result.data.settings.filters ?? [];
                resultsTemplate.value = result.data.settings.resultsTemplate ?? "";
                itemTemplate.value = result.data.settings.itemTemplate ?? "";
                preSearchTemplate.value = result.data.settings.preSearchTemplate ?? "";
                boostMatchingSegments.value = result.data.settings.boostMatchingSegments;
                boostMatchingRequestFilters.value = result.data.settings.boostMatchingRequestFilters;
                segmentBoostAmount.value = result.data.settings.segmentBoostAmount ?? null;
                requestFilterBoostAmount.value = result.data.settings.requestFilterBoostAmount ?? null;

                // Set any additional information required by the UI to paint the
                // custom settings interface.
                securityGrant.updateToken(result.data.securityGrantToken);
                contentCollectionItems.value = result.data.options.contentCollectionItems ?? [];
            }
            else {
                errorMessage.value = result.errorMessage || "Unknown error while loading custom settings.";
            }

            isLoading.value = false;
        };

        // #endregion

        // #region Event Handlers

        const onEditFilter = (filterName: string): void => {
            editingFilter.value = filters.value.find(f => f.name === filterName) ?? null;
            editingFilterHeaderMarkup.value = editingFilter.value?.headerMarkup ?? "";
        };

        const onEditFilterCancel = (): void => {
            editingFilter.value = null;
            editingFilterHeaderMarkup.value = "";
        };

        const onEditFilterSave = (): void => {
            if (editingFilter.value) {
                editingFilter.value.headerMarkup = editingFilterHeaderMarkup.value;
                editingFilter.value = null;
            }
        };

        /**
         * Event handler for when the person clicks the Save button and all
         * components have validated their values.
         */
        const onSave = async (): Promise<void> => {
            const box: CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> = {};

            setCustomSettingsBoxValue(box, "contentCollection", contentCollection.value);
            setCustomSettingsBoxValue(box, "showFiltersPanel", showFiltersPanel.value);
            setCustomSettingsBoxValue(box, "showFullTextSearch", showFullTextSearch.value);
            setCustomSettingsBoxValue(box, "showSort", showSort.value);
            setCustomSettingsBoxValue(box, "numberOfResults", numberOfResults.value);
            setCustomSettingsBoxValue(box, "searchOnLoad", searchOnLoad.value);
            setCustomSettingsBoxValue(box, "groupResultsBySource", groupResultsBySource.value);
            setCustomSettingsBoxValue(box, "enabledSortOrders", enabledSortOrders.value);
            setCustomSettingsBoxValue(box, "trendingTerm", trendingTerm.value);
            setCustomSettingsBoxValue(box, "filters", filters.value);
            setCustomSettingsBoxValue(box, "resultsTemplate", resultsTemplate.value);
            setCustomSettingsBoxValue(box, "itemTemplate", itemTemplate.value);
            setCustomSettingsBoxValue(box, "preSearchTemplate", preSearchTemplate.value);
            setCustomSettingsBoxValue(box, "boostMatchingSegments", boostMatchingSegments.value);
            setCustomSettingsBoxValue(box, "boostMatchingRequestFilters", boostMatchingRequestFilters.value);
            setCustomSettingsBoxValue(box, "segmentBoostAmount", segmentBoostAmount.value);
            setCustomSettingsBoxValue(box, "requestFilterBoostAmount", requestFilterBoostAmount.value);

            const data = {
                box
            };

            const result = await invokeBlockAction("SaveCustomSettings", data);

            if (result.isSuccess) {
                isModalOpen.value = false;
                reloadBlock();
            }
            else {
                alert(result.errorMessage || "Unable to save block settings."); 
            }
        };

        // #endregion

        provideSecurityGrant(securityGrant);

        watch(isModalOpen, () => {
            if (!isModalOpen.value) {
                emit("close");
            }
        });

        watch(contentCollection, () => {
            const collection = contentCollectionItems.value.find(l => areEqual(l.value, contentCollection.value));
            const newFilters = [...filters.value];

            if (!collection) {
                console.log("no selection");
                return;
            }

            const collectionFilters = collection.filters ?? [];

            // Check for any filters on the selected collection that don't already
            // exist in the array of filters.
            for (const f of collectionFilters) {
                if (!newFilters.some(a => a.sourceKey === f.value)) {
                    console.log("filters missing", f, newFilters);
                    newFilters.push({
                        show: false,
                        sourceKey: f.value,
                        name: f.text,
                        headerMarkup: ""
                    });
                }
            }

            // Check for any filters that are not in the selected collection
            // that should be removed.
            for (let filterIndex = 0; filterIndex < newFilters.length;) {
                if (!collectionFilters.some(f => f.value === newFilters[filterIndex].sourceKey)) {
                    newFilters.splice(filterIndex, 1);
                }
                else {
                    filterIndex++;
                }
            }

            filters.value = newFilters;
        });

        startLoading();

        return {
            boostMatchingRequestFilters,
            boostMatchingSegments,
            contentCollection,
            contentCollectionItems,
            editingFilter,
            editingFilterHeaderMarkup,
            enabledSortOrders,
            enabledSortOrdersItems,
            errorMessage,
            filters,
            groupResultsBySource,
            isLoading,
            isModalOpen,
            isTrendingSortEnabled,
            itemTemplate,
            numberOfResults,
            onEditFilter,
            onEditFilterCancel,
            onEditFilterSave,
            onSave,
            preSearchTemplate,
            requestFilterBoostAmount,
            saveButtonText,
            searchOnLoad,
            segmentBoostAmount,
            showFiltersPanel,
            showFullTextSearch,
            showSort,
            resultsTemplate,
            trendingTerm
        };
    },

    template: `
<Modal v-model="isModalOpen"
    title="Content Collection View Settings"
    :saveText="saveButtonText"
    @save="onSave">

    <Alert v-if="errorMessage"
        v-text="errorMessage"
        alertType="warning" />

    <LoadingIndicator v-else-if="isLoading" :delay="500" />

    <div v-else>

        <div class="row">
            <div class="col-md-6">
                <DropDownList v-model="contentCollection"
                    label="Content Collection"
                    :items="contentCollectionItems"
                    rules="required" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="showFiltersPanel"
                    label="Show Filters Panel" />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="showFullTextSearch"
                    label="Show Full-Text Search" />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="showSort"
                    label="Show Sort" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <NumberBox v-model="numberOfResults"
                    rules="gte:0"
                    label="Number of Results" />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="searchOnLoad"
                    label="Search On Load"
                    help="Determines if initial content should be shown when the block is loaded." />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="groupResultsBySource"
                    label="Group Results By Source"
                    help="This will group the results by the source. When enabled the number of results will be used for each source type." />
            </div>
        </div>

        <CheckBoxList v-if="showSort"
            v-model="enabledSortOrders"
            label="Enabled Sort Orders"
            help="Determines the sort options that should be made available."
            horizontal
            :repeatColumns="5"
            :items="enabledSortOrdersItems" />

        <div class="row">
            <div class="col-md-6">
                <TextBox v-if="isTrendingSortEnabled"
                    v-model="trendingTerm"
                    label="Trending Term"
                    rules="required"
                    help="The term that should be used in the sort dropdown to describe popular/trending items." />
            </div>
        </div>

        <CodeEditor v-model="resultsTemplate"
            label="Results Template"
            mode="lava"
            rules="required" />

        <CodeEditor v-model="itemTemplate"
            label="Item Template"
            mode="lava"
            rules="required" />

        <CodeEditor v-model="preSearchTemplate"
            label="Pre-Search Template"
            mode="lava" />

        <div v-if="showFiltersPanel">
            <SectionHeader title="Filters"
                description="Determine which filters you would like to show in what order." />

            <div v-if="editingFilter" class="margin-b-md">
                <h3 class="title">{{ editingFilter.name }} Filter</h3>

                <CodeEditor v-model="editingFilterHeaderMarkup"
                    label="Filter Header Markup"
                    mode="lava" />

                <RockFormFieldError label="Filter" error="You must finish editing the filter before proceeding." />

                <div class="actions">
                    <RockButton btnType="primary" btnSize="xs" @click="onEditFilterSave">Save</RockButton>
                    <RockButton btnType="link" btnSize="xs" @click="onEditFilterCancel">Cancel</RockButton>
                </div>
            </div>

            <FilterGrid v-else v-model="filters" @edit="onEditFilter" />
        </div>

        <SectionHeader title="Personalization"
            description="The settings below allow you to boost items based on personalization settings in Rock." />
        
        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="boostMatchingSegments"
                    label="Boost Matching Segments"
                    help="Determines if the search should boost shared segments between the individual and the content results." />

                <NumberBox v-if="boostMatchingSegments"
                    v-model="segmentBoostAmount"
                    label="Segment Boost Amount"
                    help="The amount of boost to apply to each shared segment. A value of 1 = no boost, a value of > 1 will increase the match score while a value of < 1 will reduce the match score." />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="boostMatchingRequestFilters"
                    label="Boost Matching Request Filters"
                    help="Determines if the search should boost shared segments current request and the content results." />

                <NumberBox v-if="boostMatchingRequestFilters"
                    v-model="requestFilterBoostAmount"
                    label="Request Filter Boost Amount"
                    help="The amount of boost to apply to each shared request filter. A value of 1 = no boost, a value of > 1 will increase the match score while a value of < 1 will reduce the match score." />
            </div>
        </div>

    </div>

</Modal>
`
});
