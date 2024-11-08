<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingConfiguration" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {

        $('.js-inactivate-scheduled-transaction').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmPreventOnCancel(event, 'Are you sure you want to inactivate this scheduled transaction?');
        });

        $('.js-delete-saved-account').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmDelete(event, 'saved account');
        });

        $('.js-delete-pledge').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmDelete(event, 'pledge');
        });
    });
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">
                        <i class="fa fa-clipboard-check"></i>
                        Giving Configuration</h1>
                </div>
                <div class="">
                    <Rock:ModalAlert ID="mdWarningAlert" runat="server" />
                    <div class="panel-body">
                        <asp:LinkButton ID="btnAddTransaction" runat="server" CssClass="btn btn-default btn-sm btn-block" Text="Add One-time Gift" OnClick="btnAddTransaction_Click" />
                    </div>

                    <div class="giving-text-to-give panel-body border-top border-panel">
                        <div class="row">
                            <div class="col-sm-10">
                                <h5 class="my-0">Text-To-Give Settings</h5>
                            </div>
                            <div class="col-sm-2">
                                <asp:LinkButton ID="btnEditTextToGive" runat="server" data-shortcut-key="e" ToolTip="Edit Text-To-Give Settings" Text="<i class='fa fa-pencil'></i>" CssClass="btn btn-default btn-square btn-xs pull-right" OnClick="btnEditTextToGive_Click" CausesValidation="false" />
                            </div>
                        </div>

                        <asp:Panel ID="pnlTextToGive" runat="server" Visible="false">
                            <div id="pnlTextToGiveView" runat="server">
                                <div class="row">
                                    <div class="col-md-6 col-sm-12">
                                        <strong class="d-block text-sm">Default Account</strong>
                                        <span class="d-block text-sm text-muted margin-b-sm"><asp:Literal ID="lTTGDefaultAccount" runat="server" /></span>
                                    </div>
                                    <div class="col-md-6 col-sm-12">
                                        <strong class="d-block text-sm">Saved Account</strong>
                                        <span class="d-block text-sm text-muted"><asp:Literal ID="lTTGSavedAccount" runat="server" /></span>
                                    </div>
                                </div>
                            </div>

                            <div id="pnlTextToGiveEdit" runat="server" visible="false">
                                <div class="row margin-t-sm">
                                    <div class="col-md-5 col-sm-12">
                                        <Rock:AccountPicker ID="apAccountPicker" runat="server" Label="Default Account" />
                                    </div>
                                    <div class="col-md-7 col-sm-12">
                                        <Rock:RockDropDownList ID="ddlSavedAccount" label="Saved Account" runat="server" DataTextField="Name" DataValueField="Id" />
                                    </div>
                                </div>

                                <div>
                                    <asp:LinkButton ID="btnSaveTextToGive" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-sm btn-primary" OnClick="btnSaveTextToGive_Click" />
                                    <asp:LinkButton ID="btnCanceTextToGive" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-sm btn-link" CausesValidation="false" OnClick="btnCanceTextToGive_Click" />
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlTextToGiveAddSettings" runat="server" Visible="false" CssClass="row">
                            <div class="col-sm-12">
                                <asp:LinkButton ID="lbTextToGiveAdd"
                                    runat="server"
                                    CssClass="btn btn-default btn-square btn-xs mt-3 pull-right"
                                    ToolTip="Add Text-To-Give Configuration"
                                    Text="<i class='fa fa-plus'></i>"
                                    OnClick="btnEditTextToGive_Click"
                                    CausesValidation="false" />
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="giving-scheduled panel-body border-top border-panel">
                        <div class="d-flex justify-content-between align-items-start">
                            <h5 class="my-0">Scheduled Transactions</h5>
                            <asp:LinkButton ID="btnShowInactiveScheduledTransactions" runat="server" CssClass="text-sm" Text="Show Inactive" OnClick="btnShowInactiveScheduledTransactions_Click" />
                        </div>
                        <asp:HiddenField ID="hfShowInactiveScheduledTransactions" runat="server" />
                        <table class="table table-inline">
                            <asp:Repeater ID="rptScheduledTransaction" runat="server" OnItemDataBound="rptScheduledTransaction_ItemDataBound">
                                <ItemTemplate>
                                    <tr runat="server" id="trScheduledTransaction" class="rollover-container-nested">
                                        <td class="pl-0 py-4">
                                            <strong class="d-block text-sm"><asp:Literal ID="lScheduledTransactionAccountName" runat="server" /></strong>
                                            <span class="d-block text-sm text-muted">
                                                <asp:Literal ID="lScheduledTransactionFrequencyAndNextPaymentDate" runat="server" />
                                            </span>
                                            <span class="d-block text-sm text-muted">
                                                <asp:Panel ID="pnlScheduledTransactionCreditCardInfo" runat="server" >
                                                    <asp:Literal ID="lScheduledTransactionCardTypeLast4" runat="server" /> <span class="o-30">|</span> <asp:Literal ID="lScheduledTransactionExpiration" runat="server" />
                                                </asp:Panel>
                                                <asp:Literal ID="lScheduledTransactionOtherCurrencyTypeInfo" runat="server" />
                                            </span>
                                            <span class="d-block text-sm text-muted">
                                                <asp:Literal ID="lScheduledTransactionSavedAccountName" runat="server" />
                                            </span>
                                        </td>

                                        <td class="w-1 text-right pr-0 py-4">
                                            <span class="d-block"><asp:Literal ID="lScheduledTransactionStatusHtml" runat="server" /></span>
                                            <span class="scheduled-total font-weight-semibold text-sm d-block">
                                                <asp:Literal ID="lScheduledTransactionTotalAmount" runat="server"/>
                                            </span>
                                            <div class="rollover-item">
                                                <div class="d-flex mt-3">
                                                    <asp:LinkButton ID="btnScheduledTransactionEdit" runat="server" OnCommand="rptScheduledTransaction_Edit" CssClass="btn btn-sm btn-link text-muted py-0 px-1">
                                                    <i class="fa fa-pencil"></i>
                                                    </asp:LinkButton>
                                                    <asp:LinkButton runat="server" ID="btnScheduledTransactionInactivate" OnCommand="rptScheduledTransaction_Inactivate" CssClass="btn btn-sm btn-link text-muted py-0 px-1 ml-1 js-inactivate-scheduled-transaction" Visible="true">
                                                        <i class="fa fa-times"></i>
                                                    </asp:LinkButton>
                                                </div>
                                            </div>
                                        </td>

                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                        <div class="row">
                            <div class="col-sm-12">
                                <asp:LinkButton ID="lbAddScheduledTransaction"
                                    runat="server"
                                    CssClass="btn btn-default btn-square btn-xs mt-3 pull-right"
                                    ToolTip="Add Scheduled Transaction"
                                    Text="<i class='fa fa-plus'></i>"
                                    OnClick="lbAddScheduledTransaction_Click" />
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlSavedAccounts" runat="server" CssClass="giving-saved-accounts panel-body border-top border-panel">
                        <h5 class="my-0">Saved Accounts</h5>
                        <table class="table table-inline table-hide-last-border">
                            <asp:Repeater ID="rptSavedAccounts" runat="server" OnItemDataBound="rptSavedAccounts_ItemDataBound">
                                <ItemTemplate>
                                    <tr class="rollover-container-nested">
                                        <td class="pl-0 py-4">
                                            <strong class="d-block text-sm"><asp:Literal ID="lSavedAccountName" runat="server" /></strong>
                                            <span class="d-block text-sm text-muted"><asp:Literal ID="lScheduledTransactionFrequency" runat="server"/>
                                                <asp:Panel ID="pnlSavedAccountCreditCardInfo" runat="server" >
                                                    <asp:Literal ID="lSavedAccountCardTypeLast4" runat="server" /> <span class="o-30">|</span> <asp:Literal ID="lSavedAccountExpiration" runat="server" />
                                                </asp:Panel>
                                                <asp:Literal ID="lSavedAccountOtherCurrencyTypeInfo" runat="server" />
                                            </span>
                                        </td>

                                        <td class="w-1 text-right pr-0 py-4">
                                            <span class="d-block"><asp:Literal ID="lSavedAccountStatusHtml" runat="server" /></span>
                                            <div class="rollover-item">
                                            <asp:LinkButton runat="server" ID="btnSavedAccountDelete" OnCommand="rptSavedAccounts_Delete" CssClass="btn btn-sm btn-link text-muted py-0 px-1 js-delete-saved-account">
                                                <i class="fa fa-times"></i>
                                            </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                    </asp:Panel>
                    <div class="giving-pledges panel-body border-top border-panel">
                        <h5 class="my-0">Pledges</h5>
                        <table class="table table-inline">
                            <asp:Repeater ID="rptPledges" runat="server" OnItemDataBound="rptPledges_ItemDataBound">
                                <ItemTemplate>
                                    <tr class="rollover-container-nested">
                                        <td class="pl-0 py-4">
                                            <strong class="d-block text-sm"><asp:Literal ID="lPledgeDate" runat="server" /></strong>
                                            <span class="d-block text-sm text-muted">
                                                <asp:Literal ID="lPledgeAccountName" runat="server" />
                                                <asp:Literal ID="lPledgeFrequency" runat="server" />
                                            </span>
                                        </td>

                                        <td class="w-1 text-right pr-0 py-4">
                                            <span class="scheduled-total font-weight-semibold text-sm d-block">
                                                <asp:Literal ID="lPledgeTotalAmount" runat="server" />
                                            </span>
                                            <div class="rollover-item">
                                                <div class="d-flex justify-content-end">
                                                    <asp:LinkButton runat="server" ID="btnPledgeEdit" OnCommand="rptPledges_Edit" CssClass="btn btn-sm btn-link text-muted py-0 px-1">
                                                    <i class="fa fa-pencil"></i>
                                                    </asp:LinkButton>
                                                    <asp:LinkButton runat="server" ID="btnPledgeDelete"  OnCommand="rptPledges_Delete" CssClass="btn btn-sm btn-link text-muted py-0 px-1 ml-1 js-delete-pledge">
                                                    <i class="fa fa-times"></i>
                                                    </asp:LinkButton>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                        <div class="row">
                            <div class="col-sm-12">
                                <asp:LinkButton ID="lbAddPledge"
                                    runat="server"
                                    CssClass="btn btn-default btn-square btn-xs mt-3 pull-right"
                                    Tooltip="Add Pledge"
                                    Text="<i class='fa fa-plus'></i>"
                                    OnClick="lbAddPledge_Click" />
                            </div>
                        </div>
                    </div>
                    <asp:Panel ID="pnlStatement" runat="server" CssClass="giving-statements panel-body border-top border-panel">
                        <h5 class="mt-0 mb-3">Contribution Statements</h5>
                        <asp:Repeater ID="rptContributionStatementsYYYY" runat="server" OnItemDataBound="rptContributionStatementsYYYY_ItemDataBound" OnItemCommand="rptContributionStatementsYYYY_ItemCommand">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnContributionStatementYYYY" runat="server" CssClass="btn btn-sm btn-default" CommandName="Select">
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
