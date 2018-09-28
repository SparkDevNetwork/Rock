<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionView.ascx.cs" Inherits="RockWeb.Blocks.Finance.ScheduledTransactionView" %>

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
                            </div>
                            <div class="col-xs-6">
                                <asp:Literal ID="lDetailsRight" runat="server" />
                                <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"><i class="fa fa-refresh"></i> Refresh Now</asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">

                        <asp:Panel ID="pnlViewAccounts" runat="server" CssClass="clearfix">
                            <label>Accounts</label>
                            <Rock:Grid ID="gAccountsView" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" ShowHeader="false">
                                <Columns>
                                    <Rock:RockTemplateField>
                                        <ItemTemplate><%# AccountName( (int)Eval("AccountId") ) %></ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                    <Rock:CurrencyField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                </Columns>
                            </Rock:Grid>
                            <div class="actions pull-right">
                                <asp:LinkButton ID="lbChangeAccounts" runat="server" Text="Change Account Allocation" CssClass="btn btn-link btn-xs" CausesValidation="false" OnClick="lbChangeAccounts_Click" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlEditAccounts" runat="server" Visible="false" CssClass="clearfix">
                            <div class="grid">
                                <Rock:Grid ID="gAccountsEdit" runat="server" EmptyDataText="No Account Details" RowItemText="Account" DisplayType="Light" ShowHeader="false"
                                    ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockTemplateField>
                                            <ItemTemplate><%# AccountName( (int)Eval("AccountId") ) %></ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField DataField="Summary" SortExpression="Summary" />
                                        <Rock:CurrencyField DataField="Amount" SortExpression="Amount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"/>
                                        <Rock:EditField OnClick="gAccountsEdit_EditClick" />
                                        <Rock:DeleteField OnClick="gAccountsEdit_DeleteClick" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <div class="actions pull-right">
                                <asp:LinkButton ID="lbSaveAccounts" runat="server" Text="Save" CssClass="btn btn-primary btn-xs" OnClick="lbSaveAccounts_Click" />
                                <asp:LinkButton ID="lbCancelAccounts" runat="server" Text="Cancel" CssClass="btn btn-link btn-xs" OnClick="lbCancelAccounts_Click" />
                            </div>
                        </asp:Panel>

                        <dl>
                            <asp:Repeater ID="rptrNotes" runat="server">
                                <ItemTemplate>
                                    <dt><%# Eval("Caption") %></dt>
                                    <dd><%# Eval("Text") %>
                                        <small>- <%# Eval("Person") %> on <%# Eval("Date") %> at <%# Eval("Time") %></small>
                                    </dd>
                                </ItemTemplate>
                            </asp:Repeater>
                        </dl>

                    </div>
                </div>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbUpdate" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbUpdate_Click" />
                    <asp:LinkButton ID="lbCancelSchedule" runat="server" Text="Cancel Schedule" CssClass="btn btn-danger js-cancel-txn" CausesValidation="false" OnClick="lbCancelSchedule_Click" Visible="false" />
                    <asp:LinkButton ID="lbReactivateSchedule" runat="server" Text="Reactivate Schedule" CssClass="btn btn-success js-reactivate-txn" CausesValidation="false" OnClick="lbReactivateSchedule_Click" Visible="false" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" />
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

    </ContentTemplate>
</asp:UpdatePanel>
