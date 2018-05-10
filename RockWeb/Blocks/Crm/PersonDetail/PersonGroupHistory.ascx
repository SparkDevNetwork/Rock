<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonGroupHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonGroupHistory" %>

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
                    <asp:LinkButton ID="btnApplyOptions" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApplyOptions_Click" />
                </div>
            </asp:Panel>

            <div class="panel-body">
                <div class="todo">
                    <Rock:RockLiteral ID="lSelectedGroupTypes" Label="Group Types" runat="server" Visible="false" />
                </div>

                <div class="person-group-history-swimlanes">
                    <code>
                        <pre>
                            <asp:Panel ID="divRawResponse" runat="server" />
                        </pre>
                    </code>
                </div>
                <div class="grouptype-legend">
                    <label>Group Type Legend</label>
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
                    var jsonString = JSON.stringify(data, null, 2);
                    $('#<%=divRawResponse.ClientID%>').html(jsonString);
                    });
            });


        </script>
    </ContentTemplate>
</asp:UpdatePanel>
