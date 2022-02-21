<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentTemplateDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSignatureDocumentTemplateId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-signature"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbError" runat="server" Text="Error Occurred trying to retrieve templates" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Model.SignatureDocumentTemplate, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Model.SignatureDocumentTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="2" ValidateRequestMode="Disabled" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbDocumentTerm" runat="server" Label="Document Term" MaxLength="100" Help="How the document should be referred to (e.g Waiver, Contract, Statement, etc.)" />
                            <Rock:RockRadioButtonList ID="rblSignatureType" runat="server" Label="Signature Input Type" Help="The input type for the signature. Drawn will display an area where the individual can use the mouse or a finger to draw a representation of their signature. Typed will allow them to type their name as their digital signature. Both are legally acceptable in the US and Canada. The drawn value is considered Personally identifiable information (PII) and is more sensive to keep. It is encrypted in the database." RepeatDirection="Horizontal" />
                        </div>
                        <div class="col-md-6">
                            <Rock:BinaryFileTypePicker ID="bftpFileType" runat="server" Label="File Type" Required="true" Help="Determines which file type is used when storing the signed document" />
                            <Rock:RockDropDownList ID="ddlCompletionSystemCommunication" runat="server" Label="Completion Email Template" Help="The email template to use when sending the signed document upon completion." />
                        </div>
                    </div>

                    <p class="text-right">
                        <small><a href="#" onclick="$('.js-template-tips').slideToggle();return false;" class="">Template Tips</a></small>
                    </p>
                    
                    <div class="js-template-tips well" style="display: none">
                        <h2>Template Tips</h2>
                        <p>Below are some tips to assist you in your template creation. The merge fields that you use to customize your templates will vary depending on where they are being used.</p>

                        <label>Merge Fields for Workflow Electronic Signatures</label>
                        <p>Below are some common merge fields for templates used for the electronic signature workflow action. Note that the attribute keys will need to map to what you have configured in your workflow template.</p>
                        <div class='row'>
                            <div class='col-md-6'>
                                {{ Workflow | Attribute:'SignedByPerson' }}<br>
                                {{ Workflow | Attribute:'AppliesToPerson' }}<br>
                                {{ Workflow | Attribute:'AssignedToPerson' }}<br>
                            </div>
                            <div class='col-md-6'>

                            </div>
                        </div>

                        <br />

                        <label>Merge Fields for Event Registration</label>
                        <p>Below are some common merge fields for templates used for event registration. Again, the attribute value keys will be different in your registration.</p>
                        <div class='row'>
                            <div class='col-md-6'>
                                <p><b>Registrant Fields</b></p>
                                {{ Registrant.FirstName }}<br />
                                {{ Registrant.LastName }}
                            </div>
                            <div class='col-md-6'>
                                <p><b>Registration Fields</b></p>
                                {{ Registration | Attribute:'VehicleDescription' }}
                            </div>
                        </div>

                    </div>


                    <div class="well">

                        <Rock:NotificationBox ID="nbSignatureLavaTemplateWarning" runat="server" NotificationBoxType="Validation" Text="Lava Template is required." Visible="false" />

                        <div class="clearfix">
                            <Rock:Toggle ID="tglTemplatePreview" runat="server" CssClass="pull-right" OnText="Preview" OffText="Edit" Checked="false" ButtonSizeCssClass="btn-xs" OnCssClass="btn-info" OffCssClass="btn-info" OnCheckedChanged="tglTemplatePreview_CheckedChanged" />
                        </div>
                        
                        <Rock:CodeEditor ID="ceESignatureLavaTemplate" runat="server" EditorMode="Lava"  Label="Lava Template" Help="The Lava template that makes up the body of the document." EditorHeight="500" />
                        <Rock:RockControlWrapper ID="rcwSignatureDocumentPreview" runat="server" Visible="false" Label="Lava Template Preview" Help="Preview of Signature Template as a PDF">

                        <Rock:PDFViewer ID="pdfSignatureDocumentPreview" runat="server" />
                    </Rock:RockControlWrapper>

                    </div>
                    


                    <%-- Legacy Signature Provider Settings --%>
                    <asp:Panel ID="pnlLegacySignatureProviderSettings" runat="server" class="well">
                        <label>Legacy Signature Provider Settings</label>
                        <span>Support for these providers will be fully removed in the next full release.</span>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ComponentPicker ID="cpExternalProvider" runat="server" ContainerType="Rock.Security.DigitalSignatureContainer, Rock" Label="External Digital Signature Provider"
                                    OnSelectedIndexChanged="cpExternalProvider_SelectedIndexChanged" AutoPostBack="true" Required="false" Help="This will be obsolete in a future version of Rock. Leave this blank to use the Rock's built-in Electronic Signature." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlExternalProviderTemplate" runat="server" Label="External Provider Template" Help="A template that has been created with your digital signature provider" Required="false" Visible="true" />
                            </div>
                        </div>

                    </asp:Panel>


                    <div class="actions">
                        <asp:LinkButton ID="btnSaveType" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveType_Click" />
                        <asp:LinkButton ID="btnCancelType" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelType_Click" />
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
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
