<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:PersonPicker ID="ppFilterPerson" runat="server" LabelText="Filter by person"/>
            <Rock:AccountPicker ID="fpFilterAccount" runat="server" LabelText="Filter by account"/>
        </Rock:GridFilter>
        <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" AllowSorting="True" AllowPaging="True" OnRowSelected="gPledges_Edit">
            <Columns>
                <asp:BoundField DataField="Person" HeaderText="Person" SortExpression="PersonId"/>
                <asp:BoundField DataField="Account" HeaderText="Account" SortExpression="AccountId"/>
                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" DataFormatString="{0:C}"/>
                <asp:TemplateField HeaderText="Payment Schedule">
                    <ItemTemplate>
                         <%# string.Format( "{0:C}", Eval( "FrequencyAmount" ) ) %> <%# Eval( "FrequencyTypeValue" ) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                <Rock:DeleteField OnClick="gPledges_Delete"/>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>