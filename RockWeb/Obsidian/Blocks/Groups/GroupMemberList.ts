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
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';
import { defineComponent, inject } from 'vue';
import store from '../../Store/Index';
import Grid, { FilterOptions, SortDirection, SortProperty } from '../../Controls/Grid';
import GridRow from '../../Controls/GridRow';
import GridColumn from '../../Controls/GridColumn';
import GridSelectColumn from '../../Controls/GridSelectColumn';
import GridProfileLinkColumn from '../../Controls/GridProfileLinkColumn';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import Alert from '../../Elements/Alert';

type GroupMemberViewModel = {
    FullName: string;
    GroupMemberId: number;
    PersonId: number;
    PhotoUrl: string;
    RoleName: string;
    StatusName: string;
};

type GetGroupMemberListResponse = {
    GroupMembers: GroupMemberViewModel[]
};

export default defineComponent({
    name: 'Groups.GroupMemberList',
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
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc
        };
    },
    data() {
        return {
            isLoading: false,
            errorMessage: '',
            members: [] as GroupMemberViewModel[],
            sortProperty: {
                Direction: SortDirection.Ascending,
                Property: ''
            } as SortProperty
        };
    },
    computed: {
        groupId(): number {
            return (store.getters.groupContext || {}).Id || 0;
        },
    },
    methods: {
        async fetchGroupMembers(): Promise<void> {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.invokeBlockAction<GetGroupMemberListResponse>('GetGroupMemberList', {
                    groupId: this.groupId,
                    filterOptions: {
                        Take: 50,
                        Skip: 0
                    } as FilterOptions,
                    sortProperty: this.sortProperty
                });

                if (result.data && result.data.GroupMembers) {
                    this.members = result.data.GroupMembers;
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
        onRowClick(rowContext): void {
            const groupMemberId = rowContext.rowId;
            location.href = '/GroupMember/' + groupMemberId;
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
            <Grid :gridData="members" rowIdKey="GroupMemberId" #default="rowContext" v-model:sortProperty="sortProperty" rowItemText="Group Member">
                <GridRow :rowContext="rowContext" @click:body="onRowClick">
                    <GridSelectColumn />
                    <GridColumn title="Name" property="FullName" sortExpression="Person.LastName,Person.NickName">
                        <div
                            class="photo-icon photo-round photo-round-xs pull-left margin-r-sm"
                            :style="{
                                backgroundImage: 'url(' + rowContext.rowData.PhotoUrl + ')',
                                backgroundSize: 'cover',
                                backgroundRepeat: 'no-repeat'
                            }"></div>
                        {{rowContext.rowData.FullName}}
                    </GridColumn>
                    <GridColumn title="Role" property="RoleName" sortExpression="GroupRole.Name" />
                    <GridColumn title="Member Status" property="StatusName" sortExpression="GroupMemberStatus" />
                    <GridProfileLinkColumn property="PersonId" />
                </GridRow>
            </Grid>
        </div>
    </template>
</PaneledBlockTemplate>`
});
