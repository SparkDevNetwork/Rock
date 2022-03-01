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
import { computed, defineComponent, nextTick, onMounted, PropType, ref, watch } from "vue";
import { ListItem } from "../ViewModels";
import RockFormField from "./rockFormField";

type OptionGroup = {
    text: string;

    options: ListItem[];
};

export default defineComponent({
    name: "DropDownList",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },

        options: {
            type: Array as PropType<ListItem[]>,
            default: []
        },

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        blankValue: {
            type: String as PropType<string>,
            default: ""
        },

        formControlClasses: {
            type: String as PropType<string>,
            default: ""
        },

        enhanceForLongLists: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        grouped: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        let isMounted = false;

        const internalValue = ref(props.modelValue);

        /** The select element that will be used by the chosen plugin. */
        const theSelect = ref<HTMLElement | null>(null);

        /** The compiled list of CSS classes for the select element. */
        const compiledFormControlClasses = computed((): string => {
            if (props.enhanceForLongLists) {
                return props.formControlClasses + " chosen-select";
            }

            return props.formControlClasses;
        });

        const optionsWithoutGroup = computed((): ListItem[] => {
            return props.options
                .filter(o => !o.category);
        });

        const optionGroups = computed((): OptionGroup[] => {
            const groups: OptionGroup[] = [];

            for (const o of props.options) {
                if (!o.category) {
                    continue;
                }

                const matchedGroups = groups.filter(g => g.text == o.category);

                if (matchedGroups.length >= 1) {
                    matchedGroups[0].options.push(o);
                }
                else {
                    groups.push({
                        text: o.category,
                        options: [o]
                    });
                }
            }

            return groups;
        });

        /** Uses jQuery to get the chosen element */
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const getChosenJqueryEl = (): any => {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const jquery = <any>window[<any>"$"];
            const $chosenDropDown = jquery(theSelect.value);

            if (!$chosenDropDown || !$chosenDropDown.length) {
                return undefined;
            }

            return $chosenDropDown;
        };

        /**
         * Create the destroy the chosen picker in accordance with our current
         * settings in the properties.
         */
        const createOrDestroyChosen = (): void => {
            if (!isMounted) {
                return;
            }

            const $chosenDropDown = getChosenJqueryEl();

            if (props.enhanceForLongLists) {
                $chosenDropDown
                    .chosen({
                        width: "100%",
                        allow_single_deselect: true,
                        placeholder_text_multiple: " ",
                        placeholder_text_single: " "
                    })
                    .change((ev: Event) => {
                        internalValue.value = (ev.target as HTMLSelectElement).value;
                    });
            }
            else {
                $chosenDropDown.chosen("destroy");
            }
        };

        /**
         * Synchronizes our internal value and then makes sure the chosen
         * picker, if we have one, is synced up as well.
         */
        const syncInternalValue = (): void => {
            internalValue.value = props.modelValue;
            const selectedOption = props.options.find(o => o.value === internalValue.value) || null;

            if (!selectedOption) {
                internalValue.value = props.showBlankItem ?
                    props.blankValue :
                    (props.options[0]?.value || props.blankValue);
            }

            if (props.enhanceForLongLists) {
                nextTick(() => {
                    const $chosenDropDown = getChosenJqueryEl();
                    $chosenDropDown.trigger("chosen:updated");
                });
            }
        };

        watch(() => props.modelValue, () => syncInternalValue());
        watch(() => props.options, () => syncInternalValue());
        watch(() => props.enhanceForLongLists, () => createOrDestroyChosen());
        watch(internalValue, () => {
            if (props.modelValue !== internalValue.value) {
                emit("update:modelValue", internalValue.value);
            }
        });

        onMounted(() => {
            isMounted = true;
            createOrDestroyChosen();

            // Fixup issues that may have cropped up during chosen initialization.
            if (props.modelValue !== internalValue.value) {
                emit("update:modelValue", internalValue.value);
            }
        });

        syncInternalValue();

        return {
            compiledFormControlClasses,
            internalValue,
            optionGroups,
            optionsWithoutGroup,
            theSelect
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-drop-down-list"
    name="dropdownlist">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :class="compiledFormControlClasses" v-bind="field" v-model="internalValue" ref="theSelect">
                <option v-if="showBlankItem" :value="blankValue"></option>

                <template v-if="grouped">
                    <option v-for="o in optionsWithoutGroup" :key="o.value" :value="o.value">{{o.text}}</option>
                    <optgroup v-for="g in optionGroups" :key="g.text" :label="g.text">
                        <option v-for="o in g.options" :key="o.value" :value="o.value">{{o.text}}</option>
                    </optgroup>
                </template>

                <option v-else v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </template>
</RockFormField>`
});
