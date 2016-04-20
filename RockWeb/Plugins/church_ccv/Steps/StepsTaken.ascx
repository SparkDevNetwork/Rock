<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepsTaken.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Steps.StepsTaken" %>

<script>

    Sys.Application.add_load( function () {
        $('.value-tip').tooltip();

        $(".js-settings-toggle").on("click", function () {
            $('.js-settings-panel').slideToggle();
        });
    });

</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-road"></i> Steps Taken</h1>
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlDate" runat="server" CssClass="js-settings-toggle cursor-pointer" />
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
                    <li id="liCampus" runat="server" class="active">
                        <asp:LinkButton ID="lbCampus" runat="server" Text="Campus" OnClick="lbTab_Click" />
                    </li>
                    <li id="liPastor" runat="server">
                        <asp:LinkButton ID="lbPastor" runat="server" Text="Pastor" OnClick="lbTab_Click" />
                    </li>
                    <li id="liAdults" runat="server" >
                        <asp:LinkButton ID="lbAdults" runat="server" Text="Adults" OnClick="lbTab_Click" />
                    </li>
                    <li id="liStepDetails" runat="server">
                        <asp:LinkButton ID="lbStepDetails" runat="server" Text="Step Details" OnClick="lbTab_Click" />
                    </li>
                </ul>

                <asp:Panel ID="pnlCampus" runat="server">
                    <h4>Not Implemented</h4>
                </asp:Panel>

                <asp:Panel ID="pnlPastor" runat="server" Visible="false">
                    <h4>Not Implemented</h4>
                </asp:Panel>

                <asp:Panel ID="pnlAdults" runat="server" Visible="false">
                    <h4>Not Implemented</h4>
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
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
