<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <asp:HiddenField runat="server" ID="hfGroupId" />
            
            <div id="pnlGroupMembers" runat="server">

                <h4><asp:Literal ID="lHeading" runat="server" Text="Group Members" /></h4>
                
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <Rock:NotificationBox ID="nbRoleWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

                <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                    <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                </Rock:GridFilter>
                <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit">
                    <Columns>
                        <asp:BoundField DataField="Person.FirstName" HeaderText="First Name" SortExpression="Person.FirstName" />
                        <asp:BoundField DataField="Person.LastName" HeaderText="Last Name" SortExpression="Person.LastName" />
                        <asp:BoundField DataField="GroupRole.Name" HeaderText="Role" SortExpression="GroupRole.Name" />
                        <asp:BoundField DataField="GroupMemberStatus" HeaderText="Status" SortExpression="GroupMemberStatus" />
                        <Rock:DeleteField OnClick="DeleteGroupMember_Click" />
                    </Columns>
                </Rock:Grid>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
