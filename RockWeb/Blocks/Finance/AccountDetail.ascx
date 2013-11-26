<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountDetail" %>

<asp:UpdatePanel ID="pnlAccountListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
           
            <asp:HiddenField ID="hfAccountId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lActionTitle" runat="server"></asp:Literal>
                </h1>
                <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

            <div id="pnlViewDetails" runat="server">
                <p class="description">
                    <asp:Literal ID="lAccountDescription" runat="server"></asp:Literal>
                </p>

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

            </div>

            <div id="pnlEditDetails" runat="server">

                <asp:ValidationSummary ID="valAccountDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server"
                            SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server"
                            SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6"> 
                        <Rock:AccountPicker ID="apParentAccount" runat="server" Label="Parent Account" />
                        <Rock:RockDropDownList ID="ddlAccountType" runat="server" Label="Account Type" />
                        <Rock:DataTextBox ID="tbPublicName" runat="server"
                            SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="PublicName" />
                    </div>
                    <div class="col-md-6">                
                        <Rock:DataTextBox ID="tbGLCode" runat="server"
                            SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="GlCode" />
                        <Rock:DatePicker ID="dtpStartDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="StartDate" Label="Start Date" />
                        <Rock:DatePicker ID="dtpEndDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="EndDate" Label="End Date" />
                        <Rock:RockCheckBox ID="cbIsTaxDeductible" runat="server" Label="Tax Deductible" />           
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
