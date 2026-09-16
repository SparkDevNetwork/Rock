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
import { getFieldEditorProps } from "./utils";
import ConnectionTypeSettingsPicker from "@Obsidian/Controls/connectionTypeSettingsPicker.obs";
import { ConfigurationValueKey, ConnectionTypeSettings } from "./connectionTypeSettingsField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ValidationRule } from "@Obsidian/ValidationRules";


export const EditComponent = defineComponent({
    name: "ConnectionTypeSettingsField.Edit",

    components: {
        ConnectionTypeSettingsPicker
    },

    props: {
        ...getFieldEditorProps(),

        // Forward IsRequired to Type + Opportunity only; Status and Source are optional.
        rules: {
            type: [Array, Object, String] as PropType<ValidationRule | ValidationRule[]>,
            default: ""
        }
    },

    setup(props, { emit }) {
        const connectionType = ref<ListItemBag | null>(null);
        const connectionOpportunity = ref<ListItemBag | null>(null);
        const connectionStatus = ref<ListItemBag | null>(null);
        const connectionTypeSource = ref<ListItemBag | null>(null);

        function parseList(key: string): ListItemBag[] {
            const raw = props.configurationValues?.[key];
            if (!raw) {
                return [];
            }
            try {
                const parsed = JSON.parse(raw);
                return Array.isArray(parsed) ? parsed as ListItemBag[] : [];
            }
            catch {
                return [];
            }
        }

        function parseMap(key: string): Record<string, ListItemBag[]> {
            const raw = props.configurationValues?.[key];
            if (!raw) {
                return {};
            }
            try {
                const parsed = JSON.parse(raw);
                return parsed && typeof parsed === "object" ? parsed as Record<string, ListItemBag[]> : {};
            }
            catch {
                return {};
            }
        }

        const connectionTypes = computed<ListItemBag[]>(() => parseList(ConfigurationValueKey.ConnectionTypes));
        const connectionOpportunitiesByType = computed<Record<string, ListItemBag[]>>(() => parseMap(ConfigurationValueKey.ConnectionOpportunitiesByType));
        const connectionStatusesByType = computed<Record<string, ListItemBag[]>>(() => parseMap(ConfigurationValueKey.ConnectionStatusesByType));
        const connectionTypeSourcesByType = computed<Record<string, ListItemBag[]>>(() => parseMap(ConfigurationValueKey.ConnectionTypeSourcesByType));

        watch(() => props.modelValue, () => {
            const raw = props.modelValue;
            let parsed: ConnectionTypeSettings | null = null;

            if (raw) {
                try {
                    parsed = JSON.parse(raw) as ConnectionTypeSettings;
                }
                catch {
                    parsed = null;
                }
            }

            connectionType.value = parsed?.connectionType ?? null;
            connectionOpportunity.value = parsed?.connectionOpportunity ?? null;
            connectionStatus.value = parsed?.connectionStatus ?? null;
            connectionTypeSource.value = parsed?.connectionTypeSource ?? null;
        }, { immediate: true });

        watch([connectionType, connectionOpportunity, connectionStatus, connectionTypeSource], () => {
            const newValue: ConnectionTypeSettings = {
                connectionType: connectionType.value,
                connectionOpportunity: connectionOpportunity.value,
                connectionStatus: connectionStatus.value,
                connectionTypeSource: connectionTypeSource.value
            };

            const serialized = JSON.stringify(newValue);

            // Skip the phantom emit on initial mount when the rebuilt JSON
            // matches the input exactly.
            if (serialized === (props.modelValue ?? "")) {
                return;
            }

            emit("update:modelValue", serialized);
        });

        return {
            connectionType,
            connectionOpportunity,
            connectionStatus,
            connectionTypeSource,
            connectionTypes,
            connectionOpportunitiesByType,
            connectionStatusesByType,
            connectionTypeSourcesByType,
            rules: computed(() => props.rules)
        };
    },

    template: `
<ConnectionTypeSettingsPicker
    v-model:connectionType="connectionType"
    v-model:connectionOpportunity="connectionOpportunity"
    v-model:connectionStatus="connectionStatus"
    v-model:connectionTypeSource="connectionTypeSource"
    :connectionTypes="connectionTypes"
    :connectionOpportunitiesByType="connectionOpportunitiesByType"
    :connectionStatusesByType="connectionStatusesByType"
    :connectionTypeSourcesByType="connectionTypeSourcesByType"
    :rules="rules" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ConnectionTypeSettingsField.Configuration",

    template: ``
});
