<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" Title="Note" Visible="true" Text="Consider making these changes when everyone else is sleeping.  Once you save the changes, your website will be restarted."></Rock:NotificationBox>

        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"  />
        <fieldset>
            <Rock:RockDropDownList ID="ddTimeZone" runat="server" CausesValidation="false" Label="Time Zone" Help="Set should be the time zone you want Rock to operate in (regardless of what time zone the server is set to.)" ></Rock:RockDropDownList>
            <Rock:RockCheckBox ID="cbRunJobsInIISContext" runat="server" Label="Enable Run Jobs In IIS Context" Help="When checked, Rock's job engine runs on the webserver. This setting allows you to disable this in order to run jobs as a Windows Agent. See the 'Jobs' section in the Admin guide for more information on this topic." />
            <Rock:NumberBox ID="numbMaxSize" runat="server" NumberType="Integer" Label="Max Upload File Size" MinimumValue="1" MaximumValue="10000" AppendText="MB"></Rock:NumberBox>
        </fieldset>

        <div class="actions margin-t-lg">
            <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" OnClick="bbtnSaveConfig_Click" Text="Save" DataLoadingText="Saving..."></Rock:BootstrapButton>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
