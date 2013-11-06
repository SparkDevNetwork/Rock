<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:PersonPicker ID="ppFilterPerson" runat="server" Label="Filter by person"/>
            <Rock:AccountPicker ID="fpFilterAccount" runat="server" Label="Filter by account" AllowMultiSelect="True"/>
        </Rock:GridFilter>
        <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" AllowSorting="True" AllowPaging="True" OnRowSelected="gPledges_Edit">
            <Columns>
                <asp:BoundField DataField="Person" HeaderText="Person" SortExpression="PersonId"/>
                <asp:BoundField DataField="Account" HeaderText="Account" SortExpression="AccountId"/>
                <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" DataFormatString="{0:C}"/>
                <asp:BoundField DataField="PledgeFrequencyValue" HeaderText="Payment Schedule" SortExpression="PledgeFrequencyValue" />
                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                <Rock:DeleteField OnClick="gPledges_Delete"/>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>