<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSSFeed.ascx.cs" Inherits="RockWeb.Blocks.Cms.RSSFeed" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbRSSFeed" runat="server" NotificationBoxType="Info" Visible="false" />
        <asp:Panel ID="pnlContent" runat="server" Visible="false">
            <div class="banner">
                <h1>
                    <asp:Literal ID="litTitle" runat="server" /></h1>
            </div>
            <Rock:Grid ID="gRSSItems" runat="server" AllowSorting="false" AllowPaging="false" DataKeyNames="guid">
                <Columns>
                    <asp:BoundField HeaderText="Title" DataField="title" />
                    <asp:BoundField HeaderText="Publish Date" DataField="pubDate" DataFormatString="{0:g}" />
                    <asp:BoundField HeaderText="Description" DataField="description" />
                    <asp:HyperLinkField Target="_blank" Text="Link" DataNavigateUrlFields="link"/>
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
