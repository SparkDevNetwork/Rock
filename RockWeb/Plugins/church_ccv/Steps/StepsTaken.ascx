<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepsTaken.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Steps.StepsTaken" %>

<script>

    Sys.Application.add_load( function () {
        $('.value-tip').tooltip();

        $(".js-settings-toggle").on("click", function () {
            $('.js-settings-panel').slideToggle();
        });

        $('.js-date-tooltip').tooltip();
    });
</script>

<style>
    .measurechart {
        position: relative;
        height: 200px;
        margin-bottom: 24px;
    }
    .measurechart-legend {
        position: absolute;
        top: 2px;
        left: 4px;
    }

    .measurechart-legend h1 {
        margin: 0 0 4px 0;
        font-size: 18px;
    }

    .measurechart-legend-value {
        margin-top: -14px;
        letter-spacing: -2px;
    }

    .measurechart-legend-value span {
        font-size: 42px;
        font-weight: 900;
    }

    .measurechart-legend-value i {
        font-size: 38px;
        margin-left: -6px;
    }

    .chartwrapper {
        position:absolute;
        bottom: 0;
        height: 120px;
        width: 100%;
    }

    .adultmetric-cell {
        border: 3px solid #6a6a6a;
        margin: 12px;
        padding: 12px;
    }

    .adultmetric-cell h1 {
        margin: 0 0 4px 0;
        font-size: 18px;
    }

    .adultmetric-value {
        font-size: 42px;
        font-weight: 900;
        margin-top: -12px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-road"></i> Steps Taken</h1>
                <div class="pull-right">
                    <div class="js-date-tooltip" data-toggle="tooltip" data-placement="top" title="Click to change date range.">
                        <Rock:HighlightLabel ID="hlDate" runat="server" CssClass="js-settings-toggle cursor-pointer" />
                    </div>
                </div>
            </div>
            <div class="panel-body">
                <div class="panel-settings js-settings-panel" style="display: none;">
                    <div class="row">
                        <div class="col-md-8">
                            <Rock:SlidingDateRangePicker ID="drpDateRange" runat="server" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" EnabledSlidingDateRangeUnits="Week, Month, Year" />
                         </div>
                        <div class="col-md-4 text-right">
                            <asp:LinkButton ID="lbSetDateRange" runat="server" CssClass="btn btn-primary btn-sm margin-t-lg" Text="Update" OnClick="lbSetDateRange_Click"  />
                        </div>
                    </div>
                </div>

                <ul class="nav nav-pills margin-b-md">
                    <li id="liAdults" runat="server" class="active">
                        <asp:LinkButton ID="lbAdults" runat="server" Text="Adults" OnClick="lbTab_Click" />
                    </li>
                    <li id="liPastor" runat="server">
                        <asp:LinkButton ID="lbPastor" runat="server" Text="Pastor" OnClick="lbTab_Click" />
                    </li>
                    <li id="liTotals" runat="server" >
                        <asp:LinkButton ID="lbTotals" runat="server" Text="Totals" OnClick="lbTab_Click" />
                    </li>
                    <li id="liStepDetails" runat="server">
                        <asp:LinkButton ID="lbStepDetails" runat="server" Text="Step Details" OnClick="lbTab_Click" />
                    </li>
                </ul>

                <asp:Panel ID="pnlAdults" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <h2><asp:Literal ID="lCampusCampus" runat="server" Text="All Campuses" /></h2>
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpCampusCampus" runat="server" OnSelectedIndexChanged="cpCampusCampus_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>
                    
                    <div class="row">
                        <asp:Repeater ID="rptCampusMeasures" runat="server" OnItemDataBound="rptCampusMeasures_ItemDataBound">
                            <ItemTemplate>

                                <div class="col-md-6">
                                    <div class="measurechart" style="border: 3px solid <%# Eval("Color") %>; color: <%# Eval("Color") %>;">
                                        <div class="measurechart-legend">
                                            <h1><%# Eval("Title") %></h1>
                                            <div class="measurechart-legend-value"><i class="fa fa-fw <%# Eval("IconCssClass") %>"></i> <span><asp:Literal ID="lChartValue" runat="server" /></span></div>
                                        </div>
                                        <div class="chartwrapper">
                                            <Rock:LineChart ID="lcMeasure" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="120px" />
                                        </div>
                                    </div>
                                </div>

                            </ItemTemplate>
                        </asp:Repeater>
                    </div> 
                </asp:Panel>

                <asp:Panel ID="pnlPastor" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <h2><asp:Literal ID="lPastorPastor" runat="server" Text="All Pastors" /></h2>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlPastor" Label="Pastor" runat="server" OnSelectedIndexChanged="ddlPastor_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>

                    <div class="row">
                        <asp:Repeater ID="rptPastorMeasures" runat="server" OnItemDataBound="rptPastorMeasures_ItemDataBound">
                            <ItemTemplate>

                                <div class="col-md-6">
                                    <div class="measurechart" style="border: 3px solid <%# Eval("Color") %>; color: <%# Eval("Color") %>;">
                                        <div class="measurechart-legend">
                                            <h1><%# Eval("Title") %></h1>
                                            <div class="measurechart-legend-value"><i class="fa fa-fw <%# Eval("IconCssClass") %>"></i> <span><asp:Literal ID="lChartValue" runat="server" /></span></div>
                                        </div>
                                        <div class="chartwrapper">
                                            <Rock:LineChart ID="lcMeasure" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="120px" />
                                        </div>
                                    </div>
                                </div>

                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlTotals" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <h2><asp:Literal ID="lAdultsCampus" runat="server" Text="All Campuses" /></h2>
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpAdultsCampus" runat="server" OnSelectedIndexChanged="cpAdultsCampus_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <div class="adultmetric-cell">
                                <h1>Unique Adults Who Took A Step</h1>
                                <div class="adultmetric-value"><asp:Literal ID="lAdultUniqueAdults" runat="server" /> <i class="fa fa-user"></i></div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="adultmetric-cell">
                                <h1>Total Number of Steps Taken</h1>
                                <div class="adultmetric-value"><asp:Literal ID="lAdultsTotalSteps" runat="server" /> <i class="fa fa-road"></i></div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="adultmetric-cell">
                                <h1>Average Steps Per Person</h1>
                                <div class="adultmetric-value"><asp:Literal ID="lAdultsAvergeSteps" runat="server" /> <i class="fa fa-calculator"></i></div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlStepDetails" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <h2><asp:Literal ID="lDetailCampus" runat="server" Text="All Campuses" /></h2>
                        </div>
                        <div class="col-md-6">
                            <Rock:CampusPicker ID="cpDetailCampus" runat="server" OnSelectedIndexChanged="cpDetailCampus_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>
                    
                    <div class="grid">
                         <Rock:GridFilter ID="gfStepDetails" runat="server">
                            <Rock:RockDropDownList ID="ddlMeasureType" runat="server" Label="Step Type" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gStepDetails" runat="server">
                            <Columns>
                                <asp:BoundField DataField="DateTaken" HeaderText="Date Taken" dataformatstring="{0:M/d/yyyy}" />
                                <asp:BoundField DataField="StepMeasureTitle" HeaderText="Step" />
                                <asp:BoundField DataField="FullName" HeaderText="Name" />
                                <asp:BoundField DataField="Campus" HeaderText="Campus" />
                                <asp:BoundField DataField="Address" HeaderText="Address" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
