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
import { computed, defineComponent, PropType } from "vue";
import { getFieldType } from "../Fields/index";
import { ComparisonValue } from "../Reporting/comparisonValue";
import { FilterMode } from "../Reporting/filterMode";
import { useVModelPassthrough } from "../Util/component";
import { PublicFilterableAttribute } from "../ViewModels/publicFilterableAttribute";

export default defineComponent({
    name: "RockAttributeFilter",

    props: {
        modelValue: {
            type: Object as PropType<ComparisonValue>,
            default: { value: "" }
        },
        attribute: {
            type: Object as PropType<PublicFilterableAttribute>,
            required: true
        },
        required: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        filterMode: {
            type: Number as PropType<FilterMode>,
            default: FilterMode.Simple
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** The internal value used by the filter component. */
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** The field type instance being edited. */
        const field = computed(() => {
            return getFieldType(props.attribute.fieldTypeGuid);
        });

        /** The filter component to use to display and edit the value. */
        const filterComponent = computed(() => field.value?.getFilterComponent());

        /** The configuration values for the editor component. */
        const configurationValues = computed(() => props.attribute.configurationValues ?? {});

        return {
            configurationValues,
            filterComponent,
            internalValue
        };
    },
    template: `
<component :is="filterComponent"
    v-model="internalValue"
    :configurationValues="configurationValues"
    :required="required"
    :filterMode="filterMode" />
`
});
