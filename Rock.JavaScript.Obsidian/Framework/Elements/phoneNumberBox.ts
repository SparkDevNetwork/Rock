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
import { newGuid } from "../Util/guid";
import RockFormField from "./rockFormField";
import { formatPhoneNumber, stripPhoneNumber } from "../Services/string";

export default defineComponent({
    name: "PhoneNumberBox",
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },
        inputGroupClasses: {
            type: String as PropType<string>,
            default: ""
        }
    },
    emits: [
        "update:modelValue"
    ],
    data: function () {
        return {
            internalValue: ""
        };
    },
    methods: {
        onChange(): void {
            this.internalValue = this.formattedValue;
        }
    },
    computed: {
        strippedValue(): string {
            return stripPhoneNumber(this.internalValue);
        },
        formattedValue(): string {
            return formatPhoneNumber(this.internalValue);
        }
    },
    watch: {
        formattedValue(): void {
            // The value that should be stored for phone number attribute values is the formatted version.
            // This seems backwards, but actually keeps parity with the web forms functionality.
            this.$emit("update:modelValue", this.formattedValue);
        },
        modelValue: {
            immediate: true,
            handler(): void {
                const stripped = stripPhoneNumber(this.modelValue);

                if (stripped !== this.strippedValue) {
                    this.internalValue = formatPhoneNumber(stripped);
                }
            }
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    @change="onChange"
    formGroupClasses="rock-phonenumber-box"
    name="phonenumberbox">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="input-group phone-number-box" :class="inputGroupClasses">
                <span class="input-group-addon">
                    <i class="fa fa-phone-square"></i>
                </span>
                <input v-model="internalValue" :id="uniqueId" type="text" class="form-control" v-bind="field" />
            </div>
        </div>
    </template>
</RockFormField>`
});
