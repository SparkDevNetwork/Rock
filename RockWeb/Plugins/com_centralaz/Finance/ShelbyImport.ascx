<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShelbyImport.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.ShelbyImport" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.showLog = function () {
            $("div[id$='_messageContainer']").fadeIn();
            $("div[id$='_pnlConfiguration']").fadeOut();
        }

        proxy.client.receiveNotification = function (name, message) {
            if (name.startsWith("shelbyImport-"))
            {
                var fields = name.split("-");
                $("#"+fields[1]).html(message);
            }
        }

        $.connection.hub.start().done(function () {
            // hub started... do stuff here if you want to let the user know something
            console.log("SignalR hub started.");
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlImport" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Import Shelby Contributions</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbSuccessMessage" runat="server" NotificationBoxType="Success" />
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

                <asp:Panel ID="pnlConfiguration" runat="server">
                    <h2>Settings</h2>
                    <Rock:RockTextBox runat="server" ID="tbBatchName" Label="Batch Name" ToolTip="The name you wish to use for this batch import." OnTextChanged="tbBatchName_TextChanged" AutoPostBack="true"></Rock:RockTextBox>

                    <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" Required="true" ToolTip="The campus you are assigning to the batch." />

                    <h2>Verify/Set Account Mapping</h2>
                    <table class="table table-striped table-hover table-condensed">
                    <asp:Repeater ID="rptAccountMap" runat="server" OnItemDataBound="rptAccountMap_ItemDataBound">
                        <HeaderTemplate></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="col-md-4">
                                    <asp:Literal ID="litFundName" runat="server"></asp:Literal>
                                    <asp:HiddenField ID="hfFundId" runat="server" />
                                    <span class="pull-right"><asp:Literal ID="litAccontSaveStatus" runat="server"></asp:Literal></span>
                                </td>
                                <td class="col-md-8">
                                    <Rock:RockDropDownList ID="rdpAcccounts" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rdpAcccounts_SelectedIndexChanged"></Rock:RockDropDownList>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                    </table>

                </asp:Panel>

                <p>
                    <Rock:NumberRangeEditor runat="server" ID="nreBatchRange" Label="Batch Range" LowerValue="1" RangeLabel=" to " />
                    <asp:LinkButton runat="server" ID="lbImport" CssClass="btn btn-primary" OnClick="lbImport_Click">
                        <i class="fa fa-arrow-up"></i> Import
                    </asp:LinkButton>

                    <asp:LinkButton runat="server" ID="lbClearSession" CssClass="btn btn-link" OnClick="lbClearSession_Click">Clear Session</asp:LinkButton> <asp:Literal runat="server" ID="lSessionStats"></asp:Literal>
                </p>

                <!-- SignalR client notification area -->
                <div class="well" id="messageContainer" runat="server" style="">
                    <div id="processingUsers"></div>
                    <div id="processingBatches"></div>
                    <div id="processingTransactions"></div>
                </div>

                <asp:Panel ID="pnlErrors" runat="server" Visible="false" CssClass="alert alert-danger block-message error">
                    <h3>Error Transactions</h3>
                    <asp:GridView ID="gErrors" runat="server" AutoGenerateColumns="true" CssClass="table table-striped" BorderColor="White"></asp:GridView>
                </asp:Panel>

                <Rock:NotificationBox ID="nbBatch" runat="server" NotificationBoxType="Success" />

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">Batch Summary</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gBatchList" runat="server" AllowSorting="false" AllowPaging="false">
                        <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:DateField DataField="BatchStartDateTime" HeaderText="Date" SortExpression="BatchStartDateTime" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                                <Rock:RockBoundField DataField="AccountingSystemCode" HeaderText="Accounting Code" SortExpression="AccountingSystemCode" />
                                <Rock:RockBoundField DataField="TransactionCount" HeaderText="<span class='hidden-print'>Transaction Count</span><span class='visible-print-inline'>Txns</span>" HtmlEncode="false" SortExpression="TransactionCount" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                                <Rock:CurrencyField DataField="TransactionAmount" HeaderText="<span class='hidden-print'>Transaction Total</span><span class='visible-print-inline'>Txn Total</span>" HtmlEncode="false" SortExpression="TransactionAmount" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockTemplateField HeaderText="Variance" ItemStyle-HorizontalAlign="Right">
                                    <ItemTemplate>
                                        <span class='<%# (decimal)Eval("Variance") != 0 ? "label label-danger" : "" %>'><%# this.FormatValueAsCurrency((decimal)Eval("Variance")) %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="AccountSummaryHtml" HeaderText="Accounts" HtmlEncode="false" />
                                <Rock:RockTemplateField HeaderText="Status" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='<%# Eval("StatusLabelClass") %>'><%# Eval("StatusText") %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false" ColumnPriority="Desktop" />
                        </Columns>
<%--                        <Columns>
                            <asp:TemplateField SortExpression="TransactionId" HeaderText="Batch ID">
                                <ItemTemplate>
                                    <asp:Literal ID="lTransactionID" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Transaction.ProcessedDateTime" HeaderText="Transaction Date" SortExpression="TransactionDate" />
                            <asp:TemplateField SortExpression="FullName" HeaderText="Full Name">
                                <ItemTemplate>
                                    <asp:Literal ID="lFullName" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Transaction.FinancialPaymentDetail.CurrencyTypeValue" HeaderText="Transaction Type" SortExpression="TransactionType" />
                            <asp:BoundField DataField="Account" HeaderText="Fund Name" SortExpression="FundName" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                        </Columns>--%>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
