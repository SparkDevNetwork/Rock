<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlGroupMembers" runat="server">

                <div class="panel panel-block">
                
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Members" />
                        </h1>

                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" /> &nbsp;
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <Rock:NotificationBox ID="nbRoleWarning" runat="server" NotificationBoxType="Warning" Title="No roles!" Visible="false" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit" >
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="GroupRole.Name" />
                                    <Rock:RockBoundField DataField="GroupMemberStatus" HeaderText="Status" SortExpression="GroupMemberStatus" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

            <Rock:ModalDialog ID="mdAlternatePlacement" runat="server" Visible="false" ValidationGroup="vgAlternatePlacement" 
                Title="<i class='fa fa-share'></i> Alternate Placement" OnSaveClick="mdAlternatePlacement_SaveClick"
                SaveButtonText="Place">
                <Content>
                    <Rock:NotificationBox ID="nbAlternatePlacementWarning" runat="server" NotificationBoxType="Warning" />
                    <asp:HiddenField ID="hfAlternatePlacementGroupMemberId" runat="server" />
                    <asp:HiddenField ID="hfAlternatePlacementTargetGroupId" runat="server" />
                    <Rock:RockLiteral ID="lAlternatePlacementGroupMemberName" runat="server" Label="Group Member" />
                    <Rock:RockLiteral ID="lAlternatePlacementTargetGroupName" runat="server" Label="Place in group" />
                    <Rock:RockTextBox ID="tbAlternatePlacementNote" runat="server" Label="Note" Rows="4" TextMode="MultiLine" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
