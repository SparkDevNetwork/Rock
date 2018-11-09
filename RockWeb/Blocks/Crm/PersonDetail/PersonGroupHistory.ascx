<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonGroupHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonGroupHistory" %>

<style>
.panel-fullwidth {
    margin: -15px -15px 0 -15px;
}

.swimlanes .grid-background {
  fill: none; }

.swimlanes .grid-header {
  fill: #f7f7f7;
  stroke: #e5e5e5;
  stroke-width: .3; }

.swimlanes .grid-row {
  fill: #fff; }

.swimlanes .grid-row:nth-child(even) {
  fill: #f9f9f9; }

.swimlanes .row-line {
  stroke: #e5e5e5; }

.swimlanes .tick {
  stroke: #e5e5e5;
  stroke-width: .2; }
  .swimlanes .tick.thick {
    stroke-width: .4; }

.swimlanes .today-highlight {
  opacity: .5;
  fill: #fcf8e3; }

.swimlanes .bar {
  user-select: none;
  transition: stroke-width .3s ease;
  fill: #b8c2cc;
  stroke: #8d99a6;
  stroke-width: 0;
  opacity: 0.8 }

.swimlanes .bar-invalid {
  fill: transparent;
  stroke: #8d99a6;
  stroke-width: 1;
  stroke-dasharray: 5; }
  .swimlanes .bar-invalid ~ .bar-label {
    fill: #555; }

.swimlanes .bar-label {
  font-size: 12px;
  font-weight: 400;
  fill: #000;
  dominant-baseline: central;
  text-anchor: middle; }
  .swimlanes .bar-label.big {
    fill: #555;
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
  fill: #555; }

.swimlanes .lower-text {
  font-size: 12px;
  fill: #333; }

.swimlanes .hide {
  display: none; }

.swimlanes-container {
  position: relative;
  overflow: scroll;
  font-size: 12px; }
  .swimlanes-container .popup-wrapper {
    position: absolute;
    top: 0;
    left: 0;
    padding: 0;
    color: #959da5;
    background: rgba(0, 0, 0, 0.8);
    border-radius: 3px; }
    .swimlanes-container .popup-wrapper .title {
      padding: 10px;
      border-bottom: 1px solid #fff; }
      .swimlanes-container .popup-wrapper .title a {
        color: #fff;
        font-size: 16px; }
    .swimlanes-container .popup-wrapper .subtitle {
      padding: 10px;
      color: #dfe2e5; }
    .swimlanes-container .popup-wrapper .pointer {
      position: absolute;
      height: 5px;
      margin: 0 0 0 -5px;
      border: 5px solid transparent;
      border-top-color: rgba(0, 0, 0, 0.8); }

</style>

<script src="/scripts/rock-swimlanes.min.js"></script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfStartDateTime" runat="server" />
        <asp:HiddenField ID="hfStopDateTime" runat="server" />
        <asp:HiddenField ID="hfGroupTypeIds" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-history"></i>
                    Group History
                </h1>
                <a class="btn btn-xs btn-default pull-right margin-l-sm" onclick="javascript: toggleOptions()">
                    <i title="Options" class="fa fa-gear"></i>
                </a>
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

            <div class="panel-body">
                <asp:Panel ID="groupHistorySwimlanes" CssClass="panel-fullwidth" runat="server" />

                <div class="grouptype-legend">
                    <label>Group Types</label>
                    <div class="grouptype-legend">
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

                $.ajax({
                    url: restUrl,
                    dataType: 'json',
                    contentType: 'application/json'
                }).done(function (data) {
                    var swimlanes_vis = new Swimlanes('#<%=groupHistorySwimlanes.ClientID%>', data);
                });


                $(document).mousemove(function(e) {
                        currentMousePos.x = e.pageX - $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').offset().left + $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').scrollLeft(),
                        currentMousePos.y = e.pageY + $('#<%=groupHistorySwimlanes.ClientID%> .swimlanes-container').scrollTop()
                });
            });


        </script>
    </ContentTemplate>
</asp:UpdatePanel>
