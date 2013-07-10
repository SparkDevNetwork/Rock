<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Accounts.ascx.cs" Inherits="RockWeb.Blocks.Finance.Accounts" %>
<asp:UpdatePanel ID="pnlAccountListUpdatePanel" runat="server">
<ContentTemplate>
    <asp:Panel ID="pnlAccountList" runat="server">
        <h4>Financial Accounts</h4>
        <Rock:GridFilter ID="rAccountFilter" runat="server" OnApplyFilterClick="rAccountFilter_ApplyFilterClick">
            <Rock:LabeledTextBox ID="txtAccountName" runat="server" LabelText="Name" />
            <Rock:DatePicker ID="dtStartDate" runat="server" LabelText="From Start Date" />
            <Rock:DatePicker ID="dtEndDate" runat="server" LabelText="To End Date" />
            <Rock:DataDropDownList ID="ddlIsActive" runat="server" LabelText="Account Status">
                <asp:ListItem Text="Any" Value="Any" />
                <asp:ListItem Text="Active" Value="True" />
                <asp:ListItem Text="Inactive" Value="False" />
            </Rock:DataDropDownList>
            <Rock:DataDropDownList ID="ddlIsTaxDeductible" runat="server" LabelText="Tax Deductible">
                <asp:ListItem Text="Any" Value="Any" />
                <asp:ListItem Text="Yes" Value="True" />
                <asp:ListItem Text="No" Value="False" />
            </Rock:DataDropDownList>
        </Rock:GridFilter>
        
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="rGridAccount" runat="server" AllowSorting="true" RowItemText="account" OnRowSelected="rGridAccount_Edit">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="PublicName" HeaderText="Public Name" SortExpression="PublicName" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="IsTaxDeductible" HeaderText="Tax Deductible" SortExpression="IsTaxDeductible" />
                <asp:BoundField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                <asp:BoundField DataField="StartDate" HeaderText="Starts On" SortExpression="StartDate" DataFormatString="{0:d}" />
                <asp:BoundField DataField="EndDate" HeaderText="Ends On" SortExpression="EndDate" DataFormatString="{0:d}" />
                <Rock:DeleteField OnClick="rGridAccount_Delete" />
            </Columns>
        </Rock:Grid>
    </asp:Panel>

    <asp:Panel ID="pnlAccountDetails" runat="server" Visible="false">

        <asp:HiddenField ID="hfAccountId" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />
        <fieldset>
            <legend>
                <asp:Literal ID="lAction" runat="server">Account</asp:Literal>
            </legend>

            <div class="row-fluid">

                <div class="span6">
                    <Rock:DataTextBox ID="tbName" runat="server"
                        SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbPublicName" runat="server"
                        SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="PublicName" />
                    <Rock:DataTextBox ID="tbDescription" runat="server"
                        SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    <Rock:DataTextBox ID="tbOrder" runat="server"
                        SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Order" />
                    <Rock:DataTextBox ID="tbGLCode" runat="server"
                        SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="GlCode" />
                </div>

                <div class="span6">                
                    <Rock:DatePicker ID="dtpStartDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="StartDate" LabelText="Start Date" />
                    <Rock:DatePicker ID="dtpEndDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="EndDate" LabelText="End Date" />
                    <Rock:LabeledCheckBox ID="cbIsActive" runat="server" LabelText="Active" />
                    <Rock:LabeledCheckBox ID="cbIsTaxDeductible" runat="server" LabelText="Tax Deductible" />
                    <Rock:LabeledDropDownList ID="ddlAccountType" runat="server" LabelText="Account Type" />
                    <Rock:AccountPicker ID="apParentAccount" runat="server" LabelText="Parent Account" />                
                </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSaveAccount" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveAccount_Click" />
            <asp:LinkButton ID="btnCancelAccount" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
