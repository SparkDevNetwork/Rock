<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleStatusBoard.ascx.cs" Inherits="GroupScheduleStatusBoard" %>

<asp:UpdatePanel ID="upScheduleStatusBoard" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-groupscheduler">
            <div class="panel-heading panel-follow">
                <h1 class="panel-title"><i class="fa fa-calendar"></i>Schedule Status Board
                </h1>
                <div class="panel-labels">
                    <asp:LinkButton ID="btnGroups" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnGroups_Click">
                        <i class="fa fa-users"></i>
                        Groups
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnDates" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnDates_Click">
                        <i class="fa fa-calendar"></i>
                        Dates
                    </asp:LinkButton>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>
            <div class="scrollable">
                <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false" />
                <div>
                    <asp:Literal ID="lGroupStatusTableHTML" runat="server" Visible="true" />
                </div>
            </div>
        </asp:Panel>
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <%-- Groups Picker Modal --%>
        <Rock:ModalDialog ID="dlgGroups" runat="server" Title="Groups" Visible="false" OnSaveClick="dlgGroups_SaveClick">
            <Content>
                <Rock:GroupPicker ID="gpGroups" runat="server" Label="Select Group(s)" AllowMultiSelect="true" LimitToSchedulingEnabledGroups="true" />
            </Content>
        </Rock:ModalDialog>

        <%-- Date Range Picker Modal --%>
        <Rock:ModalDialog ID="dlgDateRangeSlider" runat="server" Title="Dates" Visible="false" OnSaveClick="dlgDateRangeSlider_SaveClick">
            <Content>
                <Rock:RangeSlider ID="rsDateRange" runat="server" Label="Number of upcomming weeks to display." MaxValue="16" MinValue="1" />
            </Content>
        </Rock:ModalDialog>
        <script type="text/javascript">

            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize();

                $('.js-group-header').on('click', function () {
                    $(this).find('.fa').css({'transform' : 'rotate(-90deg)'});
                    var $groupLocations = $(this).closest('.js-group-locations');
                    var locationsExpanded = $groupLocations.data('locations-expanded') == 1;
                    if (locationsExpanded) {
                        $('.js-location-row', $groupLocations)
                            .children('td')
                            .children()
                            .slideUp();
                        //.addClass('hidden-row');
                        $groupLocations.data('locations-expanded', 0);
                    }
                    else {
                    $(this).find('.fa').css({'transform' : 'rotate(0deg)'});
                        $('.js-location-row', $groupLocations).children('td')
                            .children()
                            .slideDown();
                        $groupLocations.data('locations-expanded', 1);
                    }
                });
            });
        </script>
    </ContentTemplate>


</asp:UpdatePanel>
