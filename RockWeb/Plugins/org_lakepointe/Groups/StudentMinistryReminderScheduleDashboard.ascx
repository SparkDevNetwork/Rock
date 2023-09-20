<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StudentMinistryReminderScheduleDashboard.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.StudentMinistryReminderScheduleDashboard" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <style type="text/css">
            table.schedule-list thead tr th, table.schedule-list tbody tr th {
                color: #fff;
                font-size:16px;
                background-color:#f04b28;
				font-weight:600;

            }

        </style>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" NotificationBoxType="Danger" />
        <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlView" runat="server">
            <div class="row">
                <div class="col-md-12">
                    <p>This page displays the current status of the scheduled jobs that send out reminder texts to students.
                        The times listed below are the scheduled times when the messages will be sent, not the times of the events themselves.
                    </p>
                    <p>Toggling the respective indicators will enable or disable those particular scheduled jobs.</p>
                </div>
            </div>
            <asp:Repeater ID="rDashboard" runat="server">
<HeaderTemplate>
                <table class="table table-bordered schedule-list">
                    <thead>
                        <tr>
                            <th>Campus</th>
                            <th>Connect</th>
                            <th>Groups</th>
                            <th>United</th>
                        </tr>
                    </thead>
                    <tbody>
</HeaderTemplate>

<ItemTemplate>
                        <tr>
                            <th ><asp:Literal ID="lCampus" runat="server" /></th>
                            <td>
                                <Rock:Toggle ID="tConnect" runat="server" OnText="Enabled" OffText="Disabled" OnCssClass="btn-success" OffCssClass="btn-danger" />
                                <asp:Literal ID="lConnect" runat="server" />
                            </td>
                            <td>
                                <Rock:Toggle ID="tGroups" runat="server" OnText="Enabled" OffText="Disabled" OnCssClass="btn-success" OffCssClass="btn-danger" />
                                <asp:Literal ID="lGroups" runat="server" />
                            </td>
                            <td>
                                <Rock:Toggle ID="tUnited" runat="server" OnText="Enabled" OffText="Disabled" OnCssClass="btn-success" OffCssClass="btn-danger" />
                                <asp:Literal ID="lUnited" runat="server" />
                            </td>
                        </tr>
</ItemTemplate>

<FooterTemplate>
                    </tbody>
                </table>
</FooterTemplate>

            </asp:Repeater>
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>