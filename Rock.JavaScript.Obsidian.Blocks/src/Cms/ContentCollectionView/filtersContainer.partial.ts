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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import CheckBoxList from "@Obsidian/Controls/checkBoxList";
import DropDownList from "@Obsidian/Controls/dropDownList";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import SectionContainer from "@Obsidian/Controls/sectionContainer";
import { SearchFilterBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/searchFilterBag";
import { ContentCollectionFilterControl } from "@Obsidian/Enums/Cms/contentCollectionFilterControl";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";


export const Filter = defineComponent({
    name: "Cms.ContentCollectionView.Filter",

    components: {
        CheckBoxList,
        DropDownList,
        RadioButtonList,
    },

    props: {
        modelValue: {
            type: Object as PropType<string>,
            required: true
        },

        filter: {
            type: Object as PropType<SearchFilterBag>,
            required: true
        }
    },

    emits: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const singleValue = ref(props.modelValue);
        const multipleValue = ref(props.modelValue.split(","));

        const filterClass = computed((): string => {
            const filterSlugName = props.filter.label?.replace(/[^a-zA-Z0-9 ]/g, "").replace(/ /g, "-").toLowerCase() ?? "";

            return `filter-${filterSlugName}`;
        });

        const headerMarkup = computed((): string => {
            return props.filter.headerMarkup ?? "";
        });

        const items = computed((): ListItemBag[] => {
            // If this is goind to render as radio items then we need an All option.
            if (isPills.value && !isMultiSelect.value) {
                const radioItems = [...(props.filter.items ?? [])];

                radioItems.splice(0, 0, {
                    value: "",
                    text: "All"
                });

                return radioItems;
            }

            return props.filter.items ?? [];
        });

        const isMultiSelect = computed((): boolean => {
            return props.filter.isMultipleSelection;
        });

        const isDropDownList = computed((): boolean => {
            return props.filter.control === ContentCollectionFilterControl.Dropdown;
        });

        const isPills = computed((): boolean => {
            return props.filter.control === ContentCollectionFilterControl.Pills;
        });

        const isBoolean = computed((): boolean => {
            return props.filter.control === ContentCollectionFilterControl.Boolean;
        });

        const label = computed((): string => {
            return props.filter.label ?? "";
        });

        watch(() => props.modelValue, () => {
            singleValue.value = props.modelValue;
            multipleValue.value = props.modelValue.split(",");
        });

        watch(singleValue, () => {
            if (props.modelValue !== singleValue.value) {
                emit("update:modelValue", singleValue.value);
            }
        });

        watch(multipleValue, () => {
            if (props.modelValue !== multipleValue.value.join(",")) {
                emit("update:modelValue", multipleValue.value.join(","));
            }
        });

        return {
            filterClass,
            headerMarkup,
            isBoolean,
            isDropDownList,
            isPills,
            isMultiSelect,
            items,
            label,
            multipleValue,
            singleValue
        };
    },

    template: `
<div :class="filterClass">
    <div v-if="headerMarkup" class="filter-header" v-html="headerMarkup"></div>

    <CheckBoxList v-if="isPills && isMultiSelect"
        :label="label"
        v-model="multipleValue"
        :items="items"
        horizontal />

    <RadioButtonList v-if="isPills && !isMultiSelect"
        :label="label"
        v-model="singleValue"
        :items="items"
        horizontal />

    <DropDownList v-if="isDropDownList && isMultiSelect"
        :label="label"
        v-model="multipleValue"
        :items="items"
        multiple />

    <DropDownList v-if="isDropDownList && !isMultiSelect"
        :label="label"
        v-model="singleValue"
        :items="items" />
</div>
    `
});

export default defineComponent({
    name: "CMS.ContentCollectionView.FilterContainer",

    components: {
        Filter,
        SectionContainer
    },

    props: {
        filters: {
            type: Array as PropType<SearchFilterBag[]>,
            required: true
        },

        filterValues: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    emits: {
        "update:filterValues": (_value: Record<string, string>) => true
    },

    setup(props, { emit }) {
        const filterValues = ref<Record<string, string>>(props.filterValues);

        const getFilterValue = (filter: SearchFilterBag): string => {
            return filterValues.value[(filter.label ?? "").toLowerCase()] ?? "";
        };

        const setFilterValue = (filter: SearchFilterBag, value: string): void => {
            const newValues = {...filterValues.value};
            newValues[(filter.label ?? "").toLowerCase()] = value;
            filterValues.value = newValues;

            emit("update:filterValues", newValues);
        };

        watch(() => props.filterValues, () => {
            updateRefValue(filterValues, props.filterValues);
        });

        return {
            getFilterValue,
            setFilterValue
        };
    },

    template: `
<div class="collectionsearch-filters">
    <h3 class="title">Filters</h3>

    <Filter v-for="item in filters"
        :key="item.sourceKey"
        :modelValue="getFilterValue(item)"
        @update:modelValue="setFilterValue(item, $event)"
        :filter="item" />
</div>
`
});
