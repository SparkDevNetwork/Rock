<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hfGroupId" />
        <div id="pnlGroupMembers" runat="server">
            <h4>Group Members</h4>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit" IsPersonList="true">
                <Columns>
                    <asp:BoundField DataField="Person.FirstName" HeaderText="First Name" SortExpression="Person.FullName" />
                    <asp:BoundField DataField="Person.LastName" HeaderText="Last Name" SortExpression="Person.FullNameLastFirst" />
                    <asp:BoundField DataField="GroupRole.Name" HeaderText="Group Role" SortExpression="GroupRole.Name" />
                    <asp:BoundField DataField="GroupMemberStatus" HeaderText="Member Status" SortExpression="GroupMemberStatus" />
                    <Rock:DeleteField OnClick="DeleteGroupMember_Click" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
