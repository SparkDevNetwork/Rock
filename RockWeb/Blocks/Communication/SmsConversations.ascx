<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsConversations.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsConversations" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNoNumbers" runat="server" NotificationBoxType="Warning" Text='No "SMS Phone Numbers" are available to view. Either there are none configured or you do not have access to them.' Visible="false"></Rock:NotificationBox>

        <div class="panel panel-block" runat="server" id="divMain" visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments"></i>SMS Conversations</h1>


                <div class="panel-labels">
                    <!--  style="position:absolute;right:15px;top:10px;" -->
                    <Rock:HighlightLabel ID="hlSmsNumber" runat="server" CssClass="pull-left input-width-lg input-xs" />
                    <asp:HiddenField ID="hfSmsNumber" runat="server" />
                    <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlSmsNumbers_SelectedIndexChanged" CssClass="pull-left input-width-lg input-xs" />

                    <a href="#" class="btn btn-xs btn-default btn-square pull-left margin-l-sm" onclick="$('.js-sms-configuration').slideToggle()">
                        <i class="fa fa-cog"></i>
                    </a>
                </div>
            </div>

            <div class="js-sms-configuration" style="display: none">
                <div class="well margin-b-none">
                    <div class="row">
                        <%-- The list of phone numbers that do not have "Enable Mobile Conversations" enabled --%>
                        <div class="col-md-3">
                            <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read Messages" OnCheckedChanged="tglShowRead_CheckedChanged" OnText="Yes" OffText="No" Checked="true" ButtonSizeCssClass="btn-sm" />
                        </div>
                        <div class="col-md-3">
                        </div>
                    </div>
                </div>

            </div>

            <div class="sms-conversations-container">
                <div class="conversation-list">
                    <div class="well">
                        <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-default btn-sm btn-square" OnClick="btnCreateNewMessage_Click" ToolTip="New Message"><i class="fa fa-edit"></i></asp:LinkButton>
                        <asp:LinkButton ID="lbPersonFilter" runat="server" CssClass="btn btn-default btn-sm btn-square pull-right" OnClientClick="$('.js-person-filter').slideToggle(); return false;"><i class="fa fa-filter"></i></asp:LinkButton>
                        <div id="divPersonFilter" runat="server" class="js-person-filter" style="display: none; padding-top: 10px;">
                            <Rock:PersonPicker ID="ppPersonFilter" runat="server" Label="Recipient" OnSelectPerson="ppPersonFilter_SelectPerson" />
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
                                            <Rock:HiddenFieldWithClass ID="hfMessageKey" runat="server" CssClass="js-messageKey"  />

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
                            <a href="#" class="conversation-back js-back pull-left margin-r-md">
                                <i class="fa fa-chevron-left"></i>
                            </a>
                            <asp:Literal ID="litSelectedRecipientDescription" runat="server"></asp:Literal>
                            <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible="false" CausesValidation="false" CssClass="btn btn-default btn-xs pull-right" OnClick="lbLinkConversation_Click"></asp:LinkButton>
                        </div>
                        <div class="messages-outer-container">
                            <div class="conversation">
                                <asp:Repeater ID="rptConversation" runat="server" OnItemDataBound="rptConversation_ItemDataBound" Visible="false">
                                    <ItemTemplate>
                                        <div class="message outbound" id="divCommunication" runat="server">
                                            <Rock:HiddenFieldWithClass ID="hfCommunicationRecipientId" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfCommunicationMessageKey" runat="server" />
                                            <div class="bubble" id="divCommunicationBody" runat="server"><asp:Literal ID="lSMSMessage" runat="server" /></div>
                                            <div class="message-meta">
                                                <span class="sender-name"><asp:Literal ID="lSenderName" runat="server" /></span>
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

                        <div class="footer">
                            <Rock:RockTextBox ID="tbNewMessage" runat="server" TextMode="multiline" Rows="1" Placeholder="Type a message" CssClass="js-input-message" Visible="false" autofocus></Rock:RockTextBox>
                            <Rock:BootstrapButton ID="btnSend" runat="server" CssClass="btn btn-primary js-send-text-button" Text="Send" DataLoadingText="Sending..." OnClick="btnSend_Click" Visible="false"></Rock:BootstrapButton>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

            </div>

            <script>
                Sys.Application.add_load(function () {
                    var objDiv = $(".messages-outer-container")[0];
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
                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" Placeholder="Type a message" Required="true" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Message is required" ValidateRequestMode="Disabled" />
            </Content>
        </Rock:ModalDialog>

        <%-- Link to Person --%>
        <Rock:ModalDialog ID="mdLinkToPerson" runat="server" Title="Link Phone Number to Person" OnSaveClick="mdLinkToPerson_SaveClick" ValidationGroup="vgLinkToPerson" >
            <Content>
                <asp:HiddenField ID="hfNamelessPersonId" runat="server" />

                <Rock:NotificationBox ID="nbAddPerson" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgLinkToPerson" />

                <Rock:Toggle ID="tglLinkPersonMode" runat="server" OnText="Link Existing Person" CssClass="margin-b-md" OffText="Add New Person" ActiveButtonCssClass="btn-primary" OnCheckedChanged="tglLinkPersonMode_CheckedChanged" />

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
