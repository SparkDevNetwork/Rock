<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Scheduled Transaction List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive" Text="Yes" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gList" AllowSorting="false" runat="server" EmptyDataText="No Scheduled Transactions Found" 
                        ShowConfirmDeleteDialog="true" RowItemText="Scheduled Transaction" OnRowSelected="gList_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="AuthorizedPersonAlias.Person" HeaderText="Contributor" />
                            <Rock:RockBoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:C2}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />
                            <Rock:DateField DataField="StartDate" HeaderText="Starting" />
                            <Rock:DateField DataField="EndDate" HeaderText="Ending" />
                            <Rock:DateField DataField="NextPaymentDate" HeaderText="Next Payment" />
                            <Rock:DefinedValueField DataField="FinancialPaymentDetail.CurrencyTypeValueId" HeaderText="Currency Type" />
                            <Rock:RockBoundField DataField="TransactionCode" HeaderText="Transaction Code" />
                            <Rock:RockBoundField DataField="GatewayScheduleId" HeaderText="Schedule ID" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DateTimeField DataField="LastStatusUpdateDateTime" HeaderText="Last Refresh" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>





