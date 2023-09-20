<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.Rockumentation.BookDetail" %>
<%@ Register Namespace="com.blueboxmoon.Rockumentation.UI" Assembly="com.blueboxmoon.Rockumentation" TagPrefix="RM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorized" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-book"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlContentType" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlIndexed" runat="server" LabelType="Success" Text="Indexed" />
                </div>
            </div>

            <div class="panel-body">
                <fieldset>
                    <dl>
                        <dt>Slug</dt>
                        <dd><asp:Literal ID="lSlug" runat="server" /></dd>

                        <dt>Description</dt>
                        <dd><asp:Literal ID="lDescription" runat="server" /></dd>
                    </dl>

                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" ShowCategoryLabel="false" />

                    <div class="actions">
                        <a id="lbViewBook" runat="server" class="btn btn-primary">View Book</a>
                        <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-link" Text="Edit" OnClick="lbEdit_Click" />

                        <div class="pull-right">
                            <a id="lbSecurity" runat="server" class="btn btn-default btn-sm">
                                <i class="fa fa-lock"></i>
                            </a>
                        </div>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlVersionList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-code-branch"></i> Versions</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:GridFilter ID="gfDocumentationBookVersion" runat="server" OnApplyFilterClick="gfDocumentationBookVersion_ApplyFilterClick" OnClearFilterClick="gfDocumentationBookVersion_ClearFilterClick" OnDisplayFilterValue="gfDocumentationBookVersion_DisplayFilterValue">
                        <Rock:RockTextBox ID="tbVersionFilter" runat="server" Label="Version" />
                        <Rock:RockDropDownList ID="ddlIsPublishedFilter" runat="server" Label="Published">
                            <asp:ListItem Text="" Value="" />
                            <asp:ListItem Text="Yes" Value="Yes" />
                            <asp:ListItem Text="No" Value="No" />
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gDocumentationBookVersion" runat="server" RowItemText="Version" AllowSorting="true" TooltipField="Description" OnRowSelected="gDocumentationBookVersion_RowSelected" OnGridRebind="gDocumentationBookVersion_GridRebind" OnRowDataBound="gDocumentationBookVersion_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Version" HeaderText="Version" SortExpression="Version" />
                            <Rock:BoolField DataField="IsLocked" HeaderText="Locked" SortExpression="IsLocked" />
                            <Rock:BoolField DataField="IsPublished" HeaderText="Published" SortExpression="IsPublished" />
                            <Rock:DeleteField OnClick="gDocumentationBookVersion_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-book"></i>
                    <asp:Literal ID="lEditTitle" runat="server" />
                </h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <asp:ValidationSummary ID="vSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbEditTitle" runat="server" SourceTypeName="com.blueboxmoon.Rockumentation.Model.DocumentationBook, com.blueboxmoon.Rockumentation" PropertyName="Title" Label="Title" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlContentType" runat="server" Label="Content Editor" Required="true" />
                    </div>
                </div>

                <Rock:DataTextBox ID="tbEditDescription" runat="server" SourceTypeName="com.blueboxmoon.Rockumentation.Model.DocumentationBook, com.blueboxmoon.Rockumentation" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:BinaryFileTypePicker ID="pAttachmentFileType" runat="server" Label="Attachment File Type" Help="The file type to use when users upload attachments to an article." />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditAllowVersionBrowsing" runat="server" Label="Allow Version Browsing" Help="Allows users to pick which version of the book they want to look at. Users with edit access to the version can always browse." />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6"></div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsIndexed" runat="server" Label="Allow Searching" Help="Enables indexing of articles in this book." />
                    </div>
                </div>

                <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" />

                <Rock:PanelWidget ID="wpArticleAttributes" runat="server" Title="Article Attributes">
                    <div class="grid">
                        <Rock:Grid ID="gArticleAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Article Attribute" ShowConfirmDeleteDialog="false">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                <Rock:EditField OnClick="gArticleAttributes_Edit" />
                                <Rock:DeleteField OnClick="gArticleAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdConfirmConversion" runat="server" Title="Confirm" SaveButtonText="Save" CancelLinkVisible="true" OnSaveClick="mdConfirmConversion_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbConfirmConversion" runat="server" NotificationBoxType="Warning" Text="Are you sure you want to convert the content editor type? You will not be able to convert back." />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgArticleAttributes" runat="server" Title="Article Attributes" OnSaveClick="dlgArticleAttributes_SaveClick" ValidationGroup="ArticleAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtArticleAttributes" runat="server" ShowActions="false" ValidationGroup="ArticleAttributes" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
