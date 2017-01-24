<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfAccountId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <fieldset id="fieldsetViewDetails" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:Literal ID="lblDetails" runat="server" />
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnImportPCO" runat="server" Text="Import Users From PCO" CssClass="pull-right btn btn-link" CausesValidation="false" OnClick="btnImportPCO_Click" />
                    </div>
                </fieldset>

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.PCOAccount, com.minecartstudio.PCOSync" PropertyName="Name" Required="true" />
                            <Rock:RockCheckBox ID="cbPermissionDowngrade" runat="server" Label="Allow Permission Downgrade" Text="Yes" 
                                Help="Should the PCO Sync downgrade a person's permission in PCO if they are moved to a group in Rock with less privileges (e.g. Editor group to Viewer group)?" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbApplicationId" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.PCOAccount, com.minecartstudio.PCOSync" PropertyName="ApplicationId" Label="PCO Application Id" Required="true" AutoPostBack="true" OnTextChanged="tbToken_TextChanged" />
                            <Rock:DataTextBox ID="tbSecret" runat="server" SourceTypeName="com.minecartstudio.PCOSync.Model.PCOAccount, com.minecartstudio.PCOSync" PropertyName="Secret" Label="PCO Secret" Required="true" AutoPostBack="true" OnTextChanged="tbToken_TextChanged" />
                            <Rock:RockDropDownList ID="ddlWelcomeEmailTemplate" runat="server" Label="Welcome Email" DataValueField="id" DataTextField="subject" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

                <asp:Panel ID="pnlAdd" runat="server" Visible="false" >

                    <Rock:NotificationBox ID="nbAdd" runat="server" Visible="false" NotificationBoxType="Success" />

                    <div class="row">
                        <asp:PlaceHolder ID="phAddGroups" runat="server" />
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnImport" runat="server" Text="Import" CssClass="btn btn-primary" OnClick="btnImport_Click" />
                        <asp:LinkButton ID="btnCancelImport" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelImport_Click" />
                    </div>

                </asp:Panel>
 
            </div>
                 
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
