<%@ Page Title="" Language="C#" MasterPageFile="~/Themes/Test/Layouts/Site.Master" AutoEventWireup="true" CodeFile="TwoColumn.aspx.cs" Inherits="Rock.Themes.Test.Layouts.TwoColumn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
    <div class="two-column-layout">
        <asp:Panel ID="Heading" runat="server" class="header"></asp:Panel>
        <asp:Panel ID="FirstColumn" runat="server" class="first-column"></asp:Panel>
        <asp:Panel ID="SecondColumn" runat="server" class="second-column"></asp:Panel>
        <asp:Panel ID="Footer" runat="server" class="footer"></asp:Panel>
    </div>
</asp:Content>
