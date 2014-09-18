<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentItemList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentItemList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bullhorn"></i> 
                    <asp:Literal ID="lContentChannel" runat="server" /> Items
                </h1>
            </div>
            
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gContentItems" runat="server" EmptyDataText="No Items Found" RowItemText="Item" AllowSorting="true" OnRowSelected="gContentItems_Edit">
                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="Item" SortExpression="Title" />
                            <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                            <asp:TemplateField HeaderText="Status" SortExpression="Status">
                                <ItemTemplate><%# DisplayStatus( Eval("Status") ) %></ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
