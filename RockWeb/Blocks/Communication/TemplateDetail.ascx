<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:HiddenField ID="hfCommunicationTemplateId" runat="server" />

                <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Name" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:ImageUploader ID="imgTemplatePreview" runat="server" Label="Template Preview Image" Help="The preview of this template to show when selecting a template for a new communication" />
                    </div>
                </div>

                <asp:Panel ID="pnlEmailTemplate" CssClass="js-email-template" runat="server">
                    <h2>Email</h2>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFromName" runat="server" Label="From Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFromAddress" runat="server" Label="From Address" />
                            <asp:HiddenField ID="hfShowAdditionalFields" runat="server" />
                            <div class="pull-right">
                                <a href="#" class="btn btn-xs btn-link js-show-additional-fields">Show Additional Fields</a>
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlEmailSummaryAdditionalFields" runat="server" CssClass="js-additional-fields" Style="display: none">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbReplyToAddress" runat="server" Label="Reply To Address" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbCCList" runat="server" Label="CC List" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbBCCList" runat="server" Label="BCC List" />
                            </div>
                        </div>
                    </asp:Panel>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEmailSubject" runat="server" Label="Email Subject" />
                            <asp:UpdatePanel ID="upFileAttachments" runat="server">
                                <ContentTemplate>
                                    <asp:HiddenField ID="hfAttachedBinaryFileIds" runat="server" />
                                    <Rock:FileUploader ID="fupAttachments" runat="server" Label="Attachments" OnFileUploaded="fupAttachments_FileUploaded" />
                                    <asp:Literal ID="lAttachmentListHtml" runat="server" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <div class="col-md-6">
                            
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceEmailTemplate" runat="server" Label="Message Template" EditorHeight="400" EditorMode="Html" />
                </asp:Panel>

                <asp:Panel ID="pnlSMSTemplate" CssClass="js-sms-template" runat="server">
                    <h2>SMS</h2>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlSMSFrom" runat="server" Label="From" Help="The number to originate message from (configured under Admin Tools > General Settings > Defined Types > SMS From Values)." />
                            <Rock:RockControlWrapper ID="rcwSMSMessage" runat="server" Label="Message" Help="<span class='tip tip-lava'></span>">
                                <Rock:MergeFieldPicker ID="mfpSMSMessage" runat="server" CssClass="margin-b-sm pull-right" OnSelectItem="mfpMessage_SelectItem"  />
                                <asp:HiddenField ID="hfSMSCharLimit" runat="server" />
                                <asp:Label ID="lblSMSMessageCount" runat="server" CssClass="badge margin-all-sm pull-right" />
                                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" />
                                <Rock:NotificationBox ID="nbSMSTestResult" CssClass="margin-t-md" runat="server" NotificationBoxType="Success" Text="Test SMS has been sent." Visible="false" />
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

        <script>

            Sys.Application.add_load(function ()
            {
                $('.js-show-additional-fields').off('click').on('click', function ()
                {
                    $('#<%=hfShowAdditionalFields.ClientID %>').val(!$('.js-addition-fields').is(':visible'));
                    $('.js-additional-fields').slideToggle();
                });

                if ($('#<%=hfShowAdditionalFields.ClientID %>').val() == "true") {
                    $('.js-additional-fields').show();
                }
            });

            function removeAttachment(source, hf, fileId)
            {
                // Get the attachment list
                var $hf = $('#' + hf);
                var fileIds = $hf.val().split(',');

                // Remove the selected attachment 
                var removeAt = $.inArray(fileId, fileIds);
                fileIds.splice(removeAt, 1);
                $hf.val(fileIds.join());

                // Remove parent <li>
                $(source).closest($('li')).remove();
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>


