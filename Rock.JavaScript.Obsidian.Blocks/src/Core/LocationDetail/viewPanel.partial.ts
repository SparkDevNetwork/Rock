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
import { LocationBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationBag";
import { LocationDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationDetailOptionsBag";

export default defineComponent({
    name: "Core.LocationDetail.ViewPanel",

    props: {
        modelValue: {
            type: Object as PropType<LocationBag>,
            required: false
        },

        options: {
            type: Object as PropType<LocationDetailOptionsBag>,
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

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const leftSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.imageId) {
                valueBuilder.addHtmlValue("", `<img src='/GetImage.ashx?id=${props.modelValue.imageId}&maxwidth=150&maxheight=150'>`);

            }
            else {
                valueBuilder.addHtmlValue("", "<img src='/Assets/Images/no-picture.svg?' style='max-width:150px; max-height:150px;'>");
            }

            return valueBuilder.build();
        });

        /** The values to display at half-width on the left side of the block. */
        const rightSideValues = computed((): ValueDetailListItem[] => {
            const valueBuilder = new ValueDetailListItemBuilder();

            if (!props.modelValue) {
                return valueBuilder.build();
            }

            if (props.modelValue.parentLocation?.text){
                valueBuilder.addTextValue("Parent Location", props.modelValue.parentLocation.text ?? "");
            }

            if (props.modelValue.locationTypeValue?.text) {
                valueBuilder.addTextValue("Location Type", props.modelValue.locationTypeValue.text ?? "");
            }

            if (props.modelValue.printerDevice?.text) {
                valueBuilder.addTextValue("Printer", props.modelValue.printerDevice.text ?? "");
            }

            if (props.modelValue.softRoomThreshold) {
                valueBuilder.addTextValue("Threshold", props.modelValue.softRoomThreshold?.toString() ?? "");
            }

            if (props.modelValue.firmRoomThreshold) {
                valueBuilder.addTextValue("Threshold (Absolute)", props.modelValue.firmRoomThreshold?.toString() ?? "");
            }

            if (props.modelValue.formattedHtmlAddress) {
                valueBuilder.addHtmlValue("Address", props.modelValue.formattedHtmlAddress ?? "");
            }

            return valueBuilder.build();
        });

        const locationMapsHtml = computed((): string => {

            let mapsHtml = "";

            if (!props.modelValue) {
                return mapsHtml;
            }

            if (props.modelValue.geoPointImageHtml) {
                mapsHtml += props.modelValue.geoPointImageHtml;
            }

            if (props.modelValue.geoFenceImageHtml) {
                mapsHtml += props.modelValue.geoFenceImageHtml;
            }

            return mapsHtml;

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
            locationMapsHtml,
            topValues
        };
    },

    template: `
<fieldset>

    <ValueDetailList :modelValue="topValues" />

    <div class="row">
        <div class="col-md-3">
            <ValueDetailList :modelValue="leftSideValues" />
        </div>

        <div class="col-md-5">
            <ValueDetailList :modelValue="rightSideValues" />
        </div>

        <div class="col-md-4 location-maps">
            <span v-html="locationMapsHtml"></span>
        </div>
    </div>

    <AttributeValuesContainer :modelValue="attributeValues" :attributes="attributes" :numberOfColumns="2" />
</fieldset>
`
});
