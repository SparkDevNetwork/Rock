<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bullhorn"></i> 
                    <asp:Literal ID="lContentType" runat="server" /> Channels
                </h1>
            </div>
            
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gContentChannels" runat="server" EmptyDataText="No Channels Found" RowItemText="Channel" AllowSorting="true" TooltipField="Description" OnRowSelected="gContentChannels_Edit">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Channel" SortExpression="Name" />
                            <asp:BoundField DataField="TotalItems" HeaderText="Total Items" SortExpression="TotalItems" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <asp:BoundField DataField="ActiveItems" HeaderText="Active Items" SortExpression="ActiveItems" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:DateField DataField="ItemLastCreated" HeaderText="Last Item Added" SortExpression="ItemLastCreated" />
                            <Rock:BoolField DataField="EnableRss" HeaderText="RSS Enabled" SortExpression="EnableRss" />
                            <asp:BoundField DataField="ChannelUrl" HeaderText="Url" SortExpression="ChannelUrl" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
