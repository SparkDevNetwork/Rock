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

import { computed, defineComponent, reactive, Ref, ref, watch } from "vue";
import DropDownList from "../../../../Elements/dropDownList";
import Modal from "../../../../Controls/modal";
import Panel from "../../../../Controls/panel";
import RockButton from "../../../../Elements/rockButton";
import RockLabel from "../../../../Elements/rockLabel";
import RockForm from "../../../../Controls/rockForm";
import Switch from "../../../../Elements/switch";
import TextBox from "../../../../Elements/textBox";
import ConfigurableZone from "./configurableZone";
import FieldEditAside from "./fieldEditAside";
import FormContentModal from "./formContentModal";
import FormContentZone from "./formContentZone";
import GeneralAside from "./generalAside";
import PersonEntryEditAside from "./personEntryEditAside";
import SectionEditAside from "./sectionEditAside";
import SectionZone from "./sectionZone";
import { DragSource, DragTarget, IDragSourceOptions } from "../../../../Directives/dragDrop";
import { areEqual, Guid, newGuid } from "../../../../Util/guid";
import { List } from "../../../../Util/linq";
import { FormBuilderSettings, FormTemplateListItem, GeneralAsideSettings, IAsideProvider, SectionAsideSettings } from "./types";
import { FormField, FormFieldType, FormPersonEntry, FormSection } from "../Shared/types";
import { PropType } from "vue";
import { useFormSources } from "./utils";
import { confirmDelete } from "../../../../Util/dialogs";
import { ListItem } from "../../../../ViewModels";

/**
 * Get the drag source options for the section zones. This allows the user to
 * drag a zone placeholder into the form to add a new zone.
 * 
 * @param sections The (reactive) array of sections to update.
 *
 * @returns The IDragSourceOptions object to use for drag operations.
 */
function getSectionDragSourceOptions(sections: FormSection[]): IDragSourceOptions {
    return {
        id: newGuid(),
        copyElement: true,
        dragDrop(operation) {
            operation.element.remove();

            if (operation.targetIndex !== undefined) {
                sections.splice(operation.targetIndex, 0, {
                    guid: newGuid(),
                    title: "",
                    description: "",
                    showHeadingSeparator: false,
                    type: null,
                    fields: []
                });
            }
        }
    };
}

/**
 * Get the drag source options for the field types. This allows the user to
 * drag a new field type placeholder into the form to add a new field.
 *
 * @param sections The (reactive) array of sections to update.
 * @param availableFieldTypes The list of field types that are available to be used.
 *
 * @returns The IDragSourceOptions object to use for drag operations.
 */
function getFieldDragSourceOptions(sections: FormSection[], availableFieldTypes: Ref<FormFieldType[]>): IDragSourceOptions {
    return {
        id: newGuid(),
        copyElement: true,
        dragOver(operation) {
            if (operation.targetContainer && operation.targetContainer instanceof HTMLElement) {
                operation.targetContainer.closest(".zone-section")?.classList.add("highlight");
            }
        },
        dragOut(operation) {
            if (operation.targetContainer && operation.targetContainer instanceof HTMLElement) {
                operation.targetContainer.closest(".zone-section")?.classList.remove("highlight");
            }
        },
        dragShadow(operation) {
            if (operation.shadow) {
                operation.shadow.classList.remove("col-xs-6");
                operation.shadow.classList.add("flex-col", "flex-col-12");
            }
        },
        dragDrop(operation) {
            operation.element.remove();

            const fieldTypeGuid = (operation.element as HTMLElement).dataset.fieldType ?? "";
            const sectionGuid = (operation.targetContainer as HTMLElement).dataset.sectionId ?? "";
            const section = new List(sections).firstOrUndefined(s => areEqual(s.guid, sectionGuid));
            const fieldType = new List(availableFieldTypes.value).firstOrUndefined(f => areEqual(f.guid, fieldTypeGuid));

            if (section && fieldType && operation.targetIndex !== undefined) {
                const existingKeys: string[] = [];

                // Find all existing attribute keys.
                for (const sect of sections) {
                    if (sect.fields) {
                        for (const field of sect.fields) {
                            existingKeys.push(field.key);
                        }
                    }
                }

                // Find a key that isn't used.
                const baseKey = fieldType.text.replace(/[^a-zA-Z0-9_\-.]/g, "");
                let key = baseKey;
                let keyCount = 0;
                while (existingKeys.includes(key)) {
                    keyCount++;
                    key = `${baseKey}${keyCount}`;
                }

                if (!section.fields) {
                    section.fields = [];
                }

                section.fields.splice(operation.targetIndex, 0, {
                    guid: newGuid(),
                    fieldTypeGuid: fieldType.guid,
                    name: fieldType.text,
                    key: key,
                    size: 12,
                    configurationValues: {},
                    defaultValue: ""
                });
            }
        }
    };
}

/**
 * Get the drag source options for re-ordering the fields. This allows the user
 * to drag and drop existing fields to move them around the form.
 *
 * @param sections The (reactive) array of sections to update.
 *
 * @returns The IDragSourceOptions object to use for drag operations.
 */
function getFieldReorderDragSourceOptions(sections: FormSection[]): IDragSourceOptions {
    return {
        id: newGuid(),
        copyElement: false,
        handleSelector: ".zone-actions > .zone-action-move",
        dragOver(operation) {
            if (operation.targetContainer && operation.targetContainer instanceof HTMLElement) {
                operation.targetContainer.closest(".zone-section")?.classList.add("highlight");
            }
        },
        dragOut(operation) {
            if (operation.targetContainer && operation.targetContainer instanceof HTMLElement) {
                operation.targetContainer.closest(".zone-section")?.classList.remove("highlight");
            }
        },
        dragDrop(operation) {
            const sourceSectionGuid = (operation.sourceContainer as HTMLElement).dataset.sectionId ?? "";
            const targetSectionGuid = (operation.targetContainer as HTMLElement).dataset.sectionId ?? "";
            const sourceSection = new List(sections).firstOrUndefined(s => areEqual(s.guid, sourceSectionGuid));
            const targetSection = new List(sections).firstOrUndefined(s => areEqual(s.guid, targetSectionGuid));

            if (sourceSection?.fields && targetSection?.fields && operation.targetIndex !== undefined) {
                const field = sourceSection.fields[operation.sourceIndex];

                sourceSection.fields.splice(operation.sourceIndex, 1);
                targetSection.fields.splice(operation.targetIndex, 0, field);
            }
        }
    };
}

/**
 * Get the drag source options for re-ordering the sections. This allows the user
 * to drag and drop existing sections to move them around the form.
 *
 * @param sections The (reactive) array of sections to update.
 *
 * @returns The IDragSourceOptions object to use for drag operations.
 */
function getSectionReorderDragSourceOptions(sections: FormSection[]): IDragSourceOptions {
    return {
        id: newGuid(),
        copyElement: false,
        handleSelector: ".zone-section > .zone-actions > .zone-action-move",
        dragDrop(operation) {
            if (operation.targetIndex !== undefined) {
                const section = sections[operation.sourceIndex];

                sections.splice(operation.sourceIndex, 1);
                sections.splice(operation.targetIndex, 0, section);
            }
        }
    };
}

// Unique identifiers for the standard zones.
const formHeaderZoneGuid = "C7D522D0-A18C-4CB0-B604-B2E9727E9E33";
const formFooterZoneGuid = "317E5892-C156-4614-806F-BE4CAB67AC10";
const personEntryZoneGuid = "5257312E-102C-4026-B558-10184AFEAC4D";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.FormBuilderTab",

    components: {
        ConfigurableZone,
        DropDownList,
        FieldEditAside,
        FormContentModal,
        FormContentZone,
        GeneralAside,
        Modal,
        Panel,
        RockButton,
        RockForm,
        RockLabel,
        PersonEntryEditAside,
        SectionEditAside,
        SectionZone,
        Switch,
        TextBox
    },

    directives: {
        DragSource,
        DragTarget,
    },

    props: {
        modelValue: {
            type: Object as PropType<FormBuilderSettings>,
            required: true
        },

        templateOverrides: {
            type: Object as PropType<FormTemplateListItem>
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        // #region Values

        const sources = useFormSources();

        const sectionTypeOptions = sources.sectionTypeOptions ?? [];

        /**
         * The section that are currently displayed on the form. This is reactive
         * since we make live updates to it in various places.
         */
        const sections = reactive<FormSection[]>(props.modelValue.sections ?? []);

        /** The header HTML content that will appear above the form. */
        const formHeaderContent = ref(props.modelValue.headerContent ?? "");

        /** The footer HTML content that will appear below the form. */
        const formFooterContent = ref(props.modelValue.footerContent ?? "");

        /** The header HTML content while it is being edited in the modal. */
        const formHeaderEditContent = ref("");

        /** The footer HTML content while it is being edited in the modal. */
        const formFooterEditContent = ref("");

        /** All the field types that are available for use when designing a form. */
        const availableFieldTypes = ref(sources.fieldTypes ?? []);

        /** The settings object used by the general aside form settings. */
        const generalAsideSettings = ref<GeneralAsideSettings>({
            campusSetFrom: props.modelValue.campusSetFrom,
            hasPersonEntry: props.modelValue.allowPersonEntry
        });

        /** The settings object used by the section aside. */
        const sectionAsideSettings = ref<SectionAsideSettings | null>(null);

        /** The settings object used by the person entry aside. */
        const personEntryAsideSettings = ref<FormPersonEntry>(props.modelValue.personEntry ?? {});

        // Generate all the drag options.
        const sectionDragSourceOptions = getSectionDragSourceOptions(sections);
        const sectionReorderDragSourceOptions = getSectionReorderDragSourceOptions(sections);
        const fieldDragSourceOptions = getFieldDragSourceOptions(sections, availableFieldTypes);
        const fieldReorderDragSourceOptions = getFieldReorderDragSourceOptions(sections);

        /** The body element that will be used for drag and drop operations. */
        const bodyElement = ref<HTMLElement | null>(null);

        /** The component instance that is displaying the general form settings. */
        const generalAsideComponentInstance = ref<IAsideProvider | null>(null);

        /** The component instance that is displaying the person entry editor. */
        const personEntryAsideComponentInstance = ref<IAsideProvider | null>(null);

        /** The component instance that is displaying the section editor. */
        const sectionEditAsideComponentInstance = ref<IAsideProvider | null>(null);

        /** The component instance that is displaying the field editor. */
        const fieldEditAsideComponentInstance = ref<IAsideProvider | null>(null);

        /** The component instance that is displaying the person entry editor. */
        const personEntryEditAsideComponentInstance = ref<IAsideProvider | null>(null);

        /** The identifier of the zone currently being edited. */
        const activeZone = ref("");

        /** The form field that is currently being edited in the aside. */
        const editField = ref<FormField | null>(null);

        // #endregion

        // #region Computed Values

        /** The current aside being displayed. */
        const activeAside = computed((): IAsideProvider | null => {
            if (showGeneralAside.value) {
                return generalAsideComponentInstance.value;
            }
            else if (personEntryAsideComponentInstance.value) {
                return personEntryAsideComponentInstance.value;
            }
            else if (sectionEditAsideComponentInstance.value) {
                return sectionEditAsideComponentInstance.value;
            }
            else if (fieldEditAsideComponentInstance.value) {
                return fieldEditAsideComponentInstance.value;
            }
            else if (personEntryEditAsideComponentInstance.value) {
                return personEntryEditAsideComponentInstance.value;
            }
            else {
                return null;
            }
        });

        /** True if the general aside should be currently displayed. */
        const showGeneralAside = computed((): boolean => {
            return !showFieldAside.value && !showSectionAside.value && !showPersonEntryAside.value;
        });

        /** True if the field editor aside should be currently displayed. */
        const showFieldAside = computed((): boolean => {
            return editField.value !== null;
        });

        /** True if the section editor aside should be currently displayed. */
        const showSectionAside = computed((): boolean => {
            return sectionAsideSettings.value !== null;
        });

        /** True if the person entry editor aside should be currently displayed. */
        const showPersonEntryAside = computed((): boolean => activeZone.value === personEntryZoneGuid);

        /** True if the form has a person entry section. */
        const hasPersonEntry = computed((): boolean => {
            if (props.templateOverrides?.isPersonEntryConfigured ?? false) {
                return true;
            }

            return generalAsideSettings.value.hasPersonEntry ?? false;
        });

        /** True if the form header model should be open. */
        const isFormHeaderActive = computed({
            get: (): boolean => {
                return activeZone.value === formHeaderZoneGuid;
            },
            set(value: boolean) {
                if (!value && activeZone.value === formHeaderZoneGuid) {
                    closeAside();
                }
            }
        });

        /** True if the form header model should be open. */
        const isFormFooterActive = computed({
            get: (): boolean => {
                return activeZone.value === formFooterZoneGuid;
            },
            set(value: boolean) {
                if (!value && activeZone.value === formFooterZoneGuid) {
                    closeAside();
                }
            }
        });

        /** True if the person entry zone is currently active. */
        const isPersonEntryActive = computed((): boolean => activeZone.value === personEntryZoneGuid);

        /** True if the person entry setting has been force enabled by the template. */
        const isPersonEntryForced = computed((): boolean => props.templateOverrides?.isPersonEntryConfigured ?? false);

        /** The configure icon to use for the person entry zone. */
        const personEntryZoneIconClass = computed((): string => {
            // If we are forced then don't allow the user to configure the person entry.
            if (isPersonEntryForced.value) {
                return "";
            }

            return "fa fa-gear";
        });

        /** The form header content specified in the template. */
        const templateFormHeaderContent = computed((): string => props.templateOverrides?.formHeader ?? "");

        /** The form footer content specified in the template. */
        const templateFormFooterContent = computed((): string => props.templateOverrides?.formFooter ?? "");

        /**
         * Contains all the existing attribute keys in an array of ListItems.
         * Each item has a value that is the attribute guid and text that is
         * the key string.
         */
        const existingKeys = computed((): ListItem[] => {
            const existingKeys: ListItem[] = [];

            // Find all existing attribute keys.
            for (const sect of sections) {
                if (sect.fields) {
                    for (const field of sect.fields) {
                        existingKeys.push({
                            value: field.guid,
                            text: field.key
                        });
                    }
                }
            }

            return existingKeys;
        });

        // #endregion

        // #region Functions

        /**
         * Checks if we can safely close the current aside panel.
         *
         * @returns true if the aside can be closed, otherwise false.
         */
        const canCloseAside = (): boolean => {
            if (activeAside.value) {
                return activeAside.value.isSafeToClose();
            }
            else {
                return true;
            }
        };

        /**
         * Closes any currently open aside and returns control to the general
         * form settings aside.
         */
        const closeAside = (): void => {
            editField.value = null;
            activeZone.value = "";
        };

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the form header section wants to configure itself.
         */
        const onConfigureFormHeader = (): void => {
            if (!canCloseAside()) {
                return;
            }

            closeAside();

            formHeaderEditContent.value = formHeaderContent.value;
            activeZone.value = formHeaderZoneGuid;
        };

        /**
         * Event handler for when the form footer section wants to configure itself.
         */
        const onConfigureFormFooter = (): void => {
            if (!canCloseAside()) {
                return;
            }

            closeAside();

            formFooterEditContent.value = formFooterContent.value;
            activeZone.value = formFooterZoneGuid;
        };

        /**
         * Event handler for when the person entry section wants to configure itself.
         */
        const onConfigurePersonEntry = (): void => {
            if (!canCloseAside()) {
                return;
            }

            closeAside();

            activeZone.value = personEntryZoneGuid;
        };

        /**
         * Event handler for when any field section wants to configure itself.
         * 
         * @param section The section that is requesting to start configuration.
         */
        const onConfigureSection = (section: FormSection): void => {
            if (!canCloseAside()) {
                return;
            }

            closeAside();

            activeZone.value = section.guid;
            sectionAsideSettings.value = {
                guid: section.guid,
                title: section.title ?? "",
                description: section.description ?? "",
                showHeadingSeparator: section.showHeadingSeparator ?? false,
                type: section.type ?? null
            };
        };

        /**
         * Event handler for when any field wants to configure itself.
         * 
         * @param field The field that is requesting to start configuration.
         */
        const onConfigureField = (field: FormField): void => {
            if (!canCloseAside()) {
                return;
            }

            closeAside();

            for (const section of sections) {
                for (const existingField of (section.fields ?? [])) {
                    if (areEqual(existingField.guid, field.guid)) {
                        activeZone.value = existingField.guid;
                        editField.value = existingField;

                        return;
                    }
                }
            }
        };

        /**
         * Event handler for when any aside wants to close itself.
         */
        const onAsideClose = (): void => {
            if (!canCloseAside()) {
                return;
            }

            activeZone.value = "";
            editField.value = null;
            sectionAsideSettings.value = null;
        };

        /**
         * Event handler for when the edit field aside has updated the field
         * values or configuration.
         * 
         * @param value The new form field details.
         */
        const onEditFieldUpdate = (value: FormField): void => {
            editField.value = value;

            // Find the original field that was just updated and replace it with
            // the new value.
            for (const section of sections) {
                if (section.fields) {
                    const existingFieldIndex = section.fields.findIndex(f => areEqual(f.guid, value.guid));

                    if (existingFieldIndex !== -1) {
                        section.fields.splice(existingFieldIndex, 1, value);
                        return;
                    }
                }
            }
        };

        /**
         * Event handler for when the edit field aside wants to delete the field
         * from the section.
         */
        const onFieldDelete = async (guid: Guid): Promise<void> => {
            if (!(await confirmDelete("field"))) {
                return;
            }

            // Find the original field and delete it.
            for (const section of sections) {
                if (section.fields) {
                    const existingFieldIndex = section.fields.findIndex(f => areEqual(f.guid, guid));

                    if (existingFieldIndex !== -1) {
                        section.fields.splice(existingFieldIndex, 1);
                        break;
                    }
                }
            }

            if (areEqual(guid, editField.value?.guid ?? null)) {
                closeAside();
            }
        };

        /**
         * Event handler for when a section's settings have been updated in the
         * aside.
         * 
         * @param value The new section settings.
         */
        const onEditSectionUpdate = (value: SectionAsideSettings): void => {
            sectionAsideSettings.value = value;

            // Find the original section that was just updated and update its
            // values.
            for (const section of sections) {
                if (areEqual(section.guid, value.guid)) {
                    section.title = value.title;
                    section.description = value.description;
                    section.showHeadingSeparator = value.showHeadingSeparator;
                    section.type = value.type;

                    return;
                }
            }
        };

        /**
         * Event handler for when the edit section aside wants to delete the section.
         */
        const onSectionDelete = async (guid: Guid): Promise<void> => {
            if (!(await confirmDelete("section"))) {
                return;
            }

            // Find the original section and delete it.
            const existingSectionIndex = sections.findIndex(s => areEqual(s.guid, guid));

            if (existingSectionIndex !== -1) {
                sections.splice(existingSectionIndex, 1);
            }

            if (areEqual(guid, sectionAsideSettings.value?.guid ?? null)) {
                closeAside();
            }
        };

        /**
         * Event handler for when the person entry settings have been updated
         * in the aside.
         * 
         * @param value The new person entry settings.
         */
        const onEditPersonEntryUpdate = (value: FormPersonEntry): void => {
            personEntryAsideSettings.value = value;
        };

        /**
         * Event handler for when the form header content is saved.
         */
        const onFormHeaderSave = (): void => {
            formHeaderContent.value = formHeaderEditContent.value;

            closeAside();
        };

        /**
         * Event handler for when the form footer content is saved.
         */
        const onFormFooterSave = (): void => {
            formFooterContent.value = formFooterEditContent.value;

            closeAside();
        };

        // #endregion

        // Wait for the body element to load and then update the drag options.
        watch(bodyElement, () => {
            sectionDragSourceOptions.mirrorContainer = bodyElement.value ?? undefined;
            sectionReorderDragSourceOptions.mirrorContainer = bodyElement.value ?? undefined;
            fieldDragSourceOptions.mirrorContainer = bodyElement.value ?? undefined;
            fieldReorderDragSourceOptions.mirrorContainer = bodyElement.value ?? undefined;
        });

        // Watch for changes in the template override person entry setting. If
        // it changes then we close the aside if the person entry aside is up.
        watch(() => props.templateOverrides, (newValue, oldValue) => {
            if ((newValue?.isPersonEntryConfigured ?? false) !== (oldValue?.isPersonEntryConfigured ?? false)) {
                if (isPersonEntryActive.value) {
                    closeAside();
                }
            }
        });

        // Watch for changes to our settings and emite the new modelValue.
        watch([sections, formHeaderContent, formFooterContent, generalAsideSettings, personEntryAsideSettings], () => {
            const newValue: FormBuilderSettings = {
                allowPersonEntry: generalAsideSettings.value.hasPersonEntry,
                campusSetFrom: generalAsideSettings.value.campusSetFrom,
                footerContent: formFooterContent.value,
                headerContent: formHeaderContent.value,
                personEntry: personEntryAsideSettings.value,
                sections: sections
            };

            emit("update:modelValue", newValue);
        });

        return {
            activeZone,
            availableFieldTypes,
            bodyElement,
            editField,
            existingKeys,
            fieldDragSourceOptions,
            fieldDragTargetId: fieldDragSourceOptions.id,
            fieldEditAsideComponentInstance,
            fieldReorderDragSourceOptions,
            formFooterContent,
            formFooterEditContent,
            formHeaderContent,
            formHeaderEditContent,
            generalAsideComponentInstance,
            generalAsideSettings,
            hasPersonEntry,
            isFormFooterActive,
            isFormHeaderActive,
            isPersonEntryActive,
            isPersonEntryForced,
            onAsideClose,
            onConfigureField,
            onConfigureFormHeader,
            onConfigureFormFooter,
            onConfigurePersonEntry,
            onConfigureSection,
            onEditFieldUpdate,
            onEditPersonEntryUpdate,
            onEditSectionUpdate,
            onFieldDelete,
            onFormFooterSave,
            onFormHeaderSave,
            onSectionDelete,
            personEntryAsideSettings,
            personEntryEditAsideComponentInstance,
            personEntryZoneIconClass,
            sectionAsideSettings,
            sectionDragSourceOptions,
            sectionDragTargetId: sectionDragSourceOptions.id,
            sectionReorderDragSourceOptions,
            sectionTypeOptions,
            sections,
            showFieldAside,
            showGeneralAside,
            showPersonEntryAside,
            showSectionAside,
            templateFormFooterContent,
            templateFormHeaderContent
        };
    },

    template: `
<div ref="bodyElement" class="d-flex" style="flex-grow: 1; overflow-y: hidden;">
    <div class="d-flex flex-column" style="background-color: #f8f9fa; min-width: 320px; max-width: 480px; flex: 1 0; overflow-y: hidden;">
        <GeneralAside v-if="showGeneralAside"
            v-model="generalAsideSettings"
            ref="generalAsideComponentInstance"
            :isPersonEntryForced="isPersonEntryForced"
            :fieldTypes="availableFieldTypes"
            :sectionDragOptions="sectionDragSourceOptions"
            :fieldDragOptions="fieldDragSourceOptions" />

        <FieldEditAside v-else-if="showFieldAside"
            :modelValue="editField"
            ref="fieldEditAsideComponentInstance"
            :fieldTypes="availableFieldTypes"
            :existingKeys="existingKeys"
            @update:modelValue="onEditFieldUpdate"
            @close="onAsideClose" />

        <SectionEditAside v-else-if="showSectionAside"
            :modelValue="sectionAsideSettings"
            ref="sectionEditAsideComponentInstance"
            @update:modelValue="onEditSectionUpdate"
            @close="onAsideClose" />

        <PersonEntryEditAside v-else-if="showPersonEntryAside"
            :modelValue="personEntryAsideSettings"
            ref="personEntryEditAsideComponentInstance"
            @update:modelValue="onEditPersonEntryUpdate"
            @close="onAsideClose" />
    </div>

    <div class="p-3 d-flex flex-column" style="flex: 3 1; overflow-y: auto;">
        <FormContentZone v-if="templateFormHeaderContent" :modelValue="templateFormHeaderContent" placeholder="" iconCssClass="" />

        <FormContentZone :modelValue="formHeaderContent" :isActive="isFormHeaderActive" @configure="onConfigureFormHeader" placeholder="Form Header" />

        <ConfigurableZone v-if="hasPersonEntry" :modelValue="isPersonEntryActive" :iconCssClass="personEntryZoneIconClass" @configure="onConfigurePersonEntry">
            <div class="zone-body">
                <div class="text-center text-muted">Person Entry Form</div>
            </div>
        </ConfigurableZone>

        <div style="flex-grow: 1; display: flex; flex-direction: column;" v-drag-target="sectionDragTargetId" v-drag-source="sectionReorderDragSourceOptions" v-drag-target:2="sectionReorderDragSourceOptions.id">
            <SectionZone v-for="section in sections"
                :key="section.guid"
                v-model="section"
                :activeZone="activeZone"
                :dragTargetId="fieldDragTargetId"
                :reorderDragOptions="fieldReorderDragSourceOptions"
                :sectionTypeOptions="sectionTypeOptions"
                @configure="onConfigureSection(section)"
                @configureField="onConfigureField"
                @delete="onSectionDelete"
                @deleteField="onFieldDelete">
            </SectionZone>
        </div>

        <FormContentZone :modelValue="formFooterContent" :isActive="isFormFooterActive" @configure="onConfigureFormFooter" placeholder="Form Footer" />

        <FormContentZone v-if="templateFormFooterContent" :modelValue="templateFormFooterContent" placeholder="" iconCssClass="" />
    </div>
</div>

<FormContentModal v-model="formHeaderEditContent" v-model:isVisible="isFormHeaderActive" title="Form Header" @save="onFormHeaderSave" />

<FormContentModal v-model="formFooterEditContent" v-model:isVisible="isFormFooterActive" title="Form Footer" @save="onFormFooterSave" />
`
});
