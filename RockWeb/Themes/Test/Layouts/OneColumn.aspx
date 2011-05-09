<%@ Page Title="" Language="C#" MasterPageFile="~/Themes/Test/Layouts/Site.Master" AutoEventWireup="true" CodeFile="OneColumn.aspx.cs" Inherits="Rock.Themes.Test.Layouts.OneColumn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
    <div class="one-column-layout">
        <asp:Panel ID="Heading" runat="server" class="header"></asp:Panel>
        <asp:Panel ID="Content" runat="server" class="one-column-content"></asp:Panel>
        <asp:Panel ID="Footer" runat="server" class="footer"></asp:Panel>
    </div>
</asp:Content>
