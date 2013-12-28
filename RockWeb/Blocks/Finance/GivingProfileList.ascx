<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileList.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileList" %>

<asp:UpdatePanel ID="upFinancialGivingProfile" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="gfSettings" runat="server">
            <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive" Text="Yes" />
        </Rock:GridFilter>
        <Rock:Grid ID="rGridGivingProfile" AllowSorting="false" runat="server" EmptyDataText="No Scheduled Transactions Found" 
            ShowConfirmDeleteDialog="true" RowItemText="Scheduled Transaction" OnRowSelected="rGridGivingProfile_Edit">
            <Columns>
                <asp:BoundField DataField="AuthorizedPerson" HeaderText="Contributor" />
                <asp:BoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />
                <Rock:DateField DataField="StartDate" HeaderText="Starting" />
                <Rock:DateField DataField="EndDate" HeaderText="Ending" />
                <asp:BoundField DataField="NumberOfPayments" HeaderText="# Payments" />
                <Rock:DateField DataField="NextPaymentDate" HeaderText="Next Payment" />
                <asp:BoundField DataField="TransactionCode" HeaderText="Transaction Code" />
                <asp:BoundField DataField="GatewayScheduleId" HeaderText="Schedule ID" />
                <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                <Rock:DateField DataField="LastStatusUpdateDateTime" HeaderText="Last Update" />
                <Rock:DeleteField OnClick="rGridGivingProfile_Delete" />
            </Columns>

        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>





