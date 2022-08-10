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
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import TextBox from "@Obsidian/Controls/textBox";
import Switch from "@Obsidian/Controls/switch";
import DropDownList from "@Obsidian/Controls/dropDownList";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse";
import WorkflowTypePicker from "@Obsidian/Controls/workflowTypePicker";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { MediaFolderBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaFolderDetail/mediaFolderBag";
import { MediaFolderDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/MediaFolderDetail/mediaFolderDetailOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export default defineComponent({
    name: "Cms.MediaFolderDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<MediaFolderBag>,
            required: true
        },

        options: {
            type: Object as PropType<MediaFolderDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        TextBox,
        Switch,
        TransitionVerticalCollapse,
        DropDownList,
        RadioButtonList,
        WorkflowTypePicker
    },

    emits: {
        "update:modelValue": (_value: MediaFolderBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const description = propertyRef(props.modelValue.description ?? "", "Description");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const isContentChannelSyncEnabled = propertyRef(props.modelValue.isContentChannelSyncEnabled ?? false, "IsContentChannelSyncEnabled");
        const contentChannelValue = propertyRef(props.modelValue.contentChannel?.value ?? "", "ContentChannelId");
        const contentChannelOptions = ref<ListItemBag[]>(props.options.contentChannels ?? []);
        const contentChannelAttributes = ref<Record<string, ListItemBag[]>>(props.options.contentChannelAttributes ?? {});
        const contentChannelAttributeValue = propertyRef(props.modelValue.contentChannelAttribute?.value ?? "", "ContentChannelAttributeId");
        const contentChannelItemStatus = propertyRef(props.modelValue.contentChannelItemStatus ?? "", "ContentChannelItemStatus");
        const workflowType = propertyRef(props.modelValue.workflowType ?? {}, "WorkflowTypeId");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [description, name, isContentChannelSyncEnabled, contentChannelValue, contentChannelAttributeValue, contentChannelItemStatus, workflowType];

        // #endregion

        // #region Computed Values
        const contentChannelItemAttributes = computed((): ListItemBag[] => contentChannelAttributes.value[contentChannelValue.value]);
        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(isContentChannelSyncEnabled, props.modelValue.isContentChannelSyncEnabled ?? false);
            updateRefValue(contentChannelValue, props.modelValue.contentChannel?.value ?? "");
            updateRefValue(contentChannelAttributeValue, props.modelValue.contentChannelAttribute?.value ?? "");
            updateRefValue(contentChannelItemStatus, props.modelValue.contentChannelItemStatus ?? "");
            updateRefValue(workflowType, props.modelValue.workflowType ?? {});
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: MediaFolderBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                description: description.value,
                name: name.value,
                isContentChannelSyncEnabled: isContentChannelSyncEnabled.value,
                contentChannel: { value: contentChannelValue.value },
                contentChannelItemStatus: contentChannelItemStatus.value,
                contentChannelAttribute: { value: contentChannelAttributeValue.value },
                workflowType: workflowType.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            description,
            name,
            isContentChannelSyncEnabled,
            channelStatuses: [
                { text: "Pending Approval", value: "Pending Approval" },
                { text: "Approved", value: "Approved" },
                { text: "Denied", value: "Denied" }
            ] as ListItemBag[],
            contentChannelOptions,
            contentChannelItemAttributes,
            contentChannelValue,
            contentChannelAttributeValue,
            contentChannelItemStatus,
            workflowType
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>
    </div>
    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />
    <WorkflowTypePicker v-model="workflowType"
        label="WorkFlow" />
    <div class="mt-3">
        <div class="mb-3 galleryContent-reflectionToggle">
            <Switch v-model="isContentChannelSyncEnabled" text="Enable Content Channel Sync" />
        </div>
        <TransitionVerticalCollapse>
            <div v-if="isContentChannelSyncEnabled">
                <div class="row">
                    <div class="col-md-6">
                        <DropDownList v-model="contentChannelValue"
                            label="Content Channel"
                            :items="contentChannelOptions"
                            rules="required" />
                        <DropDownList v-model="contentChannelAttributeValue"
                                      label="Media File Attribute"                                     
                                      :items="contentChannelItemAttributes"
                                      rules="required" />
                    </div>
                    <div class="col-md-6">
                        <RadioButtonList v-model="contentChannelItemStatus"
                                         label="Content Channel Item Status"
                                         :items="channelStatuses"
                                         horizontal
                                         rules="required"/>
                    </div>
                </div>
            </div>
        </TransitionVerticalCollapse>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
