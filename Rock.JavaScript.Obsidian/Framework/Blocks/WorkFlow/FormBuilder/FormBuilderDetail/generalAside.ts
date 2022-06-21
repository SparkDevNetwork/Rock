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
import RockField from "../../../../Controls/rockField";
import { DragSource, IDragSourceOptions } from "../../../../Directives/dragDrop";
import Alert from "../../../../Elements/alert";
import DropDownList from "../../../../Elements/dropDownList";
import RockLabel from "../../../../Elements/rockLabel";
import Switch from "../../../../Elements/switch";
import { toNumberOrNull } from "../../../../Services/number";
import { ListItem } from "../../../../ViewModels";
import TransitionVerticalCollapse from "../../../../Elements/transitionVerticalCollapse";
import { CampusSetFrom, FormFieldType } from "../Shared/types";
import ConfigurableZone from "./configurableZone";
import { GeneralAsideSettings } from "./types";
import { useFormSources } from "./utils";

const campusSetFromOptions: ListItem[] = [
    {
        value: CampusSetFrom.CurrentPerson.toString(),
        text: "Current Person"
    },
    {
        value: CampusSetFrom.WorkflowPerson.toString(),
        text: "Workflow Person"
    },
    {
        value: CampusSetFrom.QueryString.toString(),
        text: "Query String"
    }
];

export default defineComponent({
    name: "Workflow.FormBuilderDetail.GeneralAside",
    components: {
        Alert,
        ConfigurableZone,
        DropDownList,
        RockField,
        RockLabel,
        Switch,
        TransitionVerticalCollapse
    },

    directives: {
        DragSource
    },

    props: {
        modelValue: {
            type: Object as PropType<GeneralAsideSettings>,
            required: true
        },

        sectionDragOptions: {
            type: Object as PropType<IDragSourceOptions>,
            required: true
        },

        fieldDragOptions: {
            type: Object as PropType<IDragSourceOptions>,
            required: true
        },

        isPersonEntryForced: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            return true;
        }
    },

    setup(props, { emit }) {
        /**
         * The value for the drop down that specifies where to set the campus
         * context from for this form.
         */
        const campusSetFrom = ref(props.modelValue.campusSetFrom?.toString() ?? "");

        /** True if the form has a person entry section. */
        const hasPersonEntry = ref(props.modelValue.hasPersonEntry ?? false);

        const fieldTypes = useFormSources().fieldTypes ?? [];
        const isAdditionalFieldsVisible = ref(false);

        /** The field types to display in the common field types section. */
        const commonFieldTypes = computed((): FormFieldType[] => {
            return fieldTypes.filter(f => f.isCommon);
        });

        /** The field types to display in the advanced fiel types section. */
        const advancedFieldTypes = computed((): FormFieldType[] => {
            return fieldTypes.filter(f => !f.isCommon);
        });

        const additionalFieldsClass = computed((): string => {
            return isAdditionalFieldsVisible.value ? "fa fa-chevron-up" : "fa fa-chevron-down";
        });

        /** Used to temporarily disable emitting the modelValue when something changes. */
        let autoSyncModelValue = true;

        const onAdditionalFieldsClick = (): void => {
            isAdditionalFieldsVisible.value = !isAdditionalFieldsVisible.value;
        };

        // Watch for changes in the model value and update our internal values.
        watch(() => props.modelValue, () => {
            autoSyncModelValue = false;
            campusSetFrom.value = props.modelValue.campusSetFrom?.toString() ?? "";
            hasPersonEntry.value = props.modelValue.hasPersonEntry ?? false;
            autoSyncModelValue = true;
        });

        // Watch for changes in our internal values and update the modelValue.
        watch([campusSetFrom, hasPersonEntry], () => {
            if (!autoSyncModelValue) {
                return;
            }

            const value: GeneralAsideSettings = {
                campusSetFrom: toNumberOrNull(campusSetFrom.value) ?? undefined,
                hasPersonEntry: hasPersonEntry.value
            };

            emit("update:modelValue", value);
        });

        return {
            additionalFieldsClass,
            advancedFieldTypes,
            campusSetFrom,
            campusSetFromOptions,
            commonFieldTypes,
            hasPersonEntry,
            isAdditionalFieldsVisible,
            onAdditionalFieldsClick
        };
    },

    template: `
<div class="form-sidebar">
    <div class="sidebar-header">
        <span class="title">Field List</span>
    </div>

    <div class="sidebar-body">
        <div class="panel-body">
            <div v-drag-source="sectionDragOptions">
                <div class="form-template-item form-template-item-section">
                    <i class="fa fa-expand fa-fw"></i>
                    Section
                </div>
            </div>

            <div class="mt-3">
                <RockLabel>Common Fields</RockLabel>

                <div class="form-template-item-list" v-drag-source="fieldDragOptions">
                    <div v-for="field in commonFieldTypes" class="form-template-item form-template-item-field" :data-field-type="field.guid">
                        <span class="inline-svg icon" v-html="field.svg"></span>
                        <div class="text">{{ field.text }}</div>
                    </div>
                </div>

                <div @click="onAdditionalFieldsClick" class="mt-2">
                    <RockLabel>Additional Fields <i :class="additionalFieldsClass"></i></RockLabel>
                </div>

                <TransitionVerticalCollapse>
                    <div v-if="isAdditionalFieldsVisible" class="form-template-item-list" v-drag-source="fieldDragOptions">
                        <div v-for="field in advancedFieldTypes" class="form-template-item form-template-item-field" :data-field-type="field.guid">
                            <span class="inline-svg icon" v-html="field.svg"></span>
                            <div class="text">{{ field.text }}</div>
                        </div>
                    </div>
                </TransitionVerticalCollapse>
            </div>

            <div class="mt-3">
                <Switch v-if="!isPersonEntryForced" v-model="hasPersonEntry" text="Enable Person Entry" />

                <Alert v-else alertType="info">
                    Person entry is enabled on the template and cannot be changed.
                </Alert>

                <DropDownList v-model="campusSetFrom" label="Campus Set From" :options="campusSetFromOptions" />
            </div>
        </div>
    </div>
</div>
`
});
