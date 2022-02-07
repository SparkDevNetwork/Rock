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
import { ListItem } from "../ViewModels";
import { defineComponent, PropType } from "vue";
import { newGuid } from "../Util/guid";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "ListBox",
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: Array as PropType<Array<string>>,
            default: []
        },

        options: {
            type: Array as PropType<ListItem[]>,
            required: true
        },

        formControlClasses: {
            type: String as PropType<string>,
            default: ""
        },

        enhanceForLongLists: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    data: function () {
        return {
            uniqueId: `rock-listbox-${newGuid()}`,
            internalValue: [] as string[],
            isMounted: false
        };
    },

    computed: {
        /** The compiled list of CSS classes (props and calculated from other inputs) for the select element */
        compiledFormControlClasses(): string {
            if (this.enhanceForLongLists) {
                return this.formControlClasses + " chosen-select";
            }

            return this.formControlClasses;
        }
    },

    methods: {
        /** Uses jQuery to get the chosen element */
        getChosenJqueryEl() {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const jquery = <any>window[<any>"$"];
            let $chosenDropDown = jquery(this.$refs["theSelect"]);

            if (!$chosenDropDown || !$chosenDropDown.length) {
                $chosenDropDown = jquery(`#${this.uniqueId}`);
            }

            return $chosenDropDown;
        },

        createOrDestroyChosen() {
            if (!this.isMounted) {
                return;
            }

            const $chosenDropDown = this.getChosenJqueryEl();

            if (this.enhanceForLongLists) {
                $chosenDropDown
                    .chosen({
                        width: "100%",
                        placeholder_text_multiple: " ",
                        placeholder_text_single: " "
                    })
                    .change(() => {
                        this.internalValue = $chosenDropDown.val();
                    });
            }
            else {
                $chosenDropDown.chosen("destroy");
            }
        },

        syncValue() {
            if (this.internalValue.length === this.modelValue.length && this.internalValue.every((v, i) => v === this.modelValue[i])) {
                return;
            }

            this.internalValue = this.modelValue;

            if (this.enhanceForLongLists) {
                this.$nextTick(() => {
                    const $chosenDropDown = this.getChosenJqueryEl();
                    $chosenDropDown.trigger("chosen:updated");
                });
            }
        }
    },

    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.syncValue();
            }
        },

        options: {
            immediate: true,
            handler() {
                this.syncValue();
            }
        },

        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },

        enhanceForLongLists() {
            this.createOrDestroyChosen();
        }
    },

    mounted() {
        this.isMounted = true;
        this.createOrDestroyChosen();
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-drop-down-list"
    name="dropdownlist">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :class="compiledFormControlClasses" v-bind="field" v-model="internalValue" ref="theSelect" multiple>
                <option v-if="showBlankItem" :value="blankValue"></option>
                <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </template>
</RockFormField>
`
});
