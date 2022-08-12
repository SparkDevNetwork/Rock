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
import CheckBox from "@Obsidian/Controls/checkBox";
import TextBox from "@Obsidian/Controls/textBox";
import CodeEditor from "@Obsidian/Controls/codeEditor";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker";
import LavaCommandPicker from "@Obsidian/Controls/lavaCommandPicker";
import NumberBox from "@Obsidian/Controls/numberBox";
import DatePicker from "@Obsidian/Controls/datePicker";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { PersistedDatasetBag } from "@Obsidian/ViewModels/Blocks/Cms/PersistedDatasetDetail/persistedDatasetBag";
import { PersistedDatasetDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/PersistedDatasetDetail/persistedDatasetDetailOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export default defineComponent({
    name: "Cms.PersistedDatasetDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<PersistedDatasetBag>,
            required: true
        },

        options: {
            type: Object as PropType<PersistedDatasetDetailOptionsBag>,
            required: true
        }
    },

    components: {
        CheckBox,
        TextBox,
        CodeEditor,
        EntityTypePicker,
        LavaCommandPicker,
        NumberBox,
        DatePicker
    },

    emits: {
        "update:modelValue": (_value: PersistedDatasetBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const description = propertyRef(props.modelValue.description ?? "", "Description");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const accessKey = propertyRef(props.modelValue.accessKey ?? "", "AccessKey");
        const buildScript = propertyRef(props.modelValue.buildScript ?? "", "BuildScript");
        const entityType = propertyRef(props.modelValue.entityType ?? {}, "EntityTypeId");
        const enabledLavaCommands = ref<ListItemBag[]>(props.modelValue.enabledLavaCommands ?? []);
        const allowManualRefresh = propertyRef(props.modelValue.allowManualRefresh ?? false, "AllowManualRefresh");
        const refreshInterval = propertyRef(props.modelValue.refreshIntervalHours, "RefreshIntervalMinutes");
        const memoryCacheDuration = propertyRef(props.modelValue.memoryCacheDurationHours, "MemoryCacheDurationMS");
        const expiresOn = propertyRef(props.modelValue.expireDateTime ?? "", "ExpireDateTime");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [description, isActive, name, accessKey, buildScript, entityType, allowManualRefresh, refreshInterval, memoryCacheDuration, expiresOn];

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(accessKey, props.modelValue.accessKey ?? "");
            updateRefValue(buildScript, props.modelValue.buildScript ?? "");
            updateRefValue(entityType, props.modelValue.entityType ?? {});
            updateRefValue(enabledLavaCommands, props.modelValue.enabledLavaCommands ?? []);
            updateRefValue(allowManualRefresh, props.modelValue.allowManualRefresh ?? false);
            updateRefValue(refreshInterval, props.modelValue.refreshIntervalHours);
            updateRefValue(memoryCacheDuration, props.modelValue.memoryCacheDurationHours);
            updateRefValue(expiresOn, props.modelValue.expireDateTime ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([...propRefs], () => {
            const newValue: PersistedDatasetBag = {
                ...props.modelValue,
                description: description.value,
                isActive: isActive.value,
                name: name.value,
                accessKey: accessKey.value,
                buildScript: buildScript.value,
                entityType: entityType.value,
                enabledLavaCommands: enabledLavaCommands.value,
                allowManualRefresh: allowManualRefresh.value,
                refreshIntervalHours: refreshInterval.value,
                memoryCacheDurationHours: memoryCacheDuration.value,
                expireDateTime: expiresOn.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            description,
            isActive,
            name,
            accessKey,
            buildScript,
            entityType,
            enabledLavaCommands,
            allowManualRefresh,
            refreshInterval,
            memoryCacheDuration,
            expiresOn
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />

            <TextBox v-model="accessKey"
                label="Access Key"
                rules="required"
                help="The key to use to uniquely identify this dataset. This will be the key to use when using the PersistedDataset lava filter." />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                help="Set this to false to have the PersistedDataset lava filter return null for this dataset, and to exclude this dataset when rebuilding."
                label="Active" />
        </div>
    </div>

    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />

    <CodeEditor v-model="buildScript"
        label="Build Script"
        help="Lava Template to use for building JSON that will be used as the cached dataset object."
        theme="rock"
        mode="text"
        :editorHeight="200" />

    <LavaCommandPicker v-model="enabledLavaCommands"
        :multiple="true"
        label="Enabled Lava Commands" />

    <div class="row">
        <div class="col-md-2">
            <NumberBox v-model="refreshInterval"
                label="Refresh Interval"
                help="How often the dataset should be updated by the Update Persisted Dataset job."
                :decimalCount="0"
                rules="required|gte:0">
                <template #append>
                    <span class="input-group-addon">Hour(s)</span>
                </template>
            </NumberBox>

            <NumberBox v-model="memoryCacheDuration"
                label="Memory Cache Duration"
                help="How long the persisted object should be cached in memory. This is a sliding timeline, so each time the object is read the counter will reset. Leave blank to not cache the object in memory which will mean it will be deserialized into the object on each request (still fast)."
                :decimalCount="0">
                <template #append>
                    <span class="input-group-addon">Hour(s)</span>
                </template>
            </NumberBox>

            <DatePicker v-model="expiresOn"
                label="Expires on"
                help="Set this to consider the dataset inactive after the specified date. This will mean that its value is no longer updated by the refresh job and that it will return empty when requested through Lava."
                :displayCurrentOption="false"
                :isCurrentDateOffset="false" />
        </div>

        <div class="col-md-4 col-md-offset-6">
            <EntityTypePicker v-model="entityType"
                label="Entity Type"
                help="Set this to indicate which EntityType the JSON object should be associated with. This will be used by the PersistedDataset Lava Filter when entity related options such as 'AppendFollowing' are specified.'"
                :multiple="false"
                :includeGlobalOption="false" />

            <CheckBox v-model="allowManualRefresh"
                help="Determines if the persisted dataset can be manually refreshed in the Persisted Dataset list."
                label="Allow Manual Refresh" />
        </div>
    </div>

</fieldset>
`
});
