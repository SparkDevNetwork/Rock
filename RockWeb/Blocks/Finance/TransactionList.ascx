<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField ID="hfTransactionViewMode" runat="server" />

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-credit-card"></i> <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
                    
                    <div class="pull-right">                    
                        <Rock:ButtonDropDownList ID="bddlOptions" runat="server" FormGroupCssClass="panel-options" Title="Options" SelectionStyle="Checkmark" OnSelectionChanged="bddlOptions_SelectionChanged">
                            <asp:ListItem Text="Show Images" Value="1" />
                            <asp:ListItem Text="Show Summary" Value="0" />
                        </Rock:ButtonDropDownList>

                        <div class="btn-group panel-toggle pull-right">
                            <asp:LinkButton ID="btnTransactions" CssClass="btn btn-xs btn-primary" runat="server" Text="Transactions" OnClick="btnTransactionsViewMode_Click" />
                            <asp:LinkButton ID="btnTransactionDetails" CssClass="btn btn-xs btn-default" runat="server" Text="Transaction Details" OnClick="btnTransactionsViewMode_Click" />
                        </div>
                    </div>
                    
                </div>
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbClosedWarning" CssClass="alert-grid" runat="server" NotificationBoxType="Info" Title="Note"
                        Text="This batch has been closed and transactions cannot be edited." Visible="false" Dismissable="false" />

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfTransactions" runat="server">
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                            <Rock:NumberRangeEditor ID="nreAmount" runat="server" Label="Amount Range" NumberType="Double" />
                            <Rock:RockTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code"></Rock:RockTextBox>
                            <Rock:AccountPicker ID="apAccount" runat="server" Label="Account" AllowMultiSelect="true" />
                            <Rock:RockDropDownList ID="ddlTransactionType" runat="server" Label="Transaction Type" />
                            <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" Label="Currency Type" />
                            <Rock:RockDropDownList ID="ddlCreditCardType" runat="server" Label="Credit Card Type" />
                            <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" />
                            <Rock:CampusPicker ID="campCampus" runat="server" Label="Campus (of Batch)" />
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <Rock:Grid ID="gTransactions" runat="server" EmptyDataText="No Transactions Found" 
                            RowItemText="Transaction" OnRowSelected="gTransactions_Edit" AllowSorting="true" ExportSource="ColumnOutput" >
                            <Columns>
                                <Rock:SelectField></Rock:SelectField>
                                <Rock:RockLiteralField ID="lPersonFullNameReversed" HeaderText="Person" 
                                    SortExpression="_PERSONNAME_" /> 
                                <Rock:RockBoundField DataField="TransactionDateTime" HeaderText="Date / Time" SortExpression="TransactionDateTime" />                
                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                                <Rock:RockTemplateField HeaderText="Currency Type" >
                                    <ItemTemplate>
                                        <asp:Literal ID="lCurrencyType" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="TransactionCode" HeaderText="Transaction Code" SortExpression="TransactionCode" ColumnPriority="DesktopSmall" />                
                                <Rock:RockLiteralField ID="lBatchId" HeaderText="Batch Id" SortExpression="BatchId" ColumnPriority="DesktopSmall" ItemStyle-HorizontalAlign="Right" ExcelExportBehavior="NeverInclude"  />                
                                <Rock:RockTemplateField HeaderText="Accounts" >
                                    <ItemTemplate><%# GetAccounts( Container.DataItem ) %></ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Summary" HeaderText="Summary" SortExpression="Summary" ColumnPriority="DesktopLarge" />                
                                <Rock:RockBoundField DataField="Status" HeaderText="Status" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                <Rock:DateTimeField DataField="SettledDate" HeaderText="Settled Date/Time" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                <Rock:RockBoundField DataField="SettledGroupId" HeaderText="Processor Batch Id" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                <Rock:RockLiteralField ID="lTransactionImage" HeaderText="Image" />
                                <Rock:DeleteField OnClick="gTransactions_Delete" Visible="false"/>
                            </Columns>
                        </Rock:Grid>

                        <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" CssClass="margin-b-none" Dismissable="true"></Rock:NotificationBox>

                    </div>

                </div>
            </div>

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title">Total Results</h1>
                            <div class="panel-labels">
                                <Rock:HighlightLabel ID="lbFiltered" runat="server" LabelType="Default" Visible="false" />
                            </div>
                        </div>
                        <div class="panel-body">
                            <asp:Repeater ID="rptAccountSummary" runat="server">
                                <ItemTemplate>
                                    <div class='row'>
                                        <div class='col-xs-8'><%#Eval("Name")%></div>
                                        <div class='col-xs-4 text-right'><%#Eval("TotalAmount")%></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <div class='row'>
                                <div class='col-xs-8'><b>Total: </div>
                                <div class='col-xs-4 text-right'>
                                    <asp:Literal ID="lGrandTotal" runat="server" /></b>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgReassign" runat="server" Title="Reassign Transactions" ValidationGroup="Reassign"
            SaveButtonText="Reassign" OnSaveClick="dlgReassign_SaveClick" OnCancelScript="clearActiveDialog();" >
            <Content>

                <div class="row">
                    <div class="col-sm-6">
                        <Rock:PersonPicker ID="ppReassign" runat="server" Label="Reassign Selected Transactions To" Required="true" ValidationGroup="Reassign" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockRadioButtonList ID="rblReassingBankAccounts" runat="server" Label="Reassign Bank Accounts" Required="true" ValidationGroup="Reassign"
                            Help="In addition to the selected transactions, how should all of the saved bank accounts for this person be reassigned?">
                            <asp:ListItem Text="Move bank accounts to selected individual" Value="MOVE" Selected="True" />
                            <asp:ListItem Text="Copy bank accounts to selected individual" Value="COPY" />
                            <asp:ListItem Text="Do not adjsut bank accounts" Value="NONE" />
                        </Rock:RockRadioButtonList>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>


    </ContentTemplate>
</asp:UpdatePanel>
