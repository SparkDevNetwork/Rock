<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGrid.ascx.cs" Inherits="RockWeb.Blocks.TestGrid" %>

<Rock:Grid ID="rGrid" runat="server" Width="500px" Height="200px" Editable="true" class="data-grid">
    <Rock:GridColumn DataField="Id" HeaderText="Page ID" Width="80" />
    <Rock:GridColumn DataField="Name" HeaderText="Page Name" CanEdit="true" Width="120" />
    <Rock:GridColumn DataField="Description" HeaderText="Description" Width="220" />
    <Rock:GridBoolColumn DataField="System" HeaderText="System" CanEdit="true" Width="80" />
</Rock:Grid>

