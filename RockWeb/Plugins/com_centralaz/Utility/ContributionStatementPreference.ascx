<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementPreference.ascx.cs" Inherits="RockWeb.Plugins.com_CentralAZ.Utility.ContributionStatementPreference" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbConfigurationError" runat="server" Visible="false" NotificationBoxType="Warning" Title="Configuration Error" Text="This block is not yet configured or it's misconfigured."></Rock:NotificationBox>

		<asp:Panel ID="pnlPreferences" runat="server">
            <asp:HiddenField ID="hfAttributeId" runat="server" />
            <asp:HiddenField ID="hfAttributeKey" runat="server" />
			<Rock:RockRadioButtonList ID="rblPreference" runat="server" RepeatDirection="Vertical" OnSelectedIndexChanged="rblPreference_CheckedChanged" AutoPostBack="true"/>
            <asp:PlaceHolder ID="phAttributes" runat="server" />
		</asp:Panel>
        <Rock:NotificationBox ID="nbSuccess" runat="server" Visible="false" NotificationBoxType="Success" Text="Your preference was saved."></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbFail" runat="server" Visible="false" NotificationBoxType="Danger" Text="Hmmm...something went wrong and your preference was not saved. If it happens again please let us know."></Rock:NotificationBox>
   </ContentTemplate>
</asp:UpdatePanel>
