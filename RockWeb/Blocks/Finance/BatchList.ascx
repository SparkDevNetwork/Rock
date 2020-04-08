<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBatchList" runat="server">
            <asp:HiddenField ID="hfAction" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-archive"></i>&nbsp;Batch List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfBatchFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpBatchDate" runat="server" Label="Date Range" />
                            <Rock:CampusPicker ID="campCampus" runat="server" />
                            <Rock:DefinedValuePicker ID="dvpTransactionType" runat="server"  Label="Contains Transaction Type" />
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                            <Rock:RockTextBox ID="tbAccountingCode" runat="server" Label="Accounting Code"></Rock:RockTextBox>
                            <Rock:DefinedValuePicker ID="dvpSourceType" runat="server" Label="Contains Source Type" />
                            <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gBatchList" runat="server" RowItemText="Batch" OnRowSelected="gBatchList_Edit" AllowSorting="true" CssClass="js-grid-batch-list">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:DateField DataField="BatchStartDateTime" HeaderText="Date" SortExpression="BatchStartDateTime" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                                <Rock:RockBoundField DataField="AccountingSystemCode" HeaderText="Accounting Code" SortExpression="AccountingSystemCode" />
                                <Rock:RockBoundField DataField="TransactionCount" HeaderText="<span class='hidden-print'>Transaction Count</span><span class='visible-print-inline'>Txns</span>" HtmlEncode="false" SortExpression="TransactionCount" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:CurrencyField DataField="TransactionAmount" HeaderText="<span class='hidden-print'>Transaction Total</span><span class='visible-print-inline'>Txn Total</span>" HtmlEncode="false" SortExpression="TransactionAmount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockLiteralField HeaderText="Amount Variance" ItemStyle-HorizontalAlign="Right" ID="lVarianceAmount" HeaderStyle-HorizontalAlign="Right" OnDataBound="lVarianceAmount_DataBound" />
                                <Rock:RockLiteralField HeaderText="Count Variance " ItemStyle-HorizontalAlign="Right" ID="lVarianceItemCount" HeaderStyle-HorizontalAlign="Right" OnDataBound="lVarianceItemCount_DataBound" />
                                <Rock:RockBoundField DataField="AccountSummaryHtml" HeaderText="Accounts" HtmlEncode="false" />
                                <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="Campus.Name" ColumnPriority="Desktop" />
                                <Rock:RockLiteralField HeaderText="Status" ID="lBatchStatus" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" OnDataBound="lBatchStatus_DataBound" />
                                <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false" ColumnPriority="Desktop" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Literal ID="lSummary" runat="server" />
                </div>
            </div>
            <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true"></Rock:NotificationBox>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>