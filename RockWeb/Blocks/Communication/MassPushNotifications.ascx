<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MassPushNotifications.ascx.cs" Inherits="RockWeb.Blocks.Communication.MassPushNotifications" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfSelectedCommunicationTemplateId" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-mobile"></i>&nbsp;Mass Push Notifications</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoMediumError" runat="server" NotificationBoxType="Danger" Visible="false">
                    You have not enabled your push medium with a valid push transport.
                </Rock:NotificationBox>

                <%-- Template Selection --%>
                <asp:Panel ID="pnlTemplateSelection" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-sm-8">
                            <h1 class="step-title">Communication Template</h1>
                        </div>
                        <div class="col-sm-4">
                            <div class="pull-right">
                                <Rock:CategoryPicker ID="cpCommunicationTemplate" runat="server" AllowMultiSelect="false" Label="Category Filter" EntityTypeName="Rock.Model.CommunicationTemplate" OnSelectItem="cpCommunicationTemplate_SelectItem" />
                            </div>
                        </div>
                    </div>

                    <div class="row margin-t-lg template-selection">
                        <asp:Repeater ID="rptSelectTemplate" runat="server" OnItemDataBound="rptSelectTemplate_ItemDataBound">
                            <ItemTemplate>
                                <div class="col-md-4 col-sm-6">
                                    <asp:LinkButton ID="btnSelectTemplate" CssClass="communication-template" runat="server" OnClick="btnSelectTemplate_Click">
                                        <div class="row">
                                            <div class="col-xs-5">
                                                <asp:Literal ID="lTemplateImagePreview" runat="server"></asp:Literal>
                                            </div>
                                            <div class="col-xs-7">
                                                <label><asp:Literal ID="lTemplateName" runat="server"></asp:Literal></label>
                                                <p><asp:Literal ID="lTemplateDescription" runat="server"></asp:Literal></p>
                                            </div>
                                        </div>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="btnTemplateSelectionNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgTemplateSelection" CausesValidation="true" OnClick="btnTemplateSelectionNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Push Editor --%>
                <asp:Panel ID="pnlPushEditor" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsPushEditor" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgPushEditor" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phPushControl" runat="server" />
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbPushValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbPushEditorPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPushEditorPrevious_Click" />
                        <asp:LinkButton ID="lbPushEditorSendTest" runat="server" Text="Send Test" CssClass="btn btn-default" ValidationGroup="vgPushEditor" CausesValidation="true" OnClick="lbPushEditorSendTest_Click" />
                        <asp:LinkButton ID="lbPushEditorSend" runat="server" Text="Send" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgPushEditor" CausesValidation="true" OnClick="lbPushEditorSend_Click" />
                    </div>

                    <Rock:ModalDialog ID="mdPushSendTest" runat="server" Title="Send Test">
                        <Content>
                            <Rock:PersonPicker ID="ppPushSendTestTo" runat="server" EnableSelfSelection="true" Label="Person" Required="true" ValidationGroup="vgPushSendTest" />

                            <Rock:NotificationBox ID="nbPushTestResult" CssClass="margin-t-md" runat="server" NotificationBoxType="Success" Text="Test Push Notification has been sent." Visible="false" Dismissable="true" />

                            <asp:LinkButton ID="lbPushSendTestMessage" runat="server" Text="Send Test Message" CssClass="btn btn-primary" OnClick="lbPushSendTestMessage_Click" />
                        </Content>
                    </Rock:ModalDialog>
                </asp:Panel>

                <%-- Finished Result Panel --%>
                <asp:Panel ID="pnlResult" runat="server" Visible="false" CssClass="js-pnl-result">
                    <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
                </asp:Panel>
            </div>

        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                // make sure scroll position is set to top after navigating (so that
                // stuff doesn't roll out of view if navigating from a tall to a short height)
                $('.js-wizard-navigation').off('click');
                $('.js-wizard-navigation').on('click', function () {
                    $('html, body').animate({
                        scrollTop: $('#<%=upnlContent.ClientID%>')
                    }, 'fast');
                });
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
