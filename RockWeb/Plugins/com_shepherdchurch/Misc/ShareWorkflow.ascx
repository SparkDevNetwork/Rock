<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShareWorkflow.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.Misc.ShareWorkflow" %>

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
                        <Rock:WorkflowTypePicker ID="wtpExport" runat="server" Label="Workflow Type" Help="The workflow type to be exported." />
                        <asp:Button ID="btnExport" runat="server" Text="Export" CssClass="btn btn-primary" OnClick="btnExport_Click" />
                        <asp:Button ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-info" OnClick="btnPreview_Click" />
                        <asp:Button ID="btnStage2" runat="server" Text="Stage 2" CssClass="btn btn-info" OnClick="btnStage2_Click" Visible="false" />
                    </div>

                    <div class="col-md-6">
                        <Rock:FileUploader ID="fuImport" runat="server" Label="Import File" OnFileUploaded="fuImport_FileUploaded" />
                    </div>
                </div>

                <Rock:Grid ID="gPreview" runat="server" AllowSorting="true" AllowPaging="false" AutoGenerateColumns="false" OnRowDataBound="gPreview_RowDataBound" OnGridRebind="gPreview_GridRebind" DataKeyNames="Guid">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <Rock:RockBoundField DataField="ShortType" HeaderText="Type" SortExpression="ShortType" />

                        <Rock:RockTemplateField HeaderText="References">
                            <ItemTemplate>
                                <%# string.Join( "<br />", ( ( List<KeyValuePair<string, string>> ) Eval( "Parents" ) ).Select( kv => string.Format( "{0} via {1}", kv.Key, kv.Value ) ).ToArray() ) %>
                            </ItemTemplate>
                        </Rock:RockTemplateField>

                        <Rock:LinkButtonField HeaderText="Tree" CssClass="btn btn-default btn-sm fa fa-tree" HeaderStyle-HorizontalAlign="Center" OnClick="lbTree_Click"></Rock:LinkButtonField>

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

                        <Rock:RockTemplateField HeaderText="Action" SortExpression="Action" Visible="false">
                            <ItemTemplate>
                                <Rock:RockDropDownList ID="ddlAction" runat="server" CssClass="input-sm">
                                    <asp:ListItem Text="Include" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Ignore" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="Select On Import" Value="3"></asp:ListItem>
                                </Rock:RockDropDownList>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
                <asp:Literal ID="ltPreview" runat="server"></asp:Literal>
                <pre><asp:Literal ID="ltDebug" runat="server"></asp:Literal></pre>
            </div>
        
        </asp:Panel>

        <Rock:ModalDialog ID="mdTree" runat="server" Title="Reference Tree">
            <Content>
                <asp:Literal ID="ltModalTree" runat="server"></asp:Literal>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>