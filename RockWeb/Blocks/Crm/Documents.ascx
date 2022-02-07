<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Documents.ascx.cs" Inherits="RockWeb.Blocks.Crm.Documents" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <Rock:NotificationBox ID="nbMessage" runat="server" Text="" Visible="false" NotificationBoxType="Warning" Mode="PassThrough"></Rock:NotificationBox>

            <asp:Panel ID="pnlList" runat="server">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="<%=this.icon %>"></i> <%=this.title %></h1>
                        <div class="form-inline panel-labels">
                            <asp:DropDownList ID="ddlDocumentType" runat="server" Label="Document Types" CssClass="form-control input-xs" IncludeGlobalOption="true" AutoPostBack="true" OnSelectedIndexChanged="ddlDocumentType_SelectedIndexChanged" />
                        </div>
                    </div>

                    <%-- Grid --%>
                    <Rock:Grid ID="gFileList" runat="server" DisplayType="Light" OnRowDataBound="gFileList_RowDataBound">
                        <Columns>
                            <Rock:RockTemplateField HeaderStyle-Width="1px">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfDocumentId" runat="server" Value='<%# Eval("Id") %>' />
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                            <Rock:RockTemplateField ShowHeader="false"  ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px">
                                <ItemTemplate>
                                    <i class="fa-fw <%# Eval("DocumentType.IconCssClass") %>"></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                            <Rock:RockBoundField DataField="Name" HeaderText="Name"></Rock:RockBoundField>
                            <Rock:RockBoundField DataField="DocumentType.Name" HeaderText="Type"></Rock:RockBoundField>
                            <Rock:RockBoundField DataField="CreatedByPersonName" HeaderText="Created By"></Rock:RockBoundField>
                            <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="Created On" FormatAsElapsedTime="true" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left"></Rock:DateTimeField>

                            <Rock:RockTemplateField ShowHeader="false"  ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px" ItemStyle-CssClass="grid-select-field">
                                <ItemTemplate>
                                    <button class="btn btn-square btn-link text-color" data-toggle="tooltip" title="<%# Eval("Description") %>">
                                        <i class="fa fa-sticky-note"></i>
                                    </button>
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                            <Rock:RockTemplateField ShowHeader="false" ItemStyle-CssClass="grid-columncommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbDownload" runat="server" CssClass="btn btn-default" Text="<i class='fa fa-download'></i>" ToolTip="Download Document" OnClick="gFileListDownload_Click"></asp:LinkButton>
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                            <Rock:SecurityField ID="securityField" TitleField="Name" ToolTip="Secure Document"/>
                            <Rock:DeleteField OnClick="gFileList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAddEdit" runat="server" Visible="false">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="<%=this.icon %>"></i> <%=this.title %></h1>
                    </div>
                    <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                    <asp:HiddenField ID="hfDocumentId" runat="server" />

                    <%-- Edit Controls --%>
                        <div class="panel-body">
                        <asp:ValidationSummary ID="valAddEditDocumentSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" DisplayMode="BulletList" />
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlAddEditDocumentType" runat="server" Label="Document Type" Required="true" OnSelectedIndexChanged="ddlAddEditDocumentType_SelectedIndexChanged" AutoPostBack="true"/>
                                <Rock:RockTextBox ID="tbDocumentName" runat="server" Label="Document Name" Required="true"></Rock:RockTextBox>
                                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine"></Rock:RockTextBox>
                            </div>
                            <div class="col-md-6">
                                <Rock:FileUploader ID="fuUploader" runat="server" DisplayMode="DropZone" IsBinaryFile="true" Required="true" Label="Document File" RequiredErrorMessage="A Document File is required." FormGroupCssClass="label-hidden fileupload-group-lg" UploadButtonText="Drop File Here or Click to Select"></Rock:FileUploader>
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="true" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>