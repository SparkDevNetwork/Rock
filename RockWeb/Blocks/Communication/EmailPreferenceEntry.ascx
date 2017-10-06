<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailPreferenceEntry.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailPreferenceEntry" %>

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
        <Rock:NotificationBox ID="nbEmailPreferenceSuccessMessage" runat="server" NotificationBoxType="Success" Visible="false" />
        <Rock:NotificationBox ID="nbUnsubscribeSuccessMessage" runat="server" NotificationBoxType="Success" Visible="false" />

        <script>
            Sys.Application.add_load(function () {
                // add some margin to 'Please unsubsribe from list..' to set it apart a little (if it exists)
                $('#<%=rblEmailPreference.ClientID%>').find('input[value=4]').closest('label').addClass('margin-t-sm margin-b-md');
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
