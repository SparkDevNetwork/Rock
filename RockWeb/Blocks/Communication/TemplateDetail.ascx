<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.TemplateDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:HiddenField ID="hfCommunicationTemplateId" runat="server" />

                <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.CommunicationTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Required="false" Label="Category" EntityTypeName="Rock.Model.CommunicationTemplate" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:ImageUploader ID="imgTemplatePreview" runat="server" Label="Template Preview Image" Help="The preview of this template to show when selecting a template for a new communication" />
                    </div>
                </div>

                <Rock:PanelWidget ID="pnlEmailTemplate" Title="Email" TitleIconCssClass="fa fa-envelope" CssClass="js-email-template" runat="server" Expanded="true">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFromName" runat="server" Label="From Name" Help="<small><span class='tip tip-lava'></span></small>" />
                        </div>
                        <div class="col-md-6">
                            <Rock:EmailBox ID="tbFromAddress" runat="server" Label="From Address" AllowMultiple="false" Help="<small><span class='tip tip-lava'></span></small>" AllowLava="true" />
                            <asp:HiddenField ID="hfShowAdditionalFields" runat="server" />
                            <div class="pull-right">
                                <a href="#" class="btn btn-xs btn-link js-show-additional-fields">Show Additional Fields</a>
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlEmailSummaryAdditionalFields" runat="server" CssClass="js-additional-fields" Style="display: none">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:EmailBox ID="tbReplyToAddress" runat="server" Label="Reply To Address" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:EmailBox ID="tbCCList" runat="server" Label="CC List" AllowMultiple="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:EmailBox ID="tbBCCList" runat="server" Label="BCC List" AllowMultiple="true" />
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

                    <label class="control-label">Message Template</label>
                    
                    <div class="well">
                    <Rock:Toggle ID="tglPreviewAdvanced" runat="server" CssClass="pull-right" OnText="Preview" OffText="Advanced" Checked="true" ButtonSizeCssClass="btn-xs" OnCssClass="btn-info" OffCssClass="btn-info" OnCheckedChanged="tglPreviewAdvanced_CheckedChanged" />
                    
                    <asp:Panel ID="pnlAdvanced" runat="server" CssClass="margin-t-md">
                        <div class="row">
                            <div class="col-md-8">
                                <div class="js-help-container">
                                    <a class="help" href="javascript: $('.js-template-help').toggle;"><i class="fa fa-question-circle"></i></a>        
                                    <div class="alert alert-info js-template-help" id="nbTemplateHelp" runat="server" style="display: none;"></div>                                
                                </div>
                                <Rock:CodeEditor ID="ceEmailTemplate" runat="server" EditorHeight="400" EditorMode="Html" />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockCheckBox ID="cbCssInliningEnabled" runat="server" Text="CSS Inlining Enabled" Help="Enable CSS Inlining to move styles to inline attributes. This can help maximize compatibility with email clients." />
                                <Rock:KeyValueList ID="kvlMergeFields" runat="server" Label="Lava Fields" KeyPrompt="Key" Help="Add any fields and their default values that can be used as lava merge fields within the template html. Any fields with a 'Color' suffix will use a Color Picker as the value editor." ValuePrompt="Default Value" />
                                
                                <asp:LinkButton ID="lbUpdateLavaFields" runat="server" Text="Update Lava Fields" CssClass="btn btn-xs btn-action" OnClick="lbUpdateLavaFields_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlPreview" runat="server" CssClass="margin-t-md">
                        <asp:UpdatePanel ID="upnlEmailPreview" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnlEmailPreview" runat="server" Visible="false">
                                    <div class="row">
                                        <div class="col-md-9">
                                            <div id="pnlEmailPreviewContainer" runat="server" class="email-preview js-email-preview device-browser center-block">
                                                <iframe id="ifEmailPreview" name="emailpreview-iframe" class="emaileditor-iframe js-emailpreview-iframe email-wrapper" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" frameborder="0" border="0" cellspacing="0" scrolling="yes"></iframe>
                                            </div>
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:RockControlWrapper ID="rcwPreviewMode" runat="server" Label="Preview Mode">
                                                
                                                <div class="btn-group" role="group">
                                                    <button type="button" class="btn btn-xs btn-info active js-preview-desktop">
                                                        <i class="fa fa-desktop"></i>
                                                        Desktop
                                                    </button>
                                                    <button type="button" class="btn btn-xs btn-default js-preview-mobile">
                                                        <i class="fa fa-mobile"></i>
                                                        Mobile
                                                    </button>
                                                </div>
                                            </Rock:RockControlWrapper>

                                            <asp:Panel ID="pnlTemplateLogo" runat="server" class="row">
                                                <div class="col-md-6">
                                                    <Rock:ImageUploader ID="imgTemplateLogo" runat="server" Label="Logo" Help="The Logo that can be included in the contents of the message" OnImageUploaded="imgTemplateLogo_ImageUploaded" OnImageRemoved="imgTemplateLogo_ImageUploaded" />
                                                </div>
                                            </asp:Panel>

                                            <asp:HiddenField ID="hfLavaFieldsState" runat="server" />
                                            <asp:PlaceHolder ID="phLavaFieldsControls" runat="server" />
                                            <asp:LinkButton ID="btnUpdateTemplatePreview" runat="server" CssClass="btn btn-xs btn-action" Text="Update" OnClick="btnUpdateTemplatePreview_Click" CausesValidation="false" />
                                        </div>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </asp:Panel>

                    </div>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pnlSMSTemplate" Title="SMS" TitleIconCssClass="fa fa-mobile-phone" CssClass="js-sms-template" runat="server">
                    <h2>SMS</h2>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DefinedValuePicker ID="dvpSMSFrom" runat="server" Label="From" Help="The number to originate message from (configured under Admin Tools > Communications > SMS Phone Numbers)." />
                            <Rock:RockControlWrapper ID="rcwSMSMessage" runat="server" Label="Message" Help="<span class='tip tip-lava'></span>">
                                <Rock:MergeFieldPicker ID="mfpSMSMessage" runat="server" CssClass="margin-b-sm pull-right" OnSelectItem="mfpMessage_SelectItem" />
                                <asp:HiddenField ID="hfSMSCharLimit" runat="server" />
                                <asp:Label ID="lblSMSMessageCount" runat="server" CssClass="badge margin-all-sm pull-right" />
                                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" />
                                <Rock:NotificationBox ID="nbSMSTestResult" CssClass="margin-t-md" runat="server" NotificationBoxType="Success" Text="Test SMS has been sent." Visible="false" />
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

        <script>

            Sys.Application.add_load(function () {
                if ($('#<%=pnlEmailPreview.ClientID%>').length) {
                    var $emailPreviewIframe = $('.js-emailpreview-iframe');
                    var $previewModal = $('#<%=pnlEmailPreview.ClientID%>');

                    // set opacity to 0 to hide flicker when loading
                    $previewModal.fadeTo(0, 0);

                    $emailPreviewIframe.height('auto');

                    $emailPreviewIframe.load(function () {
                        new ResizeSensor($('#<%=pnlEmailPreviewContainer.ClientID%>'), function () {
                            $('#<%=ifEmailPreview.ClientID%>', window.parent.document).height($('#<%=pnlEmailPreviewContainer.ClientID%>').height());
                        });

                        var newHeight = $(this.contentWindow.document).height();
                        if ($(this).height() != newHeight) {
                            $(this).height(newHeight);
                        }

                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(newHeight);

                        if ($previewModal.is(':visible')) {
                            // set opacity back to 1 now that it is loaded and sized
                            $previewModal.fadeTo(0, 1);
                        }
                    });
                }

                $('.js-show-additional-fields').off('click').on('click', function () {
                    var isVisible = !$('.js-additional-fields').is(':visible');
                    $('#<%=hfShowAdditionalFields.ClientID %>').val(isVisible);
                    $('.js-show-additional-fields').text(isVisible ? 'Hide Additional Fields' : 'Show Additional Fields');
                    $('.js-additional-fields').slideToggle();
                    return false;
                });

                if ($('#<%=hfShowAdditionalFields.ClientID %>').val() == "true") {
                    $('.js-additional-fields').show();
                    $('.js-show-additional-fields').text('Hide Additional Fields');
                }

                // resize the email preview when the Mobile/Desktop modes are clicked
                $('.js-preview-mobile, .js-preview-desktop').off('click').on('click', function (a, b, c) {
                    var $emailPreviewIframe = $('.js-emailpreview-iframe');

                    if ($(this).hasClass('js-preview-mobile')) {
                        $('.js-preview-mobile').removeClass('btn-default').addClass('btn-info').addClass('active')
                        $('.js-preview-desktop').removeClass('btn-info').removeClass('active').addClass('btn-default');
                        var mobileContainerHeight = '585px';

                        $('.js-email-preview').removeClass("device-browser").addClass("device-mobile");
                        var mobilePreviewHeight = $('.js-email-preview').height();

                        $emailPreviewIframe.height(mobileContainerHeight);
                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(mobileContainerHeight);
                    }
                    else {
                        $('.js-preview-desktop').removeClass('btn-default').addClass('btn-info').addClass('active')
                        $('.js-preview-mobile').removeClass('btn-info').removeClass('active').addClass('btn-default');
                        $('.js-email-preview').removeClass("device-mobile").addClass("device-browser");
                        $emailPreviewIframe.height('auto');

                        var emailPreviewIframe = $emailPreviewIframe[0];
                        var newHeight = $(emailPreviewIframe.contentWindow.document).height();
                        if ($(emailPreviewIframe).height() != newHeight) {
                            $(emailPreviewIframe).height(newHeight);
                        }

                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(newHeight);
                    }
                });

                $('.js-revertlavavalue').off('click').on('click', function () {
                    var valueControlId = $(this).attr('data-value-control');
                    var defaultValue = $(this).attr('data-default');
                    var $colorPicker = $('#' + valueControlId).closest('.rock-colorpicker-input');
                    if ($colorPicker.length) {
                        $colorPicker.colorpicker('setValue', defaultValue);
                    }
                    else {
                        $('#' + valueControlId).val(defaultValue);
                    }
                    $(this).css('visibility', 'hidden');
                });
            });

            function removeAttachment(source, hf, fileId) {
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


