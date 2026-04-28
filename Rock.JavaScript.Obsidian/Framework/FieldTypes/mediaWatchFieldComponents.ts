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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import MediaPlayer from "@Obsidian/Controls/mediaPlayer.obs";
import MediaElementPicker from "@Obsidian/Controls/mediaElementPicker.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ConfigurationKey } from "./mediaWatchField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { emptyGuid } from "@Obsidian/Utility/guid";

export const EditComponent = defineComponent({
    name: "MediaWatchField.Edit",

    components: {
        MediaPlayer
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<number>(0);

        const configuration = computed(() => {
            let mediaElement: ListItemBag = {};

            try {
                mediaElement = JSON.parse(props.configurationValues[ConfigurationKey.MediaElement] ?? "{}") as ListItemBag;
            }
            catch (e) {
                // Do Nothing
            }

            const config = {
                requiredPercentage: toNumberOrNull(props.configurationValues[ConfigurationKey.CompletionPercentage]),
                resumeInDays: toNumberOrNull(props.configurationValues[ConfigurationKey.AutoResumeInDays]),
                maxWidth: props.configurationValues[ConfigurationKey.MaxWidth],
                validationMessage: props.configurationValues[ConfigurationKey.ValidationMessage],
                mediaGuid: mediaElement?.value ?? emptyGuid,
            };

            return config;
        });

        const isEditMode = computed(() => props.dataEntryMode !== "defaultValue");

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            const modelProp = toNumberOrNull(props.modelValue) ?? 0;

            internalValue.value = modelProp / 100;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, (val) => {
            emit("update:modelValue", `${val * 100}`);
        });

        return {
            internalValue,
            configuration,
            isEditMode
        };
    },

    template: `
<MediaPlayer v-if="isEditMode"
    v-model:watchedPercentage="internalValue"
    :mediaElementGuid="configuration.mediaGuid"
    :requiredWatchPercentage="configuration.requiredPercentage"
    :autoResumeInDays="configuration.resumeInDays"
    :combinePlayStatisticsInDays="configuration.resumeInDays"
    :maxVideoWidth="configuration.maxWidth"
    :requiredErrorMessage="configuration.validationMessage" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "MediaWatchField.Configuration",

    components: {
        MediaElementPicker,
        NumberBox,
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const completionPercentage = ref<number>();
        const autoResumeInDays = ref<number>();
        const maxWidth = ref<string>("");
        const validationMessage = ref<string>("");
        const mediaAccount = ref<ListItemBag>();
        const mediaFolder = ref<ListItemBag>();
        const mediaElement = ref<ListItemBag>();

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different than the current value.
            newValue[ConfigurationKey.CompletionPercentage] = completionPercentage.value?.toString() ?? "";
            newValue[ConfigurationKey.AutoResumeInDays] = autoResumeInDays.value?.toString() ?? "";
            newValue[ConfigurationKey.MaxWidth] = maxWidth.value;
            newValue[ConfigurationKey.ValidationMessage] = validationMessage.value;
            newValue[ConfigurationKey.MediaAccount] = JSON.stringify(mediaAccount.value);
            newValue[ConfigurationKey.MediaFolder] = JSON.stringify(mediaFolder.value);
            newValue[ConfigurationKey.MediaElement] = JSON.stringify(mediaElement.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationKey.CompletionPercentage] !== (props.modelValue[ConfigurationKey.CompletionPercentage])
                || newValue[ConfigurationKey.AutoResumeInDays] !== (props.modelValue[ConfigurationKey.AutoResumeInDays])
                || newValue[ConfigurationKey.MaxWidth] !== (props.modelValue[ConfigurationKey.MaxWidth])
                || newValue[ConfigurationKey.ValidationMessage] !== (props.modelValue[ConfigurationKey.ValidationMessage])
                || newValue[ConfigurationKey.MediaAccount] !== (props.modelValue[ConfigurationKey.MediaAccount])
                || newValue[ConfigurationKey.MediaFolder] !== (props.modelValue[ConfigurationKey.MediaFolder])
                || newValue[ConfigurationKey.MediaElement] !== (props.modelValue[ConfigurationKey.MediaElement]);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            completionPercentage.value = toNumberOrNull(props.modelValue[ConfigurationKey.CompletionPercentage]) ?? undefined;
            autoResumeInDays.value = toNumberOrNull(props.modelValue[ConfigurationKey.AutoResumeInDays]) ?? undefined;
            maxWidth.value = props.modelValue[ConfigurationKey.MaxWidth];
            validationMessage.value = props.modelValue[ConfigurationKey.ValidationMessage];

            try {
                mediaAccount.value = JSON.parse(props.modelValue[ConfigurationKey.MediaAccount]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }

            try {
                mediaFolder.value = JSON.parse(props.modelValue[ConfigurationKey.MediaFolder]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }

            try {
                mediaElement.value = JSON.parse(props.modelValue[ConfigurationKey.MediaElement]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(completionPercentage, () => maybeUpdateConfiguration(ConfigurationKey.CompletionPercentage, completionPercentage.value?.toString() ?? ""));
        watch(autoResumeInDays, () => maybeUpdateConfiguration(ConfigurationKey.AutoResumeInDays, autoResumeInDays.value?.toString() ?? ""));
        watch(maxWidth, () => maybeUpdateConfiguration(ConfigurationKey.MaxWidth, maxWidth.value));
        watch(validationMessage, () => maybeUpdateConfiguration(ConfigurationKey.ValidationMessage, validationMessage.value));
        watch(mediaAccount, () => maybeUpdateConfiguration(ConfigurationKey.MediaAccount, JSON.stringify(mediaAccount.value) ?? ""));
        watch(mediaFolder, () => maybeUpdateConfiguration(ConfigurationKey.MediaFolder, JSON.stringify(mediaFolder.value) ?? ""));
        watch(mediaElement, () => maybeUpdateConfiguration(ConfigurationKey.MediaElement, JSON.stringify(mediaElement.value) ?? ""));

        return {
            completionPercentage,
            autoResumeInDays,
            maxWidth,
            validationMessage,
            mediaAccount,
            mediaFolder,
            mediaElement
        };
    },

    template: `
<MediaElementPicker v-model="mediaElement"
    v-model:account="mediaAccount"
    v-model:folder="mediaFolder"
    label="Media"
    help="The media file that will be watched by the individual."
    rules="required" />

<NumberBox v-model="completionPercentage"
    label="Completion Percentage"
    help="The percentage of the video that the individual must view in order for the video to be considered watched. Instead of setting this to 100% you probably want a few points below that."
    rules="required"
    :minimumValue="0"
    :maximumValue="100"
    inputGroupClasses="input-width-sm">
    <template #inputGroupAppend>
        <span class="input-group-addon">%</span>
    </template>
</NumberBox>

<NumberBox v-model="autoResumeInDays"
    label="Auto Resume In Days"
    help="The video player will look back this many days for a previous watch session and attempt to auto-resume from that point."
    :minimumValue="-1"
    :maximumValue="3650"
    :decimalCount="0"
    inputClasses="input-width-sm" />

<TextBox v-model="maxWidth"
    label="Maximum Video Width"
    help="The maximum width of the video. This unit can be expressed in pixels (e.g. 250px) or percent (e.g. 75%). If no unit is provided, pixels is assumed."
    inputClasses="input-width-sm" />

<TextBox v-model="validationMessage"
    label="Validation Message"
    help="The message that should be show when the individual does not watch the required amount of the video."
    textMode="multiLine"
    rows="3" />
`
});
