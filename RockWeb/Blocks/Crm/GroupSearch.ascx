<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.GroupSearch" %>

<Rock:Grid ID="gGroups" runat="server" EmptyDataText="No Groups Found">
    <Columns>
        <asp:HyperLinkField 
            DataNavigateUrlFields="Id" 
            DataNavigateUrlFormatString="~/Group/{0}"
            DataTextField="Name" 
            SortExpression="Name" 
            HeaderText="Group"  />
    </Columns>
</Rock:Grid>


