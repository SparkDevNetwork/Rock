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
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import DropDownList from "@Obsidian/Controls/dropDownList";
import KeyValueList, { KeyValueItem } from "@Obsidian/Controls/keyValueList";
import LocationPicker from "@Obsidian/Controls/locationItemPicker";
import PersonPicker from "@Obsidian/Controls/personPicker";
import CheckBox from "@Obsidian/Controls/checkBox";
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox";
import TextBox from "@Obsidian/Controls/textBox";
import UrlLinkBox from "@Obsidian/Controls/urlLinkBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { CampusBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusBag";
import { CampusDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusDetailOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";

export default defineComponent({
    name: "Core.CampusDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<CampusBag>,
            required: true
        },

        options: {
            type: Object as PropType<CampusDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        CheckBox,
        DefinedValuePicker,
        DropDownList,
        KeyValueList,
        LocationPicker,
        PersonPicker,
        PhoneNumberBox,
        TextBox,
        UrlLinkBox
    },

    emits: {
        "update:modelValue": (_value: CampusBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const campusStatusValue = propertyRef(props.modelValue.campusStatusValue ?? null, "CampusStatusValueId");
        const campusTypeValue = propertyRef(props.modelValue.campusTypeValue ?? null, "CampusTypeValueId");
        const description = propertyRef(props.modelValue.description ?? "", "Description");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const leaderPersonAlias = propertyRef(props.modelValue.leaderPersonAlias ?? null, "LeaderPersonAliasId");
        const location = propertyRef(props.modelValue.location ?? null, "LocationId");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const phoneNumber = propertyRef(props.modelValue.phoneNumber ?? "", "PhoneNumber");
        const serviceTimes = propertyRef((props.modelValue.serviceTimes ?? []).map((s): KeyValueItem => ({ key: s.value, value: s.text })), "ServiceTimes");
        const shortCode = propertyRef(props.modelValue.shortCode ?? "", "ShortCode");
        const timeZoneId = propertyRef(props.modelValue.timeZoneId ?? "", "TimeZoneId");
        const url = propertyRef(props.modelValue.url ?? "", "Url");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [campusStatusValue,
            campusTypeValue,
            description,
            isActive,
            leaderPersonAlias,
            location,
            name,
            phoneNumber,
            serviceTimes,
            shortCode,
            timeZoneId,
            url];

        // #endregion

        // #region Computed Values

        const isTimeZoneVisible = computed((): boolean => {
            return props.options.isMultiTimeZoneSupported === true;
        });

        const timeZoneOptions = computed((): ListItemBag[] => {
            return props.options.timeZoneOptions ?? [];
        });


        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(campusStatusValue, props.modelValue.campusStatusValue ?? null);
            updateRefValue(campusTypeValue, props.modelValue.campusTypeValue ?? null);
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(leaderPersonAlias, props.modelValue.leaderPersonAlias ?? null);
            updateRefValue(location, props.modelValue.location ?? null);
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(phoneNumber, props.modelValue.phoneNumber ?? "");
            updateRefValue(serviceTimes, (props.modelValue.serviceTimes ?? []).map((s): KeyValueItem => ({ key: s.value, value: s.text })));
            updateRefValue(shortCode, props.modelValue.shortCode ?? "");
            updateRefValue(timeZoneId, props.modelValue.timeZoneId ?? "");
            updateRefValue(url, props.modelValue.url ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: CampusBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                campusStatusValue: campusStatusValue.value,
                campusTypeValue: campusTypeValue.value,
                description: description.value,
                isActive: isActive.value,
                leaderPersonAlias: leaderPersonAlias.value,
                location: location.value,
                name: name.value,
                phoneNumber: phoneNumber.value,
                serviceTimes: serviceTimes.value.map((s): ListItemBag => ({ value: s.key ?? "", text: s.value ?? "" })),
                shortCode: shortCode.value,
                timeZoneId: timeZoneId.value,
                url: url.value
            };

            emit("update:modelValue", newValue);
        });

        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            campusStatusDefinedTypeGuid: DefinedType.CampusStatus,
            campusStatusValue,
            campusTypeDefinedTypeGuid: DefinedType.CampusType,
            campusTypeValue,
            description,
            isActive,
            isTimeZoneVisible,
            leaderPersonAlias,
            location,
            name,
            phoneNumber,
            serviceTimes,
            shortCode,
            timeZoneId,
            timeZoneOptions,
            url
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />
        </div>
    </div>

    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />

    <div class="row">
        <div class="col-md-6">
            <DefinedValuePicker v-model="campusStatusValue"
                label="Status"
                :definedTypeGuid="campusStatusDefinedTypeGuid" />

            <TextBox v-model="shortCode"
                label="Code" />

            <DropDownList v-if="isTimeZoneVisible"
                v-model="timeZoneId"
                label="Time Zone"
                help="The time zone you want certain time calculations of the Campus to operate in. Leave this blank to use the default Rock TimeZone."
                :items="timeZoneOptions" />

            <PersonPicker v-model="leaderPersonAlias"
                label="Campus Leader" />

            <KeyValueList v-model="serviceTimes"
                label="Service Times"
                help="A list of days and times that this campus has services." />
        </div>

        <div class="col-md-6">
            <DefinedValuePicker v-model="campusTypeValue"
                label="Type"
                :definedTypeGuid="campusTypeDefinedTypeGuid" />

            <UrlLinkBox v-model="url"
                label="URL"
                requiresTrailingSlash />

            <PhoneNumberBox v-model="phoneNumber"
                label="Phone Number" />

            <LocationPicker v-model="location"
                label="Location"
                help="Select a Campus location."
                rules="required" />
        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
