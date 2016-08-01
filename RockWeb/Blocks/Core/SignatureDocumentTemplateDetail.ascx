<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentTemplateDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSignatureDocumentTemplateId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa fa-pencil-square-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbError" runat="server" Text="Error Occurred trying to retrieve templates" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Model.SignatureDocumentTemplate, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Model.SignatureDocumentTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:ComponentPicker ID="cpProvider" runat="server" ContainerType="Rock.Security.DigitalSignatureContainer, Rock" Label="Digital Signature Provider" OnSelectedIndexChanged="cpProvider_SelectedIndexChanged" AutoPostBack="true" Required="true" />
                            <Rock:RockDropDownList ID="ddlTemplate" runat="server" Label="Template" Help="A template that has been created with your digital signature provider" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:BinaryFileTypePicker ID="bftpFileType" runat="server" Label="File Type" Required="true" 
                                Help="The file type to use when saving signed documents of this type." />
                            <Rock:RockDropDownList ID="ddlSystemEmail" runat="server" Label="Invite Email" Required="true"
                                Help="The System Email that should be sent when requesting a signature for documents of this type." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSaveType" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveType_Click" />
                        <asp:LinkButton ID="btnCancelType" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelType_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row margin-b-md">
                        <div class="col-md-12">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lLeftDetails" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lRightDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
