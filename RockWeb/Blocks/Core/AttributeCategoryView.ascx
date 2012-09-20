<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeCategoryView.ascx.cs" Inherits="RockWeb.Blocks.Core.AttributeCategoryView" %>

    <section class="attribute-group">
        <header><%=_category%> <a href="#" class="edit"><i class="icon-edit"></i></a></header>
        <ul id="ulAttributes" runat="server"></ul>
    </section>

