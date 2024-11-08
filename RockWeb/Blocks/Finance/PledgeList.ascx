<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeList" %>

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
                            <Rock:RockCheckBox ID="cbFilterActiveOnly" runat="server" Checked="false" Label="Active Only" Help="When checked, this will include only those pledges whose active period encompasses the current date, meaning their start date has passed and their end date has not yet been reached." />
                            <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
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

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title">Total Results</h1>
                            <div class="panel-labels">
                            </div>
                        </div>
                        <div class="panel-body">
                            <asp:Repeater ID="rptAccountSummary" runat="server">
                                <ItemTemplate>
                                    <div class='row'>
                                        <div class='col-xs-8'><%#Eval("Name")%></div>
                                        <div class='col-xs-4 text-right'><%#Eval("TotalAmount")%></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <div class='row'>
                                <div class='col-xs-8'><b>Total: </div>
                                <div class='col-xs-4 text-right'>
                                    <asp:Literal ID="lGrandTotal" runat="server" /></b>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
