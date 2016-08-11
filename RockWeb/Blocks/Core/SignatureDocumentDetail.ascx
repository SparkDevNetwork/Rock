<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSignatureDocumentId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlDocumentType" runat="server" Label="Signature Document Type" DataValueField="Id" DataTextField="Name" />
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.SignatureDocument, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:RockRadioButtonList ID="rbStatus" runat="server" Label="Status" RepeatDirection="Horizontal"></Rock:RockRadioButtonList>
                                </div>
                                <div class="col-sm-6">
                                    <Rock:RockLiteral ID="lStatusLastUpdated" runat="server" Label="Status Last Updated" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:FileUploader ID="fuDocument" runat="server" Label="Document" />
                            <Rock:RockLiteral ID="lDocumentKey" runat="server" Label="Document Key" />
                            <Rock:RockLiteral ID="lRequestDate" runat="server" Label="Last Request Date" />
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
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lLeftDetails" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lRightDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnCancelView" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
