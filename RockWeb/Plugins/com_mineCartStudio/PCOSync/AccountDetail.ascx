<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfAccountId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                    <div id="pnlEditDetails" runat="server">

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.Account, com.minecartstudio.PCOSync" PropertyName="Name" Required="true" />
                            <Rock:DataTextBox ID="tbApplicationId" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.Account, com.minecartstudio.PCOSync" PropertyName="ApplicationId" Label="PCO Application Id" Required="true" />
                            <Rock:DataTextBox ID="tbSecret" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.Account, com.minecartstudio.PCOSync" PropertyName="Secret" Label="PCO Secret" Required="true" />
                            <Rock:RockCheckBox ID="cbPermissionDowngrade" runat="server" Label="Allow Permission Downgrade" Text="Yes" 
                                Help="Should the PCO Sync downgrade a person's permission in PCO if they are moved to a group in Rock with less privileges (e.g. Editor group to Viewer group)?" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlAdministrators" runat="server" Label="PCO Administrators" Help="The ROCK security role that should be synced to PCO with 'Administrator' privileges." />
                            <Rock:RockDropDownList ID="ddlEditors" runat="server" Label="PCO Editors" Help="The ROCK security role that should be synced to PCO with 'Editor' privileges." />
                            <Rock:RockDropDownList ID="ddlSchedulers" runat="server" Label="PCO Schedulers" Help="The ROCK security role that should be synced to PCO with 'Scheduler' privileges." />
                            <Rock:RockDropDownList ID="ddlViewers" runat="server" Label="PCO Viewers" Help="The ROCK security role that should be synced to PCO with 'Viewer' privileges." />
                            <Rock:RockDropDownList ID="ddlScheduledViewers" runat="server" Label="PCO Scheduled Viewers" Help="The ROCK security role that should be synced to PCO with 'Scheduled Viewer' privileges." />
                        </div>
                    </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblLeftDetails" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lblRightDetails" runat="server" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                </div>

            </fieldset>

            </div>
                 
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
