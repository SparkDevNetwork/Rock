<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmbedTableau.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Tableau.EmbedTableau" %>
 
<Rock:ModalAlert ID="TableauWarning" runat="server" />

<script type='text/javascript' src='<%= JavaScriptLocation %>'></script>

<div class='tableauPlaceholder' style='width: 944px; height: 1636px;'>
    <object class='tableauViz' width='<%= Width %>' height='<%= Height %>' style='display:none;'>
        <param name='host_url' value='<%= TableauServer %>//' />
        <param name='site_root' value='' />
        <param name='name' value='<%= VisualationName %>' />
        <param name='tabs' value='no' />
        <param name='toolbar' value='yes' />
        <param name='ticket' value='<%= responseFromServer %>' />
        <param name='showVizHome' value='n' /></object></div>