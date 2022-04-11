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
import { DragSource, DragTarget, IDragSourceOptions } from "../../../../Directives/dragDrop";
import { areEqual, Guid, newGuid } from "../../../../Util/guid";
import { PublicEditableAttributeValue, ListItem } from "../../../../ViewModels";
import ConfigurableZone from "./configurableZone";
import { FormField, FormSection } from "../Shared/types";

function getAttributeValueFromField(field: FormField): PublicEditableAttributeValue {
    return {
        attributeGuid: newGuid(),
        fieldTypeGuid: field.fieldTypeGuid,
        name: !(field.isHideLabel ?? false) ? field.name : "",
        key: field.key,
        configurationValues: field.configurationValues,
        value: field.defaultValue,
        isRequired: field.isRequired ?? false,
        description: field.description ?? "",
        order: 0,
        categories: []
    };
}

const fieldWrapper = defineComponent({
    name: "Workflow.FormBuilderDetail.SectionZone.FieldWrapper",

    components: {
        RockField
    },

    props: {
        modelValue: {
            type: Object as PropType<FormField>,
            required: true
        }
    },

    setup(props) {
        const attributeValue = ref<PublicEditableAttributeValue>(getAttributeValueFromField(props.modelValue));

        watch(() => props.modelValue, () => {
            attributeValue.value = getAttributeValueFromField(props.modelValue);
        }, {
            deep: true
        });

        return {
            attributeValue
        };
    },

    template: `
<RockField :attributeValue="attributeValue" isEditMode />
`
});

export default defineComponent({
    name: "Workflow.FormBuilderDetail.SectionZone",
    components: {
        ConfigurableZone,
        RockField,
        FieldWrapper: fieldWrapper
    },

    directives: {
        DragSource,
        DragTarget
    },

    props: {
        modelValue: {
            type: Object as PropType<FormSection>,
            required: true
        },

        dragTargetId: {
            type: String as PropType<Guid>,
            required: true
        },

        reorderDragOptions: {
            type: Object as PropType<IDragSourceOptions>,
            required: true
        },

        activeZone: {
            type: String as PropType<string>,
            required: false
        },

        sectionTypeOptions: {
            type: Array as PropType<ListItem[]>,
            default: []
        }
    },

    emits: [
        "configureField",
        "delete",
        "deleteField"
    ],

    setup(props, { emit }) {
        /** The unique identifier of the section being rendered. */
        const sectionGuid = ref(props.modelValue.guid);

        /** The title to display at the top of the section. */
        const title = ref(props.modelValue.title);

        /** The description to display at the top of the section. */
        const description = ref(props.modelValue.description);

        /** True if the header separator line should be displayed. */
        const showSeparator = ref(props.modelValue.showHeadingSeparator);

        /** The visual type of the section. */
        const sectionType = ref(props.modelValue.type);

        /** The fields that exist in this section. */
        const fields = ref(props.modelValue.fields);

        /** The CSS class name to apply to the section. */
        const sectionTypeClass = computed((): string => {
            if (sectionType.value) {
                const sectionTypeValue = sectionType.value;
                const matches = props.sectionTypeOptions.filter(t => areEqual(sectionTypeValue, t.value));

                if (matches.length > 0) {
                    return matches[0].category ?? "";
                }
            }

            return "";
        });

        /** True if the section is active, that is highlighted. */
        const isSectionActive = computed((): boolean => props.activeZone === sectionGuid.value);

        /**
         * Determines the column size CSS class to use for the given field.
         * 
         * @param field The field to be rendered.
         *
         * @returns The CSS classes to apply to the element.
         */
        const getFieldColumnSize = (field: FormField): string => `flex-col flex-col-${field.size}`;

        /**
         * Checks if the field is active, that is currently being edited.
         * 
         * @param field The field in question.
         *
         * @returns true if the field is active and should be highlighted.
         */
        const isFieldActive = (field: FormField): boolean => {
            return field.guid === props.activeZone;
        };

        /**
         * Event handler for when a field is requesting edit mode.
         * 
         * @param field The field requesting to being edit mode.
         */
        const onConfigureField = (field: FormField): void => {
            emit("configureField", field);
        };

        /**
         * Event handler for when the delete button of the section is clicked.
         */
        const onDelete = (): void => {
            emit("delete", props.modelValue.guid);
        };

        /**
         * Event handler for when the delete button of a field is clicked.
         * 
         * @param field The field to be deleted.
         */
        const onDeleteField = (field: FormField): void => {
            emit("deleteField", field.guid);
        };

        // Watch for changes in the model properties. We don't do a deep watch
        // on props.modelValue because we don't want to re-render everything
        // if, for example, a field configuration value changes. That is handled
        // by other components already.
        watch(() => [props.modelValue.guid, props.modelValue.title, props.modelValue.description, props.modelValue.showHeadingSeparator, props.modelValue.type, props.modelValue.fields], () => {
            console.log("section changed");
            sectionGuid.value = props.modelValue.guid;
            title.value = props.modelValue.title;
            description.value = props.modelValue.description;
            showSeparator.value = props.modelValue.showHeadingSeparator;
            sectionType.value = props.modelValue.type;
            fields.value = props.modelValue.fields;
        });

        return {
            description,
            fields,
            getFieldColumnSize,
            isFieldActive,
            isSectionActive,
            onConfigureField,
            onDelete,
            onDeleteField,
            sectionGuid,
            sectionTypeClass,
            showSeparator,
            title
        };
    },

    template: `
<ConfigurableZone class="zone-section" :modelValue="isSectionActive">
    <div class="zone-body d-flex flex-column" style="min-height: 100%;">
        <div class="d-flex flex-column" :class="sectionTypeClass" style="flex-grow: 1;">
            <div>
                <h1 v-if="title">{{ title }}</h1>
                <div v-if="description" class="mb-2">{{ description }}</div>
                <hr v-if="showSeparator" />
            </div>

            <div class="form-section" v-drag-source="reorderDragOptions" v-drag-target="reorderDragOptions.id" v-drag-target:2="dragTargetId" :data-section-id="sectionGuid">
                <ConfigurableZone v-for="field in fields" :key="field.guid" :modelValue="isFieldActive(field)" :class="getFieldColumnSize(field)" :data-field-id="field.guid" @configure="onConfigureField(field)">
                    <div class="zone-body">
                        <FieldWrapper :modelValue="field" />
                    </div>

                    <template #preActions>
                        <i class="fa fa-bars fa-fw zone-action zone-action-move"></i>
                        <span class="zone-action-pad"></span>
                    </template>
                    <template #postActions>
                        <i class="fa fa-times fa-fw zone-action" @click.stop="onDeleteField(field)"></i>
                    </template>
                </ConfigurableZone>
            </div>
        </div>
    </div>
    <template #preActions>
        <i class="fa fa-bars fa-fw zone-action zone-action-move"></i>
        <span class="zone-action-pad"></span>
    </template>
    <template #postActions>
        <i class="fa fa-times fa-fw zone-action" @click.stop="onDelete"></i>
    </template>
</ConfigurableZone>
`
});
