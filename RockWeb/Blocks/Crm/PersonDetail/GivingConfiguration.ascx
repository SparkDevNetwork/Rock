<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingConfiguration" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {

        $('.js-inactivate-scheduled-transaction').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmPreventOnCancel(event, 'Are you sure you want to inactivate this scheduled transaction?');
        });

        $('.js-delete-scheduled-transaction').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmDelete(event, 'scheduled transaction');
        });

        $('.js-delete-saved-account').on('click', function (event) {
            event.stopImmediatePropagation();
            return Rock.dialogs.confirmDelete(event, 'saved account');
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
                <div class="panel-body">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <asp:LinkButton ID="btnAddTransaction" runat="server" CssClass="btn btn-default btn-block" Text="Add One-time Gift" OnClick="btnAddTransaction_Click" />
                    <asp:LinkButton ID="btnAddScheduledTransaction" runat="server" CssClass="btn btn-default btn-block" Text="New Scheduled Transaction" OnClick="btnAddScheduledTransaction_Click" />

                    <div class="">
                        <h6 class="mt-4">Scheduled Transactions</h6>
                        <asp:LinkButton ID="btnShowInactiveScheduledTransactions" runat="server" CssClass="btn btn-link btn-sm" Text="Show Inactive" OnClick="btnShowInactiveScheduledTransactions_Click" />
                    </div>

                    <asp:HiddenField ID="hfShowInactiveScheduledTransactions" runat="server" />
                    <table class="table table-condensed">
                        <asp:Repeater ID="rptScheduledTransaction" runat="server" OnItemDataBound="rptScheduledTransaction_ItemDataBound">
                            <ItemTemplate>
                                <tr runat="server" id="trScheduledTransaction">
                                    <td>
                                        <span class="d-block"><asp:Literal ID="lScheduledTransactionAccountName" runat="server" /></span>
                                        <span class="d-block small text-muted">
                                            <asp:Literal ID="lScheduledTransactionFrequencyAndNextPaymentDate" runat="server" />
                                        </span>

                                        <span class="d-block small text-muted">
                                            <asp:Literal ID="lScheduledTransactionCardTypeLast4" runat="server" /> | <asp:Literal ID="lScheduledTransactionExpiration" runat="server" />
                                        </span>

                                        <span class="d-block small text-muted">
                                            <asp:Literal ID="lScheduledTransactionSavedAccountName" runat="server" />
                                        </span>
                                    </td>
                                    <td class="align-middle text-right"><asp:Literal ID="lScheduledTransactionTotalAmount" runat="server"/></td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton ID="btnScheduledTransactionEdit" runat="server" OnCommand="rptScheduledTransaction_Edit" CssClass="btn btn-sm btn-square btn-link text-muted">
                                        <i class="fa fa-pencil"></i>
                                        </asp:LinkButton>
                                    </td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" ID="btnScheduledTransactionInactivate" OnCommand="rptScheduledTransaction_Inactivate" CssClass="btn btn-sm btn-square btn-link text-muted js-inactivate-scheduled-transaction" Visible="true">
                                            <i class="fa fa-times"></i>
                                        </asp:LinkButton>

                                        <%-- Note that Delete and Inactivate scheduled transactions both just Inactivate the Transaction. See engineering notes in rptScheduledTransaction_Delete --%>
                                        <asp:LinkButton runat="server" ID="btnScheduledTransactionDelete" OnCommand="rptScheduledTransaction_Delete" CssClass="btn btn-sm btn-square btn-link text-muted js-delete-scheduled-transaction" Visible="false">
                                            <i class="fa fa-times"></i>
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="row">
                        <div class="col-sm-12">
                            <asp:LinkButton ID="lbAddScheduledTransaction"
                                runat="server"
                                CssClass="btn btn-default btn-square btn-xs margin-t-sm margin-b-md pull-right"
                                Text="<i class='fa fa-plus'></i>"
                                OnClick="lbAddScheduledTransaction_Click" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlSavedAccounts" runat="server">
                        <h6 class="mt-4">Saved Accounts</h6>
                        <table class="table table-condensed">
                            <asp:Repeater ID="rptSavedAccounts" runat="server" OnItemDataBound="rptSavedAccounts_ItemDataBound">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <span class="d-block"><asp:Literal ID="lSavedAccountName" runat="server" /></span>
                                            <span class="d-block small text-muted"><asp:Literal ID="lScheduledTransactionFrequency" runat="server"/>
                                                <asp:Literal ID="lSavedAccountCardTypeLast4" runat="server" /> | <asp:Literal ID="lSavedAccountExpiration" runat="server" />
                                            </span>
                                        </td>

                                        <td class="w-1 align-middle">
                                            <span class="d-block"><asp:Literal ID="lSavedAccountInUseStatusHtml" runat="server" /></span>

                                            <asp:LinkButton runat="server" ID="btnSavedAccountDelete" OnCommand="rptSavedAccounts_Delete" CssClass="btn btn-sm btn-square btn-link text-muted">
                                                <i class="fa fa-times"></i>
                                            </asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                    </asp:Panel>
                    <h6 class="mt-4">Pledges</h6>
                    <table class="table table-condensed">
                        <asp:Repeater ID="rptPledges" runat="server" OnItemDataBound="rptPledges_ItemDataBound">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <span class="d-block"><asp:Literal ID="lPledgeDate" runat="server" /></span>
                                        <span class="d-block small text-muted"><asp:Literal ID="lPledgeAccountName" runat="server" /></span>
                                    </td>
                                    <td class="align-middle text-right"><asp:Literal ID="lPledgeTotalAmount" runat="server" /></td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" ID="btnPledgeEdit" OnCommand="rptPledges_Edit" CssClass="btn btn-sm btn-square btn-link text-muted">
                                        <i class="fa fa-pencil"></i>
                                        </asp:LinkButton>
                                    </td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" ID="btnPledgeDelete"  OnCommand="rptPledges_Delete" CssClass="btn btn-sm btn-square btn-link text-muted">
                                        <i class="fa fa-times"></i>
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>
                    <div class="row">
                        <div class="col-sm-12">
                            <asp:LinkButton ID="lbAddPledge"
                                runat="server"
                                CssClass="btn btn-default btn-square btn-xs margin-t-sm margin-b-md pull-right"
                                Text="<i class='fa fa-plus'></i>"
                                OnClick="lbAddPledge_Click" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlStatement" runat="server">
                        <h6 class="mt-4">Contribution Statements</h6>
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
