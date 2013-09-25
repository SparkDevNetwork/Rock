<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.PledgeDetail" %>

<asp:UpdatePanel ID="upPledgeDetails" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="False">
            <asp:HiddenField ID="hfPledgeId" runat="server" />

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
        
            <fieldset>
                <legend><asp:Literal ID="lActionTitle" runat="server"/></legend>
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            
                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="True"/>
                <Rock:AccountPicker ID="fpFund" runat="server" Label="Fund" Required="True"/>
            
                <Rock:DataTextBox ID="tbAmount" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="TotalAmount" Label="Total Amount" PrependText="$" Required="True" />
                <Rock:DatePicker ID="dtpStartDate" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="StartDate" Label="Start Date" Required="True"/>
                <Rock:DatePicker ID="dtpEndDate" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="EndDate" Label="End Date" Required="True"/>
                <Rock:DataDropDownList ID="ddlFrequencyType" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="PledgeFrequencyValue" Label="Payment Schedule" Required="True"/>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>