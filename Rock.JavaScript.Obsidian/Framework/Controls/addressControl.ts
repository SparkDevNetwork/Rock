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
import RockFormField from "./rockFormField";
import DropDownList from "./dropDownList";
import RockLabel from "./rockLabel";
import TextBox from "./textBox";
import { newGuid } from "@Obsidian/Utility/guid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { normalizeRules, rulesPropType } from "@Obsidian/Utility/validationRules";
import { updateRefValue } from "@Obsidian/Utility/component";

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

export default defineComponent({
    name: "AddressControl",

    components: {
        RockFormField,
        TextBox,
        RockLabel,
        DropDownList
    },

    props: {
        modelValue: {
            type: Object as PropType<AddressControlValue>,
            default: {}
        },

        rules: rulesPropType
    },

    emits: {
        "update:modelValue": (_v: AddressControlValue) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue);
        }, { deep: true });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        }, { deep: true });

        // Custom address validation
        const fieldRules = computed(() => {
            const rules = normalizeRules(props.rules);

            if (rules.includes("required")) {
                const index = rules.indexOf("required");

                rules[index] = (value: unknown) => {
                    try {
                        const val = JSON.parse(value as string) as AddressControlValue;
                        if (!val || !val.street1 || !val.city || !val.postalCode) {
                            return "is required";
                        }

                        if (!/^\d{5}(-\d{4})?$/.test(val.postalCode)) {
                            return "needs a valid Zip code";
                        }
                    }
                    catch (e) {
                        return "is required";
                    }

                    return true;
                };
            }
            else {
                rules.push((value: unknown) => {
                    try {
                        const val = JSON.parse(value as string) as AddressControlValue;
                        if (!val || !val.street1) {
                            // can be empty
                            return true;
                        }

                        if (!val.city || !val.postalCode) {
                            // If we have a street value, we also need a city and zip
                            const missing: string[] = [];
                            if (!val.city) {
                                missing.push("City");
                            }
                            if (!val.postalCode) {
                                missing.push("Zip");
                            }
                            return "must include " + missing.join(", ");
                        }

                        if (!/^\d{5}(-\d{4})?$/.test(val.postalCode)) {
                            return "needs a valid Zip code";
                        }
                    }
                    catch (e) {
                        return "must be filled out correctly.";
                    }

                    return true;
                });
            }

            return rules;
        });

        // RockFormField doesn't watch for deep changes, so it doesn't notice when individual pieces
        // change. This rememdies that by turning the value into a single string.
        const fieldValue = computed(() => {
            return JSON.stringify(internalValue.value);
        });

        return {
            internalValue,
            stateOptions,
            fieldRules,
            fieldValue
        };
    },

    template: `
<RockFormField formGroupClasses="address-control" name="addresscontrol" :modelValue="fieldValue" :rules="fieldRules" >
    <div class="control-wrapper">
        <div>
            <div class="form-group">
                <TextBox placeholder="Address Line 1" v-model="internalValue.street1" validationTitle="Address Line 1" />
            </div>
            <div class="form-group">
                <TextBox placeholder="Address Line 2" v-model="internalValue.street2" validationTitle="Address Line 2" />
            </div>
            <div class="form-row">
                <div class="form-group col-sm-6">
                    <TextBox placeholder="City" v-model="internalValue.city" validationTitle="City" />
                </div>
                <div class="form-group col-sm-3">
                    <DropDownList :showBlankItem="false" v-model="internalValue.state" :items="stateOptions" validationTitle="State" />
                </div>
                <div class="form-group col-sm-3">
                    <TextBox placeholder="Zip" v-model="internalValue.postalCode" validationTitle="Zip" />
                </div>
            </div>
        </div>
    </div>
</RockFormField>
`
});
