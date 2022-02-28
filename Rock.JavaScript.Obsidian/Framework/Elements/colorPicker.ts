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

// TODO: This should be replaced with a version that does not require jQuery.
export default defineComponent({
    name: "ColorPicker",
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        placeholder: {
            type: String as PropType<string>,
            default: ""
        }
    },
    emits: [
        "update:modelValue"
    ],
    data: function () {
        return {
            internalValue: this.modelValue
        };
    },
    mounted(): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const $colorPicker = (<any>window[<any>"$"])(this.$refs.colorPicker);

        $colorPicker.colorpicker();

        // Vue will not detect the change if it happens from jQuery, so we need
        // to detect the change and make sure Vue knows.
        $colorPicker.find("> input").on("change", () => {
            this.internalValue = <string>$colorPicker.find("> input").val();
        });
    },
    computed: {
    },
    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue() {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-color-picker"
    name="colorpicker">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div ref="colorPicker" class="input-group input-width-lg">
                <input v-model="internalValue" :id="uniqueId" type="text" class="form-control" v-bind="field" :placeholder="placeholder" />
                <span class="input-group-addon">
                    <i></i>
                </span>
            </div>
        </div>
    </template>
</RockFormField>`
});
