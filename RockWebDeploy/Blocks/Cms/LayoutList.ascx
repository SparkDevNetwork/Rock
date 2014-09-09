<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.LayoutList, RockWeb" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField runat="server" ID="hfSiteId" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-th"></i> Layout List</h1>
                </div>
                <div class="panel-body">
                    <div id="pnlLayouts" runat="server">

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                        <div class="grid grid-panel">
                            <Rock:Grid ID="gLayouts" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gLayouts_Edit">
                                <Columns>
                                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <asp:TemplateField HeaderText="Layout File" SortExpression="FileName">
                                        <ItemTemplate><%# GetFilePath( Eval( "FileName" ).ToString() ) %></ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:DeleteField OnClick="DeleteLayout_Click" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    
                    </div>
                </div>
            </div>

            <Rock:NotificationBox runat="server" NotificationBoxType="Info" Title="Note" 
                        Text="Rock will ensure that at least one layout entry exists for each layout file in the site's theme folder.  If all the the layouts for a layout file are deleted, Rock will automatically and immediately recreate a new layout entry." />

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
