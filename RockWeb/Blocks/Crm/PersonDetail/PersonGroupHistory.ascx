<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonGroupHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonGroupHistory" %>

<style>
.swimlanes {
  min-width: 100%;
}
.swimlanes .grid-background {
  fill: none; }

.swimlanes .grid-header {
  fill: var(--color-interface-softer);
  stroke: var(--color-interface-soft);
  stroke-width: .3; }

.swimlanes .grid-row {
  fill: var(--color-interface-softest); }

.swimlanes .grid-row:nth-child(even) {
  fill: var(--color-interface-softer); }

.swimlanes .row-line {
  stroke: var(--color-interface-soft);
}

.swimlanes .tick {
  stroke: var(--color-interface-softer);
  stroke-width: .2; }
  .swimlanes .tick.thick {
    stroke-width: .4; }

.swimlanes .today-highlight {
  opacity: .5;
  fill: var(--color-primary-soft); }

.swimlanes .bar {
  user-select: none;
  transition: stroke-width .3s ease;
  fill: var(--color-interface-soft);
  stroke: var(--color-interface-medium);
  stroke-width: 0;
  opacity: 0.8 }

.swimlanes .bar-invalid {
  fill: transparent;
  stroke: var(--color-interface-medium);
  stroke-width: 1;
  stroke-dasharray: 5; }
  .swimlanes .bar-invalid ~ .bar-label {
    fill: var(--color-interface-strong); }

.swimlanes .bar-label {
  font-size: 12px;
  font-weight: 400;
  fill: var(--color-interface-stronger);
  dominant-baseline: central;
  text-anchor: middle; }
  .swimlanes .bar-label.big {
    fill: var(--color-interface-strong);
    text-anchor: start; }

.swimlanes .bar-wrapper {
  cursor: pointer; }
  .swimlanes .bar-wrapper:hover .bar, .swimlanes .bar-wrapper.active .bar {
    stroke-width: 2;
    opacity: .8; }
  .swimlanes .bar-wrapper.is-leader .bar {
    opacity: 1; }

.swimlanes .lower-text,
.swimlanes .upper-text {
  font-size: 14px;
  text-anchor: middle; }

.swimlanes .upper-text {
  font-weight: 800;
  fill: var(--color-interface-strong); }

.swimlanes .lower-text {
  font-size: 12px;
  fill: var(--color-interface-stronger); }

.swimlanes .hide {
  display: none; }

.swimlanes-container {
  position: relative;
  overflow: auto;
  border-bottom: 1px solid var(--color-interface-soft);
  font-size: 12px; }
  .swimlanes-container .popup-wrapper {
    position: absolute;
    top: 0;
    left: 0;
    padding: 0;
    color: var(--color-interface-medium);
    background: var(--color-interface-strong);
    border-radius: 3px; }
    .swimlanes-container .popup-wrapper .title {
      padding: 10px;
      border-bottom: 1px solid var(--color-interface-softest);
      .swimlanes-container .popup-wrapper .title a {
        color: var(--color-interface-softest);
        font-size: 16px; }
    }
    .swimlanes-container .popup-wrapper .subtitle {
      padding: 10px;
      color: var(--color-interface-softer); }
    .swimlanes-container .popup-wrapper .pointer {
      position: absolute;
      height: 5px;
      margin: 0 0 0 -5px;
      border: 5px solid transparent;
      border-top-color: var(--color-interface-strong); }

.grouptype-legend {
  display: flex;
  margin-top: var(--spacing-medium);
  align-items: center;
  gap: var(--spacing-medium);
  label {
    color: var(--color-interface-medium);
    font-weight: var(--font-weight-medium);
    font-size: var(--font-size-small);
  }
}

</style>

<script src="<%=this.RockPage.ResolveRockUrl("~/scripts/rock-swimlanes.min.js", true) %>"></script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfStartDateTime" runat="server" />
        <asp:HiddenField ID="hfStopDateTime" runat="server" />
        <asp:HiddenField ID="hfGroupTypeIds" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="ti ti-history"></i>
                    Group History
                </h1>
                <div class="panel-labels">
                <div class="btn-group btn-toggle js-view-toggle">
                  <a class="btn btn-default btn-xs" href="#" data-view="Month">Month</a>
                  <a class="btn btn-default btn-xs js-view-year btn-primary active" href="#" data-view="Year">Year</a>
                </div>
                  <a class="btn btn-xs btn-default btn-square" onclick="javascript: toggleOptions()">
                      <i title="Options" class="ti ti-settings"></i>
                  </a>
                </div>
            </div>

            <asp:Panel ID="pnlOptions" runat="server" Title="Options" CssClass="panel-body js-options" Style="display: none">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupTypesPicker ID="gtGroupTypesFilter" runat="server" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnApplyOptions" runat="server" Text="Apply" CssClass="btn btn-primary btn-xs" OnClick="btnApplyOptions_Click" />
                </div>
            </asp:Panel>

            <div class="panel-body styled-scroll">
                <asp:Panel ID="groupHistorySwimlanes" CssClass="panel-fullwidth" runat="server" />
                <div class="js-no-group-history" style="display:none">
                    <Rock:NotificationBox ID="nbNoGroupHistoryFound" runat="server" NotificationBoxType="Info" Text="No Group History Available" />
                </div>

                <div class="grouptype-legend">
                    <label>Group Types:</label>
                    <div class="grouptype-legend-items">
                        <asp:Repeater ID="rptGroupTypeLegend" runat="server" OnItemDataBound="rptGroupTypeLegend_ItemDataBound">
                            <ItemTemplate>
                                <span class="padding-r-sm">
                                    <asp:Literal ID="lGroupTypeBadgeHtml" runat="server" /></span>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Literal ID="lGroupTimeLegend" runat="server" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            var currentMousePos = { x: -1, y: -1 };

            function toggleOptions() {
                $('.js-options').slideToggle();
            }

            Sys.Application.add_load(function () {

                var restUrl = '<%=ResolveUrl( "~/api/GroupMemberHistoricals/GetGroupHistoricalSummary" ) %>'
                var personId = $('#<%=hfPersonId.ClientID%>').val();
                var startDateTime = $('#<%=hfStartDateTime.ClientID%>').val();
                var stopDateTime = $('#<%=hfStopDateTime.ClientID%>').val();
                var groupTypeIds = $('#<%=hfGroupTypeIds.ClientID%>').val();

                restUrl += '?PersonId=' + personId;
                if (startDateTime) {
                    restUrl += '&startDateTime=' + startDateTime
                }

                if (stopDateTime) {
                    restUrl += '&stopDateTime=' + stopDateTime
                }

                if (groupTypeIds && groupTypeIds != '') {
                    restUrl += '&groupTypeIds=' + groupTypeIds
                }

                var $swimlanesContainer = $('#<%= groupHistorySwimlanes.ClientID %>');
                var $noGroupHistory = $('.js-no-group-history');
                var $viewToggle = $('#<%= upnlContent.ClientID %> .js-view-toggle');

                $.ajax({
                    url: restUrl,
                    dataType: 'json',
                    contentType: 'application/json'
                }).done(function (data) {
                    if (data.length) {
                        var swimlanes_vis = new Swimlanes($swimlanesContainer[0], data, { view_mode: "Year" });
                        $swimlanesContainer.show();
                        $noGroupHistory.hide();

                        $viewToggle.on('click', '.btn', function() {
                          if ($(this).hasClass('active')) {
                              return;
                          }
                          $(this).addClass('btn-primary active');
                          $(this).siblings().removeClass('btn-primary active');
                          var view = $(this).data('view');
                          swimlanes_vis.change_view_mode(view)
                        });
                    } else {
                        $swimlanesContainer.hide();
                        $viewToggle.hide();
                        $noGroupHistory.show();
                    }
                });


                $(document).mousemove(function(e) {
                        currentMousePos.x = e.pageX - $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').offset().left + $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').scrollLeft(),
                        currentMousePos.y = e.pageY + $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').scrollTop()
                });
            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
