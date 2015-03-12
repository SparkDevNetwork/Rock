<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBatchList" CssClass="panel panel-block" runat="server">

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
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbAccountingCode" runat="server" Label="Accounting Code"></Rock:RockTextBox>
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBatchList" runat="server" RowItemText="Batch" OnRowSelected="gBatchList_Edit" AllowSorting="true">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DateField DataField="BatchStartDateTime" HeaderText="Date" SortExpression="BatchStartDateTime" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                            <Rock:RockBoundField DataField="AccountingSystemCode" HeaderText="Accounting Code" SortExpression="AccountingSystemCode" />
                            <Rock:RockBoundField DataField="TransactionCount" HeaderText="Transactions" SortExpression="TransactionCount" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="TransactionAmount" HeaderText="Transaction Total" SortExpression="TransactionAmount" DataFormatString="{0:C2}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockTemplateField HeaderText="Control Variance" ItemStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <span class='<%# (decimal)Eval("Variance") != 0 ? "label label-danger" : "" %>'><%# ((decimal)Eval("Variance")).ToString("C2") %></span>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="Campus.Name" ColumnPriority="Desktop" />
                            <Rock:RockTemplateField HeaderText="Status" SortExpression="Status" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class='<%# Eval("StatusLabelClass") %>'><%# Eval("StatusText") %></span>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false" ColumnPriority="Desktop" />
                            <Rock:DeleteField OnClick="gBatchList_Delete" />
                        </Columns>
                    </Rock:Grid>

                    <div class="batch-list-summary panel-body">
                        <div class="row">
                            <div class="col-md-4">
                                <span>Total:&nbsp;</span><asp:Literal ID="lBatchesTotalAmount" runat="server" />

                                <h5>Accounts</h5>
                                <Rock:Grid ID="gAccountSummary" runat="server" DisplayType="Light" CssClass="batch-list-account-summary" ShowHeader="false" GridLines="None">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:CurrencyField DataField="TotalAmount" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <div class="col-md-8">
                            </div>
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true"></Rock:NotificationBox>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>





