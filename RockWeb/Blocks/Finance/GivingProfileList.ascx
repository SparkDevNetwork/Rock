<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileList.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileList" %>

<asp:UpdatePanel ID="upFinancialGivingProfile" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="rFBFilter" runat="server">
            <Rock:DateTimePicker ID="dtpGivingProfileDate" runat="server" SourceTypeName="Rock.Model.FinancialScheduledTransaction, Rock" PropertyName="StartDate" LabelText="Date" />
        </Rock:GridFilter>

        <Rock:Grid ID="rGridGivingProfile" runat="server" EmptyDataText="No Scheduled Contributions Found" OnRowDataBound="rGridGivingProfile_RowDataBound"
             ShowConfirmDeleteDialog="true"  OnRowSelected="rGridGivingProfile_Edit">
            <Columns>
                
                <asp:BoundField DataField="Id" HeaderText="Id"  />
                <asp:BoundField DataField="AuthorizedPerson" HeaderText="Contributor" />
                <asp:BoundField DataField="TransactionFrequencyValue" HeaderText="Frequency" />
                <asp:TemplateField HeaderText="Start Date">
                    <ItemTemplate>
                        <span><%# Eval("StartDate") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
              <asp:BoundField DataField="EndDate" HeaderText="End Date" />
                
              <asp:BoundField DataField="NumberOfPayments" HeaderText="# Payments"  />
                
              <asp:BoundField DataField="GatewayId" HeaderText="Gateway"  />
                
              <asp:BoundField DataField="TransactionCode" HeaderText="Transaction Code"  />
              
                <asp:BoundField DataField="IsActive" HeaderText="Active" />

              <asp:BoundField DataField="CardReminderDate" HeaderText="Expiration Date"  />
                
              <Rock:DeleteField OnClick="rGridGivingProfile_Delete" />

            </Columns>

        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>





