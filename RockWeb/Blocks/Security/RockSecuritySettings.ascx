<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockSecuritySettings.ascx.cs" Inherits="RockWeb.Blocks.Security.RockSecuritySettings" %>
<%@ Import Namespace="Rock.Utility.Enums" %>
<%@ Import Namespace="Rock" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-shield-alt"></i>
                    Security Settings</h1>
            </div>
            <div class="panel-body">
                <h3 class="mt-0">Rock Security Settings</h3>
                <p>
                    The settings below allow you to adjust how security in Rock is configured in your instance. Be sure that you fully understand each setting to ensure proper configuration.
                </p>
                <hr />
                <label for="" class="control-label m-0">Account Protection Profile Levels</label>
                <div class="mb-3">Below is a listing of the criteria for the account protection profile levels.</div>
                <div class="row d-flex flex-wrap">
                    <div class="col-xs-6 col-md-3">
                        <div class="well-message well-message-info text-left h-100 border border-info p-3">
                            <h4 class="mt-0">Low</h4>
                            <ul>
                                <li>No risk items</li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-xs-6 col-md-3">
                        <div class="well-message well-message-warning text-left h-100 border border-warning p-3">
                            <h4 class="mt-0">Medium</h4>
                            <ul>
                                <li>Has login account</li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-xs-6 col-md-3">
                        <div class="well-message well-message-critical text-left h-100 border border-critical p-3">
                            <h4 class="mt-0">High</h4>
                            <ul>
                                <li>Active Scheduled Financial Transaction</li>
                                <li>Saved Payment Account</li>
                                <li>In a Security Role Marked w/ High Elevated Security </li>
                            </ul>
                        </div>
                    </div>
                    <div class="col-xs-6 col-md-3">
                        <div class="well-message well-message-danger text-left h-100 border border-danger p-3">
                            <h4 class="mt-0">Extreme</h4>
                            <ul>
                                <li>In a Security Role Marked w/ Extreme Elevated Security </li>
                            </ul>
                        </div>
                    </div>
                </div>
                <hr />
                <Rock:NotificationBox
                    runat="server"
                    ID="nbRecommendHigherSettingsForDuplicateDetection"
                    NotificationBoxType="Danger"
                    Visible="true"
                    Text="We highly recommend that you prevent duplicate detection for individuals with an Account Protection Profile of High and Extreme"
                    CssClass="js-duplicate-detection-notification-box d-none" />

                <Rock:NotificationBox
                    runat="server"
                    ID="nbSaveResult"
                    NotificationBoxType="Danger"
                    Visible="false" />

                <div class="row mt-3">
                    <div class="col-md-6 col-sm-12 col-xs-12">
                        <Rock:RockCheckBoxList
                            runat="server"
                            ID="cblIgnoredAccountProtectionProfiles"
                            Label="Disable Duplicate Checking for the Following Protection Profiles"
                            Help="This disables duplication protection checks for individuals with the selected Account Protection Profiles. People with these checked values will always create duplicates (i.e., they will not match existing records.) We highly recommend enabling this for the High and the Extreme profile."
                            RepeatDirection="Horizontal"
                            CssClass="js-ignored-protection-profile" />
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-md-6">
                        <Rock:RockCheckBox
                            ID="cbDisablePredictableIds"
                            runat="server"
                            Label="Disable Predictable IDs"
                            Help="When checked, the GetFile, GetImage and GetAvatar endpoints will use IdKeys and GUID values instead of predictable IDs." />
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-md-6">
                        <Rock:RockDropDownList
                            runat="server"
                            ID="ddlHighRoles"
                            Label="Allow Merges of Account Protection Profile - High"
                            EnhanceForLongLists="true"
                            Help="Merging records with an Account Protection Profile of High will be limited to individuals in the security role configured." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList
                            runat="server"
                            ID="ddlExtremeRoles"
                            Label="Allow Merges of Account Protection Profile - Extreme"
                            EnhanceForLongLists="true"
                            Help="Merging records with an Account Protection Profile of Extreme will be limited to individuals in the security role configured." />
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-md-6 col-12">
                        <Rock:RockCheckBoxList
                            runat="server"
                            ID="cblDisableTokensForAccountProtectionProfiles"
                            Label="Disable Usage of Personal Tokens for the Following Protection Profiles"
                            Help="Any protection profiles selected here will not be allowed to use impersonation tokens or tokens to authenticate a person."
                            RepeatDirection="Horizontal"
                            CssClass="js-ignored-protection-profile" />
                    </div>
                    <div class="col-md-6 col-12">
                        <Rock:RockCheckBoxList
                            runat="server"
                            ID="cblRequireTwoFactorAuthenticationForAccountProtectionProfiles"
                            Label="Require Two-Factor Authentication for the Following Protection Profiles"
                            Help="2FA - Selected protection profiles will require two-factor authentication when logging in."
                            RepeatDirection="Horizontal"
                            CssClass="js-ignored-protection-profile" />
                        <Rock:NotificationBox
                            runat="server"
                            ID="nbTwoFactorAuthenticationDisabled"
                            NotificationBoxType="Warning"
                            Visible="false"
                            Text="Please update your login pages to use the latest Login Block to enable Two-Factor Authentication." />
                    </div>
                </div>
                <Rock:PanelWidget runat="server" ID="pnlAuthenticationSettings" Title="Authentication Settings" CssClass="mt-3">
                    <Body>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox
                                    runat="server"
                                    ID="nbPasswordlessSignInDailyIpThrottle"
                                    NumberType="Integer"
                                    MinimumValue="1"
                                    Label="Passwordless Sign In Daily IP Throttle"
                                    Help="The maxiumum number of passwordless attempts that are allowed from a single IP address in a single day."
                                    CssClass="input-width-sm" />
                            </div>
                            <div class="col-md-6">
                                <Rock:NumberBox
                                    runat="server"
                                    ID="nbPasswordlessSignInSessionDuration"
                                    NumberType="Integer"
                                    MinimumValue="1"
                                    AppendText="minutes"
                                    Label="Passwordless Session Duration"
                                    Help="The amount of time in minutes that a passwordless session is valid."
                                    CssClass="input-width-md" />
                            </div>
                        </div>
                        <div class="row mt-3">
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList
                                    runat="server"
                                    ID="cblDisablePasswordlessSignInForAccountProtectionProfiles"
                                    Label="Disable Passwordless Sign In for the Following Protection Profiles"
                                    Help="Determines which individuals can use passwordless login depending on their protection profile."
                                    RepeatDirection="Horizontal" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList
                                    runat="server"
                                    ID="ddlPasswordlessConfirmationCommunicationTemplate"
                                    Label="Passwordless Confirmation Communication Template"
                                    DataTextField="Title"
                                    DataValueField="Guid"
                                    EnhanceForLongLists="true"
                                    Help="The system communication template to use for passwordless confirmations." />
                            </div>
                        </div>
                        <div class="row mt-3">
                            <div class="col-md-6">
                                <Rock:DateTimePicker
                                    runat="server"
                                    ID="dtpRejectAuthenticationCookiesIssuedBefore"
                                    Label="Reject Authentication Cookies Issued Before"
                                    Help="If a date and time are entered here, any authentication cookies issued before then will be rejected." />
                            </div>
                        </div>
                    </Body>
                </Rock:PanelWidget>
                <asp:Panel ID="pnlEditModeActions" runat="server" CssClass="actions">
                    <asp:LinkButton
                        runat="server"
                        ID="btnSave"
                        AccessKey="s"
                        ToolTip="Alt+s"
                        Text="Save"
                        CssClass="btn btn-primary"
                        OnClick="btnSave_Click" />
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $(function () {
            showHideWarningBox();
            $(".js-ignored-protection-profile").change(function () {
                showHideWarningBox();
            });
        });
    });

    function showHideWarningBox() {
        const highValue = <%=AccountProtectionProfile.High.ConvertToInt()%>;
        const extremeValue = <%=AccountProtectionProfile.Extreme.ConvertToInt()%>;

        const ignoredProfileCheckboxes = $(".js-ignored-protection-profile :checked");
        let highFound = false;
        let extremeFound = false;
        for (const chk of ignoredProfileCheckboxes) {
            const chkValue = $(chk).val();

            if (!highFound && highValue == chkValue) {
                highFound = true;
            }

            if (!extremeFound && extremeValue == chkValue) {
                extremeFound = true;
            }
        }

        if (!highFound || !extremeFound) {
            $(".js-duplicate-detection-notification-box").removeClass("d-none");
        } else {
            $(".js-duplicate-detection-notification-box").addClass("d-none");
        }
    }
</script>
