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
import { CampusBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusBag";
import { CampusDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusDetailOptionsBag";
import { List } from "@Obsidian/Utility/linq";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

export default defineComponent({
    name: "Core.CampusDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<CampusBag>,
            required: false
        },

        options: {
            type: Object as PropType<CampusDetailOptionsBag>,
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

            if (props.modelValue.campusStatusValue?.text) {
                valueBuilder.addTextValue("Status", props.modelValue.campusStatusValue.text);
            }

            if (props.modelValue.shortCode) {
                valueBuilder.addTextValue("Code", props.modelValue.shortCode);
            }

            if (props.options.isMultiTimeZoneSupported && props.modelValue.timeZoneId) {
                const tz = new List(props.options.timeZoneOptions ?? [])
                    .where(tz => tz.value === props.modelValue?.timeZoneId)
                    .firstOrUndefined();

                valueBuilder.addTextValue("Time Zone", tz ? tz.text ?? "" : props.modelValue.timeZoneId);
            }

            if (props.modelValue.leaderPersonAlias?.text) {
                valueBuilder.addTextValue("Campus Leader", props.modelValue.leaderPersonAlias.text);
            }

            if (props.modelValue.serviceTimes && props.modelValue.serviceTimes.length > 0) {
                const htmlValue = props.modelValue.serviceTimes
                    .map(s => `${escapeHtml(s.value ?? "")} ${escapeHtml(s.text ?? "")}`)
                    .join("<br>");

                valueBuilder.addHtmlValue("Service Times", htmlValue);
            }

            if (props.modelValue.campusSchedules && props.modelValue.campusSchedules.length > 0) {
                valueBuilder.addTextValue("Campus Schedules", props.modelValue.campusSchedules.map(s => s.schedule?.text ?? "").join(", "));
            }

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const rightSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.campusTypeValue?.text) {
                valueBuilder.addTextValue("Type", props.modelValue.campusTypeValue.text);
            }

            if (props.modelValue.url) {
                valueBuilder.addTextValue("URL", props.modelValue.url);
            }

            if (props.modelValue.phoneNumber) {
                valueBuilder.addTextValue("Phone Number", props.modelValue.phoneNumber);
            }

            if (props.modelValue.location?.text) {
                valueBuilder.addTextValue("Location", props.modelValue.location.text);
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
        <strong>Note</strong> Because this campus is used by Rock, editing is not enabled.
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

    <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
</fieldset>
`
});
