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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import RockField from "../../../Controls/rockField";
import Alert from "../../../Elements/alert.vue";
import { ComparisonType } from "../../../Reporting/comparisonType";
import { FilterExpressionType } from "../../../Reporting/filterExpressionType";
import { RegistrationEntryBlockFormFieldRuleViewModel, RegistrationEntryBlockFormFieldViewModel } from "./types";

function isRuleMet(rule: RegistrationEntryBlockFormFieldRuleViewModel, fieldValues: Record<Guid, unknown>): boolean {
    const value = fieldValues[rule.comparedToRegistrationTemplateFormFieldGuid] || "";

    if (typeof value !== "string") {
        return false;
    }

    const strVal = value.toLowerCase().trim();
    const comparison = rule.comparedToValue.toLowerCase().trim();

    if (!strVal) {
        return false;
    }

    switch (rule.comparisonType) {
        case ComparisonType.EqualTo:
            return strVal === comparison;
        case ComparisonType.NotEqualTo:
            return strVal !== comparison;
        case ComparisonType.Contains:
            return strVal.includes(comparison);
        case ComparisonType.DoesNotContain:
            return !strVal.includes(comparison);
    }

    return false;
}

export default defineComponent({
    name: "Event.RegistrationEntry.RegistrantAttributeField",

    components: {
        Alert,
        RockField
    },

    props: {
        field: {
            type: Object as PropType<RegistrationEntryBlockFormFieldViewModel>,
            required: true
        },

        fieldValues: {
            type: Object as PropType<Record<Guid, unknown>>,
            required: true
        }
    },

    setup(props) {
        const isVisible = computed(() => {
            switch (props.field.visibilityRuleType) {
                case FilterExpressionType.GroupAll:
                    return props.field.visibilityRules.every(vr => isRuleMet(vr, props.fieldValues));

                case FilterExpressionType.GroupAllFalse:
                    return props.field.visibilityRules.every(vr => !isRuleMet(vr, props.fieldValues));

                case FilterExpressionType.GroupAny:
                    return props.field.visibilityRules.some(vr => isRuleMet(vr, props.fieldValues));

                case FilterExpressionType.GroupAnyFalse:
                    return props.field.visibilityRules.some(vr => !isRuleMet(vr, props.fieldValues));
            }

            return true;
        });

        const attribute = ref(props.field.attribute);
        const value = ref(props.fieldValues[props.field.guid] ?? "");

        // Detect changes like switch from one person to another.
        watch(() => props.fieldValues[props.field.guid], () => {
            value.value = props.fieldValues[props.field.guid];
        });

        watch(value, () => {
            props.fieldValues[props.field.guid] = value.value;
        });

        return {
            isVisible,
            attribute,
            value
        };
    },

    template: `
<template v-if="isVisible">
    <RockField v-if="attribute" v-model="value" isEditMode :attribute="attribute" />
    <Alert v-else alertType="danger">Could not resolve attribute field</Alert>
</template>`
});
