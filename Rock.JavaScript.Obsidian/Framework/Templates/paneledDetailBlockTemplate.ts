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

import { computed, defineComponent, PropType, ref } from "vue";
import PaneledBlockTemplate from "./paneledBlockTemplate";
import { useVModelPassthrough } from "../Util/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockForm from "../Controls/rockForm";
import RockButton from "../Elements/rockButton";
import { isPromise } from "../Util/util";
import { confirmDelete } from "../Util/dialogs";

export default defineComponent({
    name: "PaneledDetailBlockTemplate",

    components: {
        PaneledBlockTemplate,
        RockButton,
        RockForm
    },

    props: {
        title: {
            type: String as PropType<string>,
            required: false
        },

        iconClass: {
            type: String as PropType<string>,
            required: false
        },

        labels: {
            type: Array as PropType<ListItemBag[]>,
            required: false
        },

        entityTitle: {
            type: String as PropType<string>,
            default: "Entity"
        },

        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isEditAllowed: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        isDeleteAllowed: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        onCancelEdit: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onEdit: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onSave: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onDelete: {
            type: Function as PropType<() => void | PromiseLike<void>>,
            required: false
        }
    },

    emits: {
        "update:isEditMode": (_value: boolean) => true
    },

    setup(props, { emit }) {
        const isEditMode = useVModelPassthrough(props, "isEditMode", emit);

        const isEditDisabled = ref(false);

        const hasActions = computed((): boolean => {
            if (isEditMode.value) {
                return true;
            }
            else {
                return props.isEditAllowed || props.isDeleteAllowed;
            }
        });

        const hasLabels = computed((): boolean => {
            return !!props.labels && props.labels.length > 0;
        });

        const blockTitle = computed((): string => {
            return props.title ?? "";
        });

        const blockLabels = computed((): ListItemBag[] => {
            if (!props.labels) {
                return [];
            }

            return props.labels.map(label => {
                return {
                    value: `label label-${label.value}`,
                    text: label.text
                };
            });
        });

        const onDeleteClick = async (): Promise<void> => {
            if (props.onDelete) {
                if (!await confirmDelete(props.entityTitle)) {
                    return;
                }

                const result = props.onDelete();

                if (isPromise(result)) {
                    await result;
                }
            }
        };

        const onSaveSubmit = async (): Promise<void> => {
            if (props.onSave) {
                let result = props.onSave();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === true) {
                    isEditMode.value = false;
                }
            }
        };

        const onEditCancelClick = async (): Promise<void> => {
            if (props.onCancelEdit) {
                let result = props.onCancelEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === false) {
                    return;
                }
            }

            isEditMode.value = false;
        };

        const onEditClick = async (): Promise<void> => {
            if (props.onEdit) {
                let result = props.onEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === true) {
                    isEditMode.value = true;
                }
            }
        };

        return {
            blockLabels,
            blockTitle,
            isEditDisabled,
            hasActions,
            hasLabels,
            isEditMode,
            onDeleteClick,
            onEditCancelClick,
            onEditClick,
            onSaveSubmit,
        };
    },

    template: `
<PaneledBlockTemplate>
    <template #title>
        <i if="iconClass" :class="iconClass"></i>
        {{ blockTitle }}
    </template>

    <template #titleAside>
        <div v-if="hasLabels" class="panel-labels">
            <span v-for="label in blockLabels" :class="label.value">{{ label.text }}</span>
        </div>
    </template>

    <template #default>
        <RockForm @submit="onSaveSubmit">
        <slot />

        <div v-if="hasActions" class="actions">
            <template v-if="isEditMode">
                <RockButton type="submit" btnType="primary" autoDisable>Save</RockButton>
                <RockButton btnType="link" @click="onEditCancelClick">Cancel</RockButton>
            </template>

            <template v-else>
                <RockButton v-if="isEditAllowed" btnType="primary" @click="onEditClick" autoDisable>Edit</RockButton>
                <RockButton v-if="isDeleteAllowed" btnType="link" @click="onDeleteClick" autoDisable>Delete</RockButton>
            </template>
        </div>
        </RockForm>
    </template>
</PaneledBlockTemplate>
`
});
