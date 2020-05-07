<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DocumentTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.DocumentTypeDetail" %>

<asp:UpdatePanel ID="upDocumentTypeDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valDocumentTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-12">
                            <asp:Literal ID="lDocumentTypeDescription" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DocumentType, Rock" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:BinaryFileTypePicker ID="bftpBinaryFileType" runat="server" Required="true" Label="File Type" />
                        </div>
                        <div class="col-md-6">
                            <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Entity Type" Required="true" EnhanceForLongLists="true" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="rcbManuallySelectable" runat="server" Label="Manually Selectable" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbIconCssClass" runat="server" Label="Icon CSS Class" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12 text-right">
                            <asp:HiddenField ID="hfShowAdvancedSettings" runat="server" />
                            <asp:LinkButton ID="lbToggleAdvancedSettings" runat="server" OnClick="lbToggleAdvancedSettings_Click" CausesValidation="false" />
                        </div>
                    </div>
                    <div class="row" id="divAdvanced" runat="server">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbQualifierColumn" runat="server" Label="Entity Qualifier Column" Required="false"
                                Help="If you would like the document type to only apply to specific entities of the specified type you can provide a column to filter on for that entity. For example if you would like the documents to be specific to a group of a certain type the ‘Column’ would be ‘GroupTypeId’. You’ll also need to provide a Entity Qualifer Value." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbQualifierValue" runat="server" Label="Entity Qualifier Value" Required="false"
                                Help="Once you provide a Entity Qualifer Column, you’ll need to provide the value in that column to filter on. In the example of groups of a certain type, the value would be the Group Type Id to filter on (e.g. 12)." />
                        </div>
                        <div class="col-sm-12">
                            <Rock:CodeEditor ID="ceTemplate" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Default Document Name Template" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
