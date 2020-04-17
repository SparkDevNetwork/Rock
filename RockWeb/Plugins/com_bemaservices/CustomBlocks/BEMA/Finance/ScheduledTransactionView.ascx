<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionView.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Finance.ScheduledTransactionView" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i>Scheduled Transaction Details
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-xs-6">
                                <asp:Literal ID="lDetailsLeft" runat="server" />

                                <%/* BEMA.FE1.Start */ %>
                                <asp:Literal ID="lCancelationInformation" runat="server" />
                                <%/* BEMA.FE1.End */ %>

                            </div>
                            <div class="col-xs-6">
                                <asp:Literal ID="lDetailsRight" runat="server" />
                                <asp:LinkButton ID="btnRefresh" runat="server" OnClick="btnRefresh_Click"><i class="fa fa-refresh"></i> Refresh Now</asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <asp:Panel ID="pnlViewAccounts" runat="server" CssClass="clearfix">
                            <label>Accounts</label>
                            <Rock:Grid ID="gAccountsView" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" ShowHeader="false">
                                <Columns>
                                    <Rock:RockLiteralField ID="lAccountsViewAccountName" OnDataBound="lAccountName_DataBound" />
                                    <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                    <Rock:CurrencyField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                </Columns>
                            </Rock:Grid>
                            <div class="actions pull-right">
                                <asp:LinkButton ID="btnChangeAccounts" runat="server" Text="Change Account Allocation" CssClass="btn btn-link btn-xs" CausesValidation="false" OnClick="btnChangeAccounts_Click" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlEditAccounts" runat="server" Visible="false" CssClass="clearfix">
                            <div class="grid">
                                <Rock:Grid ID="gAccountsEdit" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" ShowHeader="false"
                                    ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockLiteralField ID="lAccountsEditAccountName" OnDataBound="lAccountName_DataBound" />
                                        <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                        <Rock:CurrencyField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"/>
                                        <Rock:EditField OnClick="gAccountsEdit_EditClick" />
                                        <Rock:DeleteField OnClick="gAccountsEdit_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <div class="actions pull-right">
                                <asp:LinkButton ID="btnSaveAccounts" runat="server" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="btnSaveAccounts_Click" />
                                <asp:LinkButton ID="btnCancelAccounts" runat="server" Text="Cancel" CssClass="btn btn-link btn-xs" OnClick="btnCancelAccounts_Click" />
                            </div>
                        </asp:Panel>

                        <%/* BEMA.FE2.Start */ %>
                        <div class="row">
							<div class="col-md-6">
                                <Rock:RockLiteral ID="lSummary" runat="server" Label="Summary" />
                            </div>
                            <div class="col-md-6">
                                <asp:Literal Id="lEntityInfo" runat="server" />
                            </div>
                        </div>
                        <%/* BEMA.FE2.End */ %>

                    </div>
                </div>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="btnUpdate" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnUpdate_Click" />
                    <asp:LinkButton ID="btnCancelSchedule" runat="server" Text="Cancel Schedule" CssClass="btn btn-danger js-cancel-txn" CausesValidation="false" OnClick="btnCancelSchedule_Click" Visible="false" />
                    <asp:LinkButton ID="btnReactivateSchedule" runat="server" Text="Reactivate Schedule" CssClass="btn btn-success js-reactivate-txn" CausesValidation="false" OnClick="btnReactivateSchedule_Click" Visible="false" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                </div>

                <Rock:NotificationBox ID="nbError" CssClass="margin-t-lg" runat="server" Visible="false" NotificationBoxType="Danger" Dismissable="true" />

            </div>

        </div>


        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdAccount" runat="server" Title="Account" SaveButtonText="OK" OnSaveClick="mdAccount_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Account">
            <Content>
                <asp:ValidationSummary ID="valSummaryAccount" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Account" />
                <asp:HiddenField ID="hfAccountGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" Required="true" ValidationGroup="Account" />
                        <Rock:CurrencyBox ID="tbAccountAmount" runat="server" Label="Amount" Required="true" ValidationGroup="Account" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbAccountSummary" runat="server" Label="Summary" TextMode="MultiLine" Rows="3" ValidationGroup="Account" />
                        <div class="attributes">
                            <asp:PlaceHolder ID="phAccountAttributeEdits" runat="server" />
                        </div>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <%/* BEMA.FE1.Start */ %>
        <Rock:ModalDialog ID="mdCancelSchedule" runat="server" Title="Cancel Schedule" SaveButtonText="Save" OnSaveClick="mdCancelSchedule_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="CancelSchedule">
            <Content>
                <asp:ValidationSummary ID="valCancelSchedule" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="CancelSchedule" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbCancelReason" runat="server" Label="Reason For Cancelation" TextMode="MultiLine" Rows="3" ValidationGroup="CancelSchedule" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
        <%/* BEMA.FE1.End */ %>
    </ContentTemplate>
</asp:UpdatePanel>
