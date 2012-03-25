<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGrid.ascx.cs" Inherits="RockWeb.Blocks.TestGrid" %>
<script type="text/javascript">
    function addItem() {
        alert("Adding Item");
        return false;
    }
</script>

<Rock:Grid ID="rGrid" runat="server" Caption="Listing of the Top Pages in Rock" >
    <Columns>
        <asp:HyperLinkField DataTextField="Name" DataNavigateUrlFormatString="~/page/{0}" DataNavigateUrlFields="Id" HeaderText="Page Name" />
        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
        <Rock:BoolField DataField="System" HeaderText="System" SortExpression="System"></Rock:BoolField>
        <Rock:DeleteField/>
    </Columns>
</Rock:Grid>
