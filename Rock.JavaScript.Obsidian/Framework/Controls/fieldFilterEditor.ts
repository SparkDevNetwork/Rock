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

import { PropType, defineComponent, ref, TransitionGroup, watch } from "vue";
import { FieldFilterRuleRow } from "./fieldFilterRuleRow";
import DropDownList from "../Elements/dropDownList";
import { ListItem } from "../ViewModels";
import { newGuid } from "../Util/guid";
import { useVModelPassthrough } from "../Util/component";
import { FieldFilterSource} from "../ViewModels/Reporting/fieldFilterSource";
import { FieldFilterGroup } from "../ViewModels/Reporting/fieldFilterGroup";
import { FieldFilterRule } from "../ViewModels/Reporting/fieldFilterRule";
import { FilterExpressionType } from "../Reporting/filterExpressionType";

type ShowHide = "Show" | "Hide";
type AllAny = "All" | "Any";

// Maps for converting between `FilterExpressionType` and `ShowHide`/`AllAny`
const filterExpressionTypeMap: Record<ShowHide, Record<AllAny, FilterExpressionType>> = {
    Show: {
        All: FilterExpressionType.GroupAll,
        Any: FilterExpressionType.GroupAny
    },
    Hide: {
        All: FilterExpressionType.GroupAllFalse,
        Any: FilterExpressionType.GroupAnyFalse
    }
};

const filterExpressionToShowHideMap: ShowHide[] = ["Show", "Show", "Hide", "Hide"]; // Use FilterExpressionType - 1 as index
const filterExpressionToAllAnyMap: AllAny[] = ["All", "Any", "All", "Any"]; // Use FilterExpressionType - 1 as index


export default defineComponent({
    name: "FieldVisibilityRulesEditor",

    components: {
        TransitionGroup,
        DropDownList,
        FieldFilterRuleRow
    },

    props: {
        modelValue: {
            type: Object as PropType<FieldFilterGroup>,
            required: true
        },
        sources: {
            type: Array as PropType<FieldFilterSource[]>,
            required: true
        },
        title: {
            type: String as PropType<string>,
            default: ""
        },
        allowNestedGroups: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: ["update:modelValue"],

    setup(props, { emit }) {
        const filterGroup = useVModelPassthrough(props, "modelValue", emit, { deep: true });

        // Make sure non-required properties are initiated
        filterGroup.value.rules = filterGroup.value.rules || [];

        // We currently don't support nested groups, so fire a warning if anyone tries to use them
        watch(() => props.allowNestedGroups, () => {
            if (props.allowNestedGroups === true) {
                console.warn('Nested Filter Groups are not supported yet. Please set `allowNestedGroups` to `false`.')
            }
        });

        const showHide = ref<ShowHide>(filterExpressionToShowHideMap[filterGroup.value.expressionType - 1]);
        const showHideOptions: ListItem[] = [
            { text: "Show", value: "Show" },
            { text: "Hide", value: "Hide" }
        ];

        const allAny = ref<AllAny>(filterExpressionToAllAnyMap[filterGroup.value.expressionType - 1]);
        const allAnyOptions: ListItem[] = [
            { text: "All", value: "All" },
            { text: "Any", value: "Any" }
        ];

        watch(() => props.modelValue, () => {
            filterGroup.value.rules = filterGroup.value.rules || [];
        }, { immediate: true });

        watch([showHide, allAny], () => {
            filterGroup.value.expressionType = filterExpressionTypeMap[showHide.value][allAny.value];
        });

        watch(() => filterGroup.value.expressionType, () => {
            showHide.value = filterExpressionToShowHideMap[filterGroup.value.expressionType - 1];
            allAny.value = filterExpressionToAllAnyMap[filterGroup.value.expressionType - 1];
        })

        function addRule():void {
            (filterGroup.value.rules as FieldFilterRule[]).push({ 
                guid: newGuid(),
                comparisonType: 0,
                value: "",
                sourceType: 0,
                attributeGuid: props.sources[0].attribute?.attributeGuid
            });
        }

        function removeRule(rule: FieldFilterRule): void {
            filterGroup.value.rules = (filterGroup.value.rules || []).filter((val: FieldFilterRule) => val !== rule);
        }

        return {
            showHide,
            showHideOptions,
            allAny,
            allAnyOptions,
            filterGroup,
            addRule,
            removeRule,
        };
    }, 

    template: `
<div class="filtervisibilityrules-container">
    <div class="filtervisibilityrules-rulesheader">
        <div class="filtervisibilityrules-type form-inline form-inline-all">
            <DropDownList v-model="showHide" :options="showHideOptions" :show-blank-item="false" formControlClasses="input-width-sm margin-r-sm" />
            <div class="form-control-static margin-r-sm">
                <span class="filtervisibilityrules-fieldname">{{ title }}</span><span class="filtervisibilityrules-if"> if</span>
            </div>
            <DropDownList v-model="allAny" :options="allAnyOptions" :show-blank-item="false" formControlClasses="input-width-sm margin-r-sm" />
            <span class="form-control-static">of the following match:</span>
        </div>
    </div>

    <div class="filtervisibilityrules-ruleslist ">
        <FieldFilterRuleRow v-for="rule in filterGroup.rules" :key="rule.guid" v-model="rule" :sources="sources" @removeRule="removeRule" />
    </div>

    <div class="filter-actions">
        <button class="btn btn-xs btn-action add-action" @click.prevent="addRule"><i class="fa fa-filter"></i> Add Criteria</button>
    </div>
</div>
`
});
