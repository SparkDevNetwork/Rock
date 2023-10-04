<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReachDonationImport.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Finance.ReachDonationImport" %>
<asp:UpdatePanel ID="upnlReachImport" runat="server" >
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:Panel ID="pnlUploadFile" runat="server" CssClass="panel panel-block" DefaultButton="btnUpload">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-files-o"></i><asp:Literal ID="lUploadTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DatePicker ID="dpBatchDate" runat="server" AllowFutureDateSelection="false" AllowPastDateSelection="true" Label="Batch Date" Required="true" RequiredErrorMessage="Batch Date is Required" />
                                <Rock:DefinedValuesPicker ID="dvpPaymentTypes" runat="server" Required="true" Label="Payment Types to Import" RequiredErrorMessage="Select at least one payment type to import." />
                             </div>
                            <div class="col-md-6">
                                <Rock:FileUploader id="fuReachFile" runat="server" Required="true" Label="Reach File" />
                            </div>
                        </div>
                    </fieldset>
                    <div class="actions">
                        <asp:LinkButton ID="btnUpload" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Upload" CssClass="btn btn-primary" OnClick="btnUpload_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlFileDetails" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-alt"></i><asp:Literal ID="lFileDetailsTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">
                    <Rock:NotificationBox id ="nbDetailMessage" runat="server" />
                    <asp:Panel ID="pnlBatch" runat="server" Visible="false">
                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="rlFileName" runat="server" Label="File Name" />
                                    <Rock:RockLiteral ID="rlBatchName" runat="server" Label="Batch Name" />
                                    <Rock:RockLiteral ID="rlBatchDate" runat="server" Label="Batch Date"  />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="rlTotalRows" runat="server" Label="Contribution Records" />
                                    <Rock:RockLiteral ID="rlImportedTransactions" runat="server" Label="Imported Records" />
                                    <Rock:RockLiteral ID="rlErrors" runat="server" Label="Skipped Records" />
                                    
                                </div>
                            </div>
                        </fieldset>
                    </asp:Panel>
                </div>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
