<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSearch" %>

<Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found">
    <Columns>
        <asp:HyperLinkField 
            DataNavigateUrlFields="Id" 
            DataNavigateUrlFormatString="~/person/{0}"
            DataTextField="FullNameLastFirst" 
            SortExpression="FullNameLastFirst" 
            HeaderText="Person"  />
        <asp:BoundField
            DataField="Age"  
            HeaderText="Age"  />
        <asp:BoundField             
            HeaderText="Spouse"  />
        <Rock:DefinedValueField
            DataField="PersonStatusValueId"
            HeaderText="Member Status" />
        <Rock:DefinedValueField
            DataField="RecordStatusValueId"
            HeaderText="Record Status"  />
    </Columns>
</Rock:Grid>


