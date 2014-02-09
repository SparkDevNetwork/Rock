<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" Title="Note" Visible="true" Text="Consider making these changes when everyone else is sleeping.  Once you save the changes, your website will be restarted."></Rock:NotificationBox>

        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"  />
        <fieldset>
            <dl>
                <dt>Time Zone</dt>
                <dd>
                    <Rock:RockDropDownList ID="ddTimeZone" runat="server" CausesValidation="false"></Rock:RockDropDownList>
                </dd>

                <dt>Max Upload File Size</dt>
                <dd>
                    <Rock:NumberBox ID="numbMaxSize" runat="server" NumberType="Integer" MinimumValue="1" MaximumValue="10000" AppendText="MB"></Rock:NumberBox>
                </dd>
            </dl>
        </fieldset>

        <div class="actions margin-t-lg">
            <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" OnClick="bbtnSaveConfig_Click" Text="Save" DataLoadingText="Saving..."></Rock:BootstrapButton>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
