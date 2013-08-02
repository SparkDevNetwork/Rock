<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.CategoryDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfCategoryId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <h1 class="banner">
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </h1>

                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" />
                    <Rock:CategoryPicker ID="cpParentCategory" runat="server" LabelText="Parent Category" />
                    <Rock:LabeledText ID="lblEntityTypeName" runat="server" LabelText="Entity Type" />
                    <asp:HiddenField ID="hfEntityTypeId" runat="server" />
                    <Rock:LabeledText ID="lblEntityTypeQualifierColumn" runat="server" LabelText="Entity Type Qualifier Column" />
                    <Rock:LabeledText ID="lblEntityTypeQualifierValue" runat="server" LabelText="Entity Type Qualifier Value" />
                    <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" />
                    <Rock:ImageUploader ID="imgIconSmall" runat="server" LabelText="Small Icon Image" />
                    <Rock:ImageUploader ID="imgIconLarge" runat="server" LabelText="Large Icon Image" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <h1 class="banner">
                    <asp:Literal ID="lCategoryIconHtml" runat="server" />&nbsp;
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <asp:Literal ID="lblActiveHtml" runat="server" />
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-mini pull-right" />
                    </div>
                </div>
            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
