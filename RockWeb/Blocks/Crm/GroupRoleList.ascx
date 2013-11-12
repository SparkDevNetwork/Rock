<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRoleList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupRoleList" %>

<asp:UpdatePanel ID="upGroupRoles" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField runat="server" ID="hfGroupTypeId" />
            <div id="pnlGroupRoles" runat="server">
                <h4>Roles</h4>
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <Rock:Grid ID="gGroupRoles" runat="server" OnRowSelected="gGroupRoles_Edit">
                    <Columns>
                        <Rock:ReorderField />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Description" HeaderText="Description" />
                        <asp:BoundField DataField="MinCount" HeaderText="Minimum Required" DataFormatString="{0:N0}" />
                        <asp:BoundField DataField="MaxCount" HeaderText="Maximum Allowed" DataFormatString="{0:N0}" />
                        <Rock:BoolField DataField="IsSystem" HeaderText="System" />
                        <Rock:DeleteField OnClick="gGroupRoles_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
