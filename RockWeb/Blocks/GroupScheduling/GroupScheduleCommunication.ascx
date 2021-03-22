<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleCommunication.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduleCommunication" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>

            Sys.Application.add_load(function () {

                var $communicationConfiguration = $(".js-communication-configuration");

                $('.js-locations-picker .control-label', $communicationConfiguration).on('click', function () {
                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', 'select-all-locations')";
                })

            });

        </script>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-envelope"></i>
                    Group Schedule Communication
                </h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="js-communication-configuration">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:GroupPicker ID="gpGroups" runat="server" AllowMultiSelect="true" Label="Groups" Required="true" OnSelectItem="gpGroups_SelectItem" LimitToSchedulingEnabledGroups="true" />
                            <Rock:RockCheckBoxList ID="cblInviteStatus" runat="server" Label="Invite Status" RepeatDirection="Horizontal" Required="true" />
                            <Rock:RockListBox ID="lbSchedules" runat="server" Label="Schedules" AutoPostBack="true" OnSelectedIndexChanged="lbSchedules_SelectedIndexChanged" Required="true" />
                            <Rock:RockDropDownList ID="ddlWeek" runat="server" Label="Week" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIncludeChildGroups" runat="server" Label="Include Child Groups" AutoPostBack="true" OnCheckedChanged="cbIncludeChildGroups_CheckedChanged" />
                            <Rock:RockCheckBoxList ID="cblLocations" runat="server" Label="Locations" FormGroupCssClass="js-locations-picker" Required="true" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:NotificationBox ID="nbCommunicationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                        <asp:LinkButton ID="btnCreateCommunication" runat="server" CssClass="btn btn-primary" Text="Create Communication" OnClick="btnCreateCommunication_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
