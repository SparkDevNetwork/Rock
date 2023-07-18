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
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import AddressControl from "@Obsidian/Controls/addressControl.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import DropDownList from "@Obsidian/Controls/dropDownList";
import ImageUploader from "@Obsidian/Controls/imageUploader";
import LocationPicker from "@Obsidian/Controls/locationPicker.obs";
import NumberBox from "@Obsidian/Controls/numberBox";
import RockButton from "@Obsidian/Controls/rockButton";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { LocationBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationBag";
import { LocationDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationDetailOptionsBag";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { AddressStandardizationResultBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/addressStandardizationResultBag";

export default defineComponent({
    name: "Core.LocationDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<LocationBag>,
            required: true
        },

        options: {
            type: Object as PropType<LocationDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AddressControl,
        AttributeValuesContainer,
        NotificationBox,
        CheckBox,
        DefinedValuePicker,
        DropDownList,
        ImageUploader,
        LocationPicker,
        NumberBox,
        RockButton,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: LocationBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values
        const invokeBlockAction = useInvokeBlockAction();

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const parentLocation = propertyRef(props.modelValue.parentLocation ?? null, "ParentLocationId");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const locationTypeValue = propertyRef(props.modelValue.locationTypeValue ?? null, "LocationTypeValueId");
        const printerDevice = propertyRef(props.modelValue.printerDevice?.value ?? "", "PrinterDeviceId");
        const isGeoPointLocked = propertyRef(props.modelValue.isGeoPointLocked ?? false, "IsGeoPointLocked");
        const softRoomThreshold = propertyRef(props.modelValue.softRoomThreshold ?? null, "SoftRoomThreshold");
        const firmRoomThreshold = propertyRef(props.modelValue.firmRoomThreshold ?? null, "FirmRoomThreshold");
        const addressFields = ref(props.modelValue.addressFields ?? {});
        const geoPointWellKnownText = propertyRef(props.modelValue.geoPoint_WellKnownText ?? "", "GeoPoint");
        const geoFenceWellKnownText = propertyRef(props.modelValue.geoFence_WellKnownText ?? "", "GeoFence");
        const standardizeAttemptedResult = ref("");
        const geocodeAttemptedResult = ref("");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [isActive,
            name,
            parentLocation,
            locationTypeValue,
            printerDevice,
            isGeoPointLocked,
            softRoomThreshold,
            firmRoomThreshold,
            geoPointWellKnownText,
            geoFenceWellKnownText
        ];

        // #endregion

        // #region Computed Values
        const printerDeviceOptions = computed((): ListItemBag[] => {
            return props.options.printerDeviceOptions ?? [];
        });

        const standardizationResults = computed((): string => {
            if (standardizeAttemptedResult.value || geocodeAttemptedResult.value) {
                return "Standardization Result: " + standardizeAttemptedResult.value
                    + "<br>"
                    + "Geocoding Result:" +  geocodeAttemptedResult.value;
            }
            else {
                return "";
            }
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the individual clicks the Standardize/VerifyLocation button.
         */
        const onStandardizeClick = async (): Promise<void> => {
            const result = await invokeBlockAction<AddressStandardizationResultBag>("StandardizeLocation", { addressFields: addressFields.value });

            if (result.isSuccess && result.data) {
                updateRefValue(addressFields, result.data.addressFields ?? {});
                standardizeAttemptedResult.value = result.data.standardizeAttemptedResult ?? "";
                geocodeAttemptedResult.value = result.data.geocodeAttemptedResult ?? "";
            }
        };

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(addressFields, props.modelValue.addressFields ?? {});
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(parentLocation, props.modelValue.parentLocation ?? null);
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(locationTypeValue, props.modelValue.locationTypeValue ?? null);
            updateRefValue(printerDevice, props.modelValue.printerDevice?.value ?? "");

            updateRefValue(isGeoPointLocked, props.modelValue.isGeoPointLocked ?? false);
            updateRefValue(softRoomThreshold, props.modelValue.softRoomThreshold ?? null) ;
            updateRefValue(firmRoomThreshold, props.modelValue.firmRoomThreshold ?? null);

            // TODO, temporary code until we have a GeoPicker
            updateRefValue(geoPointWellKnownText, props.modelValue.geoPoint_WellKnownText ?? "");
            updateRefValue(geoFenceWellKnownText, props.modelValue.geoFence_WellKnownText ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, addressFields, ...propRefs], () => {
            const newValue: LocationBag = {
                ...props.modelValue,
                addressFields: addressFields.value,
                attributeValues: attributeValues.value,
                isActive: isActive.value,
                name: name.value,
                locationTypeValue: locationTypeValue.value,
                parentLocation: parentLocation.value,
                printerDevice: { value: printerDevice.value },
                isGeoPointLocked: isGeoPointLocked.value,
                softRoomThreshold: softRoomThreshold.value,
                firmRoomThreshold: firmRoomThreshold.value,
                geoPoint_WellKnownText: geoPointWellKnownText.value,
                geoFence_WellKnownText: geoFenceWellKnownText.value,
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            addressFields,
            attributes,
            attributeValues,
            isActive,
            name,
            locationTypeValue,
            locationTypeDefinedTypeGuid: DefinedType.LocationType,
            parentLocation,
            printerDevice,
            printerDeviceOptions,
            isGeoPointLocked,
            softRoomThreshold,
            firmRoomThreshold,
            onStandardizeClick,
            standardizeAttemptedResult,
            geocodeAttemptedResult,
            standardizationResults,
            geoPointWellKnownText,
            geoFenceWellKnownText
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">

            <LocationPicker v-model="parentLocation"
                label="Parent Location" />

            <TextBox v-model="name"
                label="Name"
                rules="required" />

            <DefinedValuePicker v-model="locationTypeValue"
                label="Location Type"
                :showBlankItem="true"
                :definedTypeGuid="locationTypeDefinedTypeGuid" />

            <DropDownList label="Printer"
                v-model="printerDevice"
                :items="printerDeviceOptions"
                help="The printer that this location should use for printing."
                :showBlankItem="true"
                :enhanceForLongLists="false"
                :grouped="false"
                :multiple="false"
                 />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />

            <AddressControl label="" v-model="addressFields" />

            <NotificationBox v-if="standardizationResults" alertType="info" v-html="standardizationResults" />

            <RockButton
                btnSize="sm"
                btnType="action"
                @click="onStandardizeClick"
                :isLoading="isLoading"
                :autoLoading="autoLoading"
                :autoDisable="autoDisable"
                :loadingText="loadingText">
                Verify Address
            </RockButton>

            <CheckBox v-model="isGeoPointLocked"
                label="Location Locked"
                help="Locks the location to prevent verification services (standardization/geocoding) from updating the address or point."
            />

            <div class="row">
                <div class="col-sm-7">
                    <TextBox label="Point" v-model="geoPointWellKnownText" readonly />
                    <TextBox label="Geo-Fence" v-model="geoFenceWellKnownText" readonly />
                </div>
                <div class="col-sm-5">
                    <NumberBox label="Threshold" v-model="softRoomThreshold" help="The maximum number of people that room allows before a check-in will require a manager override."/>
                    <NumberBox label="Threshold (Absolute)" v-model="firmRoomThreshold" help="The absolute maximum number of people that room allows. Check-in will not allow check-in after this number of people have checked in." />
                </div>
            </div>

        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
