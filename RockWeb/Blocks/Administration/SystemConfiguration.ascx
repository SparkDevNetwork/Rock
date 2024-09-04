<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wrench"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwGeneralConfiguration" runat="server" Title="General Configuration" Expanded="true">
                    <Rock:NotificationBox ID="nbGeneralMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbEnableMultipleTimeZone" runat="server" Label="Enable Multiple Time Zone Support" ValidationGroup="GeneralSetting" Help="When checked, Mutliple Time Zone is supported." />
                            <Rock:UrlLinkBox ID="tbPDFExternalRenderEndpoint" runat="server" Label="PDF External Render Endpoint" Help="Specify a URL to use an external service like browserless.io to generate PDFs instead of using the internal PDF Generator." />
                            <Rock:NumberBox ID="nbVisitorCookiePersistenceLengthDays" runat="server" NumberType="Integer" Label="Visitor Cookie Persistence Length" CssClass="input-width-lg" MinimumValue="1" AppendText="days" ValidationGroup="GeneralSetting" Help="The number of days a visitor cookie persists." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIncludeBusinessInPersonPicker" runat="server" Label="Always Show Businesses in Person Picker" ValidationGroup="GeneralSetting" Help="When enabled, businesses will always be included in the search results of the person picker." />
                            <Rock:RockCheckBox ID="cbEnableKeepAlive" runat="server" Label="Enable Keep Alive" ValidationGroup="GeneralSetting" Help="Enable this setting to have Rock poll itself to keep it alive during times of inactivity. This setting is not needed if your AppPool's Idle Time-out is set to 0 (Highly Recommended). See the Rock Solid Internal Hosting guide for recommended AppPool settings." />
                            <Rock:NumberBox ID="nbPersonalizationCookieCacheLengthMinutes" runat="server" NumberType="Integer" Label="Personalization Segment Cookie Affinity Duration" CssClass="input-width-lg" AppendText="minutes" Help="Number of minutes old the ROCK_SEGMENT_FILTERS cookie can be before it is considered stale and will be re-fetched from the database. The default is 5 minutes if not set." />
                        </div>
                    </div>
                    <div class="actions">
                        <Rock:BootstrapButton ID="btnGeneralSave" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnGeneralSave_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="GeneralSetting"></Rock:BootstrapButton>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwUiSettings" runat="server" Title="UI Settings" Expanded="false">
                    <Rock:NotificationBox ID="nbUiSettings" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbPersonRaceLabel" runat="server" Label="Race Label" ValidationGroup="UISettings" Help="The value to use in the label wherever the Race field is used."></Rock:RockTextBox>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbPersonEthnicityLabel" runat="server" Label="Ethnicity Label" ValidationGroup="UISettings" Help="The value to use in the label wherever the Ethnicity field is used."></Rock:RockTextBox>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbCaptchaSiteKey" runat="server" Label="Captcha Site Key" ValidationGroup="UISettings" Help="The Captcha site key."></Rock:RockTextBox>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbCaptchaSecretKey" runat="server" Label="Captcha Secret Key" ValidationGroup="UISettings" Help="The Captcha secret key."></Rock:RockTextBox>
                        </div>
                    </div>

                    <Rock:RockTextBox ID="rtbSmsOptInMessage" runat="server" Label="SMS Opt-In Message" Help="This text will display next to a checkbox on blocks where a mobile phone number can be entered to enable SMS messaging for that number."></Rock:RockTextBox>

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnUiSettingSave" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnUiSettingSave_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="UISettings"></Rock:BootstrapButton>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwObservability" runat="server" Title="Observability" Expanded="false">

                    <Rock:NotificationBox ID="nbObservabilityMessages" runat="server" />
                    <asp:ValidationSummary ID="valObservabilityValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Observability" />

                    <Rock:RockCheckBox ID="cbEnableObservaility" runat="server" Label="Enable Observability" ValidationGroup="Observability" Help="Enables the observability feature set." />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:UrlLinkBox ID="urlObservabilityEndpoint" runat="server" Label="Endpoint" ValidationGroup="Observability" Help="The URL for sending observability telemetry to." CausesValidation="true" />
                            <Rock:ButtonDropDownList ID="ddlEndpointProtocol" runat="server" Label="Endpoint Protocol" ValidationGroup="Observability" Help="The protocol to use to encode the telemetry data when sending it to the endpoint." />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbObservabilitySpanCountLimit" runat="server" Label="Span Count Limit" Help="Some data collectors have a limit on the number of spans that can be attached to a single trace. Once a trace reaches this many spans it will truncate any additional spans before sending. If blank then 9,900 will be used." ValidationGroup="Observability" CausesValidation="true" />
                            <Rock:NumberBox ID="nbObservabilityMaxAttributeLength" runat="server" Label="Maximum Attribute Length" Help="Some collectors will drop spans if they have attributes that exceed this length. Rock will limit attribute values to this length. If blank then 4,000 will be used." ValidationGroup="Observability" CausesValidation="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:KeyValueList ID="kvlEndpointHeaders" runat="server" Label="Endpoint Headers" ValidationGroup="Observability" Help="List of HTTP headers to be added to the HTTP calls when sending telemetry." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbObservabilityIncludeQueryStatements" runat="server" Label="Include Query Statements" ValidationGroup="Observability" Help="Enabling this option will include all SQL query statements in database activities. Otherwise they will only be included for targeted queries." />
                            <Rock:ValueList ID="vlTargetedQueries" runat="server" Label="Targeted Queries" ValidationGroup="Observability" Help="List of query hashes that will report more in depth metrics for." />
                        </div>
                    </div>

                    <div class="alert alert-info">
                        The service name used by the observability framework is defined in the web.config.
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnObservavilitySave" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnObservabilitySave_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="Observability" />
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwExperimentalSettings" runat="server" Title="Experimental Settings" TitleIconCssClass="fa fa-flask" Expanded="false">
                    <Rock:DayOfWeekPicker ID="dowpStartingDayOfWeek" runat="server" Label="Starting Day of Week" DefaultDayOfWeek="Monday" Help="Set this to change how Rock calculates 'Sunday Date'. This setting is retro-active to any data that is stored with SundayDate." />
                    <Rock:NotificationBox ID="nbStartDayOfWeekSaveMessage" runat="server" NotificationBoxType="Warning" Text="This is an experimental setting. Changing this will change how SundayDate is calculated and will also update existing data that keeps track of 'SundayDate'." />

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:NumberBox ID="nbSecurityGrantTokenDuration" runat="server" CssClass="input-width-md" Label="Security Grant Token Duration" Help="This specifies the default duration in minutes that a security grant token will be valid for. These are used to provide additional security context to UI controls." AppendText="minutes" NumberType="Integer" MinimumValue="60" />
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbSecurityGrantTokenDurationSaveMessage" runat="server" Visible="false" NotificationBoxType="Success" Text="Security grant token duration has been successfully updated." />

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnSaveExperimental" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnSaveExperimental_Click" Text="Save" DataLoadingText="Updating..." ValidationGroup="Experimental" />

                        <span class="pull-right">
                            <asp:LinkButton ID="btnRevokeSecurityGrants" runat="server" CssClass="btn btn-default" OnClick="btnRevokeSecurityGrants_Click" OnClientClick="Rock.dialogs.confirmPreventOnCancel( event, 'Are you sure you wish to revoke all security grant tokens?');" Text="Revoke Grants" ValidationGroup="Experimental" ToolTip="Revokes all existing security grant tokens that have been issued." />
                        </span>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwWebConfigSetting" runat="server" Title="Web.Config Settings">
                    <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="true" Text="Once you save these changes, your website will be restarted. Consider making these changes at a low use time for your audience.  "></Rock:NotificationBox>
                    <fieldset>
                        <Rock:RockDropDownList ID="ddTimeZone" runat="server" CausesValidation="false" CssClass="input-width-xxl" Label="Time Zone" Help="The time zone you want Rock to operate in (regardless of what time zone the server is set to.)" ValidationGroup="WebConfigSetting"></Rock:RockDropDownList>
                        <Rock:RockCheckBox ID="cbRunJobsInIISContext" runat="server" Label="Enable Run Jobs In IIS Context" Text="Yes" Help="When checked, Rock's job engine runs on the web server. This setting allows you to disable running jobs on this server if it's participating in a Rock cluster. See the 'Jobs' section in the Admin Hero Guide for more information on this topic." ValidationGroup="WebConfigSetting" />
                        <Rock:NumberBox ID="numbMaxSize" runat="server" NumberType="Integer" Label="Max Upload File Size" CssClass="input-width-md" MinimumValue="1" MaximumValue="10000" AppendText="MB" ValidationGroup="WebConfigSetting"></Rock:NumberBox>
                        <Rock:NumberBox ID="numLoginCookieTimeout" runat="server" NumberType="Integer" Label="Login Cookie Persistence Length" CssClass="input-width-lg" MinimumValue="1" AppendText="minutes" ValidationGroup="WebConfigSetting"
                            Help="The length a login cookie persists in minutes. This should be set to a large number. This reduces 'login friction' and increases the chance that someone will remain logged in for a long period of time." />
                        <Rock:RockCheckBox ID="cbEnableAdoNetPerformanceCounters" runat="server" Label="Enable Database Performance Counters" Text="Yes" Help="When checked, metric values regarding the counts of active and available database connections will be collected, for reporting within the 'Hosting Metrics' category. Note that website performance can be impacted when this option is enabled; consider using only when investigating possible database performance issues." ValidationGroup="WebConfigSetting" />
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="rtbAzureSignalREndpoint" runat="server" Label="Azure SignalR Endpoint" ValidationGroup="UISettings" Help="The Azure SignalR endpoint."></Rock:RockTextBox>
                            </div>
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="rtbAzureSignalRAccessKey" runat="server" Label="Azure SignalR AccessKey" ValidationGroup="UISettings" Help="The Azure SignalR access key."></Rock:RockTextBox>
                            </div>
                        </div>

                        <Rock:RockTextBox ID="tbObservabilityServiceName" runat="server" Label="Observability Service Name" />
                    </fieldset>
                    <div class="actions">
                        <Rock:BootstrapButton ID="btnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnSaveConfig_Click" Text="Save" DataLoadingText="Saving..." ValidationGroup="WebConfigSetting"></Rock:BootstrapButton>
                    </div>
                </Rock:PanelWidget>
                <Rock:PanelWidget ID="pwFamilyRules" runat="server" Title="Family Rules" Expanded="false">
                    <Rock:NotificationBox ID="nbFamilyRulesMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbBibleStrictSpouse" runat="server" Label="Bible Strict Spouse" Help="When unchecked, spouse rules are relaxed." />
                            </div>
                    </div>
                    <div class="actions">
                        <Rock:BootstrapButton ID="btnFamilyRules" runat="server" CssClass="btn btn-primary" AccessKey="s" OnClick="btnFamilyRules_Click" Text="Save" DataLoadingText="Saving..." ></Rock:BootstrapButton>
                    </div>
                </Rock:PanelWidget>
            </div>
        </div>
        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);
            function pageLoaded(sender, args) {
                calculateMaxLogSize();

                $(".js-files-to-retain").change(calculateMaxLogSize);
                $(".js-max-file-size").change(calculateMaxLogSize);
            }

            function calculateMaxLogSize() {
                var numberOfFiles = $(".js-files-to-retain").val();
                var fileSize = $(".js-max-file-size").val();
                $("#maxLogSize").text(numberOfFiles * fileSize);
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
