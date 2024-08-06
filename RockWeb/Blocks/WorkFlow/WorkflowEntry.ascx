<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div id="divForm" runat="server" class="col-md-6">
                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lIconHtml" runat="server"><i class="fa fa-gear"></i></asp:Literal>
                            <asp:Literal ID="lTitle" runat="server">Workflow Entry</asp:Literal>
                        </h1>
                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlblWorkflowId" runat="server" LabelType="Info" />
                            <Rock:HighlightLabel ID="hlblDateAdded" runat="server" LabelType="Default" />
                        </div>
                    </div>
                    <div class="panel-body">

                        <asp:Literal ID="lSummary" runat="server" Visible="false" />

                        <asp:Panel ID="pnlWorkflowUserForm" CssClass="workflow-entry-panel" runat="server">

                            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                            <asp:Literal ID="lFormHeaderText" runat="server" />

                            <%-- Person Entry --%>
                            <asp:Panel ID="pnlPersonEntrySection" runat="server">
                                <asp:Literal ID="lPersonEntryPreHtml" runat="server" />
                                <asp:Literal ID="lPersonEntrySectionHeaderHtml" runat="server" />

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:CampusPicker ID="cpPersonEntryCampus" runat="server" Required="true" FormGroupCssClass="field-campus" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>

                                <%-- Special input with rock-fullname class --%>
                                <Rock:RockTextBox ID="tbRockFullName" runat="server" CssClass="rock-fullname" ValidationGroup="vgRockFullName" Placeholder="Please enter name (Required)" autocomplete="new-password" />
                                <Rock:NotificationBox ID="nbRockFullName" runat="server" NotificationBoxType="Validation" />

                                <div class="fieldgroup-personentry">
                                    <asp:Panel ID="pnlPersonEntryRow1" runat="server" CssClass="row">
                                        <%-- Person 1 --%>
                                        <asp:Panel ID="pnlPersonEntryRow1Column1" runat="server" CssClass="col-md-6">
                                            <div class="fieldgroup-person">
                                                <Rock:PersonBasicEditor ID="pePerson1" runat="server" />
                                            </div>
                                        </asp:Panel>

                                        <%-- Person 2 (Spouse) --%>
                                        <asp:Panel ID="pnlPersonEntryRow1Column2" runat="server" CssClass="col-md-6">
                                            <div class="fieldgroup-spouse">
                                                <Rock:PersonBasicEditor ID="pePerson2" runat="server" />
                                            </div>
                                        </asp:Panel>
                                    </asp:Panel>
                                </div>

                                <Rock:RockCheckBox ID="cbShowPerson2" runat="server" Text="Show Person2" Checked="false" AutoPostBack="true" OnCheckedChanged="cbShowPerson2_CheckedChanged" />

                                <%-- Person Entry Address and Marital Status --%>
                                <asp:Panel ID="pnlPersonEntryRow2" CssClass="row" runat="server">
                                    <asp:Panel ID="pnlPersonEntryRow2Column1" runat="server" CssClass="col-md-6">
                                        <Rock:AddressControl ID="acPersonEntryAddress" runat="server" Label="Address" />
                                    </asp:Panel>
                                    <asp:Panel ID="pnlPersonEntryRow2Column2" runat="server" CssClass="col-md-6">
                                        <Rock:DefinedValuePicker runat="server" ID="dvpMaritalStatus" Label="Marital Status" />
                                    </asp:Panel>
                                </asp:Panel>

                                <asp:Literal ID="lPersonEntryPostHtml" runat="server" />
                            </asp:Panel>

                            <%-- Workflow Form Attribute Controls  --%>
                            <asp:PlaceHolder ID="phWorkflowFormAttributes" runat="server" />

                            <asp:Literal ID="lFormFooterText" runat="server" />

                            <div class="actions">
                                <asp:PlaceHolder ID="phActions" runat="server" />
                                <div id="pnlCaptcha" runat="server" class="form-group">
                                    <Rock:Captcha ID="cpCaptcha" runat="server" OnTokenReceived="cpCaptcha_TokenReceived" />
                                </div>
                            </div>

                        </asp:Panel>

                        <%-- Electronic Signature UI --%>
                        <asp:Panel ID="pnlWorkflowActionElectronicSignature" runat="server" CssClass="js-validation-group" Visible="false">


                            <%-- Put the signature document html in an Iframe so it doesn't inherit styling from the page --%>
                            <div class="styled-scroll">
                                <asp:Panel ID="pnlIframeSignatureDocumentHTML" class="signaturedocument-container" runat="server">
                                    <iframe id="iframeSignatureDocumentHTML" name="signature-document-html-iframe" class="signaturedocument-iframe js-signaturedocument-iframe" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" style="width: 100%"></iframe>
                                </asp:Panel>
                            </div>
                            <Rock:ElectronicSignatureControl ID="escElectronicSignatureControl" runat="server" OnCompleteSignatureClicked="btnSignSignature_Click" CssClass="well" />

                            <script type="text/javascript">
                                function resizeIframe(el) {
                                    el.style.height = el.contentWindow.document.documentElement.scrollHeight + 'px';
                                }
                            </script>
                        </asp:Panel>

                        <%-- This needs a 'js-workflow-entry-message-notification-box' javascript hook so that Rock.Workflow.Action.ShowHtml can find it.--%>
                        <Rock:NotificationBox ID="nbMessage" runat="server" Dismissable="true" CssClass="margin-t-lg js-workflow-entry-message-notification-box" />
                    </div>

                </div>

            </div>

            <div id="divWorkflowActionUserFormNotes" runat="server" class="col-md-6">
                <Rock:NoteContainer ID="ncWorkflowNotes" runat="server" NoteLabel="Note"
                    ShowHeading="true" Title="Notes" TitleIconCssClass="fa fa-comment"
                    DisplayType="Full" UsePersonIcon="false" ShowAlertCheckBox="true"
                    ShowPrivateCheckBox="false" ShowSecurityButton="false"
                    AllowAnonymousEntry="false" AddAlwaysVisible="false"
                    SortDirection="Descending" />
            </div>

        </div>

        <script>
            function handleWorkflowActionButtonClick (validationGroup, causesValidation) {
                if (causesValidation) {
                    // make sure page is valid before doing the postback (from this button's href)
                    if (!Page_ClientValidate(validationGroup)) {
                        return false;
                    }
                }

                return true;
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
