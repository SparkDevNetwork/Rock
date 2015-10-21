<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionList.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionList" %>
<asp:UpdatePanel ID="upTransactions" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-credit-card"></i> <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
                    
                        <Rock:ButtonDropDownList ID="bddlOptions" runat="server" CssClass="panel-options pull-right" Title="Options" SelectionStyle="Checkmark" OnSelectionChanged="bddlOptions_SelectionChanged">
                            <asp:ListItem Text="Show Images" Value="1" />
                            <asp:ListItem Text="Show Summary" Value="0" />
                        </Rock:ButtonDropDownList>
                    
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
                        </Rock:GridFilter>

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <Rock:Grid ID="gTransactions" runat="server" EmptyDataText="No Transactions Found" 
                            RowItemText="Transaction" OnRowSelected="gTransactions_Edit" AllowSorting="true" ExportSource="ColumnOutput" >
                            <Columns>
                                <Rock:SelectField></Rock:SelectField>
                                <Rock:RockBoundField DataField="AuthorizedPersonAlias.Person.FullNameReversed" HeaderText="Person" 
                                    SortExpression="AuthorizedPersonAlias.Person.LastName,AuthorizedPersonAlias.Person.NickName" />
                                <Rock:RockBoundField DataField="TransactionDateTime" HeaderText="Date / Time" SortExpression="TransactionDateTime" />                
                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                                <Rock:RockTemplateField HeaderText="Currency Type" >
                                    <ItemTemplate>
                                        <asp:Literal ID="lCurrencyType" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="TransactionCode" HeaderText="Transaction Code" SortExpression="TransactionCode" ColumnPriority="DesktopSmall" />                
                                <Rock:RockTemplateField HeaderText="Accounts" >
                                    <ItemTemplate><%# GetAccounts( Container.DataItem ) %></ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Summary" HeaderText="Summary" SortExpression="Summary" ColumnPriority="DesktopLarge" />                
                                <Rock:RockLiteralField ID="lTransactionImage" HeaderText="Image" />
                                <Rock:DeleteField OnClick="gTransactions_Delete" Visible="false"/>
                            </Columns>
                        </Rock:Grid>

                        <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true"></Rock:NotificationBox>

                    </div>

                </div>
            </div>

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title">Total Results</h1>
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

    </ContentTemplate>
</asp:UpdatePanel>
