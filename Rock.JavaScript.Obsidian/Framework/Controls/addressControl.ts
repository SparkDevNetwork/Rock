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
import RockFormField from "./rockFormField";
import DropDownList from "./dropDownList";
import RockLabel from "./rockLabel";
import TextBox from "./textBox";
import { newGuid } from "@Obsidian/Utility/guid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { rulesPropType } from "@Obsidian/Utility/validationRules";

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

const stateOptions: ListItemBag[] = [
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
        },
        rules: rulesPropType
    },

    setup(props) {
        const uniqueId = props.id || `rock-addresscontrol-${newGuid()}`;

        return {
            uniqueId,
            stateOptions
        };
    },

    template: `
<div :id="uniqueId">
    <div class="form-group">
        <TextBox placeholder="Address Line 1" :rules="rules" v-model="modelValue.street1" validationTitle="Address Line 1" />
    </div>
    <div class="form-group">
        <TextBox placeholder="Address Line 2" v-model="modelValue.street2" validationTitle="Address Line 2" />
    </div>
    <div class="form-row">
        <div class="form-group col-sm-6">
            <TextBox placeholder="City" :rules="rules" v-model="modelValue.city" validationTitle="City" />
        </div>
        <div class="form-group col-sm-3">
            <DropDownList :showBlankItem="false" v-model="modelValue.state" :items="stateOptions" />
        </div>
        <div class="form-group col-sm-3">
            <TextBox placeholder="Zip" :rules="rules" v-model="modelValue.postalCode" validationTitle="Zip" />
        </div>
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
        },
        rules: rulesPropType
    },

    template: `
<RockFormField formGroupClasses="address-control" #default="{uniqueId, field}" name="addresscontrol" v-model.lazy="modelValue" :rules="rules" >
    <div class="control-wrapper">
        <AddressControlBase v-model.lazy="modelValue" :rules="rules" v-bind="field" :disabled="disabled" />
    </div>
</RockFormField>
`
});
