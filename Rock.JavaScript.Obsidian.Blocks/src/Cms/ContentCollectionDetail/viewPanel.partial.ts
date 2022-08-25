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
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import ValueDetailList from "@Obsidian/Controls/valueDetailList";
import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";
import { ContentCollectionBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionBag";
import { ContentCollectionDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentCollectionDetailOptionsBag";
import { pluralConditional } from "@Obsidian/Utility/stringUtils";

export default defineComponent({
    name: "Cms.ContentCollectionDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<ContentCollectionBag>,
            required: false
        },

        options: {
            type: Object as PropType<ContentCollectionDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        ValueDetailList
    },

    setup(props) {
        // #region Values

        const attributes = ref(props.modelValue?.attributes ?? {});
        const attributeValues = ref(props.modelValue?.attributeValues ?? {});

        // #endregion

        // #region Computed Values

        /** The values to display full-width at the top of the block. */
        const topValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.description) {
                valueBuilder.addTextValue("Description", props.modelValue.description);
            }

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const leftSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            valueBuilder.addTextValue("Collection Key", props.modelValue.collectionKey ?? "");

            const segmentsLabel = `<span class="label label-${props.modelValue.enableSegments ? "success" : "default"}">Segments</span>`;
            const requestFiltersLabel = `<span class="label label-${props.modelValue.enableRequestFilters ? "success" : "default"}">Request Filters</span>`;
            valueBuilder.addHtmlValue("Personalization", `<div class="label-container">${segmentsLabel}${requestFiltersLabel}</div>`);

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const rightSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.trendingEnabled) {
                const label = `<span class="label label-success">Enabled</span>`;
                const days = `<span class="text-xs"><i class="fa fa-calendar-alt"></i> ${props.modelValue.trendingWindowDay} ${pluralConditional(props.modelValue.trendingWindowDay, "day", "days")}</span>`;
                const items = `<span class="text-xs"><i class="fa fa-file-alt"></i> ${props.modelValue.trendingMaxItems} ${pluralConditional(props.modelValue.trendingMaxItems, "item", "items")}</span>`;
                valueBuilder.addHtmlValue("Trending", `<div class="content-collection-trending-state text-muted">${label}${days}${items}</div>`);
            }
            else {
                valueBuilder.addHtmlValue("Trending", `<span class="label label-default">Disabled</span>`);
            }

            return valueBuilder.build();
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        return {
            attributes,
            attributeValues,
            leftSideValues,
            rightSideValues,
            topValues
        };
    },

    template: `
<fieldset>
    <ValueDetailList :modelValue="topValues" />

    <div class="row">
        <div class="col-md-6">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>

        <div class="col-md-6">
            <ValueDetailList :modelValue="rightSideValues" />
        </div>
    </div>

    <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
</fieldset>
`
});
