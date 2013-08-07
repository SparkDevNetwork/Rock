<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hfGroupId" />
        <div id="pnlGroupMembers" runat="server">
            <h4>Group Members</h4>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit">
                <Columns>
                    <asp:BoundField DataField="PersonFirstName" HeaderText="First Name" SortExpression="PersonFirstName" />
                    <asp:BoundField DataField="PersonLastName" HeaderText="Last Name" SortExpression="PersonFullNameLastFirst" />
                    <asp:BoundField DataField="GroupRoleName" HeaderText="Group Role" SortExpression="GroupRoleName" />
                    <asp:BoundField DataField="GroupMemberStatus" HeaderText="Member Status" SortExpression="GroupMemberStatus" />
                    <Rock:DeleteField OnClick="DeleteGroupMember_Click" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
