<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionReconciler.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Finance.TransactionReconciler" %>
<style>
    ul {
        padding-left: 20px;
        list-style: none;
    }
</style>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-4 pull-right" style="padding-bottom: 20px;">
                <ul class="nav nav-pills pull-right">
                    <li class="active"><asp:Button runat="server" CssClass="btn btn-primary" ID="btnExportToExcel" Text="Export Batches To Excel" OnClick="btnExportToExcel_Click"/></li>
                </ul>
            </div>
        </div>

        <asp:Repeater ID="rItems" runat="server" OnItemDataBound="OnItemDataBound">
            <ItemTemplate>
                <div class="panel panel-default hidden-print" style="page-break-inside:avoid;" id="<%# Eval("RockBatch.Id") %>">
                    <div class="panel-heading" style="display:flex; flex-direction:column; justify-content: space-between;">
                        <div style="display:flex; flex-direction: row; justify-content: space-between; margin-bottom:20px;">
                            <h4 class="batchid" style="margin:0">Date: <%# Eval("RockBatch.CreatedDateTime") %></h4>
                            <h4 class="batchid" style="margin:0; text-align:right;"><span class="transactioncount deposit" style="margin-bottom: 20px;"><%# Eval("RockBatch.Name") %><br>
                                    Deposit: <%# Eval("RockBatch.ControlAmount") %>
                                </span></h4>
                        </div>
                        <div style="display:flex; flex-direction: row; justify-content: space-between; margin-bottom:20px;">
                            <h5 style="margin:0">Batch Id: <%# Eval("RockBatch.Id") %></h5>
                            <h5 style="margin:0">Transaction Count: <%# Eval("TransactionCount") %></h5>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <ul class="nav nav-pills" style="max-width:50%;">
                                    <li class="active"><a data-toggle="pill" href="#FundActivity-<%# Eval("RockBatch.Id") %>">Fund Activity</a></li>
                                    <li class="hidden-print"><a data-toggle="pill" href="#transactions-<%# Eval("RockBatch.Id") %>">Transactions</a></li>
                                </ul>
                            </div>
                             <div class="col-md-6">
                                 <ul class="nav nav-pills pull-right">
                                    <li class="active"><asp:Button runat="server" CssClass="btn btn-primary" ID="btnMarkReconciled" OnClick="btnMarkReconciled_Click"/></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div class="tab-content">
                            <div id="FundActivity-<%# Eval("RockBatch.Id") %>" class="tab-pane fade in active">
                                <table class="table table-striped table-hover" style="font-size:1.1em; min-width:320px;">
                                    <tbody>
                                        <tr>
                                            <th style="text-align:left;">Fund Name</th>
                                            <th style="text-align:right;">Fund Debit</th>
                                            <th style="text-align:right;">Fund Credit</th>
                                            <th style="text-align:right;">GL Fund</th>
                                            <th style="text-align:right;">GL Bank Account</th>
                                            <th style="text-align:right;">GL Revenue Department</th>
                                            <th style="text-align:right;">GL Revenue Account</th>
                                        </tr>
                                        <asp:Repeater ID="rItemsFunds" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td style="text-align:left;"><%# Eval("FundName") %></td>
                                                    <td style="text-align:right;"><%# Eval("DepositAmount") %></td>
                                                    <td style="text-align:right;"><%# Eval("CreditAmount") %></td>
                                                    <td style="text-align:right;"><%# Eval("GLFund") %></td>
                                                    <td style="text-align:right;"><%# Eval("GLBankAccount") %></td>
                                                    <td style="text-align:right;"><%# Eval("GLRevenueDepartment") %></td>
                                                    <td style="text-align:right;"><%# Eval("GLRevenueAccount") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>                   
                                   </tbody>
                                </table>    
                            </div>
                            <div id="transactions-<%# Eval("RockBatch.Id") %>" class="tab-pane fade in">
                                <table class="table table-striped table-hover">
                                    <thead>
                                        <tr>
                                            <th style="width: 10%; padding-right: 20px;">Id</th>
                                            <th style="width: 15%; padding-right: 20px;">Name</th>
                                            <th style="width: 15%; padding-right: 20px;">Email</th>
                                            <th style="width: 10%; padding-right: 20px;">TransactionTotal</th>
                                            <th style="width: 30%; padding-right: 20px;">Transaction Breakdown</th>
                                            <th style="width: 10%; padding-right: 20px;">Present In NMI</th>
                                            <th>NMI Status</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rItemsTransactions" runat="server" OnItemDataBound="rItemsTransactions_ItemDataBound">
                                            <ItemTemplate>
                                                <tr>
                                                    <td style="padding-right: 20px;"><a href="/Transaction/<%# Eval("RockTransaction.Id") %>" target="_blank"><%# Eval("RockTransaction.Id") %></a></td>
                                                    <td style="padding-right: 20px;"><%# Eval("RockTransaction.AuthorizedPersonAlias.Person.FullName") %></td>
                                                    <td style="padding-right: 20px;"><%# Eval("RockTransaction.AuthorizedPersonAlias.Person.Email") %></td>
                                                    <td style="padding-right: 20px;"><%# Eval("RockTransaction.TotalAmount") %></td>
                                                    <td style="padding-right: 20px; vertical-align:top">
                                                        <ul style="text-align:left">
                                                            <asp:Repeater ID="rItemsTransactionBreakdown" runat="server">
                                                                <ItemTemplate>
                                                                    <li style="text-align:left"><%# Eval("EntityType") %> (<%# Eval("EntityName") %>): <%# Eval("Amount") %></li>
                                                                </ItemTemplate>
                                                            </asp:Repeater>
                                                        </ul>
                                                    </td>
                                                    <td><i class="<%# Eval("NMITransaction") == null ? "fas fa-times-circle" : "fas fa-check-circle" %>"></i></td>
                                                    <td style="text-align:left;"><%# Eval("NMITransaction.Action.ActionType") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>  
                                        <tr>
                                           
                                        </tr>
                                    </tbody>
                                </table>        
                            </div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>