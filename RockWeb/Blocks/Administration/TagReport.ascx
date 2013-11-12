<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagReport.ascx.cs" Inherits="RockWeb.Blocks.Administration.TagReport" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <h4><asp:Literal ID="lTaggedTitle" runat="server"></asp:Literal></h4>
            <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" OnRowSelected="gReport_RowSelected" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
