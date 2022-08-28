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

import { defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import TextBox from "@Obsidian/Controls/textBox";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker";
import ColorPicker from "@Obsidian/Controls/colorPicker";
import CheckBox from "@Obsidian/Controls/checkBox";
import CodeEditor from "@Obsidian/Controls/codeEditor";
import NumberBox from "@Obsidian/Controls/numberBox";
import BinaryFileTypePicker from "@Obsidian/Controls/binaryFileTypePicker";
import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { NoteTypeBag } from "@Obsidian/ViewModels/Blocks/Core/NoteTypeDetail/noteTypeBag";
import { NoteTypeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/NoteTypeDetail/noteTypeDetailOptionsBag";

export default defineComponent({
    name: "Core.NoteTypeDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<NoteTypeBag>,
            required: true
        },

        options: {
            type: Object as PropType<NoteTypeDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        TextBox,
        EntityTypePicker,
        ColorPicker,
        CheckBox,
        CodeEditor,
        NumberBox,
        BinaryFileTypePicker,
        TransitionVerticalCollapse
    },

    emits: {
        "update:modelValue": (_value: NoteTypeBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const isSystem = propertyRef(props.modelValue.isSystem, "IsSystem");
        const entityType = propertyRef(props.modelValue.entityType ?? {}, "EntityTypeId");
        const iconCssClass = propertyRef(props.modelValue.iconCssClass ?? "", "IconCssClass");
        const backgroundColor = propertyRef(props.modelValue.backgroundColor ?? "", "BackgroundColor");
        const fontColor = propertyRef(props.modelValue.fontColor ?? "", "FontColor");
        const borderColor = propertyRef(props.modelValue.borderColor ?? "", "BorderColor");
        const userSelectable = propertyRef(props.modelValue.userSelectable, "UserSelectable");
        const requiresApprovals = propertyRef(props.modelValue.requiresApprovals, "RequiresApprovals");
        const sendApprovalNotifications = propertyRef(props.modelValue.sendApprovalNotifications, "SendApprovalNotifications");
        const allowsWatching = propertyRef(props.modelValue.allowsWatching, "AllowsWatching");
        const autoWatchAuthors = propertyRef(props.modelValue.autoWatchAuthors, "AutoWatchAuthors");
        const allowsReplies = propertyRef(props.modelValue.allowsReplies, "AllowsReplies");
        const maxReplyDepth = propertyRef(props.modelValue.maxReplyDepth, "MaxReplyDepth");
        const allowsAttachments = propertyRef(props.modelValue.allowsAttachments, "AllowsAttachments");
        const binaryFileType = propertyRef(props.modelValue.binaryFileType ?? {}, "BinaryFileTypeId");
        const approvalUrlTemplate = propertyRef(props.modelValue.approvalUrlTemplate ?? "", "ApprovalUrlTemplate");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [name, entityType, iconCssClass, backgroundColor, fontColor, borderColor, userSelectable, requiresApprovals,
            sendApprovalNotifications, allowsWatching, autoWatchAuthors, allowsReplies, maxReplyDepth, allowsAttachments, binaryFileType, approvalUrlTemplate];

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(entityType, props.modelValue.entityType ?? {});
            updateRefValue(iconCssClass, props.modelValue.iconCssClass ?? "");
            updateRefValue(backgroundColor, props.modelValue.backgroundColor ?? "");
            updateRefValue(fontColor, props.modelValue.fontColor ?? "");
            updateRefValue(borderColor, props.modelValue.borderColor ?? "");
            updateRefValue(userSelectable, props.modelValue.userSelectable);
            updateRefValue(requiresApprovals, props.modelValue.requiresApprovals);
            updateRefValue(sendApprovalNotifications, props.modelValue.sendApprovalNotifications);
            updateRefValue(allowsWatching, props.modelValue.allowsWatching);
            updateRefValue(maxReplyDepth, props.modelValue.maxReplyDepth);
            updateRefValue(allowsAttachments, props.modelValue.allowsAttachments);
            updateRefValue(binaryFileType, props.modelValue.binaryFileType ?? {});
            updateRefValue(approvalUrlTemplate, props.modelValue.approvalUrlTemplate ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: NoteTypeBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                name: name.value,
                entityType: entityType.value,
                iconCssClass: iconCssClass.value,
                backgroundColor: backgroundColor.value,
                fontColor: fontColor.value,
                borderColor: borderColor.value,
                userSelectable: userSelectable.value,
                requiresApprovals: requiresApprovals.value,
                sendApprovalNotifications: sendApprovalNotifications.value,
                allowsWatching: allowsWatching.value,
                autoWatchAuthors: autoWatchAuthors.value,
                allowsReplies: allowsReplies.value,
                maxReplyDepth: maxReplyDepth.value,
                allowsAttachments: allowsAttachments.value,
                binaryFileType: binaryFileType.value,
                approvalUrlTemplate: approvalUrlTemplate.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            name,
            isSystem,
            entityType,
            iconCssClass,
            backgroundColor,
            fontColor,
            borderColor,
            userSelectable,
            requiresApprovals,
            sendApprovalNotifications,
            allowsWatching,
            autoWatchAuthors,
            allowsReplies,
            maxReplyDepth,
            allowsAttachments,
            binaryFileType,
            approvalUrlTemplate
        };
    },

    template: `
<fieldset>
    <div class="row">

        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />

            <EntityTypePicker v-model="entityType"
                label="Entity Type"
                rules="required"
                :multiple="false"
                :includeGlobalOption="false" />

            <TextBox v-model="iconCssClass"
                label="Icon CSS Class" />

            <ColorPicker v-model="backgroundColor"
                label="Background Color" />

            <ColorPicker v-model="fontColor"
                label="Font Color" />

            <ColorPicker v-model="borderColor"
                label="Border Color" />
            
        </div>

        <div class="col-md-6">

            <CheckBox v-model="userSelectable"
                label="User Selectable"
                text="Yes" />

            <CheckBox v-model="requiresApprovals"
                label="Requires Approvals"
                text="Yes" />

            <CheckBox v-model="sendApprovalNotifications"
                label="Send Approval Notifications"
                text="Yes" />

            <CheckBox v-model="allowsWatching"
                label="Allows Watching"
                text="Yes"
                help="If enabled, an option to watch individual notes will appear, and note watch notifications will be sent on watched notes." />

            <CheckBox v-model="autoWatchAuthors"
                label="Auto Watch Authors"
                text="Yes"
                help="If enabled, the author of a note will get notifications for direct replies to the note. In other words, a 'watch' will be automatically enabled on the note." />

            <CheckBox v-model="allowsReplies"
                label="Allow Replies"
                text="Yes" />

            <TransitionVerticalCollapse>
                <div v-if="allowsReplies">
                    <NumberBox label="Max Reply Depth"
                        class="input-width-sm"
                        v-model="maxReplyDepth"/>
                </div>
            </TransitionVerticalCollapse>

            <CheckBox v-model="allowsAttachments"
                label="Allows Attachments"
                text="Yes"
                help="If enabled, then this note type will allow attachments. However, not all UI components will currently allow file uploads." />

            <TransitionVerticalCollapse>
                <div v-if="allowsAttachments">
                    <BinaryFileTypePicker v-model="binaryFileType"
                        label="Attachment File Type"
                        help="When a file is attached to a note, it will be stored using this file type."
                        :showBlankItem="showBlankItem"
                        rules="required" />
                </div>
            </TransitionVerticalCollapse>

            <CodeEditor v-model="approvalUrlTemplate"
                label="Approval URL Template"
                help="An optional lava template that can be used to generate a URL where notes of this type can be approved. If this is left blank, the approval URL will be a URL to the page (including a hash anchor to the note) where the note was originally created."
                theme="rock"
                mode="text"
                :editorHeight="200" />

        </div>

    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
