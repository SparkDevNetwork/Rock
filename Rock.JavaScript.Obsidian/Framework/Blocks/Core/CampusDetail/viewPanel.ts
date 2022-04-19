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
import AttributeValuesContainer from "../../../Controls/attributeValuesContainer";
import Alert from "../../../Elements/alert.vue";
import { escapeHtml } from "../../../Services/string";
import { List } from "../../../Util/linq";
import { CampusDetailOptionsBag, CampusBag } from "./types";
import ValueDetailList, { ValueDetailListItems } from "./valueDetailList";

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

        const leftSideValues = computed((): ValueDetailListItems => {
            const values = new ValueDetailListItems();

            if (!props.modelValue) {
                return values;
            }

            if (props.modelValue.campusStatusValue?.text) {
                values.addTextValue("Status", props.modelValue.campusStatusValue.text);
            }

            if (props.modelValue.shortCode) {
                values.addTextValue("Code", props.modelValue.shortCode);
            }

            if (props.options.isMultiTimeZoneSupported && props.modelValue.timeZoneId) {
                const tz = new List(props.options.timeZoneOptions ?? [])
                    .where(tz => tz.value === props.modelValue?.timeZoneId)
                    .firstOrUndefined();

                values.addTextValue("Time Zone", tz ? tz.text ?? "" : props.modelValue.timeZoneId);
            }

            if (props.modelValue.leaderPersonAlias?.text) {
                values.addTextValue("Campus Leader", props.modelValue.leaderPersonAlias.text);
            }

            if (props.modelValue.serviceTimes && props.modelValue.serviceTimes.length > 0) {
                const htmlValue = props.modelValue.serviceTimes
                    .map(s => `${escapeHtml(s.value ?? "")} ${escapeHtml(s.text ?? "")}`)
                    .join("<br>");

                values.addHtmlValue("Service Times", htmlValue);
            }

            if (props.modelValue.campusSchedules && props.modelValue.campusSchedules.length > 0) {
                values.addTextValue("Campus Schedules", props.modelValue.campusSchedules.map(s => s.schedule?.text ?? "").join(", "));
            }

            return values;
        });

        const rightSideValues = computed((): ValueDetailListItems => {
            const values = new ValueDetailListItems();

            if (!props.modelValue) {
                return values;
            }

            if (props.modelValue.campusTypeValue?.text) {
                values.addTextValue("Type", props.modelValue.campusTypeValue.text);
            }

            if (props.modelValue.url) {
                values.addTextValue("URL", props.modelValue.url);
            }

            if (props.modelValue.phoneNumber) {
                values.addTextValue("Phone Number", props.modelValue.phoneNumber);
            }

            if (props.modelValue.location?.text) {
                values.addTextValue("Location", props.modelValue.location.text);
            }

            return values;
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
            rightSideValues
        };
    },

    template: `
<fieldset>
    <Alert v-if="isSystem" alertType="info">
        <strong>Note</strong> Because this campus is used by Rock, editing is not enabled.
    </Alert>

    <p v-if="description" class="description">{{ description }}</p>

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
