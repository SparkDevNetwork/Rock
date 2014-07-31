<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionList" %>

<asp:UpdatePanel ID="upFinancialGivingProfile" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Scheduled Contributions List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive" Text="Yes" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="rGridGivingProfile" AllowSorting="false" runat="server" EmptyDataText="No Scheduled Transactions Found" 
                        ShowConfirmDeleteDialog="true" RowItemText="Scheduled Transaction" OnRowSelected="rGridGivingProfile_Edit">
                        <Columns>
                            <asp:BoundField DataField="AuthorizedPerson" HeaderText="Contributor" />
                            <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:C2}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <asp:BoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />
                            <Rock:DateField DataField="StartDate" HeaderText="Starting" />
                            <Rock:DateField DataField="EndDate" HeaderText="Ending" />
                            <asp:BoundField DataField="NumberOfPayments" HeaderText="# Payments" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"/>
                            <Rock:DateField DataField="NextPaymentDate" HeaderText="Next Payment" />
                            <asp:BoundField DataField="TransactionCode" HeaderText="Transaction Code" />
                            <asp:BoundField DataField="CurrencyTypeValue.Name" HeaderText="Payment Method" />
                            <asp:BoundField DataField="CreditCardTypeValue.Name" HeaderText="Card Type" />
                            <asp:BoundField DataField="GatewayScheduleId" HeaderText="Schedule ID" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DateField DataField="LastStatusUpdateDateTime" HeaderText="Last Update" />
                            <Rock:DeleteField OnClick="rGridGivingProfile_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        

    </ContentTemplate>
</asp:UpdatePanel>





