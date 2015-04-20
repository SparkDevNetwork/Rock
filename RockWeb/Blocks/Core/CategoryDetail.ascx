<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.CategoryDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server" /> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:HiddenField ID="hfCategoryId" runat="server" />

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>
                    
                        <asp:HiddenField ID="hfEntityTypeId" runat="server" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" />
                                <Rock:CategoryPicker ID="cpParentCategory" runat="server" Label="Parent Category" />
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" />
                                <Rock:DataTextBox ID="tbHighlightColor" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="HighlightColor" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lblEntityTypeName" runat="server" Label="Entity Type" />
                                <Rock:RockLiteral ID="lblEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                <Rock:RockLiteral ID="lblEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                            </div>
                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <asp:Literal ID="lblActiveHtml" runat="server" />

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upCategoryDetailConfig" runat="server">
    <ContentTemplate>
        <Rock:ModalDialog ID="mdCategoryDetailConfig" runat="server" ValidationGroup="vg_ConfigDetail" OnSaveClick="mdCategoryDetailConfig_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbRootCategoryEntityTypeWarning" runat="server" Text="Entity Type must be set in Block Settings before setting Root Category." NotificationBoxType="Warning" />
                <Rock:CategoryPicker ID="cpRootCategoryDetail" ValidationGroup="vg_ConfigDetail" runat="server" Label="Root Category" />

                <Rock:CategoryPicker ID="cpExcludeCategoriesDetail" ValidationGroup="vg_ConfigDetail" runat="server" Label="Exclude Categories" AllowMultiSelect="true" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>

