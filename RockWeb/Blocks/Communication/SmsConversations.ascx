<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsConversations.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsConversations" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNoNumbers" runat="server" NotificationBoxType="Warning" Text='No "SMS Phone Numbers" are available to view. Either there are none configured or you do not have access to them.' Visible="false"></Rock:NotificationBox>

        <div class="panel panel-block" runat="server" id="divMain" visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments"></i> SMS Conversations</h1>


                <div class="panel-labels"> <!--  style="position:absolute;right:15px;top:10px;" -->
                <Rock:HighlightLabel ID="hlSmsNumber" runat="server" CssClass="pull-left input-width-lg input-xs" />
                <asp:HiddenField ID="hfSmsNumber" runat="server" />
                <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlSmsNumbers_SelectedIndexChanged" CssClass="pull-left input-width-lg input-xs" />

                    <a href="#" class="btn btn-xs btn-default pull-left margin-l-sm" onclick="$('.js-sms-configuration').toggle()">
                        <i class="fa fa-cog"></i>
                    </a>
                </div>
            </div>

            <div class="js-sms-configuration" style="display: none">
                <div class="well margin-b-none"><div class="row">
                    <%-- The list of phone numbers that do not have "Enable Mobile Conversations" enabled --%>
                    <div class="col-md-3">
                        <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read Messages" OnCheckedChanged="tglShowRead_CheckedChanged" OnText="Yes" OffText="No" Checked="true" ButtonSizeCssClass="btn-sm" />
                    </div>
                    <div class="col-md-3">
                    </div></div>
                </div>

            </div>

            <div class="sms-conversations-container">
                <div class="conversation-list">
                <div class="header">
                    <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-default btn-sm btn-square" OnClick="btnCreateNewMessage_Click" ToolTip="New Message"><i class="fa fa-edit"></i></asp:LinkButton>
                </div>
                    <asp:UpdatePanel ID="upRecipients" runat="server" class="overflow-scroll">
                        <ContentTemplate>
                            <Rock:Grid ID="gRecipients" runat="server" OnRowSelected="gRecipients_RowSelected" OnRowDataBound="gRecipients_RowDataBound" ShowHeader="false" ShowActionRow="false" DisplayType="Light" EnableResponsiveTable="False">
                                <Columns>
                                    <Rock:RockBoundField DataField="RecipientId" Visible="false"></Rock:RockBoundField>
                                    <Rock:RockTemplateField>
                                        <ItemTemplate>
                                                <Rock:HiddenFieldWithClass ID="hfRecipientId" runat="server" CssClass="js-recipientId" Value='<%# Eval("RecipientId") %>' />
                                                <Rock:HiddenFieldWithClass ID="hfMessageKey" runat="server" CssClass="js-messageKey" Value='<%# Eval("MessageKey") %>' />

                                                <div class="layout-row">
                                                    <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") ?? Rock.Model.PhoneNumber.FormattedNumber( "", (string)Eval( "MessageKey" ) ) %>' Class="sms-name"></asp:Label>
                                                    <div class="date"><asp:Literal ID="litDateTime" runat="server" Text='<%# Eval("HumanizedCreatedDateTime") %>'></asp:Literal></div>
                                                </div>
                                                <div class="message-truncate">
                                                    <asp:Literal ID="litMessagePart" runat="server" Text='<%# Eval("SMSMessage") %>'></asp:Literal>
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
                        <Rock:HiddenFieldWithClass ID="hfSelectedRecipientId" runat="server" CssClass="js-selected-recipient-id" />
                        <Rock:HiddenFieldWithClass ID="hfSelectedMessageKey" runat="server" CssClass="js-selected-message-key" />
                        <div class="header">
                            <a href="#" class="conversation-back js-back pull-left margin-r-md">
                                <i class="fa fa-chevron-left"></i>
                            </a>
                            <asp:literal ID="litSelectedRecipientDescription" runat="server"></asp:literal>
                            <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible="false" CssClass="btn btn-default btn-xs pull-right" OnClick="lbLinkConversation_Click"></asp:LinkButton>
                        </div>
                        <div class="messages-outer-container">
                            <div class="conversation">
                                <asp:Repeater ID="rptConversation" runat="server" OnItemDataBound="rptConversation_ItemDataBound" Visible="false">
                                    <ItemTemplate>
                                        <div class="message outbound" id="divCommunication" runat="server">
                                            <Rock:HiddenFieldWithClass ID="hfCommunicationRecipientId" runat="server" Value='<%# Eval("RecipientId") %>' />
                                            <Rock:HiddenFieldWithClass ID="hfCommunicationMessageKey" runat="server" Value='<%# Eval("MessageKey") %>' />
                                            <div class="bubble" id="divCommunicationBody" runat="server"><%# Eval("SMSMessage") %></div>
                                            <div class="message-meta">
                                                <span class="sender-name"><%# Eval("FullName") %></span>
                                                <span class="date" title="<%# Eval("CreatedDateTime")%>"><%# Eval("HumanizedCreatedDateTime") %></span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:Label ID="lbNoConversationsFound" runat="server" Visible='<%# rptConversation.Items.Count == 0 %>' Text="<tr><td>No conversations found.</td></tr>" CssClass="text-muted" />
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

                    $("#<%=upConversation.ClientID %> .js-back").click(function() {
                        $('#<%=upConversation.ClientID %>').removeClass("has-focus");
                        return false;
                    });
                });

                function clearActiveDialog() {
                    __doPostBack(null,'cancel');
                }

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
        </div> <%-- End panel-block --%>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdNewMessage" runat="server" Title="New Message" OnSaveClick="mdNewMessage_SaveClick" OnCancelScript="clearActiveDialog();" SaveButtonText="Send" ValidationGroup="vgMobileTextEditor">
            <Content>
                <asp:ValidationSummary ID="vsMobileTextEditor" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgMobileTextEditor" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbNoSms" runat="server" Text="The selected person does not have an SMS enabled Phone number." Dismissable="true" Visible="false" NotificationBoxType="Warning"></Rock:NotificationBox>
                <div class="form-group">
                    <label runat="server" id="lblFromNumber" class="control-label">From</label>
                    <div><asp:Label ID="lblMdNewMessageSendingSMSNumber" runat="server" /></div>
                </div>

                <%-- person picker --%>
                <Rock:PersonPicker ID="ppRecipient" runat="server" Label="Recipient" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Please select an SMS recipient." Required="true" OnSelectPerson="ppRecipient_SelectPerson" />

                <%-- multi-line textbox --%>
                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" Placeholder="Type a message" Required="true" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Message is required" ValidateRequestMode="Disabled" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLinkConversation" runat="server" Title="Link Phone Number to Person" OnSaveClick="mdLinkConversation_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <%--<asp:HiddenField ID="hfMessageKey" runat="server" />--%>
                <asp:HiddenField ID="hfActiveTab" runat="server" />

                <ul class="nav nav-pills margin-b-md">
                    <li id="liNewPerson" runat="server" class="active"><a href='#<%=divNewPerson.ClientID%>' data-toggle="pill">Add New Person</a></li>
                    <li id="liExistingPerson" runat="server"><a href='#<%=divExistingPerson.ClientID%>' data-toggle="pill">Link Existing Person</a></li>
                </ul>

                <Rock:NotificationBox ID="nbAddPerson" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddPerson"/>

                <div class="tab-content">

                    <div id="divNewPerson" runat="server" class="tab-pane active">
                        <div class="row">
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DefinedValuePicker ID="dvpNewPersonTitle" runat="server" Label="Title" ValidationGroup="AddPerson" CssClass="input-width-md" />
                                    <Rock:RockTextBox ID="tbNewPersonFirstName" runat="server" Label="First Name" ValidationGroup="AddPerson" Required="true" autocomplete="off" />
                                    <Rock:RockTextBox ID="tbNewPersonLastName" runat="server" Label="Last Name" ValidationGroup="AddPerson" Required="true" autocomplete="off" />
                                    <Rock:DefinedValuePicker ID="dvpNewPersonSuffix" runat="server" Label="Suffix" ValidationGroup="AddPerson" CssClass="input-width-md" />
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DefinedValuePicker ID="dvpNewPersonConnectionStatus" runat="server" Label="Connection Status" ValidationGroup="AddPerson" Required="true"/>
                                    <Rock:RockRadioButtonList ID="rblNewPersonRole" runat="server" Required="true" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" ValidationGroup="AddPerson"/>
                                    <Rock:RockRadioButtonList ID="rblNewPersonGender" runat="server" Required="true" Label="Gender" RepeatDirection="Horizontal" ValidationGroup="AddPerson"/>
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DatePicker ID="dpNewPersonBirthDate" runat="server" Label="Birthdate" ValidationGroup="AddPerson" AllowFutureDateSelection="False" ForceParse="false"/>
                                    <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" ValidationGroup="AddPerson" UseAbbreviation="true" UseGradeOffsetAsValue="true" />
                                    <Rock:DefinedValuePicker ID="dvpNewPersonMaritalStatus" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Marital Status"  ValidationGroup="AddPerson"/>
                                </div>
                            </div>
                        </div>

                    </div>

                    <div id="divExistingPerson" runat="server" class="tab-pane">
                        <fieldset>
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="AddPerson" />
                        </fieldset>
                    </div>

                </div>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
