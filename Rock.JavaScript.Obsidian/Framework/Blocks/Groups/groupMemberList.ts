﻿// <copyright>
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
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";
import { defineComponent, inject } from "vue";
import { useStore } from "../../Store/index";
import Grid, { FilterOptions, RowContext, SortDirection, SortProperty } from "../../Controls/grid";
import GridRow from "../../Controls/gridRow";
import GridColumn from "../../Controls/gridColumn";
import GridSelectColumn from "../../Controls/gridSelectColumn";
import GridProfileLinkColumn from "../../Controls/gridProfileLinkColumn";
import { InvokeBlockActionFunc } from "../../Util/block";
import Alert from "../../Elements/alert";

type GroupMemberViewModel = {
    fullName: string;
    groupMemberId: number;
    personId: number;
    photoUrl: string;
    roleName: string;
    statusName: string;
};

type GetGroupMemberListResponse = {
    groupMembers: GroupMemberViewModel[]
};

const store = useStore();

export default defineComponent({
    name: "Groups.GroupMemberList",
    components: {
        PaneledBlockTemplate,
        Alert,
        Grid,
        GridRow,
        GridColumn,
        GridSelectColumn,
        GridProfileLinkColumn
    },
    setup() {
        return {
            invokeBlockAction: inject("invokeBlockAction") as InvokeBlockActionFunc
        };
    },
    data() {
        return {
            isLoading: false,
            errorMessage: "",
            members: [] as GroupMemberViewModel[],
            sortProperty: {
                direction: SortDirection.Ascending,
                property: ""
            } as SortProperty
        };
    },
    computed: {
        groupId(): number {
            return store.groupContext?.id || 0;
        },
    },
    methods: {
        async fetchGroupMembers(): Promise<void> {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;
            this.errorMessage = "";

            try {
                const result = await this.invokeBlockAction<GetGroupMemberListResponse>("GetGroupMemberList", {
                    groupId: this.groupId,
                    filterOptions: {
                        take: 50,
                        skip: 0
                    } as FilterOptions,
                    sortProperty: this.sortProperty
                });

                if (result.data && result.data.groupMembers) {
                    this.members = result.data.groupMembers;
                }
                else {
                    this.members = [];
                }
            }
            catch (e) {
                this.errorMessage = `An exception occurred: ${e}`;
            }
            finally {
                this.isLoading = false;
            }
        },
        onRowClick(rowContext: RowContext): void {
            const groupMemberId = rowContext.rowId;
            location.href = "/GroupMember/" + groupMemberId;
        }
    },
    watch: {
        async groupId(): Promise<void> {
            if (this.groupId) {
                await this.fetchGroupMembers();
            }
        },
        sortProperty: {
            deep: true,
            async handler(): Promise<void> {
                await this.fetchGroupMembers();
            }
        }
    },
    async mounted(): Promise<void> {
        if (this.groupId) {
            await this.fetchGroupMembers();
        }
    },
    template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-users"></i>
        Group Members
    </template>
    <template #default>
        <Alert v-if="errorMessage" alertType="danger">
            {{errorMessage}}
        </Alert>
        <div class="grid grid-panel">
            <Grid :gridData="members" rowIdKey="groupMemberId" #default="rowContext" v-model:sortProperty="sortProperty" rowItemText="Group Member">
                <GridRow :rowContext="rowContext" @click:body="onRowClick">
                    <GridSelectColumn />
                    <GridColumn title="Name" property="fullName" sortExpression="person.lastName,person.nickName">
                        <div
                            class="photo-icon photo-round photo-round-xs pull-left margin-r-sm"
                            :style="{
                                backgroundImage: 'url(' + rowContext.rowData.photoUrl + ')',
                                backgroundSize: 'cover',
                                backgroundRepeat: 'no-repeat'
                            }"></div>
                        {{rowContext.rowData.fullName}}
                    </GridColumn>
                    <GridColumn title="Role" property="roleName" sortExpression="groupRole.name" />
                    <GridColumn title="Member Status" property="statusName" sortExpression="groupMemberStatus" />
                    <GridProfileLinkColumn property="personId" />
                </GridRow>
            </Grid>
        </div>
    </template>
</PaneledBlockTemplate>`
});
