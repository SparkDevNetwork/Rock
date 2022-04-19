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
import DropDownList from "../Elements/dropDownList";
import TextBox from "../Elements/textBox";
import { ComparisonValue } from "../Reporting/comparisonValue";
import { areEqual } from "../Util/guid";
import { updateRefValue } from "../Util/util";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import RockAttributeFilter from "./rockAttributeFilter";

export const FieldFilterRuleRow = defineComponent({
    name: "FieldFilterRuleRow",

    components: {
        DropDownList,
        TextBox,
        RockAttributeFilter
    },

    props: {
        modelValue: {
            type: Object as PropType<FieldFilterRuleBag>,
            required: true
        },
        sources: {
            type: Array as PropType<FieldFilterSourceBag[]>,
            required: true
        }
    },

    emits: [
        "update:modelValue",
        "removeRule"
    ],

    setup(props, { emit }) {
        let internalUpdate = false;

        const attributeGuid = ref(props.modelValue.attributeGuid);
        const comparisonValue = ref<ComparisonValue>({
            comparisonType: props.modelValue.comparisonType,
            value: props.modelValue.value ?? ""
        });

        // Current Selected Attribute/Property
        const currentAttribute = computed<PublicAttributeBag>(() => {
            const source = props.sources.find(source => {
                return areEqual(attributeGuid.value ?? "", source.attribute?.attributeGuid ?? "");
            }) || props.sources[0];

            return source.attribute as PublicAttributeBag;
        });

        // Convert the list of sources into the options you can choose from the
        const attributeList = computed<ListItemBag[]>(() => {
            return props.sources.map(source => {
                return {
                    text: source.attribute?.name as string,
                    value: source.attribute?.attributeGuid as string
                };
            });
        });

        function onRemoveRuleClick(): void {
            emit("removeRule", props.modelValue);
        }

        // Watch for changes to the model value and update our internal values.
        watch(() => props.modelValue, () => {
            // Prevent a value reset.
            internalUpdate = true;

            updateRefValue(attributeGuid, props.modelValue.attributeGuid);
            updateRefValue(comparisonValue, {
                comparisonType: props.modelValue.comparisonType,
                value: props.modelValue.value ?? ""
            });

            internalUpdate = false;
        });

        // Watch for changes to our internal values and update the model value.
        watch([attributeGuid, comparisonValue], () => {
            const newValue: FieldFilterRuleBag = {
                ...props.modelValue,
                attributeGuid: attributeGuid.value,
                comparisonType: comparisonValue.value.comparisonType ?? 0,
                value: comparisonValue.value.value
            };

            emit("update:modelValue", newValue);
        });

        // Reset the rule after a new attribute is chosen
        watch(currentAttribute, () => {
            if (!internalUpdate) {
                comparisonValue.value = {
                    comparisonType: 0,
                    value: ""
                };
                attributeGuid.value = currentAttribute.value.attributeGuid;
            }
        });

        return {
            attributeGuid,
            attributeList,
            comparisonValue,
            currentAttribute,
            onRemoveRuleClick,
        };
    },

    template: `
    <div class="d-flex form-group">
        <div class="flex-fill">
            <div class="row form-row">
                <div class="filter-rule-comparefield col-md-4">
                    <DropDownList :options="attributeList" v-model="attributeGuid" :show-blank-item="false"  />
                </div>
                <div class="filter-rule-fieldfilter col-md-8">
                    <RockAttributeFilter :attribute="currentAttribute" v-model="comparisonValue" :filter-mode="1" required />
                </div>
            </div>
        </div>
        <div class="flex-shrink-0 ml-2">
            <button class="btn btn-danger btn-square" @click.prevent="onRemoveRuleClick"><i class="fa fa-times"></i></button>
        </div>
    </div>
    `
});
