<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchDetail" %>
<%@ Reference Control="~/Blocks/Finance/TransactionList.ascx" %>

<asp:UpdatePanel ID="upnlFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfBatchId" runat="server" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div class="banner"><h1><asp:Literal ID="lTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">
                <asp:ValidationSummary ID="valSummaryBatch" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" Label="Title" TabIndex="1" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" ValidationGroup="batchDetail" />
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbControlAmount" runat="server" PrependText="$" CssClass="input-width-md" Label="Control Amount" TabIndex="2" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                        <Rock:RockDropDownList ID="ddlStatus" TabIndex="3" runat="server" Label="Status"></Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="campCampus" runat="server" TabIndex="4" />
                        <Rock:DateRangePicker ID="drpBatchDate" TabIndex="5" runat="server" Label="Batch Date Range" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="lbSaveFinancialBatch_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-sm" CausesValidation="false" OnClick="lbCancelFinancialBatch_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewSummary" runat="server">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsLeft" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsRight" runat="server" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="lbEdit_Click" />
                </div>
            </fieldset>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>