<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSignatureDocumentId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-signature"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatusLastUpdated" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">

                <div id="pnlEditLegacyProviderDocumentDetails" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbLegacyProviderErrorMessage" runat="server" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlDocumentType" runat="server" Label="Signature Document Type" DataValueField="Id" DataTextField="Name" Required="true" />
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.SignatureDocument, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rbStatus" runat="server" Label="Status" RepeatDirection="Horizontal"></Rock:RockRadioButtonList>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:FileUploader ID="fuDocument" runat="server" Label="Document" />
                            <Rock:RockLiteral ID="lDocumentKey" runat="server" Label="Document Key" />
                            <Rock:RockLiteral ID="lRequestDate" runat="server" Label="Last Invite Date" />
                        </div>
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppAppliesTo" runat="server" Label="Applies To" Required="true"
                                Help="The person that this document applies to." />
                            <Rock:PersonPicker ID="ppAssignedTo" runat="server" Label="Assigned To" Required="true"
                                Help="The person that this document was assigned to for getting a signature." />
                            <Rock:PersonPicker ID="ppSignedBy" runat="server" Label="Signed By"
                                Help="The person that signed this." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSaveLegacyProviderDocument" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveLegacyProviderDocument_Click" />
                        <asp:LinkButton ID="btnCancelLegacyProviderDocument" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelLegacyProviderDocument_Click" />
                        <asp:LinkButton ID="btnSendLegacyProviderDocument" runat="server" Text="Send" CssClass="btn btn-default btn-sm pull-right" CausesValidation="false" OnClick="btnSendLegacyProviderDocument_Click" Visible="false" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lSignedBy" runat="server" Label="Signed By" />
                            <Rock:RockControlWrapper ID="rcwCompletionEmailInfo" runat="server" Label="Completion Email">
                                <asp:Literal ID="lCompletionSignedByPersonEmailAddress" runat="server" /><br />
                                <asp:Literal ID="lCompletionLastSentDateTime" runat="server" /><br />
                                <asp:LinkButton ID="btnResendCompletionEmail" runat="server" CssClass="btn btn-xs btn-default" Text="Resend" OnClick="btnResendCompletionEmail_Click" />
                                <Rock:NotificationBox ID="nbCompletionEmailResult" runat="server" Visible="false" />
                            </Rock:RockControlWrapper>

                            <Rock:RockLiteral ID="lAppliesTo" runat="server" Label="Applies To" />
                            <Rock:RockLiteral ID="lLegacyDocumentLink" runat="server" Label="Document" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lSignedOnInformation" runat="server" Label="Signed On" />
                            <Rock:RockLiteral ID="lRelatedEntity" runat="server" Label="Related Entity" />
                        </div>
                    </div>

                    <Rock:PDFViewer ID="pdfSignatureDocument" runat="server" ViewerHeight="900px" />

                    <div class="actions">
                        <asp:LinkButton ID="btnEditLegacyProviderDocument" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEditLegacyProviderDocument_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
