<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VerifySecurity.ascx.cs" Inherits="RockWeb.Blocks.Security.VerifySecurity" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlSecurity" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lock"></i> Verify Security</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">

                    <div class="grid-filter padding-all-md">
                        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                        <Rock:NotificationBox ID="nbCacheCleared" runat="server" NotificationBoxType="Success" Visible="false" Dismissable="true">
                            The authorization cache has been successfully cleared.
                        </Rock:NotificationBox>
                        <Rock:NotificationBox ID="nbPoppedLock" runat="server" NotificationBoxType="Success" Dismissable="true" />

                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Help="The person to check security against, if not set then your security is checked." />
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:EntityTypePicker ID="pEntityType" runat="server" Label="Entity Type" Required="true" IncludeGlobalOption="false" Help="Select the entity type you are wanting to verify access to. If you are trying to check the security on a page then set this to 'Page'." />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockTextBox ID="tbEntityId" runat="server" Label="Entity Id" Help="Integer ID or Guid of the entity you are trying to check security of. If you are trying to check the security of the default external homepage (which is Id #1) then set this to 1." />
                            </div>
                        </div>

                        <Rock:BootstrapButton ID="btnCheck" runat="server" CssClass="btn btn-primary btn-sm" Text="Show Security" OnClick="btnCheck_Click" />
                        <asp:Button ID="btnClearCache" runat="server" CssClass="btn btn-default btn-sm" Text="Clear Auth Cache" OnClick="btnClearCache_Click" ValidationGroup="VerifySecurity_ClearCache" />
                    </div>
                
                    <asp:Panel ID="pnlResult" runat="server" Visible="false">
                        <Rock:Grid ID="gResults" runat="server" AllowPaging="false" AllowSorting="false" OnRowDataBound="gResults_RowDataBound" DataKeyNames="Id">
                            <Columns>
                                <Rock:RockBoundField DataField="Action" HeaderText="Action"></Rock:RockBoundField>
                                <Rock:RockBoundField DataField="EntityType" HeaderText="Source Type"></Rock:RockBoundField>
                                <Rock:RockBoundField DataField="EntityId" HeaderText="Source Id"></Rock:RockBoundField>
                                <Rock:RockBoundField DataField="EntityName" HeaderText="Source Name"></Rock:RockBoundField>
                                <Rock:RockBoundField DataField="Role" HeaderText="User / Role"></Rock:RockBoundField>
                                <Rock:RockBoundField DataField="Access" HeaderText="Access" HtmlEncode="false"></Rock:RockBoundField>
                                <Rock:LinkButtonField HeaderText="" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-unlock'></i>" OnClick="gUnlock_Click" />
                            </Columns>
                        </Rock:Grid>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
