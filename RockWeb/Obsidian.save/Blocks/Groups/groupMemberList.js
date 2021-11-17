System.register(["../../Templates/paneledBlockTemplate", "vue", "../../Store/index", "../../Controls/grid", "../../Controls/gridRow", "../../Controls/gridColumn", "../../Controls/gridSelectColumn", "../../Controls/gridProfileLinkColumn", "../../Elements/alert"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var paneledBlockTemplate_1, vue_1, index_1, grid_1, gridRow_1, gridColumn_1, gridSelectColumn_1, gridProfileLinkColumn_1, alert_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (paneledBlockTemplate_1_1) {
                paneledBlockTemplate_1 = paneledBlockTemplate_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (grid_1_1) {
                grid_1 = grid_1_1;
            },
            function (gridRow_1_1) {
                gridRow_1 = gridRow_1_1;
            },
            function (gridColumn_1_1) {
                gridColumn_1 = gridColumn_1_1;
            },
            function (gridSelectColumn_1_1) {
                gridSelectColumn_1 = gridSelectColumn_1_1;
            },
            function (gridProfileLinkColumn_1_1) {
                gridProfileLinkColumn_1 = gridProfileLinkColumn_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Groups.GroupMemberList",
                components: {
                    PaneledBlockTemplate: paneledBlockTemplate_1.default,
                    Alert: alert_1.default,
                    Grid: grid_1.default,
                    GridRow: gridRow_1.default,
                    GridColumn: gridColumn_1.default,
                    GridSelectColumn: gridSelectColumn_1.default,
                    GridProfileLinkColumn: gridProfileLinkColumn_1.default
                },
                setup() {
                    return {
                        invokeBlockAction: vue_1.inject("invokeBlockAction")
                    };
                },
                data() {
                    return {
                        isLoading: false,
                        errorMessage: "",
                        members: [],
                        sortProperty: {
                            direction: grid_1.SortDirection.Ascending,
                            property: ""
                        }
                    };
                },
                computed: {
                    groupId() {
                        var _a;
                        return ((_a = store.groupContext) === null || _a === void 0 ? void 0 : _a.id) || 0;
                    },
                },
                methods: {
                    fetchGroupMembers() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.isLoading) {
                                return;
                            }
                            this.isLoading = true;
                            this.errorMessage = "";
                            try {
                                const result = yield this.invokeBlockAction("GetGroupMemberList", {
                                    groupId: this.groupId,
                                    filterOptions: {
                                        take: 50,
                                        skip: 0
                                    },
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
                        });
                    },
                    onRowClick(rowContext) {
                        const groupMemberId = rowContext.rowId;
                        location.href = "/GroupMember/" + groupMemberId;
                    }
                },
                watch: {
                    groupId() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.groupId) {
                                yield this.fetchGroupMembers();
                            }
                        });
                    },
                    sortProperty: {
                        deep: true,
                        handler() {
                            return __awaiter(this, void 0, void 0, function* () {
                                yield this.fetchGroupMembers();
                            });
                        }
                    }
                },
                mounted() {
                    return __awaiter(this, void 0, void 0, function* () {
                        if (this.groupId) {
                            yield this.fetchGroupMembers();
                        }
                    });
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
            }));
        }
    };
});
//# sourceMappingURL=groupMemberList.js.map