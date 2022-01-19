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
import { defineComponent, PropType } from "vue";
import RockFormField from "../Elements/rockFormField";
import DropDownList from "../Elements/dropDownList";
import RockLabel from "../Elements/rockLabel";
import TextBox from "../Elements/textBox";
import { newGuid } from "../Util/guid";
import { ListItem } from "../ViewModels";

export type AddressControlValue = {
    street1?: string;
    street2?: string;
    city?: string;
    state?: string;
    postalCode?: string;
    country?: string;
};

export function getDefaultAddressControlModel(): AddressControlValue {
    return {
        state: "AZ",
        country: "US"
    };
}

const stateOptions: ListItem[] = [
    "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM",
    "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA",
    "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV",
    "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW",
    "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA",
    "WA", "WV", "WI", "WY"]
    .map(o => ({ value: o, text: o }));

export const AddressControlBase = defineComponent({
    name: "AddressControlBase",

    components: {
        TextBox,
        RockLabel,
        DropDownList
    },

    props: {
        modelValue: {
            type: Object as PropType<AddressControlValue>,
            default: {}
        },

        id: {
            type: String as PropType<string>,
            default: ""
        }
    },

    setup(props) {
        const uniqueId = props.id || `rock-addresscontrol-${newGuid}`;

        return {
            uniqueId,
            stateOptions
        };
    },

    template: `
<div :id="uniqueId">
    <TextBox placeholder="Address Line 1" :rules="rules" v-model="modelValue.street1" validationTitle="Address Line 1" />
    <TextBox placeholder="Address Line 2" v-model="modelValue.street2" validationTitle="Address Line 2" />
    <div class="form-row">
        <TextBox placeholder="City" :rules="rules" v-model="modelValue.city" class="col-sm-6" validationTitle="City" />
        <DropDownList :showBlankItem="false" v-model="modelValue.state" class="col-sm-3" :options="stateOptions" />
        <TextBox placeholder="Zip" :rules="rules" v-model="modelValue.postalCode" class="col-sm-3" validationTitle="Zip" />
    </div>
</div>
`
});

export default defineComponent({
    name: "AddressControl",

    components: {
        RockFormField,
        AddressControlBase
    },

    props: {
        modelValue: {
            type: Object as PropType<AddressControlValue>,
            default: {}
        }
    },

    template: `
<RockFormField formGroupClasses="address-control" #default="{uniqueId, field}" name="addresscontrol" v-model.lazy="modelValue">
    <div class="control-wrapper">
        <AddressControlBase v-model.lazy="modelValue" v-bind="field" :disabled="disabled" />
    </div>
</RockFormField>
`
});
