<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NotificationGroupDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.NotificationGroupDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tasks"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            
            <div class="panel-body">
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Schedule</dt>
                                <dd><asp:Literal ID="lSchedule" runat="server" /></dd>

                                <dt>Devices</dt>
                                <dd><asp:Literal ID="lDevices" runat="server" /></dd>
                            </dl>
                        </div>

                        <div class="col-md-6">
                            <dl>
                                <dt>States</dt>
                                <dd><asp:Literal ID="lStates" runat="server" /></dd>

                                <dt>Device Groups</dt>
                                <dd><asp:Literal ID="lDeviceGroups" runat="server" /></dd>
                            </dl>
                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMembers" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-group"></i> Members</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gMembers" runat="server" AllowSorting="true" OnGridRebind="gMembers_GridRebind" OnGridReorder="gMembers_GridReorder" OnRowSelected="gMembers_RowSelected" OnRowDataBound="gMembers_RowDataBound">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockTemplateField HeaderText="Email" SortExpression="Email" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <i class='fa fa-check <%# (bool)Eval("Email") ? "" : "hidden" %>'></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="SMS" SortExpression="SMS" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <i class='fa fa-check <%# (bool)Eval("SMS") ? "" : "hidden" %>'></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gMembersDelete_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tasks"></i>
                    <asp:Literal ID="lEditTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Required="true" Label="Name" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlSchedule" runat="server" Required="false" Label="Schedule" Help="Notifications will only be sent out during this schedule." />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblStates" runat="server" Required="false" Label="States" Help="Notifications will only be sent when a service check changes to this state." RepeatDirection="Horizontal" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <div class="form-group">
                            <label>Devices</label>
                            <div class="control-wrapper">
                                <asp:Repeater ID="rpDevices" runat="server" OnItemCommand="rpDevices_ItemCommand">
                                    <ItemTemplate>
                                        <div class="control-static"><%# Eval("Name") %> <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-link" CommandArgument='<%# Eval("Id") %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <Rock:RockDropDownList ID="ddlDevice" runat="server" OnSelectedIndexChanged="ddlDevice_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                            </div>
                        </div>

                        <div class="form-group">
                            <label>Device Groups</label>
                            <div class="control-wrapper">
                                <asp:Repeater ID="rpDeviceGroups" runat="server" OnItemCommand="rpDeviceGroups_ItemCommand">
                                    <ItemTemplate>
                                        <div class="control-static"><%# Eval("Name") %> <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-link" CommandArgument='<%# Eval("Id") %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <Rock:RockDropDownList ID="ddlDeviceGroup" runat="server" OnSelectedIndexChanged="ddlDeviceGroup_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlEditMember" runat="server" OnSaveClick="mdlEditMember_SaveClick" ValidationGroup="EditMember">
            <Content>
                <asp:HiddenField ID="hfEditMemberId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppEditMember" runat="server" Label="Person" Required="true" ValidationGroup="EditMember" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblEditMemberNotificationMethod" runat="server" Label="Notification Method" RepeatDirection="Horizontal" Required="false" ValidationGroup="EditMember">
                            <asp:ListItem Value="Email" Text="Email" />
                            <asp:ListItem Value="SMS" Text="SMS" />
                        </Rock:RockCheckBoxList>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>