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
import Panel from "@Obsidian/Controls/panel";
import CheckBox from "@Obsidian/Controls/checkBox";
import DropDownList from "@Obsidian/Controls/dropDownList";
import TextBox from "@Obsidian/Controls/textBox";
import RockForm from "@Obsidian/Controls/rockForm";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { FormPersonEntry } from "./types";
import { FormFieldVisibility } from "./types";

/**
 * The list of options for the individual to pick from when defining if or how
 * a field is shown.
 */
const formFieldVisibilityOptions: ListItemBag[] = [
    {
        value: FormFieldVisibility.Hidden.toString(),
        text: "Hidden"
    },
    {
        value: FormFieldVisibility.Optional.toString(),
        text: "Optional"
    },
    {
        value: FormFieldVisibility.Required.toString(),
        text: "Required"
    }
];

export default defineComponent({
    name: "Workflow.FormBuilderDetail.PersonEntrySettings",
    components: {
        CheckBox,
        DropDownList,
        Panel,
        RockForm,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<FormPersonEntry>,
            default: {}
        },

        recordStatusOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        connectionStatusOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        campusTypeOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        campusStatusOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        addressTypeOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        isVertical: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue",
        "close"
    ],

    setup(props, { emit }) {
        const autofillCurrentPerson = ref(props.modelValue.autofillCurrentPerson ?? false);
        const hideIfCurrentPersonKnown = ref(props.modelValue.hideIfCurrentPersonKnown ?? false);
        const recordStatus = ref(props.modelValue.recordStatus ?? "");
        const connectionStatus = ref(props.modelValue.connectionStatus ?? "");
        const showCampus = ref(props.modelValue.showCampus ?? false);
        const campusType = ref(props.modelValue.campusType ?? "");
        const campusStatus = ref(props.modelValue.campusStatus ?? "");
        const gender = ref(props.modelValue.gender?.toString() ?? FormFieldVisibility.Hidden.toString());
        const email = ref(props.modelValue.email?.toString() ?? FormFieldVisibility.Hidden.toString());
        const mobilePhone = ref(props.modelValue.mobilePhone?.toString() ?? FormFieldVisibility.Hidden.toString());
        const birthdate = ref(props.modelValue.birthdate?.toString() ?? FormFieldVisibility.Hidden.toString());
        const address = ref(props.modelValue.address?.toString() ?? FormFieldVisibility.Hidden.toString());
        const addressType = ref(props.modelValue.addressType ?? "");
        const maritalStatus = ref(props.modelValue.maritalStatus?.toString() ?? FormFieldVisibility.Hidden.toString());
        const spouseEntry = ref(props.modelValue.spouseEntry?.toString() ?? FormFieldVisibility.Hidden.toString());
        const spouseLabel = ref(props.modelValue.spouseLabel ?? "Spouse");

        /** The column span class to apply to the columns. */
        const columnClass = computed((): string => props.isVertical ? "col-xs-12" : "col-md-3");

        // Watch for any changes in our simple field values and update the
        // modelValue.
        watch([autofillCurrentPerson, hideIfCurrentPersonKnown, recordStatus, connectionStatus, showCampus, campusType, campusStatus, gender, email, mobilePhone, birthdate, address, addressType, maritalStatus, spouseEntry, spouseLabel], () => {
            const newValue: FormPersonEntry = {
                ...props.modelValue,
                autofillCurrentPerson: autofillCurrentPerson.value,
                hideIfCurrentPersonKnown: hideIfCurrentPersonKnown.value,
                recordStatus: recordStatus.value,
                connectionStatus: connectionStatus.value,
                showCampus: showCampus.value,
                campusType: campusType.value,
                campusStatus: campusStatus.value,
                gender: toNumberOrNull(gender.value) ?? FormFieldVisibility.Hidden,
                email: toNumberOrNull(email.value) ?? FormFieldVisibility.Hidden,
                mobilePhone: toNumberOrNull(mobilePhone.value) ?? FormFieldVisibility.Hidden,
                birthdate: toNumberOrNull(birthdate.value) ?? FormFieldVisibility.Hidden,
                address: toNumberOrNull(address.value) ?? FormFieldVisibility.Hidden,
                addressType: addressType.value,
                maritalStatus: toNumberOrNull(maritalStatus.value) ?? FormFieldVisibility.Hidden,
                spouseEntry: toNumberOrNull(spouseEntry.value) ?? FormFieldVisibility.Hidden,
                spouseLabel: spouseLabel.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any incoming changes from the parent component and update
        // all our individual field values.
        watch(() => props.modelValue, () => {
            autofillCurrentPerson.value = props.modelValue.autofillCurrentPerson ?? false;
            hideIfCurrentPersonKnown.value = props.modelValue.hideIfCurrentPersonKnown ?? false;
            recordStatus.value = props.modelValue.recordStatus ?? "";
            connectionStatus.value = props.modelValue.connectionStatus ?? "";
            showCampus.value = props.modelValue.showCampus ?? false;
            campusType.value = props.modelValue.campusType ?? "";
            campusStatus.value = props.modelValue.campusStatus ?? "";
            gender.value = props.modelValue.gender?.toString() ?? FormFieldVisibility.Hidden.toString();
            email.value = props.modelValue.email?.toString() ?? FormFieldVisibility.Hidden.toString();
            mobilePhone.value = props.modelValue.mobilePhone?.toString() ?? FormFieldVisibility.Hidden.toString();
            birthdate.value = props.modelValue.birthdate?.toString() ?? FormFieldVisibility.Hidden.toString();
            address.value = props.modelValue.address?.toString() ?? FormFieldVisibility.Hidden.toString();
            addressType.value = props.modelValue.addressType ?? "";
            maritalStatus.value = props.modelValue.maritalStatus?.toString() ?? FormFieldVisibility.Hidden.toString();
            spouseEntry.value = props.modelValue.spouseEntry?.toString() ?? FormFieldVisibility.Hidden.toString();
            spouseLabel.value = props.modelValue.spouseLabel ?? "";
        });

        return {
            address,
            addressType,
            autofillCurrentPerson,
            birthdate,
            campusStatus,
            campusType,
            columnClass,
            connectionStatus,
            email,
            gender,
            hideIfCurrentPersonKnown,
            maritalStatus,
            mobilePhone,
            recordStatus,
            showCampus,
            spouseEntry,
            spouseLabel,
            formFieldVisibilityOptions
        };
    },

    template: `
<div>
    <div class="row">
        <div :class="columnClass">
            <CheckBox v-model="autofillCurrentPerson"
                label="Autofill Current Person" />
        </div>

        <div :class="columnClass">
            <CheckBox v-model="hideIfCurrentPersonKnown"
                label="Hide if Current Person Known" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="recordStatus"
                label="Record Status"
                :items="recordStatusOptions"
                rules="required" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="connectionStatus"
                label="Connection Status"
                :items="connectionStatusOptions"
                rules="required" />
        </div>
    </div>

    <div class="row">
        <div :class="columnClass">
            <CheckBox v-model="showCampus"
                label="Show Campus" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="campusType"
                label="Campus Type"
                :items="campusTypeOptions" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="campusStatus"
                label="Campus Status"
                :items="campusStatusOptions" />
        </div>
    </div>

    <div class="row">
        <div :class="columnClass">
            <DropDownList v-model="gender"
                label="Gender"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="email"
                label="Email"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="mobilePhone"
                label="Mobile Phone"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="birthdate"
                label="Birthdate"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>
    </div>

    <div class="row">
        <div :class="columnClass">
            <DropDownList v-model="address"
                label="Address"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="addressType"
                label="Address Type"
                :items="addressTypeOptions"
                rules="required" />
        </div>

        <div :class="columnClass">
            <DropDownList v-model="maritalStatus"
                label="Marital Status"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>
    </div>

    <div class="row">
        <div :class="columnClass">
            <DropDownList v-model="spouseEntry"
                label="Spouse Entry"
                :showBlankItem="false"
                :items="formFieldVisibilityOptions" />
        </div>

        <div :class="columnClass">
            <TextBox v-model="spouseLabel"
                label="Spouse Label" />
        </div>
    </div>
</div>
`
});
