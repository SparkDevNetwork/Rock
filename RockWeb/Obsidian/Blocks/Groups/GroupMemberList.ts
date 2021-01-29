import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Grid, { FilterOptions, SortDirection, SortProperty } from '../../Controls/Grid.js';
import GridRow from '../../Controls/GridRow.js';
import GridColumn from '../../Controls/GridColumn.js';
import GridSelectColumn from '../../Controls/GridSelectColumn.js';
import GridProfileLinkColumn from '../../Controls/GridProfileLinkColumn.js';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock.js';

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
        Grid: Grid<GroupMemberViewModel>(),
        GridRow: GridRow<GroupMemberViewModel>(),
        GridColumn: GridColumn<GroupMemberViewModel>(),
        GridSelectColumn: GridSelectColumn<GroupMemberViewModel>(),
        GridProfileLinkColumn: GridProfileLinkColumn<GroupMemberViewModel>()
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
                const result = await this.invokeBlockAction<GetGroupMemberListResponse>('getGroupMemberList', {
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
        <div class="alert alert-danger" v-if="errorMessage">
            {{errorMessage}}
        </div>
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
