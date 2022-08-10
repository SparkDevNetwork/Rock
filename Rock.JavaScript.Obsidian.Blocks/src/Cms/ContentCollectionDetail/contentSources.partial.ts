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

import { computed, defineComponent, onBeforeUnmount, PropType, ref, watch } from "vue";
import CheckBoxList from "@Obsidian/Controls/checkBoxList";
import DropDownList from "@Obsidian/Controls/dropDownList";
import Modal from "@Obsidian/Controls/modal";
import NumberBox from "@Obsidian/Controls/numberBox";
import Panel from "@Obsidian/Controls/panel";
import RockButton from "@Obsidian/Controls/rockButton";
import SectionHeader from "@Obsidian/Controls/sectionHeader";
import { DragReorder, useDragReorder } from "@Obsidian/Directives/dragDrop";
import { AvailableContentSourceBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/availableContentSourceBag";
import { ContentCollectionBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionBag";
import { ContentCollectionDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionDetailOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { alert, confirmDelete } from "@Obsidian/Utility/dialogs";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { ContentSourceBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentSourceBag";
import { EntityType } from "@Obsidian/SystemGuids";
import { updateRefValue } from "@Obsidian/Utility/component";
import { areEqual } from "@Obsidian/Utility/guid";
import Source from "./source.partial";

export default defineComponent({
    name: "Cms.ContentCollectionDetail.ContentSources",

    components: {
        CheckBoxList,
        DropDownList,
        Modal,
        NumberBox,
        Panel,
        RockButton,
        SectionHeader,
        Source
    },

    props: {
        /** The collection that contains the sources to display. */
        modelValue: {
            type: Object as PropType<ContentCollectionBag>,
            required: true
        }
    },

    directives: {
        DragReorder
    },

    emits: {
        "update:modelValue": (_value: ContentCollectionBag) => true
    },

    setup(props, { emit }) {
        // #region Values

        const invokeBlockAction = useInvokeBlockAction();

        const collectionSources = ref(props.modelValue.sources ?? []);

        // Values related to editing sources.
        const isAddSourceOpen = ref(false);
        const addSourceMenuRef = ref<HTMLElement | null>(null);
        const sourceEditBag = ref<ContentSourceBag | null>(null);
        const isSourceModalOpen = ref(false);
        const sourceSelectedEntity = ref("");
        const sourceSelectedEntityAttributes = ref<string[]>([]);
        const sourceEntityItems = ref<ListItemBag[]>([]);
        const sourceEntityAttributeTable = ref<Record<string, ListItemBag[]>>({});
        const sourceSelectedEntityOccurrences = ref<number | null>(null);

        // #endregion

        // #region Computed Values

        const addSourceDropdownClass = computed((): string => {
            return isAddSourceOpen.value ? "dropdown clearfix open" : "dropdown clearfix";
        });

        const sourceModalTitle = computed((): string => {
            return isAddingSource.value ? "Add Content Source" : "Edit Content Source";
        });

        const isAddingSource = computed((): boolean => {
            return !sourceEditBag.value?.guid;
        });

        const isSourceModalCalendar = computed((): boolean => {
            return areEqual(sourceEditBag.value?.entityTypeGuid, EntityType.EventCalendar);
        });

        const sourceModalEntityTitle = computed((): string => {
            if (areEqual(sourceEditBag.value?.entityTypeGuid, EntityType.ContentChannel)) {
                return "Content Channel";
            }
            else {
                return "Event Calendar";
            }
        });

        const sourceModalEntityName = computed((): string => {
            return sourceEditBag.value?.name ?? "";
        });

        const sourceEntityAttributeItems = computed((): ListItemBag[] => {
            return (sourceEntityAttributeTable.value[sourceSelectedEntity.value] ?? [])
                .map(li => {
                    return {
                        value: li.value,
                        text: li.category ? `${li.text} (${li.category})` : li.text
                    };
                });
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the person clicks outside the add source popup.
         * 
         * @param event The event that is being processed.
         */
        const onAddSourceWindowClick = (event: Event): void => {
            if (!(event.target instanceof HTMLElement) || !isAddSourceOpen.value) {
                return;
            }

            const menu = event.target?.closest(".dropdown-menu");

            // If they didn't click inside the menu popup then close the popup.
            if (menu !== addSourceMenuRef.value) {
                isAddSourceOpen.value = false;
            }
        };

        /** 
         * Event handler for toggling the add source popup.
         */
        const onAddSourceClick = (): void => {
            isAddSourceOpen.value = !isAddSourceOpen.value;
        };

        /**
         * Event handler for when the Add Calendar popup action is clicked.
         */
        const onAddCalendarSource = async (): Promise<void> => {
            isAddSourceOpen.value = false;

            // Get the event calendars that can be selected.
            const result = await invokeBlockAction<AvailableContentSourceBag[]>("GetAvailableEventCalendars");

            if (!result || !result.data) {
                alert(result.errorMessage || "Unable to get list of event calendars.");
                return;
            }

            // Set initial values to unselected.
            sourceSelectedEntity.value = "";
            sourceSelectedEntityAttributes.value = [];
            sourceSelectedEntityOccurrences.value = null;

            // Filter out any entities that are already a source.
            sourceEntityItems.value = result.data
                .filter(e => !collectionSources.value.some(s => areEqual(s.entityGuid, e.guid)))
                .map(e => ({
                    value: e.guid,
                    text: e.name
                }));

            sourceEntityAttributeTable.value = result.data.reduce((table, c) => {
                table[c.guid ?? ""] = c.attributes ?? [];
                return table;
            }, {} as Record<string, ListItemBag[]>);

            sourceEditBag.value = {
                entityTypeGuid: EntityType.EventCalendar,
                occurrencesToShow: 0,
                itemCount: 0
            };

            isSourceModalOpen.value = true;
        };

        /**
         * Event handler for when the Add Content Channel popup action is clicked.
         */
        const onAddContentChannelSource = async (): Promise<void> => {
            isAddSourceOpen.value = false;

            // Get the content channels that can be selected.
            const result = await invokeBlockAction<AvailableContentSourceBag[]>("GetAvailableContentChannels");

            if (!result || !result.data) {
                alert(result.errorMessage || "Unable to get list of content channels.");
                return;
            }

            // Set initial values to unselected.
            sourceSelectedEntity.value = "";
            sourceSelectedEntityAttributes.value = [];
            sourceSelectedEntityOccurrences.value = null;

            sourceEntityItems.value = result.data.map(c => ({
                value: c.guid,
                text: c.name
            }));

            // Filter out any entities that are already a source.
            sourceEntityItems.value = result.data
                .filter(e => !collectionSources.value.some(s => areEqual(s.entityGuid, e.guid)))
                .map(e => ({
                    value: e.guid,
                    text: e.name
                }));

            sourceEntityAttributeTable.value = result.data.reduce((table, c) => {
                table[c.guid ?? ""] = c.attributes ?? [];
                return table;
            }, {} as Record<string, ListItemBag[]>);

            sourceEditBag.value = {
                entityTypeGuid: EntityType.ContentChannel,
                occurrencesToShow: 0,
                itemCount: 0
            };

            isSourceModalOpen.value = true;
        };

        const onSourceSave = async (): Promise<void> => {
            if (!sourceEditBag.value) {
                return;
            }

            const bag: ContentSourceBag = {
                guid: sourceEditBag.value?.guid,
                entityTypeGuid: sourceEditBag.value.entityTypeGuid,
                entityGuid: sourceSelectedEntity.value,
                attributes: sourceSelectedEntityAttributes.value.map(a => ({value: a})),
                occurrencesToShow: sourceSelectedEntityOccurrences.value ?? 0,
                itemCount: 0
            };

            const data = {
                key: props.modelValue?.idKey,
                bag
            };

            const result = await invokeBlockAction<DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>>("SaveCollectionSource", data);

            if (!result || !result.isSuccess || !result.data?.entity) {
                alert(result.errorMessage || "Unable to save source.");
                return;
            }

            emit("update:modelValue", result.data.entity);
            isSourceModalOpen.value = false;
        };

        /**
         * Event handler for when the person wants to delete the specified
         * source from the collection.
         * 
         * @param source The source that should be deleted from the collection.
         */
        const onDeleteSource = async (source: ContentSourceBag): Promise<void> => {
            if (!(await confirmDelete("Collection Source"))) {
                return;
            }

            const data = {
                key: props.modelValue?.idKey,
                sourceGuid: source.guid
            };

            const result = await invokeBlockAction<DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>>("DeleteCollectionSource", data);

            if (!result || !result.isSuccess || !result.data?.entity) {
                alert(result.errorMessage || "Unable to delete the collection source.");
                return;
            }

            emit("update:modelValue", result.data.entity);
        };

        /**
         * Event handler for when the person attempts to edit a collection source.
         * 
         * @param source The source that should be edited.
         */
        const onEditSource = async (source: ContentSourceBag): Promise<void> => {
            let availableContent: AvailableContentSourceBag[];

            if (areEqual(source.entityTypeGuid, EntityType.ContentChannel)) {
                // Get the content channel data so we have the updated list of
                // available attributes.
                const result = await invokeBlockAction<AvailableContentSourceBag[]>("GetAvailableContentChannels");

                if (!result || !result.data) {
                    alert(result.errorMessage || "Unable to get list of content channels.");
                    return;
                }

                availableContent = result.data;
            }
            else if (areEqual(source.entityTypeGuid, EntityType.EventCalendar)) {
                // Get the event calendar data so we have the updated list of
                // available attributes.
                const result = await invokeBlockAction<AvailableContentSourceBag[]>("GetAvailableEventCalendars");

                if (!result || !result.data) {
                    alert(result.errorMessage || "Unable to get list of event calendars.");
                    return;
                }

                availableContent = result.data;
            }
            else {
                return;
            }

            // Set initial selections in the UI.
            sourceSelectedEntity.value = source.entityGuid ?? "";
            sourceSelectedEntityAttributes.value = source.attributes?.map(v => v.value ?? "") ?? [];
            sourceSelectedEntityOccurrences.value = source.occurrencesToShow > 0 ? source.occurrencesToShow : null;

            sourceEntityAttributeTable.value = availableContent.reduce((table, c) => {
                table[c.guid ?? ""] = c.attributes ?? [];
                return table;
            }, {} as Record<string, ListItemBag[]>);

            sourceEditBag.value = source;

            isSourceModalOpen.value = true;
        };

        /**
         * Event Handler for when the sources have been re-ordered by the person
         * via a drag and drop action.
         * 
         * @param value The value that was moved in the list.
         * @param beforeValue The value it was placed before or null if end of list.
         */
        const onSourceReorder = async (value: ContentSourceBag, beforeValue: ContentSourceBag | null): Promise<void> => {
            const data = {
                key: props.modelValue.idKey,
                guid: value.guid,
                beforeGuid: beforeValue?.guid ?? null
            };

            const result = await invokeBlockAction("ReorderSource", data);

            if (!result.isSuccess) {
                alert(result.errorMessage || "Unable to re-order sources, you might need to reload the page.");
                return;
            }
        };

        // #endregion

        const reorderDragOptions = useDragReorder(collectionSources, onSourceReorder);

        watch(() => props.modelValue, () => {
            updateRefValue(collectionSources, props.modelValue?.sources ?? []);
        });

        // Watch for when our add source popup is open or closed and add/remove
        // the event handler to auto-close.
        watch(isAddSourceOpen, () => {
            if (isAddSourceOpen.value) {
                window.addEventListener("click", onAddSourceWindowClick);
            }
            else {
                window.removeEventListener("click", onAddSourceWindowClick);
            }
        });

        // Ensure our click handler is cleaned up.
        onBeforeUnmount(() => {
            window.removeEventListener("click", onAddSourceWindowClick);
        });

        return {
            addSourceDropdownClass,
            addSourceMenuRef,
            isAddingSource,
            isSourceModalCalendar,
            isSourceModalOpen,
            collectionSources,
            onAddCalendarSource,
            onAddContentChannelSource,
            onAddSourceClick,
            onSourceSave,
            onDeleteSource,
            onEditSource,
            reorderDragOptions,
            sourceEntityAttributeItems,
            sourceEntityItems,
            sourceModalEntityName,
            sourceModalTitle,
            sourceModalEntityTitle,
            sourceSelectedEntity,
            sourceSelectedEntityAttributes,
            sourceSelectedEntityOccurrences
        };
    },

    template: `
<Panel title="Sources">
    <SectionHeader title="Content Sources" description="Content for this collection will be pulled from the following sources within Rock.">
        <template #actions>
            <div :class="addSourceDropdownClass">
                <button type="button" class="btn btn-default btn-sm dropdown-toggle" @click.prevent.stop="onAddSourceClick">
                    <i class="fa fa-plus"></i> <span class="caret"></span>
                </button>

                <ul ref="addSourceMenuRef" class="dropdown-menu dropdown-menu-right">
                    <li><a href="#" @click.prevent="onAddContentChannelSource">Add Content Channel</a></li>
                    <li><a href="#" @click.prevent="onAddCalendarSource">Add Calendar</a></li>
                </ul>
            </div>
        </template>
    </SectionHeader>

    <div class="collection-content-sources" v-drag-reorder="reorderDragOptions">
        <Source v-for="source in collectionSources" :key="source.guid" v-model="source" @delete="onDeleteSource" @edit="onEditSource" />
    </div>
</Panel>

<Modal v-model="isSourceModalOpen"
    :title="sourceModalTitle"
    class="content-source-modal"
    saveText="Save"
    @save="onSourceSave">
    <h1 v-if="!isAddingSource">{{ sourceModalEntityName }}</h1>
    <div v-else class="row">
        <div class="col-md-6">
            <DropDownList v-if="isAddingSource"
                v-model="sourceSelectedEntity"
                :label="sourceModalEntityTitle"
                :items="sourceEntityItems"
                rules="required" />
        </div>
    </div>

    <div class="row" v-if="sourceSelectedEntity">
        <div class="col-md-6">
            <CheckBoxList v-model="sourceSelectedEntityAttributes"
                label="Attributes to Include"
                help="Determines which attributes should be added to the collection index for search and retrieval."
                :items="sourceEntityAttributeItems" />
        </div>

        <div class="col-md-6">
            <NumberBox v-if="isSourceModalCalendar"
                v-model="sourceSelectedEntityOccurrences"
                label="Number of Future Occurrences to Show"
                rules="gte:1" />
        </div>
    </div>
</Modal>
`
});
