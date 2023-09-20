<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LeadershipCohort.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Forms.LeadershipCohort" %>
<style>
</style>
<asp:UpdatePanel ID="upLeadershipCohort" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />
        <asp:Panel ID="pnlMain" runat="server" Visible="true">
            <div class="row" style="background-color: #f04b28; color: #fff; padding-left: 15px; padding-right: 15px; margin-bottom: 30px;">
                <asp:Literal ID="lPageTitle" runat="server" Visible="true" />
            </div>
            <div class="row">
                <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" Visible="true" AutoPostBack="true" />
            </div>
            <div class="row">
                <Rock:RockDropDownList ID="ddlDayOfWeek" runat="server" Label="Day of Week" Visible="true" AutoPostBack="true" />
            </div>
            <div class="row">
                <Rock:RockDropDownList ID="ddlTimeOfDay" runat="server" Label="Time of Day" Visible="true" AutoPostBack="true" />
            </div>
            <div class="row">
                <Rock:RockDropDownList ID="ddlCohort" runat="server" Label="Cohort Leader" Visible="true" AutoPostBack="true" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbConfirmation" runat="server" NotificationBoxType="Success" />
        </asp:Panel>

        <asp:Panel ID="pnlNavigation" runat="server" Visible="true">
            <div class="row" style="margin-top: 20px">
                <div class="col-md-12 text-center">
                    <asp:LinkButton ID="lbSave" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vgSignup" CssClass="btn btn-primary btn-lg">Submit</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
