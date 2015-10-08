<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Pledge List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfPledges" runat="server">
                        <Rock:PersonPicker ID="ppFilterPerson" runat="server" Label="Filter by person" IncludeBusinesses="true"/>
                        <Rock:AccountPicker ID="apFilterAccount" runat="server" Label="Filter by account" AllowMultiSelect="True"/>
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" AllowSorting="True" AllowPaging="True" OnRowSelected="gPledges_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName"/>
                            <Rock:RockBoundField DataField="Account" HeaderText="Account" SortExpression="AccountId"/>
                            <Rock:CurrencyField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
                            <Rock:RockBoundField DataField="PledgeFrequencyValue" HeaderText="Payment Schedule" SortExpression="PledgeFrequencyValue" />
                            <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                            <Rock:DateField DataField="EndDate" HeaderText="Ends" SortExpression="EndDate"/>
                            <Rock:DeleteField OnClick="gPledges_Delete"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>