<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchDetail" %>
<%@ Reference Control="~/Blocks/Finance/TransactionList.ascx" %>

<asp:UpdatePanel ID="upnlFinancialBatch" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfBatchId" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-archive"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

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
                            <Rock:CurrencyBox ID="tbControlAmount" runat="server" CssClass="input-width-md" Label="Control Amount" TabIndex="2" 
                                SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                            <Rock:RockDropDownList ID="ddlStatus" TabIndex="3" runat="server" Label="Status"></Rock:RockDropDownList>
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="campCampus" runat="server" TabIndex="4" />
                            <Rock:DateRangePicker ID="drpBatchDate" TabIndex="5" runat="server" Label="Batch Date Range" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveFinancialBatch_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancelFinancialBatch_Click" />
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
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                    </div>
                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>