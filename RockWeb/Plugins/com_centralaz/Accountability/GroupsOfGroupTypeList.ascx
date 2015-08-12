<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupsOfGroupTypeList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.GroupsOfGroupTypeList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                        <asp:Literal ID="lTitle" runat="server" Text="Group List" /></h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfSettings" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">

                            <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                                <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                                <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                                <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                            </Rock:RockDropDownList>
                        </Rock:GridFilter>
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_Edit">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                <asp:BoundField DataField="GroupRole" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="MemberCount" HeaderText="Members" SortExpression="MemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:DateTimeField DataField="DateAdded" HeaderText="Added" SortExpression="DateAdded" FormatAsElapsedTime="true" />
                                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                                <Rock:DeleteField OnClick="gGroups_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
