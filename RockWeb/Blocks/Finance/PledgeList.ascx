﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-list"></i> Pledge List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfPledges" runat="server">
                            <Rock:PersonPicker ID="ppFilterPerson" runat="server" Label="Filter by Person" IncludeBusinesses="true" />
                            <Rock:AccountPicker ID="apFilterAccount" runat="server" Label="Filter by Account" AllowMultiSelect="True" />
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                            <Rock:DateRangePicker ID="drpLastModifiedDates" runat="server" Label="Last Modified" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" ExportSource="ColumnOutput" AllowSorting="True" AllowPaging="True">
                            <Columns>
                                <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                <Rock:RockBoundField DataField="Group.Name" HeaderText="For" SortExpression="Group.Name" />
                                <Rock:RockBoundField DataField="Account.Name" HeaderText="Account" SortExpression="Account.Name" />
                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
                                <Rock:DefinedValueField DataField="PledgeFrequencyValueId" HeaderText="Payment Schedule" SortExpression="PledgeFrequencyValue.Value" />
                                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate" />
                                <Rock:DateField DataField="EndDate" HeaderText="Ends" SortExpression="EndDate" />
                                <Rock:DateField DataField="ModifiedDateTime" HeaderText="Last Modified" SortExpression="ModifiedDateTime" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
