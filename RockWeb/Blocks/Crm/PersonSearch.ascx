<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSearch" %>

<Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected">
    <Columns>
        <asp:BoundField 
            DataField="FullNameLastFirst"  
            HeaderText="Person"  
            SortExpression="LastName,FirstName"  />
        <asp:BoundField 
            ItemStyle-HorizontalAlign="Right" 
            DataField="Age"  
            HeaderText="Age"  
            SortExpression="BirthYear,BirthMonth,BirthDay"  />
        <Rock:DefinedValueField
            DataField="ConnectionStatusValueId"
            HeaderText="Connection Status"
            SortExpression="ConnectionStatusValue.Name" />
        <Rock:DefinedValueField
            DataField="RecordStatusValueId"
            HeaderText="Record Status"
            SortExpression="RecordStatusValue.Name"  />
    </Columns>
</Rock:Grid>


