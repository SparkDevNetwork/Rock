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
import { MediaFolderBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaFolderDetail/mediaFolderBag";
import { MediaFolderDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaFolderDetail/mediaFolderDetailOptionsBag";

export default defineComponent({
    name: "Cms.MediaFolderDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<MediaFolderBag>,
            required: false
        },

        options: {
            type: Object as PropType<MediaFolderDetailOptionsBag>,
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

            if (props.modelValue.mediaAccount?.value) {
                valueBuilder.addTextValue("Account", props.modelValue.mediaAccount.text ?? "");
            }

            if (props.modelValue.contentChannel?.value) {
                valueBuilder.addTextValue("Content Channel", props.modelValue.contentChannel.text ?? "");
            }

            if (props.modelValue.contentChannelAttribute?.value) {
                valueBuilder.addTextValue("Content Channel Attribute", props.modelValue.contentChannelAttribute.text ?? "");
            }

            if (props.modelValue.contentChannelItemStatus) {
                valueBuilder.addTextValue("Content Channel Item Status", props.modelValue.contentChannelItemStatus ?? "");
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
