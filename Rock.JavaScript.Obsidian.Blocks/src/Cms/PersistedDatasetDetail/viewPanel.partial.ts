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
import ValueDetailList from "@Obsidian/Controls/valueDetailList";
import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";
import { PersistedDatasetBag } from "@Obsidian/ViewModels/Blocks/Cms/PersistedDatasetDetail/persistedDatasetBag";
import { PersistedDatasetDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/PersistedDatasetDetail/persistedDatasetDetailOptionsBag";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";

export default defineComponent({
    name: "Cms.PersistedDatasetDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<PersistedDatasetBag>,
            required: false
        },

        options: {
            type: Object as PropType<PersistedDatasetDetailOptionsBag>,
            required: true
        }
    },

    components: {
        ValueDetailList
    },

    setup(props) {
        // #region Values


        // #endregion

        // #region Computed Values

        /** The values to display full-width at the top of the block. */
        const topValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.name) {
                valueBuilder.addTextValue("Name", props.modelValue.name);
            }

            if (props.modelValue.accessKey) {
                valueBuilder.addTextValue("Access Key", props.modelValue.accessKey);
            }

            if (props.modelValue.description) {
                valueBuilder.addTextValue("Description", props.modelValue.description);
            }

            if (props.modelValue.enabledLavaCommands) {
                valueBuilder.addTextValue("Enabled Lava Commands", props.modelValue.enabledLavaCommands.map(c => c.text).join(", "));
            }

            if (props.modelValue.refreshIntervalHours) {
                valueBuilder.addTextValue("Refresh Interval", `${props.modelValue.refreshIntervalHours} hour(s)`);
            }

            if (props.modelValue.memoryCacheDurationHours) {
                valueBuilder.addTextValue("Memory Cache", `${props.modelValue.memoryCacheDurationHours} hour(s)`);
            }

            if (props.modelValue.expireDateTime) {
                const date = RockDateTime.parseISO(props.modelValue.expireDateTime);
                if (date) {
                    valueBuilder.addTextValue("Expires On", date.toString());
                }
            }

            if (props.modelValue.entityType?.text) {
                valueBuilder.addTextValue("Entity Type", props.modelValue.entityType.text);
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
</fieldset>
`
});
