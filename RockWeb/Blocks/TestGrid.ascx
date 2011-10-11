<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGrid.ascx.cs" Inherits="RockWeb.Blocks.TestGrid" %>

<%--<RockGrid:Table ID="rGrid" runat="server" Title="Pages" Width="500px" Height="200px" 
CssClass="data-grid" EnablePaging="true" EnableOrdering="true" style="position:relative" IdentityColumn="Id">
    <RockGrid:HyperlinkColumn DataField="Name" DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlField="Id" HeaderText="Page Name" Width="130" />
    <RockGrid:Column DataField="Description" HeaderText="Description" Width="220" />
    <RockGrid:BoolColumn DataField="System" HeaderText="System" Width="80" />
    <RockGrid:DeleteColumn HeaderText="Delete" />
</RockGrid:Table>--%>

<Rock:Grid ID="rGrid" runat="server" ShowFooter="true" DataKeyNames="id" onrowdeleting="rGrid_RowDeleting" EnableOrdering="true" EnableClientSorting="false" >
    <Columns>
        <asp:HyperLinkField DataTextField="Name" DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlFields="Id" HeaderText="Page Name" />
        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
        <Rock:BoolField DataField="System" HeaderText="System" SortExpression="System"></Rock:BoolField>
        <Rock:DeleteField/>
    </Columns>
</Rock:Grid>
