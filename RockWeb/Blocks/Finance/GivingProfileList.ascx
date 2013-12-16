<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileList.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileList" %>

<asp:UpdatePanel ID="upFinancialGivingProfile" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="gfSettings" runat="server">
            <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive" Text="Yes" />
        </Rock:GridFilter>
        <Rock:Grid ID="rGridGivingProfile" AllowSorting="false" runat="server" EmptyDataText="No Scheduled Contributions Found" ShowConfirmDeleteDialog="true" OnRowSelected="rGridGivingProfile_Edit">
            <Columns>
                <asp:BoundField DataField="AuthorizedPerson" HeaderText="Contributor" />
                <asp:BoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />
                <asp:BoundField DataField="StartDate" HeaderText="Start Date" />
                <asp:BoundField DataField="EndDate" HeaderText="End Date" />
                <asp:BoundField DataField="NumberOfPayments" HeaderText="# Payments" />
                <asp:BoundField DataField="GatewayEntityTypeId" HeaderText="Gateway" />
                <asp:BoundField DataField="TransactionCode" HeaderText="Transaction Code" />
                <asp:BoundField DataField="GatewayScheduleId" HeaderText="Schedule ID" />
                <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                <asp:BoundField DataField="CardReminderDate" HeaderText="Expiration Date" />
                <Rock:DeleteField OnClick="rGridGivingProfile_Delete" />
            </Columns>

        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>





