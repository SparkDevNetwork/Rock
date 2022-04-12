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
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import RockField from "./rockField";
import LoadingIndicator from "../Elements/loadingIndicator";
import { List } from "../Util/linq";

export default defineComponent({
    name: "AttributeValuesContainer",
    components: {
        RockField,
        LoadingIndicator
    },
    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        attributes: {
            type: Object as PropType<Record<string, PublicAttributeBag>>,
            required: true
        },
        showEmptyValues: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        showAbbreviatedName: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        const validAttributes = computed((): PublicAttributeBag[] => {
            return new List(Object.values(props.attributes))
                .orderBy(a => a.order)
                .toArray();
        });

        const values = ref({ ...props.modelValue });

        const onUpdateValue = (key: string, value: string): void => {
            values.value[key] = value;

            emit("update:modelValue", values.value);
        };

        watch(() => props.modelValue, () => {
            values.value = { ...props.modelValue };
        });

        return {
            onUpdateValue,
            validAttributes,
            values
        };
    },

    template: `
<Suspense>
    <template #default>
        <div v-for="a in validAttributes">
            <RockField
                :isEditMode="isEditMode"
                :attribute="a"
                :modelValue="values[a.key]"
                @update:modelValue="onUpdateValue(a.key, $event)"
                :showEmptyValue="showEmptyValues"
                :showAbbreviatedName="showAbbreviatedName" />
        </div>
    </template>
    <template #fallback>
        <LoadingIndicator />
    </template>
</Suspense>
`
});
