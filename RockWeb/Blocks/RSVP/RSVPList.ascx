<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSVPList.ascx.cs" Inherits="RockWeb.Blocks.RSVP.RSVPList" %>

<script type="text/javascript">
    var borderColor = '#E0E0E0';
    var bgColor_Accepted = '#16C98D';
    var bgColor_Declined = '#D4442E';
    var bgColor_NoResponse = '#FFC870';
    function drawBarChart(canvasId, accepted, declined, noResponse) {
        var barCtx = $('#' + canvasId)[0].getContext('2d');

        var barChart = new Chart(barCtx, {
            type: 'bar',
            data: {
                labels: ['Accepted', 'Declined', 'No Response'],
                datasets: [
                    {
                        label: 'Accepted',
                        backgroundColor: bgColor_Accepted,
                        borderColor: borderColor,
                        data: [ accepted ],
                    },
                    {
                        label: 'Declined',
                        backgroundColor: bgColor_Declined,
                        borderColor: borderColor,
                        data: [ declined ]
                    },
                    {
                        label: 'No Response',
                        backgroundColor: bgColor_NoResponse,
                        borderColor: borderColor,
                        data: [ noResponse ]
                    }
                ]
            },
            options: {
                scales: {
                    xAxes: [{ stacked: true }],
                    yAxes: [{ stacked: true }]
                }
            }
        });
    }
</script>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlRSVPItems" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-user-check"></i>
                            RSVP Occurrences
                        </h1>
                    </div>

                    <div class="panel-body">
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnApplyFilterClick="rFilter_ApplyFilterClick">
                                <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                                <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                    <asp:ListItem Text="All" Value="" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" DataValueField="Key" DataTextField="Value"  />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRSVPItems" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRSVPItems_RowSelected" DataKeyNames="Id">
                                <Columns>
                                    <Rock:DateField DataField="OccurrenceDate" HeaderText="Date" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" SortExpression="OccurrenceDate" />
                                    <Rock:RockBoundField DataField="ScheduleName" HeaderText="Schedule" SortExpression="ScheduleName" />
                                    <Rock:RockBoundField DataField="LocationName" HeaderText="Location" SortExpression="LocationName" />
                                    <Rock:RockBoundField DataField="InvitedCount" HeaderText="Invited" SortExpression="InvitedCount" />
                                    <Rock:RockBoundField DataField="AcceptedCount" HeaderText="Accepted" SortExpression="AcceptedCount" />
                                    <Rock:RockBoundField DataField="DeclinedCount" HeaderText="Declined" SortExpression="DeclinedCount" />
                                    <Rock:RockBoundField DataField="NoResponseCount" HeaderText="No Response" SortExpression="NoResponseCount" />
                                    <Rock:LinkButtonField CssClass="btn btn-default btn-sm" ID="btnDetails" Text="<i class='fa fa-user-check' title='Detauls'></i>" OnClick="btnDetails_Click" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
