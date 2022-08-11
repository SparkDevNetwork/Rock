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
import { MediaAccountBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaAccountDetail/mediaAccountBag";
import { MediaAccountDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaAccountDetail/mediaAccountDetailOptionsBag";

export default defineComponent({
    name: "Cms.MediaAccountDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<MediaAccountBag>,
            required: false
        },

        options: {
            type: Object as PropType<MediaAccountDetailOptionsBag>,
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

        /** The values to display at half-width on the left side of the block. */
        const leftSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.componentEntityType?.text) {
                valueBuilder.addTextValue("Status", props.modelValue.componentEntityType.text);
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
            leftSideValues
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>
    </div>
</fieldset>
`
});
