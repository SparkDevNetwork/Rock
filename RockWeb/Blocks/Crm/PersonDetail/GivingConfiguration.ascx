<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GivingConfiguration" %>
<%@ Import namespace="Rock" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-clipboard-check"></i>Giving Configuration</h1>
                </div>
                <div class="panel-body">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <asp:LinkButton ID="btnAddTransaction" runat="server" CssClass="btn btn-default btn-block" Text="Add One-time Gift" OnClick="btnAddTransaction_Click" />
                    <asp:LinkButton ID="btnAddScheduledTransaction" runat="server" CssClass="btn btn-default btn-block" Text="New Scheduled Transaction" OnClick="btnAddScheduledTransaction_Click" />
                    <h6 class="mt-4">Scheduled Transactions</h6>
                    <table class="table table-condensed">
                        <asp:Repeater ID="rptScheduledTransaction" runat="server" OnItemDataBound="rptScheduledTransaction_ItemDataBound">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <span class="d-block"><asp:Literal ID="lAccounts" runat="server" /></span>
                                        <span class="d-block small text-muted"><%# Eval("TransactionFrequencyValue.Value") %>: Next Gift
                                            <asp:Literal ID="lNextPaymentDate" runat="server" />
                                        </span>
                                    </td>
                                    <td class="align-middle text-right"><%# ((decimal)Eval("TotalAmount")).FormatAsCurrency() %></td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" OnCommand="rptScheduledTransaction_Edit" CommandArgument='<%# Eval("Guid") %>' CssClass="btn btn-sm btn-square btn-link text-muted">
                                        <i class="fa fa-pencil"></i>
                                        </asp:LinkButton>
                                    </td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" OnCommand="rptScheduledTransaction_Delete" CommandArgument='<%# Eval("Guid") %>' CssClass="btn btn-sm btn-square btn-link text-muted">
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
                            <asp:Repeater ID="rptSavedAccounts" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td class="align-middle"><%# Eval("Name") %></td>
                                        <td class="align-middle text-right"><%# Eval("FinancialPaymentDetail.AccountNumberMasked") %></td>
                                        <td class="w-1 align-middle">
                                            <asp:LinkButton runat="server" OnCommand="rptSavedAccounts_Delete" CommandArgument='<%# Eval("Guid") %>' CssClass="btn btn-sm btn-square btn-link text-muted">
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
                                        <span class="d-block"><%# Eval("Account.Name") %></span>
                                        <span class="d-block small text-muted"><asp:Literal ID="lPledgeDate" runat="server" /></span>
                                    </td>
                                    <td class="align-middle text-right"><%# ((decimal)Eval("TotalAmount")).FormatAsCurrency() %></td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" OnCommand="rptPledges_Edit" CommandArgument='<%# Eval("Guid") %>' CssClass="btn btn-sm btn-square btn-link text-muted">
                                        <i class="fa fa-pencil"></i>
                                        </asp:LinkButton>
                                    </td>
                                    <td class="w-1 align-middle">
                                        <asp:LinkButton runat="server" OnCommand="rptPledges_Delete" CommandArgument='<%# Eval("Guid") %>' CssClass="btn btn-sm btn-square btn-link text-muted">
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
                        <asp:Repeater ID="rStatements" runat="server" OnItemDataBound="rStatements_ItemDataBound" OnItemCommand="rStatements_ItemCommand">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbYear" runat="server" CssClass="btn btn-sm btn-default" CommandArgument='<%# Container.DataItem %>' CommandName="Select">
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
