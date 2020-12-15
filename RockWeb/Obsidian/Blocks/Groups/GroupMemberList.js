Obsidian.Blocks.registerBlock({
    name: 'Groups.GroupMemberList',
    inject: [
        'blockAction'
    ],
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        Grid: Obsidian.Controls.Grid,
        GridRow: Obsidian.Controls.GridRow,
        GridColumn: Obsidian.Controls.GridColumn,
        GridSelectColumn: Obsidian.Controls.GridSelectColumn,
        GridProfileLinkColumn: Obsidian.Controls.GridProfileLinkColumn
    },
    data() {
        return {
            isLoading: false,
            errorMessage: '',
            members: [],
            sortProperty: {
                direction: 0,
                property: ''
            }
        };
    },
    computed: {
        groupId() {
            return (this.$store.getters.groupContext || {}).Id || 0;
        },
    },
    methods: {
        async fetchGroupMembers() {
            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.blockAction('getGroupMemberList', {
                    groupId: this.groupId,
                    filterOptions: {
                        take: 50,
                        skip: 0
                    },
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
        onRowClick(rowContext) {
            const groupMemberId = rowContext.rowId;
            location.href = '/GroupMember/' + groupMemberId;
        }
    },
    watch: {
        async groupId() {
            if (this.groupId) {
                await this.fetchGroupMembers();
            }
        },
        sortProperty: {
            deep: true,
            async handler() {
                await this.fetchGroupMembers();
            }
        }
    },
    template:
`<PaneledBlockTemplate>
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
