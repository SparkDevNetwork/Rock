<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wrench"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwGeneralConfiguration" runat="server" Title="General Configuration" Expanded="true">
                    <Rock:NotificationBox ID="nbGeneralMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" ></Rock:NotificationBox>
                    <Rock:RockCheckBox ID="cbEnableMultipleTimeZone" runat="server" Label="Enable Multiple Time Zone Support" ValidationGroup="GeneralSetting" Help="When checked, Mutliple Time Zone is supported."/>
                    <div class="actions margin-t-lg">
                        <Rock:BootstrapButton ID="btnGeneralSave" runat="server" CssClass="btn btn-primary" AccessKey="s"  OnClick="btnGeneralSave_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="GeneralSetting"></Rock:BootstrapButton>
                    </div>
                </Rock:PanelWidget>
                <Rock:PanelWidget ID="pwWebConfigSetting" runat="server" Title="Web.Config Settings">
                    <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="true" Text="Once you save these changes, your website will be restarted. Consider making these changes at a low use time for your audience.  "></Rock:NotificationBox>
                    <fieldset>
                        <Rock:RockDropDownList ID="ddTimeZone" runat="server" CausesValidation="false" CssClass="input-width-xxl" Label="Time Zone" Help="The time zone you want Rock to operate in (regardless of what time zone the server is set to.)" ValidationGroup="WebConfigSetting"></Rock:RockDropDownList>
                        <Rock:RockCheckBox ID="cbRunJobsInIISContext" runat="server" Label="Enable Run Jobs In IIS Context" Text="Yes" Help="When checked, Rock's job engine runs on the web server. This setting allows you to disable running jobs on this server if it's participating in a Rock cluster. See the 'Jobs' section in the Admin Hero Guide for more information on this topic." ValidationGroup="WebConfigSetting"/>
                        <Rock:NumberBox ID="numbMaxSize" runat="server" NumberType="Integer" Label="Max Upload File Size" CssClass="input-width-md" MinimumValue="1" MaximumValue="10000" AppendText="MB" ValidationGroup="WebConfigSetting"></Rock:NumberBox>
                    </fieldset>
                    <div class="actions margin-t-lg">
                        <Rock:BootstrapButton ID="btnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnSaveConfig_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="WebConfigSetting"></Rock:BootstrapButton>
                    </div>

                </Rock:PanelWidget>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>