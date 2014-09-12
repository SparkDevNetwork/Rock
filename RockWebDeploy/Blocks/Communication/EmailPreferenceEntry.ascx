<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Communication.EmailPreferenceEntry, RockWeb" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Vertical"/>

        <div id="divNotInvolved" runat="server" style="display:none" >
            <Rock:RockDropDownList ID="ddlInactiveReason" runat="server" Label="Reason" />
            <Rock:RockTextBox ID="tbInactiveNote" runat="server" Label="More Info (optional)" TextMode="MultiLine" Rows="3" MaxLength="1000" />
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
        </div>
        <br />
        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Success" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
