<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PendingGroupMemberList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.PendingGroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">
            <Rock:NotificationBox ID="nbRoleWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

            <div id="pnlGroupMembers" runat="server">
                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Pending Members" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                <Rock:CampusPicker ID="cpCampusFilter" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full">
                                <Columns>
                                    <Rock:RockBoundField DataField="GroupTree" HeaderText="Group" SortExpression="Group" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Group Member" SortExpression="Person.LastName,Person.NickName" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="GroupRole.Name" />
                                    <Rock:RockBoundField DataField="GroupMemberStatus" HeaderText="Status" SortExpression="GroupMemberStatus" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
