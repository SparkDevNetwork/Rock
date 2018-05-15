<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CCVScheduledTransactionGrid.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.CCVScheduledTransactionGrid" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>

        <div class="transaction-alerts">

            <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>

        </div>
        
        <div class="grid grid-panel">
            <Rock:Grid ID="gScheduledTransactions" DisplayType="Light" runat="server" AutoGenerateColumns="False" AllowSorting="false" AllowPaging="false" RowItemText="Scheduled Transaction" ClientIDMode="Static" >
                <Columns>
                    <Rock:RockBoundField DataField="GatewayScheduleId" HeaderText="Gateway Schedule Id" />
                    <Rock:RockBoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:C}"/>
                    <Rock:RockBoundField DataField="CurrencyTypeValue" HeaderText="Payment Type"/>
                    <Rock:RockBoundField DataField="AccountNumberMasked" HeaderText="Payment Account" />
                    <Rock:RockBoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />                    
                    <Rock:RockBoundField DataField="NextPaymentDate" HeaderText="Next Gift" DataFormatString="{0:MM/d/yyyy}" />
                    <Rock:RockBoundField DataField="StartDate" HeaderText="First Gift" DataFormatString="{0:MM/d/yyyy}" />
                    <Rock:DeleteField OnClick="gScheduledTransactions_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>