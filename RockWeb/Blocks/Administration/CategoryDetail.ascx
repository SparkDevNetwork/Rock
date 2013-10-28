<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.CategoryDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfCategoryId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">
                <div class="banner">
                    <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>

                <fieldset>
                    
                    <asp:HiddenField ID="hfEntityTypeId" runat="server" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" />
                            <Rock:CategoryPicker ID="cpParentCategory" runat="server" Label="Parent Category" />
                            <Rock:RockLiteral ID="lblEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                            <Rock:RockLiteral ID="lblEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lblEntityTypeName" runat="server" Label="Entity Type" />
                            <Rock:ImageUploader ID="imgIconSmall" runat="server" Label="Small Icon Image" />
                            <Rock:ImageUploader ID="imgIconLarge" runat="server" Label="Large Icon Image" />
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-sm btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-sm" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            

            <fieldset id="fieldsetViewDetails" runat="server">
                <div class="banner">
                    <h1><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                </div>

                <asp:Literal ID="lblActiveHtml" runat="server" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-sm btn-primary" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-sm" OnClick="btnDelete_Click" />
                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-mini pull-right" />
                </div>

            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
