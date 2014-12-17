﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchDetail" %>
<%@ Reference Control="~/Blocks/Finance/TransactionList.ascx" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfBatchId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-archive"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valSummaryBatch" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" Label="Title" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" Required="true"></Rock:RockDropDownList>
                            <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Batch Start" Required="true" RequiredErrorMessage="A Batch Start Date is required" />
                            <Rock:DateTimePicker ID="dtpEnd" runat="server" Label="Batch End" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="tbControlAmount" runat="server" Label="Control Amount" />
                            <Rock:CampusPicker ID="campCampus" runat="server" Label="Campus" />
                            <Rock:DataTextBox ID="tbAccountingCode" runat="server" Label="Accounting Code" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="AccountingSystemCode"
                                Help="Optional id or code from an external accounting system." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewSummary" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <div class="row">
                        <div class="col-sm-6">
                            <asp:Literal ID="lDetails" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <div class="grid">
                                <Rock:Grid ID="gAccounts" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Account" AllowSorting="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" HeaderText="Account Totals" />
                                        <Rock:RockBoundField DataField="Amount" DataFormatString="{0:C2}" ItemStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <asp:LinkButton ID="lbMatch" runat="server" CssClass="btn btn-default pull-right" CausesValidation="false" OnClick="lbMatch_Click"><i class="fa fa-money"></i> Match Transactions</asp:LinkButton>
                    </div>
                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>