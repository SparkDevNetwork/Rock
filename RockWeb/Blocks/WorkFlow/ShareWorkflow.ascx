<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShareWorkflow.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.ShareWorkflow" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-share-square"></i> Share Workflow</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wtpExport" runat="server" Label="Workflow Type" Help="The workflow type to be exported." Required="true" ValidationGroup="export" />
                        <asp:Button ID="btnExport" runat="server" Text="Export" CssClass="btn btn-primary" OnClick="btnExport_Click" ValidationGroup="export" />
                        <asp:Button ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-default" OnClick="btnPreview_Click" ValidationGroup="export" />
                    </div>

                    <div class="col-md-6">
                        <Rock:FileUploader ID="fuImport" runat="server" Label="File" Required="true" ValidationGroup="import" />

                        <Rock:CategoryPicker ID="cpImportCategory" runat="server" Label="Category" EntityTypeName="Rock.Model.WorkflowType" Required="true" ValidationGroup="import"/>

                        <Rock:RockCheckBox ID="cbDryRun" runat="server" Label="Test Only" Help="If checked, then the import will only be tested, no changes to the database will be made." />

                        <asp:LinkButton ID="lbImport" runat="server" Text="Import" CssClass="btn btn-primary" OnClick="lbImport_Click"  ValidationGroup="import"/>
                    </div>
                </div>

                <Rock:Grid ID="gPreview" runat="server" AllowSorting="true" AllowPaging="false" AutoGenerateColumns="false" OnGridRebind="gPreview_GridRebind" DataKeyNames="Guid" CssClass="margin-t-lg" DisplayType="Light">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <Rock:RockBoundField DataField="ShortType" HeaderText="Type" SortExpression="ShortType" />

                        <Rock:RockTemplateField HeaderText="References">
                            <ItemTemplate>
                                <%# string.Join( "<br />", ( List<string> )Eval( "Paths" ) ) %>
                            </ItemTemplate>
                        </Rock:RockTemplateField>

                        <Rock:RockTemplateField HeaderText="Critical" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" SortExpression="IsCritical">
                            <ItemTemplate>
                                <%# (bool)Eval("IsCritical") == true ? "<i class='fa fa-check'></i>" : string.Empty %>
                            </ItemTemplate>
                        </Rock:RockTemplateField>

                        <Rock:RockTemplateField HeaderText="New Guid" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" SortExpression="IsNewGuid">
                            <ItemTemplate>
                                <%# (bool)Eval("IsNewGuid") == true ? "<i class='fa fa-check'></i>" : string.Empty %>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>

                <asp:Panel ID="pnlImportResults" runat="server" Visible="false" CssClass="margin-t-lg">
                    <pre><asp:Literal ID="ltImportResults" runat="server" /></pre>
                </asp:Panel>
            </div>
        
        </asp:Panel>

        <Rock:ModalDialog ID="mdTree" runat="server" Title="Reference Tree">
            <Content>
                <asp:Literal ID="ltModalTree" runat="server"></asp:Literal>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>