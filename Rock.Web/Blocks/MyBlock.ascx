<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyBlock.ascx.cs" Inherits="Rock.Web.Blocks.MyBlock" %>

<p>MY BLOCK CONTROL</p>
<%= ThemePath %>
<asp:literal ID="lPersonName" runat="server"></asp:literal><br />
<asp:literal ID="lBlockDetails" runat="server"></asp:literal><br />
<asp:literal ID="lBlockTime" runat="server"></asp:literal><br />
<asp:literal ID="lItemCache" runat="server"></asp:literal><br />
<asp:Literal ID="lItemTest" runat="server"></asp:Literal><br />
<asp:Literal ID="lParentGroups" runat="server"></asp:Literal><br />
<asp:Literal ID="lChildGroups" runat="server"></asp:Literal><br />
<asp:UpdatePanel ID="pnlAttributeValues" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlAttribute" runat="server">
            This DIV's bgcolor is from an attribute value <br />
            Best sci-fi movie: <asp:Literal ID="lMovie" runat="server"></asp:Literal>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Button ID="bFlushItemCache" runat="server" Text="Flush Item Cache" onclick="bFlushItemCache_Click" />