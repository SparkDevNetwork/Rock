<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonLeft.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.PersonLeft" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $(".photo a").fluidbox();

        var shareUrl = $('.js-share-editperson-url').val();

        if (navigator.share && shareUrl) {
            $('.js-share-editperson').show().on('click', function (event) {
                event.stopImmediatePropagation();

                // see https://css-tricks.com/how-to-use-the-web-share-api/
                var personName = $('h1.js-checkin-person-name').first().text().trim();
                navigator.share({ title: personName, url: shareUrl });
            });
        }
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:HiddenFieldWithClass ID="hfShareEditPersonUrl" runat="server" CssClass="js-share-editperson-url" />

        <!-- Photo, Name & Campus, Phone & Email, Attributes -->
        <div class="panel panel-block">
            <div class="profile-photo-container">
                <asp:Literal ID="lPhoto" runat="server" />
            </div>
            <div class="d-flex flex-column align-items-center p-2 pb-3 py-lg-3 px-lg-4">
                <h1 class="h3 title name js-checkin-person-name mt-0 text-center">
                    <asp:Literal ID="lName" runat="server" />
                    <a id="btnShare" runat="server" class="btn btn-lg btn-link text-muted p-0 js-share-editperson" style="display:none;"><i class="fa fa-share-alt-square hand"></i></a></h1>
                <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
            </div>

            <asp:Panel ID="pnlContact" runat="server" CssClass="border-top border-gray-400 p-2 p-lg-3">
                <div class="pnlSms">
                    <!-- Shows the results of the message send -->
                    <Rock:NotificationBox ID="nbSmsSendResult" runat="server" Visible="false" NotificationBoxType="Success" Dismissable="true" />
                </div>

                <asp:Repeater ID="rptrPhones" runat="server" OnItemDataBound="rptrPhones_ItemDataBound">
                    <ItemTemplate>
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <div>
                                <%# Eval("NumberFormatted") %>
                                <span class="d-block text-sm text-muted leading-snug">
                                    <%# Eval("NumberTypeValue.Value") %>
                                </span>
                            </div>
                            <div class="text-right">
                                <asp:LinkButton ID="btnSms" runat="server" Visible="false" OnClick="btnSms_Click" CssClass="btn btn-sm btn-square btn-default my-1" Text="<i class='fa fa-sms'></i>" />
                                <a class="btn btn-sm btn-square btn-default my-1" href="tel:<%# Eval("Number") %>"><i class="fa fa-phone"></i></a>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Literal ID="lEmail" runat="server" />
            </asp:Panel>

            <div id="pnlAdultFields" runat="server" class="adult-attributes border-top border-gray-400 p-2">
                <Rock:AttributeValuesContainer ID="avcAdultAttributes" runat="server" ShowCategoryLabel="false" />
            </div>
            <div id="pnlChildFields" runat="server" class="child-attributes border-top border-gray-400 p-2">
                <Rock:AttributeValuesContainer ID="avcChildAttributes" runat="server" ShowCategoryLabel="false" />
            </div>

        </div>

        <!-- Family -->
        <asp:Panel ID="pnlFamily" runat="server" CssClass="panel panel-block flex-row flex-wrap justify-content-around p-1">
            <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                <ItemTemplate>
                    <a class="group-member text-sm text-center text-color rounded p-2" href='<%# Eval("Url") %>'>
                        <asp:Literal ID="lFamilyPhoto" runat="server" />
                        <span><%# Eval("NickName") %></span>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <!-- Relationships -->
        <asp:Panel ID="pnlRelationships" runat="server" CssClass="panel panel-block flex-row flex-wrap justify-content-around p-1">
            <asp:Repeater ID="rptrRelationships" runat="server" OnItemDataBound="rptrRelationships_ItemDataBound">
                <ItemTemplate>
                    <a class="group-member text-sm text-center text-color rounded p-2" href='<%# Eval("Url") %>'>
                        <asp:Literal ID="lRelationshipPhoto" runat="server" />
                        <span><%# Eval("NickName") %></span>
                        <small><asp:Literal ID="lRelationshipName" runat="server" /></small>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <Rock:ModalDialog ID="mdSms" runat="server" ValidationGroup="vgSMS" Title="New Text Message" OnSaveClick="mdSms_SaveClick" SaveButtonText="Send" Visible="false">
            <Content>
                <Rock:NotificationBox ID="nbSmsError" runat="server" Visible="false" NotificationBoxType="Danger" />

                <div>
                    <Rock:RockLiteral ID="lSmsRecipient" runat="server" Label="Recipient" />
                </div>

                <div class="row">
                    <div class="col-xs-9">
                        <Rock:RockTextBox ID="tbSmsMessage" runat="server" TextMode="MultiLine" Rows="5" Placeholder="Your SMS message here..." Label="Message" />
                    </div>
                    <div class="col-xs-3">
                        <Rock:ImageUploader ID="imgSmsImage" runat="server" BinaryFileTypeGuid="<%# new Guid( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT ) %>" Help="Optional image to include in the message." Label="Image" />
                    </div>
                </div>

                <div class="form-group static-control">
                    <div class="row">
                        <div class="col-xs-9">
                            <label class="control-label">Snippets</label>
                        </div>

                        <div class="col-xs-3">
                            <Rock:Toggle runat="server" ID="tglUsePersonal"
                                CssClass="pull-right"
                                ButtonSizeCssClass="btn-xs"
                                OnCssClass="btn-primary"
                                OffCssClass="btn-primary"
                                OnText="Personal" OffText="Shared"
                                AutoPostBack="true"
                                OnCheckedChanged="tglUsePersonal_CheckedChanged" />
                        </div>
                    </div>

                    <div class="row">
                        <asp:Repeater ID="rptrSmsSnippets" runat="server">
                            <ItemTemplate>
                                <div class="col-xs-4 pt-2">
                                    <asp:LinkButton ID="btnSmsSnippet" runat="server" CssClass="btn btn-default btn-block overflow-hidden" OnCommand="btnSmsSnippet_Command" CommandName="InputSnippet" CommandArgument='<%#Eval("Id") %>' Text='<%#Eval("Name") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:Panel ID="pnlSmsNoSnippets" runat="server" Visible="false">
                        There are no <asp:Label ID="lblSmsSnippetType" runat="server" Text="shared" /> SMS snippets available.
                    </asp:Panel>

                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
