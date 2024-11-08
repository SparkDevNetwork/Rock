<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupList" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i> <asp:Literal ID="lTitle" runat="server" Text="Group List" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" />
                        <Rock:DefinedValuePicker ID="dvpGroupTypePurpose" runat="server" Label="Group Type Purpose"/>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_Edit" CssClass="js-grid-group-list" OnRowDataBound="gGroups_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockTemplateField HeaderText="Name" SortExpression="Name" Visible="false">
                                <ItemTemplate>
                                    <%# Eval("Name") %><br /><small><%# Eval("Path") %></small>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName"/>
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" ColumnPriority="Desktop" SortExpression="Description" />
                            <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="GroupRole" />
                            <Rock:RockLiteralField ID="lElevatedSecurityLevel" HeaderText="Elevated Security Level" Visible="true" SortExpression="ElevatedSecurityLevel" Text="" />
                            <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" SortExpression="MemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DateTimeField DataField="DateAdded" HeaderText="Added" SortExpression="DateAdded" FormatAsElapsedTime="true" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActiveOrder" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gGroups_DeleteOrArchive" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
          <Rock:ModalDialog ID="modalDetails" runat="server" Title="Add to Group" ValidationGroup="GroupName">
            <Content>
                <div>
                    <Rock:NotificationBox ID="nbModalDetailsMessage" runat="server" NotificationBoxType="Danger" Title="Error" Visible="false" />
                    <Rock:NotificationBox ID="nbModalDetailSyncMessage" runat="server" NotificationBoxType="Info" Title="Synced Group! " Visible="false" Text="The selected group uses Group Sync. Only roles that are not being synced are listed. Please make sure this person needs to be added manually instead of meeting the sync conditions."></Rock:NotificationBox>
                    <asp:ValidationSummary ID="vsValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvGroupMember" runat="server" Display="None" />
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:GroupPicker ID="gpGroup" runat="server" Label="Group" OnSelectItem="gpGroup_SelectedIndexChanged" ValidationGroup="GroupName"/>
                        <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" DataTextField="Name" DataValueField="Id" ValidationGroup="GroupName" EnhanceForLongLists="true" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" />
                        <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" ValidationGroup="GroupName" EnhanceForLongLists="false" AutoPostBack="false" />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <script>

            Sys.Application.add_load(function () {
                // delete/archive prompt

                $('#<%=gGroups.ClientID%>').find('a.grid-delete-button')
                    .on('click', function (e) {

                        var $btn = $(this);
                        var $row = $btn.closest('tr');
                        var actionName = 'delete';
                        var confirmMessage;

                        if ($row.hasClass('js-has-grouphistory')) {
                            var actionName = 'archive';
                        }

                        var groupListMode = <%=(int)GroupListGridMode%>;

                        if (groupListMode == 0) {

                            confirmMessage = 'Are you sure you want to ' + actionName + ' this person from this group?';
                        } else {
                            confirmMessage = 'Are you sure you want to ' + actionName + ' this group?';
                        }

                        e.preventDefault();

                        Rock.dialogs.confirm(confirmMessage, function (result) {
                            if (result) {
                                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        });
                    });
                });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
