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
import { PublicAttribute } from "../ViewModels";
import RockField from "./rockField";
import LoadingIndicator from "../Elements/loadingIndicator";
import { PublicAttributeValueCategory } from "../ViewModels/publicAttributeValueCategory";
import { List } from "../Util/linq";


type CategorizedAttributes = PublicAttributeValueCategory & {
    attributes: PublicAttribute[]
};


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
            type: Object as PropType<Record<string, PublicAttribute>>,
            required: true
        },
        showEmptyValues: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        showAbbreviatedName: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        displayAsTabs: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        showCategoryLabel: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        const validAttributes = computed((): PublicAttribute[] => {
            return new List(Object.values(props.attributes))
                .orderBy(a => a.order)
                .toArray();
        });

        const values = ref({ ...props.modelValue });

        const attributeCategories = computed(() => {
            const categoryList: CategorizedAttributes[] = [{
                guid: "0",
                name: "Attributes",
                order: 0,
                attributes: []
            }];

            validAttributes.value.forEach(attr => {
                console.log("Attr:", {name: attr.key, cats: attr.categories.map(cat => cat.name).join(",")});
                if (attr.categories.length > 0) {
                    const categories = [...attr.categories]; // copy, so sort doesn't cause updates
                    console.log("Categories:", categories);
                    categories.sort((a, b) => a.order - b.order).forEach((cat, i) => {
                        console.log("Category", i, cat.name);
                        const newCat: CategorizedAttributes = {attributes: [], ...cat}; // copy

                        // Make sure we only have 1 copy of any category in the list
                        if (!categoryList.some(oldCat => oldCat.guid == newCat.guid)) {
                            console.log("Add Category to list", cat.name);
                            categoryList.push(newCat);
                        }

                        if (i == 0) {
                            console.log("Add Attr to Category");
                            categoryList.find(cat => cat.guid == newCat.guid)?.attributes.push(attr);
                        }
                    });
                }
                else {
                    console.log("No Categories, add to default");
                    categoryList[0].attributes.push(attr);
                }
            });

            categoryList.sort((a, b) => a.order - b.order);



            return categoryList;
        });

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
            values,
            attributeCategories
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
