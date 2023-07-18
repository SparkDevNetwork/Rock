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
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import ValueDetailList from "@Obsidian/Controls/valueDetailList";
import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";
import { BlockTypeBag } from "@Obsidian/ViewModels/Blocks/Cms/BlockTypeDetail/blockTypeBag";
import { BlockTypeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/BlockTypeDetail/blockTypeDetailOptionsBag";

export default defineComponent({
    name: "Cms.BlockTypeDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<BlockTypeBag>,
            required: false
        },

        options: {
            type: Object as PropType<BlockTypeDetailOptionsBag>,
            required: true
        }
    },

    components: {
        NotificationBox,
        AttributeValuesContainer,
        ValueDetailList
    },

    setup(props) {
        // #region Values

        const attributes = ref(props.modelValue?.attributes ?? {});
        const attributeValues = ref(props.modelValue?.attributeValues ?? {});

        // #endregion

        // #region Computed Values

        const isSystem = computed((): boolean => props.modelValue?.isSystem ?? false);

        /** The values to display full-width at the top of the block. */
        const topValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.path) {
                valueBuilder.addTextValue("Path", props.modelValue.path);
            }

            if (props.modelValue.description) {
                valueBuilder.addTextValue("Description", props.modelValue.description);
            }

            if (!props.modelValue.path && props.modelValue.entityType) {
                valueBuilder.addHtmlValue("Status", `<span class="label label-info">${props.modelValue.entityType.text}</span>`);
            }
            else if (props.modelValue.isBlockExists) {
                valueBuilder.addHtmlValue("Status", `<span class="label label-success">Block exists on the file system.</span>`);
            }
            else {
                valueBuilder.addHtmlValue("Status", `<span class="label label-danger">The file ${props.modelValue.path} does not exist.</span>`);
            }

            const pagesStr = "Pages that use this block type";
            if (props.modelValue.pages && props.modelValue.pages.length > 0) {
                let pages = "";
                for (const page of props.modelValue.pages) {
                    pages += `<li>${page}</li>`;
                }
                valueBuilder.addHtmlValue(pagesStr, `<ul>${pages}</ul>`);
            }
            else {
                valueBuilder.addHtmlValue(pagesStr, "<span class='text-muted'><em>No pages are currently using this block</em></muted>");
            }

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const leftSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const rightSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
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
            isSystem,
            leftSideValues,
            rightSideValues,
            topValues
        };
    },

    template: `
<fieldset>
    <NotificationBox v-if="isSystem" alertType="info">
        <strong>Note</strong> Because this block type is used by Rock, editing is not enabled.
    </NotificationBox>

    <ValueDetailList :modelValue="topValues" />

    <div class="row">
        <div class="col-md-6">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>

        <div class="col-md-6">
            <ValueDetailList :modelValue="rightSideValues" />
        </div>
    </div>

    <AttributeValuesContainer v-if="isDynamicAttributesBlock" :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
</fieldset>
`
});
