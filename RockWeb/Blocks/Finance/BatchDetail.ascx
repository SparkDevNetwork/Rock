<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.BatchDetail" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfBatchId" runat="server" />        
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div id="pnlEditDetails" runat="server" class="well">
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" Label="Title" TabIndex="1" 
                                SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbControlAmount" runat="server" Label="Control Amount" TabIndex="2" 
                                SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                            <Rock:RockDropDownList ID="ddlStatus" TabIndex="3" runat="server" Label="Status"></Rock:RockDropDownList>
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpCampus" runat="server" TabIndex="4" />
                            <Rock:DateRangePicker ID="dtBatchDate" TabIndex="5" runat="server" Label="Batch Date Range" />
                        </div>
                    </div>
                </fieldset>
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveFinancialBatch_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancelFinancialBatch_Click" />
                </div>
            </div>
            <fieldset id="fieldsetViewDetails" runat="server">
                <div class="well">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />                            
                            <asp:Literal ID="lblDetails" runat="server" />
                        </div>                        
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" CausesValidation="false" OnClick="btnEdit_Click" />
                    </div>
                </div>
            </fieldset>                   
        </asp:Panel>
  
</ContentTemplate>
</asp:UpdatePanel>
