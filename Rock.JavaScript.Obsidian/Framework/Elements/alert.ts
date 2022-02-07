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

import { defineComponent, PropType, computed } from "vue";

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export enum AlertType {
    Default = "default",
    Success = "success",
    Info = "info",
    Danger = "danger",
    Warning = "warning",
    Primary = "primary",
    Validation = "validation"
}

/** Displays a bootstrap style alert box. */
const Alert = defineComponent({
    name: "Alert",
    props: {
        dismissible: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        alertType: {
            type: String as PropType<AlertType>,
            default: AlertType.Default
        }
    },
    emits: [
        "dismiss"
    ],
    setup(props, { emit }) {
        function onDismiss (): void {
            emit("dismiss");
        }

        const typeClass = computed(() => `alert-${props.alertType}`);

        return { onDismiss, typeClass };
    },
    template: `
<div class="alert" :class="typeClass">
    <button v-if="dismissible" type="button" class="close" @click="onDismiss">
        <span>&times;</span>
    </button>
    <slot />
</div>`
});

export default Alert;
