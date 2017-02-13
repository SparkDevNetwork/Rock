<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailTopicSubscriptionPreference.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.EmailTopicSubscriptionPreference" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblSubscriptions" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <h5>My Subscription Settings</h5>
        <Rock:NotificationBox ID="nbConfigurationError" runat="server" Visible="false" NotificationBoxType="Warning" Title="Configuration Error" Text="This block is not yet configured or it's misconfigured."></Rock:NotificationBox>
        <asp:HiddenField ID="hfAttributeId" runat="server" />
        <asp:HiddenField ID="hfAttributeKey" runat="server" />

        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSaveSettings_Click" SaveButtonText="Save" Title="Edit Html">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:RockDropDownList ID="ddlPersonAttribute" runat="server" Label="Subscription Attribute" OnSelectedIndexChanged="ddlPersonAttribute_SelectedIndexChanged" AutoPostBack="true" />
                            <Rock:RockCheckBoxList ID="cblPopupValues" runat="server" Label="Popup-Causing Values" RepeatDirection="Horizontal" Help="What values will cause the 'Are you sure?' popup" />
                            <Rock:RockTextBox ID="tbPopupText" runat="server" Label="Popup Text" Help="The text of the 'Are you sure?' popup" />
                            <Rock:HtmlEditor ID="htmlEditor" runat="server" ResizeMaxWidth="720" Height="140" Label="Lava Template" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:Panel ID="pnlViewPreferences" runat="server">
            <Rock:NotificationBox ID="nbSuccess" runat="server" Visible="false" NotificationBoxType="Success" Text="Your preferences were saved."></Rock:NotificationBox>
            <asp:Literal ID="lContent" runat="server" /></h1>
            <asp:Literal ID="lDebug" runat="server" /></h1>
            <asp:LinkButton ID="lbEditPreferences" runat="server" CssClass="btn btn-primary btn-xs" OnClick="lbEditPreferences_Click"><i class="fa fa-pencil"></i> Edit My Subscriptions</asp:LinkButton>
        </asp:Panel>

        <asp:Panel ID="pnlEditPreferences" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbFail" runat="server" Visible="false" NotificationBoxType="Danger" Text="Hmmm...something went wrong and your preference was not saved. If it happens again please let us know."></Rock:NotificationBox>

            <Rock:ModalDialog ID="mdConfirmUnsubscribe" CssClass="subscription-popup" runat="server" OnSaveClick="mdConfirmUnsubscribe_SaveClick" OnCancelScript="clearDialog();" SaveButtonText="Yes">
                <Content>
                    <asp:Literal ID="lWarning" runat="server" />
                </Content>
            </Rock:ModalDialog>

            <asp:RadioButton ID="radEmailAllowed" runat="server" Text=" I want to recieve emails about:" GroupName="grpEmailPreference" OnCheckedChanged="grpEmailPreference_CheckedChanged" AutoPostBack="true" />
            <asp:CheckBoxList ID="cblSubscriptions" runat="server" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="cblSubscriptions_SelectedIndexChanged" ClientIDMode="AutoID" />
            <asp:RadioButton ID="radNoMassEmail" runat="server" Text=" I do not want promotional emails." GroupName="grpEmailPreference" OnCheckedChanged="grpEmailPreference_CheckedChanged" AutoPostBack="true" />
            <asp:RadioButton ID="radDoNotEmail" runat="server" Text=" I do not want to recieve any emails." GroupName="grpEmailPreference" OnCheckedChanged="grpEmailPreference_CheckedChanged" AutoPostBack="true" />


            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
