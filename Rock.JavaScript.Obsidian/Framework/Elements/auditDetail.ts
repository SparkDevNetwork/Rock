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
import { AuditDetail as AuditDetailViewModel } from "../ViewModels/auditDetail";

export default defineComponent({
    name: "AuditDetail",

    props: {
        modelValue: {
            type: Object as PropType<AuditDetailViewModel>,
            default: {}
        }
    },

    setup(props) {
        const id = computed(() => props.modelValue.id?.toString() ?? "");

        const guid = computed(() => props.modelValue.guid ?? "");

        const createdByPersonId = computed(() => props.modelValue.createdByPersonId);

        const createdByName = computed(() => props.modelValue.createdByName ?? "");

        const createdRelativeTime = computed(() => props.modelValue.createdRelativeTime);

        const modifiedByPersonId = computed(() => props.modelValue.modifiedByPersonId);

        const modifiedByName = computed(() => props.modelValue.modifiedByName ?? "");

        const modifiedRelativeTime = computed(() => props.modelValue.modifiedRelativeTime);

        const showId = ref(true);

        const getPersonLink = (personId: number): string => `/Person/${personId}`;

        /**
         * Event handler for when the Id/Guid label is clicked.
         */
        const onIdClick = (): void => {
            showId.value = !showId.value;
        };

        return {
            createdByName,
            createdByPersonId,
            createdRelativeTime,
            getPersonLink,
            guid,
            id,
            modifiedByName,
            modifiedByPersonId,
            modifiedRelativeTime,
            onIdClick,
            showId,
        };
    },

    template: `
<div class="row">
    <div class="col-md-4">
        <dl>
            <dt>Created By</dt>
            <dd>
                <a v-if="createdByPersonId" :href="getPersonLink(createdByPersonId)">{{ createdByName }}</a>
                <span v-else-if="createdByName">{{ createdByName }}</span>
                <small v-if="createdRelativeTime">&nbsp;({{ createdRelativeTime }})</small>
            </dd>
        </dl>
    </div>

    <div class="col-md-4">
        <dl>
            <dt>Modified By</dt>
            <dd>
                <a v-if="modifiedByPersonId" :href="getPersonLink(modifiedByPersonId)">{{ modifiedByName }}</a>
                <span v-else-if="createdByName">{{ modifiedByName }}</span>
                <small v-if="modifiedRelativeTime">&nbsp;({{ modifiedRelativeTime }})</small>
            </dd>
        </dl>
    </div>

    <div class="col-md-4">
        <dl v-if="showId">
            <dt @click.stop="onIdClick">Id</dt>
            <dd>{{ id }}</dd>
        </dl>
        <dl v-else>
            <dt @click.stop="onIdClick">Guid</dt>
            <dd>{{ guid }}</dd>
        </dl>
    </div>
</div>
`
});
