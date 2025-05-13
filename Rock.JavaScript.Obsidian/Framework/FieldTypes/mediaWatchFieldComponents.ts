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
import { ConfigurationValueKey } from "./mediaWatchField.partial";
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
                mediaElement = JSON.parse(props.configurationValues[ConfigurationValueKey.MediaElement] ?? "{}") as ListItemBag;
            }
            catch (e) {
                // Do Nothing
            }

            const config = {
                requiredPercentage: toNumberOrNull(props.configurationValues[ConfigurationValueKey.CompletionPercentage]),
                resumeInDays: toNumberOrNull(props.configurationValues[ConfigurationValueKey.AutoResumeInDays]),
                maxWidth: props.configurationValues[ConfigurationValueKey.MaxWidth],
                validationMessage: props.configurationValues[ConfigurationValueKey.ValidationMessage],
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
            newValue[ConfigurationValueKey.CompletionPercentage] = completionPercentage.value?.toString() ?? "";
            newValue[ConfigurationValueKey.AutoResumeInDays] = autoResumeInDays.value?.toString() ?? "";
            newValue[ConfigurationValueKey.MaxWidth] = maxWidth.value;
            newValue[ConfigurationValueKey.ValidationMessage] = validationMessage.value;
            newValue[ConfigurationValueKey.MediaAccount] = JSON.stringify(mediaAccount.value);
            newValue[ConfigurationValueKey.MediaFolder] = JSON.stringify(mediaFolder.value);
            newValue[ConfigurationValueKey.MediaElement] = JSON.stringify(mediaElement.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.CompletionPercentage] !== (props.modelValue[ConfigurationValueKey.CompletionPercentage])
                || newValue[ConfigurationValueKey.AutoResumeInDays] !== (props.modelValue[ConfigurationValueKey.AutoResumeInDays])
                || newValue[ConfigurationValueKey.MaxWidth] !== (props.modelValue[ConfigurationValueKey.MaxWidth])
                || newValue[ConfigurationValueKey.ValidationMessage] !== (props.modelValue[ConfigurationValueKey.ValidationMessage])
                || newValue[ConfigurationValueKey.MediaAccount] !== (props.modelValue[ConfigurationValueKey.MediaAccount])
                || newValue[ConfigurationValueKey.MediaFolder] !== (props.modelValue[ConfigurationValueKey.MediaFolder])
                || newValue[ConfigurationValueKey.MediaElement] !== (props.modelValue[ConfigurationValueKey.MediaElement]);

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
            completionPercentage.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.CompletionPercentage]) ?? undefined;
            autoResumeInDays.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.AutoResumeInDays]) ?? undefined;
            maxWidth.value = props.modelValue[ConfigurationValueKey.MaxWidth];
            validationMessage.value = props.modelValue[ConfigurationValueKey.ValidationMessage];

            try {
                mediaAccount.value = JSON.parse(props.modelValue[ConfigurationValueKey.MediaAccount]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }

            try {
                mediaFolder.value = JSON.parse(props.modelValue[ConfigurationValueKey.MediaFolder]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }

            try {
                mediaElement.value = JSON.parse(props.modelValue[ConfigurationValueKey.MediaElement]) as ListItemBag;
            }
            catch (e) {
                /* Do Nothing */
            }
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(completionPercentage, () => maybeUpdateConfiguration(ConfigurationValueKey.CompletionPercentage, completionPercentage.value?.toString() ?? ""));
        watch(autoResumeInDays, () => maybeUpdateConfiguration(ConfigurationValueKey.AutoResumeInDays, autoResumeInDays.value?.toString() ?? ""));
        watch(maxWidth, () => maybeUpdateConfiguration(ConfigurationValueKey.MaxWidth, maxWidth.value));
        watch(validationMessage, () => maybeUpdateConfiguration(ConfigurationValueKey.ValidationMessage, validationMessage.value));
        watch(mediaAccount, () => maybeUpdateConfiguration(ConfigurationValueKey.MediaAccount, JSON.stringify(mediaAccount.value) ?? ""));
        watch(mediaFolder, () => maybeUpdateConfiguration(ConfigurationValueKey.MediaFolder, JSON.stringify(mediaFolder.value) ?? ""));
        watch(mediaElement, () => maybeUpdateConfiguration(ConfigurationValueKey.MediaElement, JSON.stringify(mediaElement.value) ?? ""));

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