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
                <h3>Rock Security Settings</h3>
                <p>
                    The settings below allow you to adjust how security in Rock is configured in your instance. Be sure that you fully understand each setting to ensure proper configuration.
                </p>
                <hr />
                <Rock:NotificationBox
                    runat="server"
                    ID="nbWarning"
                    NotificationBoxType="Danger"
                    Visible="true"
                    Text="We highly recommend that you prevent duplicate detection for individuals with an Account Protection Profile of High and Extreme"
                    CssClass="js-notification-box d-none" />

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
                            Help="This disables duplication protection checks for individuals with the selected Account Protection Profiles. People with these checked values will always create duplicates (i.e., they will not match existing records.) We highly recommend enabling this for all but the low profile."
                            RepeatDirection="Horizontal"
                            CssClass="js-ignored-protection-profile" />
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
                    <div class="col-md-6 col-sm-12 col-xs-12">
                        <Rock:RockCheckBoxList
                            runat="server"
                            ID="cblDisableTokensForAccountProtectionProfiles"
                            Label="Disable Usage of Personal Tokens for the Following Protection Profiles"
                            Help="Any protection profiles selected here will not be allowed to use impersonation tokens or tokens to authenticate a person."
                            RepeatDirection="Horizontal"
                            CssClass="js-ignored-protection-profile" />
                    </div>
                </div>
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
            $(".js-notification-box").removeClass("d-none");
        } else {
            $(".js-notification-box").addClass("d-none");
        }
    }
</script>
