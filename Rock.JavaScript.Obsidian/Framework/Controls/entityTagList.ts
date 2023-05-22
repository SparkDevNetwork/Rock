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
import { useHttp } from "@Obsidian/Utility/http";
import { useSuspense } from "@Obsidian/Utility/suspense";
import { ControlLazyMode } from "@Obsidian/Enums/Controls/controlLazyMode";
import { EntityTagListAddEntityTagOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListAddEntityTagOptionsBag";
import { EntityTagListCreatePersonalTagOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListCreatePersonalTagOptionsBag";
import { EntityTagListGetEntityTagsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListGetEntityTagsOptionsBag";
import { EntityTagListGetAvailableTagsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListGetAvailableTagsOptionsBag";
import { EntityTagListRemoveEntityTagOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListRemoveEntityTagOptionsBag";
import { EntityTagListTagBag } from "@Obsidian/ViewModels/Rest/Controls/entityTagListTagBag";
import { AutoComplete } from "ant-design-vue";
import { computed, defineComponent, nextTick, PropType, Ref, ref, watch } from "vue";
import { useSecurityGrantToken } from "@Obsidian/Utility/block";
import { alert, confirm } from "@Obsidian/Utility/dialogs";
import { HttpFunctions } from "@Obsidian/Types/Utility/http";

/** The type definition for a select option, since the ones from the library are wrong. */
type SelectOption = {
    value?: string;

    label: string;

    options?: SelectOption[];
};

/** Helper component to display a single tag. */
const tag = defineComponent({
    name: "EntityTagList.Tag",

    props: {
        modelValue: {
            type: Object as PropType<EntityTagListTagBag>,
            required: true
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "removeTag": (_value: string) => true
    },

    setup(props, { emit }) {
        const text = computed((): string => {
            return props.modelValue.name ?? "";
        });

        const iconCssClass = computed((): string => {
            return props.modelValue.iconCssClass ?? "";
        });

        const tagClass = computed((): string => {
            return props.modelValue.isPersonal ? "tag personal" : "tag";
        });

        const tagStyle = computed((): Record<string, string> => {
            const styles = {};

            if (props.modelValue.backgroundColor) {
                styles["background-color"] = props.modelValue.backgroundColor;
            }

            return styles;
        });

        const onRemoveTag = (): void => {
            emit("removeTag", props.modelValue.idKey ?? "");
        };

        return {
            iconCssClass,
            onRemoveTag,
            tagClass,
            tagStyle,
            text
        };
    },

    template: `
<span :class="tagClass" :style="tagStyle">
    <span v-if="iconCssClass" class="tag-icon">
        <i :class="iconCssClass"></i>
    </span>
    <span>{{ text }}</span>
    <a v-if="!disabled" href="#" title="Remove tag" @click.prevent.stop="onRemoveTag">x</a>
</span>
`
});

/**
 * Get the existing tags on an entity.
 *
 * @param entityTypeGuid The unique identifier of the entity type.
 * @param entityKey The identifier key of the entity.
 * @param securityToken The security token to grant additional access.
 *
 * @returns A promise to an array of EntityTagListTagBag objects with the existing tag information.
 */
async function getEntityTags(http: HttpFunctions, entityTypeGuid: Guid, entityKey: string, securityToken: string | null): Promise<EntityTagListTagBag[]> {
    const data: EntityTagListGetEntityTagsOptionsBag = {
        entityTypeGuid: entityTypeGuid,
        entityKey: entityKey,
        securityGrantToken: securityToken
    };

    const result = await http.post<EntityTagListTagBag[]>("/api/v2/Controls/EntityTagListGetEntityTags", undefined, data);

    if (result.isSuccess && result.data) {
        return result.data;
    }

    return [];
}

export default defineComponent({
    name: "EntityTagList",

    components: {
        AutoComplete,
        Tag: tag
    },

    props: {
        /** The unique identifier of the entity type described by entityKey. */
        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /** The identifier key for the entity whose tags should be displayed. */
        entityKey: {
            type: String as PropType<string>,
            required: false
        },

        /** The optional category unique identifier to limit tags to. */
        categoryGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /** Determines if this control should delay page rendering until the initial tag data is loaded. */
        lazyMode: {
            type: String as PropType<ControlLazyMode>,
            default: ControlLazyMode.Lazy
        },

        /** If true then the tag list will be read only. */
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
    },

    setup(props) {
        // #region Values

        const securityToken = useSecurityGrantToken();
        const http = useHttp();
        const currentTags = ref<EntityTagListTagBag[]>([]);
        const searchValue = ref("");
        const searchOptions = ref<SelectOption[]>([]);
        const isNewTagVisible = ref(false);
        const tagsInputRef = ref<HTMLElement | null>(null);
        let loadCancelledToken: Ref<boolean> | null = null;
        let searchCancelledToken: Ref<boolean> | null = null;
        let isAddNewTagCancelled: boolean = false;

        // #endregion

        // #region Functions

        /**
         * Finds an existing tag with the given name and returns it.
         *
         * @param name The name of the tag to find on the server.
         *
         * @returns An object that contains the tag information or null if no matching tag was found.
         */
        const getTagByName = async (name: string): Promise<EntityTagListTagBag | null> => {
            const data: EntityTagListGetAvailableTagsOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                categoryGuid: props.categoryGuid,
                name: name,
                securityGrantToken: securityToken.value
            };

            const result = await http.post<EntityTagListTagBag[]>("/api/v2/Controls/EntityTagListGetAvailableTags", undefined, data);

            if (result.isSuccess && result.data) {
                // Filter the matching tags to find one that matches the tag name
                // exactly rather than a prefix match.
                const tags = result.data.filter(t => t.name?.toLowerCase() === name.toLowerCase());

                if (tags.length >= 1) {
                    return tags[0];
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        };

        /**
         * Creates a new personal tag on the server with the given tag name.
         *
         * @param name The name of the tag to be created.
         *
         * @returns An object that contains the tag information that was created or null if one couldn't be created.
         */
        const createPersonalTag = async (name: string): Promise<EntityTagListTagBag | null> => {
            const data: EntityTagListCreatePersonalTagOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                categoryGuid: props.categoryGuid,
                name: name,
                securityGrantToken: securityToken.value
            };

            const result = await http.post<EntityTagListTagBag>("/api/v2/Controls/EntityTagListCreatePersonalTag", undefined, data);

            // An OK and CONFLICT both will return a valid tag.
            if ((result.isSuccess || result.statusCode === 409) && result.data) {
                return result.data;
            }
            else {
                return null;
            }
        };

        /**
         * Add an existing tag to the entity.
         *
         * @param tagKey The key identifier of the tag to be added.
         */
        const addTag = async (tagKey: string): Promise<void> => {
            const data: EntityTagListAddEntityTagOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                tagKey: tagKey,
                securityGrantToken: securityToken.value
            };

            const result = await http.post<EntityTagListTagBag>("/api/v2/Controls/EntityTagListAddEntityTag", undefined, data);

            if (result.isSuccess && result.data) {
                const newTags = [...currentTags.value];
                newTags.push(result.data);
                newTags.sort((a, b) => (a.name ?? "").localeCompare(b.name ?? ""));

                currentTags.value = newTags;
                searchValue.value = "";
            }
            else {
                alert(result.errorMessage ?? "Unable to add tag.");
            }
        };

        /**
         * Remove an existing tag from the entity.
         *
         * @param tagKey The identifier key of the tag to be removed.
         */
        const removeTag = async (tagKey: string): Promise<void> => {
            const data: EntityTagListRemoveEntityTagOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                tagKey: tagKey,
                securityGrantToken: securityToken.value
            };

            const result = await http.post<EntityTagListTagBag>("/api/v2/Controls/EntityTagListRemoveEntityTag", undefined, data);

            if (result.isSuccess) {
                const newTags = currentTags.value.filter(t => t.idKey !== tagKey);

                currentTags.value = newTags;
            }
            else {
                alert(result.errorMessage ?? "Unable to remove tag.");
            }
        };

        /**
         * Add a tag by name to the entity. If the tag doesn't exist the user
         * will be prompted to create a new personal tag.
         *
         * @param tagName The name of the tag to be added.
         */
        const addNamedTag = async (tagName: string): Promise<void> => {
            let tag = await getTagByName(tagName);

            if (tag === null) {
                if (!await confirm(`A tag called "${tagName}" does not exist. Do you want to create a new personal tag?`)) {
                    return;
                }

                tag = await createPersonalTag(tagName);

                if (tag === null) {
                    await alert("Unable to create personal tag.");
                    return;
                }
            }

            await addTag(tag.idKey ?? "");
        };

        /**
         * Start loading existing tags for the entity described in our properties.
         */
        const loadExistingTags = async (): Promise<void> => {
            // Cancel any previous load that hasn't completed.
            if (loadCancelledToken) {
                loadCancelledToken.value = true;
            }

            if (props.entityTypeGuid && props.entityKey) {
                // Start a new cancellation request.
                const cancelled = ref(false);
                loadCancelledToken = cancelled;

                const tags = await getEntityTags(http, props.entityTypeGuid, props.entityKey, securityToken.value);

                // If we haven't been cancelled, then set the value.
                if (!cancelled.value) {
                    currentTags.value = tags;
                }
            }
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called when the user selects an existing tag from the popup list.
         *
         * @param value The value of the tag that was selected.
         */
        const onSelect = (value: string): void => {
            isAddNewTagCancelled = true;
            const item = searchOptions.value.filter(o => o.value === value);

            if (item.length === 0) {
                return;
            }

            // Replace the typed in value with the friendly label otherwise the
            // text box is filled in with the value key.
            searchValue.value = item[0].label;
            addTag(item[0].value ?? "");
        };

        /**
         * Called when an autocomplete search operation should start.
         *
         * @param value The value that has been typed so far that should be searched for.
         */
        const onSearch = async (value: string): Promise<void> => {
            // Cancel any previous search that hasn't completed.
            if (searchCancelledToken) {
                searchCancelledToken.value = true;
            }

            if (!value) {
                return;
            }

            // Start a new cancellation request.
            const cancelled = ref(false);
            searchCancelledToken = cancelled;

            const data: EntityTagListGetAvailableTagsOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                name: value
            };

            const result = await http.post<EntityTagListTagBag[]>("/api/v2/Controls/EntityTagListGetAvailableTags", undefined, data);

            if (result.isSuccess && result.data) {
                searchOptions.value = result.data.map(t => {
                    return {
                        value: t.idKey ?? "",
                        label: t.name ?? ""
                    };
                });
            }
        };

        /**
         * Called when a key has been pressed while the tag search field has focus.
         *
         * @param ev The object that describes the event being handled.
         */
        const onInputKeyDown = (ev: KeyboardEvent): void => {
            if (ev.keyCode === 13 && searchValue.value) {
                const tagName = searchValue.value;
                isAddNewTagCancelled = false;

                // As horrible as this is, there doesn't seem to be a way to determine
                // the difference between hitting enter with something in the popup
                // selected vs adding a new word. So the delay gives a chance for
                // the select event to fire if there is something in the popup.
                setTimeout(() => {
                    if (!isAddNewTagCancelled) {
                        addNamedTag(tagName);
                    }
                }, 1);
            }
        };

        /**
         * Called when the remove button for an existing tag has been clicked.
         *
         * @param tagKey The identifier key of the tag to be removed.
         */
        const onRemoveTag = async (tagKey: string): Promise<void> => {
            await removeTag(tagKey);
        };

        /**
         * Event handler for when the "+" button is clicked to begin the process
         * of adding new tags to the entity.
         */
        const onAddNewTagsClick = (): void => {
            isNewTagVisible.value = true;

            // After the UI updates, put the keyboard focus on the input box.
            nextTick(() => {
                tagsInputRef.value?.focus();
            });
        };

        // #endregion

        // Watch for changes in our configuration that would require us to reload
        // all the tags.
        watch([() => props.entityTypeGuid, () => props.entityKey, () => props.categoryGuid], () => {
            loadExistingTags();
        });

        // Begin loading the tags in either eager or lazy mode.
        if (props.lazyMode === ControlLazyMode.Eager) {
            useSuspense()?.addOperation(loadExistingTags());
        }
        else {
            loadExistingTags();
        }

        return {
            currentTags,
            isNewTagVisible,
            onAddNewTagsClick,
            onInputKeyDown,
            onRemoveTag,
            onSearch,
            onSelect,
            searchOptions,
            searchValue,
            tagsInputRef
        };
    },

    template: `
<div class="taglist">
    <v-style>
.taglist .ant-select-auto-complete.ant-select {
    width: 125px;
}

.taglist .ant-select-auto-complete.ant-select > .ant-select-selector {
    border: 0px;
    padding: 0px;
    height: 22px;
    font-size: 12px;
    background: transparent;
}

.taglist .ant-select-auto-complete.ant-select-focused.ant-select > .ant-select-selector,
.taglist .ant-select-auto-complete.ant-select > .ant-select-selector:hover {
    border: 0px;
    box-shadow: initial;
    background: rgba(0,0,0,0.05);
}

.taglist .ant-select-auto-complete.ant-select > .ant-select-selector .ant-select-selection-search {
    left: 6px;
    right: 6px;
}

.taglist .ant-select-auto-complete.ant-select > .ant-select-selector .ant-select-selection-placeholder {
    position: absolute;
    margin-left: 6px;
    line-height: 22px;
}

.taglist .ant-select-auto-complete.ant-select .ant-select-selection-search-input {
    height: 22px;
}

.taglist .add-new-tags {
    float: left;
    height: 22px;
    font-size: 0.7em;
    line-height: 22px;
}
    </v-style>

    <div class="tag-wrap">
        <div class="tagsinput">
            <Tag v-for="tag in currentTags"
                :key="tag.value"
                :modelValue="tag"
                :disabled="disabled"
                @removeTag="onRemoveTag" />

                <template v-if="!disabled">
                    <AutoComplete v-if="isNewTagVisible"
                        ref="tagsInputRef"
                        v-model:value="searchValue"
                        :options="searchOptions"
                        placeholder="tag name"
                        @select="onSelect"
                        @search="onSearch"
                        @inputKeyDown="onInputKeyDown" />

                    <span v-else class="text-muted add-new-tags clickable" @click="onAddNewTagsClick">
                        <i class="fa fa-plus"></i>
                    </span>
                </template>
        </div>
    </div>
</div>
`
});
