<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMerge.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMerge" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:Grid ID="gMerge" runat="server" AllowSorting="false" EmptyDataText="No Results" />
        <asp:LinkButton ID="lbGo" runat="server" Text="Go" OnClick="lbGo_Click" />

        <asp:TextBox ID="tbTest" runat="server" ></asp:TextBox>
    </ContentTemplate>
</asp:UpdatePanel>
