<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSearch" %>

<Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true">
    <Columns>
        <asp:HyperLinkField 
            DataNavigateUrlFields="Id" 
            DataNavigateUrlFormatString="~/person/{0}"
            DataTextField="FullNameLastFirst" 
            SortExpression="LastName,FirstName"
            HeaderText="Person"  />
        <asp:BoundField 
            HeaderStyle-HorizontalAlign="Right"
            ItemStyle-HorizontalAlign="Right" 
            DataField="Age"  
            HeaderText="Age"  
            SortExpression="BirthYear,BirthMonth,BirthDay"  />
        <Rock:DefinedValueField
            DataField="PersonStatusValueId"
            HeaderText="Member Status"
            SortExpression="PersonStatusValue.Name" />
        <Rock:DefinedValueField
            DataField="RecordStatusValueId"
            HeaderText="Record Status"
            SortExpression="RecordStatusValue.Name"  />
    </Columns>
</Rock:Grid>


