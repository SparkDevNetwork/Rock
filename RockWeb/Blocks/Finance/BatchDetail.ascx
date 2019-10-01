<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchDetail" %>

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
                    <Rock:HighlightLabel ID="hlBatchId" runat="server" LabelType="Info" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                    <Rock:HighlightLabel ID="hlIsAutomated" runat="server" LabelType="Warning" Text="Automated" Visible="false" ToolTip="This is an automated batch. The system will automatically set this batch to OPEN when all transactions have been downloaded." />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valSummaryBatch" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvBatch" runat="server" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DefinedValuePicker ID="dvpBatchName" runat="server" Label="Name" Visible="false" Required="true"/>
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged"></Rock:RockDropDownList>
                            <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Batch Start" Required="true" RequiredErrorMessage="A Batch Start Date is required" />
                            <Rock:DateTimePicker ID="dtpEnd" runat="server" Label="Batch End" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="tbControlAmount" runat="server" Label="Control Amount" />
                            <Rock:NumberBox ID="nbControlItemCount" runat="server" Label="Control Item Count" NumberType="Integer" />
                            <Rock:CampusPicker ID="campCampus" runat="server" Label="Campus" />
                            <Rock:DataTextBox ID="tbAccountingCode" runat="server" Label="Accounting Code" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="AccountingSystemCode"
                                Help="Optional id or code from an external accounting system." />
                            <Rock:DataTextBox ID="tbNote" runat="server" Label="Notes" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Note" TextMode="MultiLine" Rows="4"/>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="attributes">
                                <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewSummary" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <div class="row">
                        <div class="col-sm-6">
                            <asp:Literal ID="lDetails" runat="server" />
                            <asp:PlaceHolder ID="phReadonlyAttributes" runat="server"></asp:PlaceHolder>
                        </div>
                        <div class="col-sm-6">
                            <div class="grid">
                                <Rock:Grid ID="gAccounts" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Account" AllowSorting="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" HeaderText="Account Totals" />
                                        <Rock:CurrencyField DataField="Amount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <div class="grid">
                                <Rock:Grid ID="gCurrencyTypes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="CurrencyType" AllowSorting="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" HeaderText="Currency Totals" />
                                        <Rock:CurrencyField DataField="Amount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                        <div class="pull-right">
                            <asp:LinkButton ID="lbMatch" runat="server" CssClass="btn btn-default" CausesValidation="false" OnClick="lbMatch_Click"><i class="fa fa-money"></i> Match Transactions</asp:LinkButton>
                            <asp:LinkButton ID="lbHistory" runat="server" CssClass="btn btn-default" CausesValidation="false" OnClick="lbHistory_Click"><i class="fa fa-file-text-o"></i> Audit Log</asp:LinkButton>
                        </div>
                    </div>
                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>