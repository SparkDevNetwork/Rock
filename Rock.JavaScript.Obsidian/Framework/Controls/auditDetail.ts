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
import { EntityAuditBag } from "@Obsidian/ViewModels/Utility/entityAuditBag";
import { AuditDetailGetAuditDetailsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/auditDetailGetAuditDetailsOptionsBag";
import { Guid } from "@Obsidian/Types";
import { useHttp } from "@Obsidian/Utility/http";
import { useSecurityGrantToken } from "@Obsidian/Utility/block";

export default defineComponent({
    name: "AuditDetail",

    props: {
        /**
         * The entity type unique identifier whose audit information will
         * be retrieved and displayed.
         */
        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /**
         * The entity identifier key whose audit information will be
         * retrieved and displayed.
         */
        entityKey: {
            type: String as PropType<string>,
            required: false
        }
    },

    setup(props) {
        // #region Values

        const securityGrantToken = useSecurityGrantToken();
        const http = useHttp();
        const auditBag = ref<EntityAuditBag | null>(null);

        // #endregion

        // #region Computed Values

        const id = computed(() => auditBag.value?.id?.toString() ?? "");

        const idKey = computed(() => auditBag.value?.idKey ?? "");

        const guid = computed(() => auditBag.value?.guid ?? "");

        const createdByPersonId = computed(() => auditBag.value?.createdByPersonId);

        const createdByName = computed(() => auditBag.value?.createdByName ?? "");

        const createdRelativeTime = computed(() => auditBag.value?.createdRelativeTime);

        const modifiedByPersonId = computed(() => auditBag.value?.modifiedByPersonId);

        const modifiedByName = computed(() => auditBag.value?.modifiedByName ?? "");

        const modifiedRelativeTime = computed(() => auditBag.value?.modifiedRelativeTime);

        const showId = ref(true);

        const showGuid = ref(false);

        // #endregion

        // #region Functions

        const getPersonLink = (personId: number): string => {
            return `/Person/${personId}`;
        };

        const loadAuditBag = async (): Promise<void> => {
            if (!props.entityTypeGuid || !props.entityKey) {
                auditBag.value = null;
                return;
            }

            const data: AuditDetailGetAuditDetailsOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                securityGrantToken: securityGrantToken.value
            };

            const result = await http.post<EntityAuditBag>("/api/v2/Controls/AuditDetailGetAuditDetails", undefined, data);

            auditBag.value = result.isSuccess && result.data ? result.data : null;
        };

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the Id/Guid label is clicked.
         */
        const onIdClick = (): void => {
            if (showId.value) {
                showId.value = false;
                showGuid.value = true;
            }
            else if (showGuid.value) {
                showId.value = false;
                showGuid.value = false;
            }
            else {
                showId.value = true;
                showGuid.value = false;
            }
        };

        // #endregion

        watch([() => props.entityTypeGuid, () => props.entityKey], () => {
            loadAuditBag();
        });

        loadAuditBag();

        return {
            createdByName,
            createdByPersonId,
            createdRelativeTime,
            getPersonLink,
            guid,
            id,
            idKey,
            modifiedByName,
            modifiedByPersonId,
            modifiedRelativeTime,
            onIdClick,
            showGuid,
            showId
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
            <dt @click.stop="onIdClick" class="clickable">Id</dt>
            <dd>{{ id }}</dd>
        </dl>
        <dl v-else-if="showGuid">
            <dt @click.stop="onIdClick" class="clickable">Guid</dt>
            <dd>{{ guid }}</dd>
        </dl>
        <dl v-else>
            <dt @click.stop="onIdClick" class="clickable">Id Key</dt>
            <dd>{{ idKey }}</dd>
        </dl>
    </div>
</div>
`
});
