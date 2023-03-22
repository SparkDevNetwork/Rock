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
import RockSuspense from "./rockSuspense";
import LoadingIndicator from "./loadingIndicator";
import { List } from "@Obsidian/Utility/linq";
import TabbedContent from "./tabbedContent";
import RockField from "./rockField";
import { PublicAttributeCategoryBag } from "@Obsidian/ViewModels/Utility/publicAttributeCategoryBag";
import { emptyGuid } from "@Obsidian/Utility/guid";

type CategorizedAttributes = PublicAttributeCategoryBag & {
    attributes: PublicAttributeBag[]
};

export default defineComponent({
    name: "AttributeValuesContainer",
    components: {
        RockField,
        LoadingIndicator,
        RockSuspense,
        TabbedContent,
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
        },
        displayWithinExistingRow: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        displayAsTabs: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        showCategoryLabel: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        numberOfColumns: {
            type: Number as PropType<number>,
            default: 1
        },
        entityTypeName: {
            type: String as PropType<string>,
            default: ""
        },
        disabled: {
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

        const attributeCategories = computed(() => {
            // Initialize the category list with a "default" category
            const categoryList: CategorizedAttributes[] = [{
                guid: emptyGuid,
                name: "Attributes",
                order: 0,
                attributes: []
            }];

            validAttributes.value.forEach(attr => {
                // Skip empty attributes if we are not set to display empty values or we're not editing values
                if (!props.showEmptyValues && !props.isEditMode && attr.key && (props.modelValue[attr.key] ?? "") == "") {
                    return;
                }

                if (attr.categories && attr.categories.length > 0) {
                    const categories = [...attr.categories] as PublicAttributeCategoryBag[]; // copy, so sort doesn't cause updates

                    categories.sort((a, b) => a.order - b.order).forEach((cat, i) => {
                        const newCat: CategorizedAttributes = { attributes: [], ...cat }; // copy and convert to CategorizedAttributes

                        // Make sure we only have 1 copy of any category in the list
                        if (!categoryList.some(oldCat => oldCat.guid == newCat.guid)) {
                            categoryList.push(newCat);
                        }

                        // Add this attribute to the first (in order) category it is in
                        if (i == 0) {
                            categoryList.find(cat => cat.guid == newCat.guid)?.attributes.push(attr);
                        }
                    });
                }
                else {
                    // Put in "default" category
                    categoryList[0].attributes.push(attr);
                }
            });

            // Clear out any categories that don't have any attributes assigned to them, then sort the list by category order
            return categoryList.filter(cat => cat.attributes.length > 0).sort((a, b) => a.order - b.order);
        });

        const actuallyDisplayAsTabs = computed<boolean>(() => {
            if (attributeCategories.value.length === 0) {
                return false;
            }

            const hasCategories = attributeCategories.value.length > 1 || attributeCategories.value[0].guid !== emptyGuid;

            return hasCategories && props.displayAsTabs && !props.isEditMode;
        });

        const defaultCategoryHeading = computed<string>(() => {
            if (actuallyDisplayAsTabs.value || !props.entityTypeName) {
                return "Attributes";
            }

            return `${props.entityTypeName} Attributes`;
        });

        const columnClass = computed(() => {
            let numColumns = props.numberOfColumns;

            // Need to make the columns divisible by 12
            if (numColumns < 1) {
                numColumns = 1;
            }
            else if (numColumns == 5) {
                numColumns = 4;
            }
            else if (numColumns > 6 && numColumns < 12) {
                numColumns = 6;
            }
            else if (numColumns > 12) {
                numColumns = 12;
            }

            return `col-md-${12 / numColumns}`;
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
            attributeCategories,
            actuallyDisplayAsTabs,
            defaultCategoryHeading,
            columnClass
        };
    },

    template: `
<RockSuspense>
    <template #default>
        <div v-if="displayWithinExistingRow" :class="columnClass" v-for="a in validAttributes" :key="a.attributeGuid">
            <RockField
                :isEditMode="isEditMode"
                :attribute="a"
                :modelValue="values[a.key]"
                @update:modelValue="onUpdateValue(a.key, $event)"
                :showEmptyValue="showEmptyValues"
                :showAbbreviatedName="showAbbreviatedName"
            />
        </div>

        <TabbedContent v-else-if="actuallyDisplayAsTabs" :tabList="attributeCategories">
            <template #tab="{item}">
                {{ item.name }}
            </template>
            <template #tabpane="{item}">
                <div v-for="a in item.attributes" :key="a.attributeGuid">
                    <RockField
                        :isEditMode="isEditMode"
                        :attribute="a"
                        :modelValue="values[a.key]"
                        @update:modelValue="onUpdateValue(a.key, $event)"
                        :showEmptyValue="showEmptyValues"
                        :showAbbreviatedName="showAbbreviatedName"
                    />
                </div>
            </template>
        </TabbedContent>

        <template v-else>
            <div v-for="cat in attributeCategories" key="cat.guid">
                <h4 v-if="showCategoryLabel && cat.guid == '0' && !isEditMode">{{defaultCategoryHeading}}</h4>
                <h4 v-else-if="showCategoryLabel && cat.guid != '0'">{{cat.name}}</h4>

                <div class="attribute-value-container-display row">
                    <div :class="columnClass" v-for="a in cat.attributes" :key="a.attributeGuid">
                        <RockField
                            :isEditMode="isEditMode"
                            :attribute="a"
                            :modelValue="values[a.key]"
                            @update:modelValue="onUpdateValue(a.key, $event)"
                            :showEmptyValue="showEmptyValues"
                            :showAbbreviatedName="showAbbreviatedName"
                        />
                    </div>
                </div>
            </div>
        </template>
    </template>
    <template #loading>
        <LoadingIndicator />
    </template>
</RockSuspense>
`
});
