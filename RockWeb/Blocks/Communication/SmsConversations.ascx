<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsConversations.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsConversations" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNoNumbers" runat="server" NotificationBoxType="Warning" Text='No "SMS Phone Numbers" are available to view. Either there are none configured or you do not have access to them.' Visible="false"></Rock:NotificationBox>

        <div class="panel panel-block" runat="server" id="divMain" visible="false">

            <div class="panel-heading panel-follow">
                <h1 class="panel-title"><i class="fa fa-comments"></i>SMS Conversations</h1>


                <div class="panel-labels m-0">
                    <!--  style="position:absolute;right:15px;top:10px;" -->
                    <Rock:HighlightLabel ID="hlSmsNumber" runat="server" CssClass="pull-left input-width-lg input-xs" />
                    <asp:HiddenField ID="hfSmsNumber" runat="server" />
                    <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlSmsNumbers_SelectedIndexChanged" CssClass="pull-left input-width-lg input-xs" />

                    <a href="#" class="btn btn-xs btn-default btn-square ml-2 pull-left" onclick="$('.js-sms-configuration').slideToggle()">
                        <i class="fa fa-cog"></i>
                    </a>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <div class="js-sms-configuration" style="display: none">
                <div class="well mb-0 border-top-0 border-right-0 border-left-0 rounded-0">
                    <div class="row">
                        <div class="col-md-3">
                            <%-- The list of phone numbers that do not have "Enable Mobile Conversations" enabled --%>
                            <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read Messages" OnCheckedChanged="tglShowRead_CheckedChanged" OnText="Yes" OffText="No" Checked="true" ButtonSizeCssClass="btn-sm" />
                        </div>
                    </div>
                </div>

            </div>

            <div class="sms-conversations-parent">
                <div class="sms-conversations-container styled-scroll">
                    <div class="conversation-list d-flex flex-column">
                        <div class="header">
                            <div class="clearfix">
                                <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-default btn-sm btn-square" OnClick="btnCreateNewMessage_Click" ToolTip="New Message"><i class="fa fa-edit"></i></asp:LinkButton>
                                <asp:LinkButton ID="lbPersonFilter" runat="server" CssClass="btn btn-default btn-sm btn-square pull-right" OnClientClick="$('.js-person-filter').slideToggle(); return false;"><i class="fa fa-filter"></i></asp:LinkButton>
                            </div>
                            <div id="divPersonFilter" runat="server" class="js-person-filter" style="display: none;">
                                <Rock:PersonPicker ID="ppPersonFilter" runat="server" Label="Recipient" OnSelectPerson="ppPersonFilter_SelectPerson" FormGroupCssClass="mt-2 mb-0" />
                            </div>
                        </div>
                        <asp:UpdatePanel ID="upRecipients" runat="server" class="overflow-scroll">
                            <ContentTemplate>
                                <Rock:Grid ID="gRecipients" runat="server" OnRowSelected="gRecipients_RowSelected" OnRowDataBound="gRecipients_RowDataBound" ShowHeader="false" ShowActionRow="false" DisplayType="Light" EnableResponsiveTable="False">
                                    <Columns>
                                        <Rock:RockBoundField DataField="RecipientPersonAliasId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>
                                                <Rock:HiddenFieldWithClass ID="hfRecipientPersonAliasId" runat="server" CssClass="js-recipientId" />
                                                <Rock:HiddenFieldWithClass ID="hfMessageKey" runat="server" CssClass="js-messageKey" />

                                                <div class="layout-row">
                                                    <asp:Label ID="lblName" runat="server" Class="sms-name" />
                                                    <div class="date">
                                                        <asp:Literal ID="litDateTime" runat="server" />
                                                    </div>
                                                </div>
                                                <div class="message-truncate">
                                                    <asp:Literal ID="litMessagePart" runat="server" />
                                                </div>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                    </Columns>
                                </Rock:Grid>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>

                    <asp:UpdatePanel ID="upConversation" runat="server" class="conversation-panel">
                        <ContentTemplate>
                            <Rock:HiddenFieldWithClass ID="hfSelectedRecipientPersonAliasId" runat="server" CssClass="js-selected-recipient-id" />
                            <Rock:HiddenFieldWithClass ID="hfSelectedMessageKey" runat="server" CssClass="js-selected-message-key" />
                            <div class="header">
                                <a href="#" class="conversation-back js-back pull-left mr-3">
                                    <i class="fa fa-chevron-left"></i>
                                </a>
                                <asp:Literal ID="litSelectedRecipientDescription" runat="server"></asp:Literal>
                                <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible="false" CausesValidation="false" CssClass="btn btn-default btn-xs pull-right" OnClick="lbLinkConversation_Click"></asp:LinkButton>
                                <asp:LinkButton ID="lbViewMergeRequest" runat="server" Text="View Merge Request" Visible="false" CausesValidation="false" CssClass="btn btn-default btn-xs pull-right" OnClick="lbViewMergeRequest_Click"></asp:LinkButton>
                            </div>
                            <div class="messages-outer-container">
                                <div class="messages-scroll-container">
                                    <div class="conversation">
                                        <asp:Repeater ID="rptConversation" runat="server" OnItemDataBound="rptConversation_ItemDataBound" Visible="false">
                                            <ItemTemplate>
                                                <div class="message outbound" id="divCommunication" runat="server">
                                                    <Rock:HiddenFieldWithClass ID="hfCommunicationRecipientId" runat="server" />
                                                    <Rock:HiddenFieldWithClass ID="hfCommunicationMessageKey" runat="server" />
                                                    <%-- Keep divCommunicationBody and lSMSMessage on same line for rendering --%>
                                                    <div class="bubble" id="divCommunicationBody" runat="server"><asp:Literal ID="lSMSMessage" runat="server" /></div>
                                                    <div id="divCommicationAttachments" runat="server">
                                                        <asp:Literal ID="lSMSAttachments" runat="server" /></div>
                                                    <div class="message-meta">
                                                        <span class="sender-name">
                                                            <asp:Literal ID="lSenderName" runat="server" /></span>
                                                        <asp:Label ID="lblMessageDateTime" runat="server" CssClass="date" />
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:Label ID="lblNoConversationsFound" runat="server" Visible='<%# rptConversation.Items.Count == 0 %>' Text="<tr><td>No conversations found.</td></tr>" CssClass="text-muted" />
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                                <div class="js-send-image sms-image-uploader-container" style="display: none;">
                                    <asp:LinkButton ID="lbDismiss" runat="server" CssClass="close" OnClientClick="$('.js-send-image').slideToggle(); return false;"><i class="fa fa-times"></i></asp:LinkButton>
                                    <div class="sms-image-uploader">
                                        <Rock:ImageUploader ID="ImageUploaderConversation" runat="server" BinaryFileTypeGuid="<%# new Guid( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT ) %>" Help="Optional image to include in the message." Label="Image" />
                                    </div>
                                </div>

                            </div>
                            <div class="js-notecontainer">
                                <Rock:NoteEditor ID="noteEditor" runat="server" CssClass="sms-conversation-note-editor" Visible="false" />
                            </div>
                            <div class="footer">
                                <Rock:RockTextBox ID="tbNewMessage" runat="server" TextMode="multiline" Rows="1" Placeholder="Type a message" CssClass="js-input-message" Visible="false" autofocus></Rock:RockTextBox>
                                <div class="actions">
                                    <asp:LinkButton ID="btnEditNote" runat="server" CssClass="btn btn-default btn-square edit-note-button" OnClick="btnEditNote_Click" ToolTip="Add Note" ><i class="fa fa-edit"></i></asp:LinkButton>
                                    <asp:LinkButton ID="lbShowImagePicker" runat="server" CssClass="btn btn-default image-button btn-square" OnClientClick="$('.js-send-image').slideToggle(); return false;"><i class="fa fa-camera"></i></asp:LinkButton>
                                    <Rock:BootstrapButton ID="btnSend" runat="server" CssClass="btn btn-primary send-button js-send-text-button" Text="Send" DataLoadingText="Sending..." OnClick="btnSend_Click" Visible="false" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

            <script>
                Sys.Application.add_load(function () {
                    Rock.controls.fullScreen.initialize('body');
                    var objDiv = $(".messages-scroll-container")[0];
                    objDiv.scrollTop = objDiv.scrollHeight;

                    $("#<%=upConversation.ClientID %> .js-back").on("click", function () {
                        $('#<%=upConversation.ClientID %>').removeClass("has-focus");
                        return false;
                    });
                });

                var yPos;
                var prm = Sys.WebForms.PageRequestManager.getInstance();

                function BeginRequestHandler(sender, args) {
                    if ($('#<%=upRecipients.ClientID %>') != null) {
                        // Get X and Y positions of scrollbar before the partial postback
                        yPos = $('#<%=upRecipients.ClientID %>').scrollTop();
                    }
                }

                function EndRequestHandler(sender, args) {
                    if ($('#<%=upRecipients.ClientID %>') != null) {
                        // Set X and Y positions back to the scrollbar
                        // after partial postback
                        $('#<%=upRecipients.ClientID %>').scrollTop(yPos);
                        $('#<%=tbNewMessage.ClientID %>').focus();
                    }
                }

                prm.add_beginRequest(BeginRequestHandler);
                prm.add_endRequest(EndRequestHandler);
            </script>
        </div>
        <%-- End panel-block --%>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdNewMessage" runat="server" Title="New Message" OnSaveClick="mdNewMessage_SaveClick" SaveButtonText="Send" ValidationGroup="vgMobileTextEditor">
            <Content>
                <asp:ValidationSummary ID="vsMobileTextEditor" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgMobileTextEditor" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbNoSms" runat="server" Text="The selected person does not have an SMS enabled Phone number." Dismissable="true" Visible="false" NotificationBoxType="Warning"></Rock:NotificationBox>
                <div class="form-group">
                    <label runat="server" id="lblFromNumber" class="control-label">From</label>
                    <div>
                        <asp:Label ID="lblMdNewMessageSendingSMSNumber" runat="server" />
                    </div>
                </div>

                <%-- person picker --%>
                <Rock:PersonPicker ID="ppRecipient" runat="server" Label="Recipient" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Please select an SMS recipient." Required="true" OnSelectPerson="ppRecipient_SelectPerson" />

                <%-- multi-line textbox --%>
                <div class="form-group">
                    <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" Placeholder="Type a message" Required="true" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Message is required" ValidateRequestMode="Disabled" />
                </div>

                <%-- image uploader --%>
                <Rock:ImageUploader ID="ImageUploaderModal" runat="server" BinaryFileTypeGuid="<%# new Guid( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT ) %>" Help="Optional image to include in the message." Label="Image" />
            </Content>
        </Rock:ModalDialog>

        <%-- Link to Person --%>
        <Rock:ModalDialog ID="mdLinkToPerson" runat="server" Title="Link Phone Number to Person" OnSaveClick="mdLinkToPerson_SaveClick" ValidationGroup="vgLinkToPerson">
            <Content>
                <asp:HiddenField ID="hfNamelessPersonId" runat="server" />

                <Rock:NotificationBox ID="nbMergeRequestCreated" runat="server" Heading="To prevent data loss and to ensure the highest level of security, a merge request will be created upon pressing Save." NotificationBoxType="Info" Visible="true" />
                <Rock:NotificationBox ID="nbAddPerson" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgLinkToPerson" />

                <Rock:Toggle ID="tglLinkPersonMode" runat="server" OnText="Link Existing Person" CssClass="mb-3" OffText="Add New Person" ActiveButtonCssClass="btn-primary" OnCheckedChanged="tglLinkPersonMode_CheckedChanged" />

                <asp:Panel ID="pnlLinkToNewPerson" runat="server">
                    <Rock:PersonBasicEditor ID="newPersonEditor" runat="server" ValidationGroup="vgLinkToPerson" />
                </asp:Panel>

                <asp:Panel ID="pnlLinkToExistingPerson" runat="server" Visible="false">
                    <fieldset>
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="vgLinkToPerson" />
                    </fieldset>
                </asp:Panel>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
