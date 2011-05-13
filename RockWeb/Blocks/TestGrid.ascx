<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGrid.ascx.cs" Inherits="RockWeb.Blocks.TestGrid" %>

<Rock:Grid ID="rGrid" runat="server" Title="Pages" Width="500px" Height="200px" 
EnableEdit="true" CssClass="data-grid" AsyncEditorLoading="true" EnablePaging="true" 
EnableOrdering="true" style="position:relative" IdColumnName="Id">
    <Rock:GridColumn DataField="Id" Visible="false" />
    <Rock:GridHyperlinkColumn DataField="Name" DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlField="Id" HeaderText="Page Name" Width="130" />
    <Rock:GridColumn DataField="Description" HeaderText="Description" CanEdit="true" Width="220" />
    <Rock:GridBoolColumn DataField="System" HeaderText="System" CanEdit="true" Width="80" />
</Rock:Grid>

