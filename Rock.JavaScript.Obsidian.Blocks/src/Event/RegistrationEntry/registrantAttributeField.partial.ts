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
import RockField from "@Obsidian/Controls/rockField";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { FilterExpressionType } from "@Obsidian/Core/Reporting/filterExpressionType";
import { RegistrationEntryBlockFormFieldRuleViewModel, RegistrationEntryBlockFormFieldViewModel } from "./types.partial";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { areEqual } from "@Obsidian/Utility/guid";

function isRuleMet(rule: RegistrationEntryBlockFormFieldRuleViewModel, fieldValues: Record<Guid, unknown>, formFields: RegistrationEntryBlockFormFieldViewModel[]): boolean {
    const value = fieldValues[rule.comparedToRegistrationTemplateFormFieldGuid] || "";

    if (typeof value !== "string") {
        return false;
    }

    const comparedToFormField = formFields.find(ff => areEqual(ff.guid, rule.comparedToRegistrationTemplateFormFieldGuid));
    if (!comparedToFormField?.attribute?.fieldTypeGuid) {
        return false;
    }

    const fieldType = getFieldType(comparedToFormField.attribute.fieldTypeGuid);

    if (!fieldType) {
        return false;
    }

    return fieldType.doesValueMatchFilter(value, rule.comparisonValue, comparedToFormField.attribute.configurationValues ?? {});
}

export default defineComponent({
    name: "Event.RegistrationEntry.RegistrantAttributeField",

    components: {
        NotificationBox,
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
        },

        formFields: {
            type: Array as PropType<RegistrationEntryBlockFormFieldViewModel[]>,
            required: true
        }
    },

    setup(props) {
        const isVisible = computed(() => {
            switch (props.field.visibilityRuleType) {
                case FilterExpressionType.GroupAll:
                    return props.field.visibilityRules.every(vr => isRuleMet(vr, props.fieldValues, props.formFields));

                case FilterExpressionType.GroupAllFalse:
                    return props.field.visibilityRules.every(vr => !isRuleMet(vr, props.fieldValues, props.formFields));

                case FilterExpressionType.GroupAny:
                    return props.field.visibilityRules.some(vr => isRuleMet(vr, props.fieldValues, props.formFields));

                case FilterExpressionType.GroupAnyFalse:
                    return props.field.visibilityRules.some(vr => !isRuleMet(vr, props.fieldValues, props.formFields));
            }

            return true;
        });

        const value = ref<string>((props.fieldValues[props.field.guid] as string) ?? "");
        const modifiedAttribute = computed(() => {
            if (!props.field.attribute) {
                return null;
            }

            const fieldAttribute = { ...props.field.attribute };
            fieldAttribute.isRequired = props.field.isRequired;
            return fieldAttribute;
        });

        // Detect changes like switch from one person to another.
        watch(() => props.fieldValues[props.field.guid], () => {
            value.value = props.fieldValues[props.field.guid] as string;
        });

        watch(value, () => {
            props.fieldValues[props.field.guid] = value.value;
        });

        return {
            isVisible,
            modifiedAttribute,
            value
        };
    },

    template: `
<template v-if="isVisible">
    <RockField v-if="modifiedAttribute" v-model="value" isEditMode :attribute="modifiedAttribute" />
    <NotificationBox v-else alertType="danger">Could not resolve attribute field</NotificationBox>
</template>`
});
