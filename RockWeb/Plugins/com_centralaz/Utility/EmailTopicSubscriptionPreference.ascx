<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailTopicSubscriptionPreference.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.EmailTopicSubscriptionPreference" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbConfigurationError" runat="server" Visible="false" NotificationBoxType="Warning" Title="Configuration Error" Text="This block is not yet configured or it's misconfigured."></Rock:NotificationBox>
        <asp:HiddenField ID="hfAttributeId" runat="server" />
        <asp:HiddenField ID="hfAttributeKey" runat="server" />
        <asp:Panel ID="pnlViewPreferences" runat="server">
            <Rock:NotificationBox ID="nbSuccess" runat="server" Visible="false" NotificationBoxType="Success" Text="Your preference was saved."></Rock:NotificationBox>
            <asp:Literal ID="lPreferences" runat="server" /></h1>
            <asp:LinkButton ID="lbEditPreferences" runat="server" CssClass="btn btn-primary btn-xs" OnClick="lbEditPreferences_Click"><i class="fa fa-pencil"></i> Edit Subscription Preferences</asp:LinkButton>
        </asp:Panel>
        <asp:Panel ID="pnlEditPreferences" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbFail" runat="server" Visible="false" NotificationBoxType="Danger" Text="Hmmm...something went wrong and your preference was not saved. If it happens again please let us know."></Rock:NotificationBox>
            <Rock:RockCheckBoxList ID="cblPreference" runat="server" RepeatDirection="Vertical" />
            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
