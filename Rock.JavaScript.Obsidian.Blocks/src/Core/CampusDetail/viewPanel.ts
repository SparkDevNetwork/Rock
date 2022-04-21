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
import Alert from "@Obsidian/Controls/alert";
import StaticFormControl from "@Obsidian/Controls/staticFormControl";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { List } from "@Obsidian/Utility/linq";
import { CampusDetailOptionsBag, CampusBag } from "./types";
import ValueDetailList from "@Obsidian/Controls/valueDetailList";
import { ValueDetailListItemBuilder } from "@Obsidian/Core/Controls/valueDetailListItemBuilder";
import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";

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
        Alert,
        AttributeValuesContainer,
        StaticFormControl,
        ValueDetailList
    },

    setup(props) {
        // #region Values

        const attributes = ref(props.modelValue?.attributes ?? []);
        const attributeValues = ref(props.modelValue?.attributeValues ?? {});
        const description = ref(props.modelValue?.description ?? "");

        // #endregion

        // #region Computed Values

        const isSystem = computed((): boolean => props.modelValue?.isSystem ?? false);

        const topValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.getValues();
            }

            if (props.modelValue.description) {
                valueBuilder.addTextValue("Description", props.modelValue.description);
            }

            return valueBuilder.getValues();
        });

        const leftSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.getValues();
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

            return valueBuilder.getValues();
        });

        const rightSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.getValues();
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

            return valueBuilder.getValues();
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        return {
            attributes,
            attributeValues,
            description,
            isSystem,
            leftSideValues,
            rightSideValues,
            topValues
        };
    },

    template: `
<fieldset>
    <Alert v-if="isSystem" alertType="info">
        <strong>Note</strong> Because this campus is used by Rock, editing is not enabled.
    </Alert>

    <ValueDetailList :modelValue="topValues" />

    <div class="row">
        <div class="col-md-6">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>

        <div class="col-md-6">
            <ValueDetailList :modelValue="rightSideValues" />
        </div>
    </div>

    <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" />
</fieldset>
`
});
