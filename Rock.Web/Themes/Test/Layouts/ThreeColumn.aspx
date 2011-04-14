<%@ Page Title="" Language="C#" MasterPageFile="~/Themes/Test/Layouts/Site.Master" AutoEventWireup="true" CodeBehind="ThreeColumn.aspx.cs" Inherits="Rock.Themes.Test.Layouts.ThreeColumn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="main" runat="server">
    <div class="three-column-layout">
        <asp:Panel ID="Heading" runat="server" class="header"></asp:Panel>
        <asp:Panel ID="FirstColumn" runat="server" class="first-column"></asp:Panel>
        <asp:Panel ID="SecondColumn" runat="server" class="second-column"></asp:Panel>
        <asp:Panel ID="ThirdColumn" runat="server" class="third-column"></asp:Panel>
        <asp:Panel ID="Footer" runat="server" class="footer"></asp:Panel>
    </div>
</asp:Content>
