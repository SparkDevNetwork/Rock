<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.BatchDetail" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfBatchId" runat="server" />        
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div class="banner"><h1><asp:Literal ID="lActionTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" Label="Title" TabIndex="1" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                    </div>
                </div>    
                
                <div class="row">
                    <div class="col-md-6">
                        
                        <Rock:DataTextBox ID="tbControlAmount" runat="server" PrependText="$" CssClass="input-width-md" Label="Control Amount" TabIndex="2" 
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="ControlAmount" />
                        <Rock:RockDropDownList ID="ddlStatus" TabIndex="3" runat="server" Label="Status"></Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" TabIndex="4" />
                        <Rock:DateRangePicker ID="dtBatchDate" TabIndex="5" runat="server" Label="Batch Date Range" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSaveFinancialBatch_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-sm" CausesValidation="false" OnClick="btnCancelFinancialBatch_Click" />
                </div>
            </div>
        </div>


            <fieldset id="fieldsetViewDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />                            
                        <asp:Literal ID="lblDetails" runat="server" />
                    </div>                        
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="btnEdit_Click" />
                </div>

            </fieldset>                   
        </asp:Panel>
  
</ContentTemplate>
</asp:UpdatePanel>
