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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import TextBox from "@Obsidian/Controls/textBox.obs";
import MediaElementPicker from "@Obsidian/Controls/mediaElementPicker.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationKey } from "./mediaElementField.partial";
import { asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

export const EditComponent = defineComponent({
    name: "MediaElementField.Edit",

    components: {
        MediaElementPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as ListItemBag);
        const folderValue = ref(JSON.parse(props.configurationValues[ConfigurationKey.LimitToFolder] || "{}") as ListItemBag);
        const accountValue = ref(JSON.parse(props.configurationValues[ConfigurationKey.LimitToAccount] || "{}") as ListItemBag);
        const hideMediaPicker = ref(!props.modelValue);

        const mediaPickerLabel = computed((): string => props.configurationValues[ConfigurationKey.MediaPickerLabel]);
        const enhanceForLongListsThreshold = computed((): number | null => toNumberOrNull(props.configurationValues[ConfigurationKey.EnhancedForLongListsThreshold]));
        const allowRefresh = computed((): boolean => asBooleanOrNull(props.configurationValues[ConfigurationKey.AllowRefresh]) ?? true);

        const hideFolderPicker = computed((): boolean => {
            const folderConfiguration = JSON.parse(props.configurationValues[ConfigurationKey.LimitToFolder] || "{}") as ListItemBag;
            folderValue.value = folderConfiguration;
            return !!folderConfiguration?.value;
        });

        const hideAccountPicker = computed((): boolean => {
            const accountConfiguration = JSON.parse(props.configurationValues[ConfigurationKey.LimitToAccount] || "{}") as ListItemBag;
            accountValue.value = accountConfiguration;
            return !!accountConfiguration?.value;
        });

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        watch(() => folderValue.value, () => {
            hideMediaPicker.value = !folderValue.value?.value;
        });

        return {
            internalValue,
            accountValue,
            folderValue,
            allowRefresh,
            hideAccountPicker,
            hideFolderPicker,
            hideMediaPicker,
            mediaPickerLabel,
            enhanceForLongListsThreshold,
        };
    },

    template: `
<MediaElementPicker label="Media Element"
    v-model="internalValue"
    v-model:account="accountValue"
    v-model:folder="folderValue"
    :hideRefreshButtons="!allowRefresh"
    :hideAccountPicker="hideAccountPicker"
    :hideFolderPicker="hideFolderPicker"
    :hideMediaPicker="hideMediaPicker"
    :mediaElementLabel="mediaPickerLabel"
    :enhanceForLongListsThreshold="enhanceForLongListsThreshold"/>
`
});


export const ConfigurationComponent = defineComponent({
    name: "MediaElementField.Configuration",

    components: {
        CheckBox,
        TextBox,
        MediaElementPicker,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const mediaPickerLabel = ref<string>("Media");
        const account = ref<ListItemBag>();
        const folder = ref<ListItemBag>();
        const enhancedForLongListsThreshold = ref<number | undefined>(20);
        const allowRefresh = ref(true);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationKey.MediaPickerLabel] = mediaPickerLabel.value ?? "Media";
            newValue[ConfigurationKey.LimitToAccount] = JSON.stringify(account.value);
            newValue[ConfigurationKey.LimitToFolder] = JSON.stringify(folder.value);
            newValue[ConfigurationKey.EnhancedForLongListsThreshold] = toNumberOrNull(enhancedForLongListsThreshold.value)?.toString() ?? "";
            newValue[ConfigurationKey.AllowRefresh] = asTrueFalseOrNull(allowRefresh.value) ?? "True";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationKey.MediaPickerLabel] !== (props.modelValue[ConfigurationKey.MediaPickerLabel] ?? "")
                || newValue[ConfigurationKey.LimitToAccount] !== (props.modelValue[ConfigurationKey.LimitToAccount] ?? "")
                || newValue[ConfigurationKey.LimitToFolder] !== (props.modelValue[ConfigurationKey.LimitToFolder] ?? "")
                || newValue[ConfigurationKey.EnhancedForLongListsThreshold] !== (props.modelValue[ConfigurationKey.EnhancedForLongListsThreshold] ?? "")
                || newValue[ConfigurationKey.AllowRefresh] !== (props.modelValue[ConfigurationKey.AllowRefresh] ?? "True");

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
            const threshold = props.modelValue[ConfigurationKey.EnhancedForLongListsThreshold];
            mediaPickerLabel.value = props.modelValue[ConfigurationKey.MediaPickerLabel] ?? "Media";
            account.value = JSON.parse(props.modelValue[ConfigurationKey.LimitToAccount] || "{}");
            folder.value = JSON.parse(props.modelValue[ConfigurationKey.LimitToFolder] || "{}");
            enhancedForLongListsThreshold.value = threshold === undefined || threshold === null ? 20 : toNumberOrNull(threshold) ?? undefined;
            allowRefresh.value = asBooleanOrNull(props.modelValue[ConfigurationKey.AllowRefresh]) ?? true;
        }, {
            immediate: true
        });

        const hideFolderPicker = computed((): boolean => {
            return !account.value?.value;
        });

        // Watch for changes in properties that only require a local UI update.
        watch(mediaPickerLabel, () => maybeUpdateConfiguration(ConfigurationKey.MediaPickerLabel, mediaPickerLabel.value ?? ""));
        watch(account, () => maybeUpdateConfiguration(ConfigurationKey.LimitToAccount, JSON.stringify(account.value)));
        watch(folder, () => maybeUpdateConfiguration(ConfigurationKey.LimitToFolder, JSON.stringify(folder.value)));
        watch(enhancedForLongListsThreshold, () => maybeUpdateConfiguration(ConfigurationKey.EnhancedForLongListsThreshold, enhancedForLongListsThreshold.value?.toString() ?? ""));
        watch(allowRefresh, () => maybeUpdateConfiguration(ConfigurationKey.AllowRefresh, asTrueFalseOrNull(allowRefresh.value) ?? "True"));

        return {
            mediaPickerLabel,
            account,
            folder,
            enhancedForLongListsThreshold,
            allowRefresh,
            hideFolderPicker
        };
    },

    template: `
<div>
    <TextBox label="Media Element Picker Label"
        v-model="mediaPickerLabel"
        help="The label for the media element picker." />

    <NumberBox label="Enhance For Long Lists Threshold"
        v-model="enhancedForLongListsThreshold"
        help="When the number of items exceed this value then the picker will turn on enhanced for long lists."
        :minimumValue="0" />

    <MediaElementPicker label="Limit To"
        v-model="value"
        help="Enforces the account or folder selections and hides them from the user."
        :isRefreshDisallowed="!allowRefresh"
        v-model:account="account"
        v-model:folder="folder"
        :hideFolderPicker="hideFolderPicker"
        :hideMediaPicker="true" />

    <CheckBox v-model="allowRefresh"
        label="Allow Refresh"
        help="If enabled the user will be allowed to request a refresh of the folders and media items." />
</div>
`
});
