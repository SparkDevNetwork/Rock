<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGrid.ascx.cs" Inherits="RockWeb.Blocks.TestGrid" %>

<Rock:Grid ID="rGrid" runat="server" Width="500px" Height="200px" 
EnableEdit="true" CssClass="data-grid" AsyncEditorLoading="true" EnablePaging="true" EnableOrdering="true" style="position:relative">
    <Rock:GridIntegerColumn DataField="Id" HeaderText="Page ID" Width="80" UniqueIdentifier="true" />
    <Rock:GridColumn DataField="Name" HeaderText="Page Name" CanEdit="true" Width="120" />
    <Rock:GridColumn DataField="Description" HeaderText="Description" CanEdit="true" Width="220" />
    <Rock:GridBoolColumn DataField="System" HeaderText="System" CanEdit="true" Width="80" />
</Rock:Grid>

