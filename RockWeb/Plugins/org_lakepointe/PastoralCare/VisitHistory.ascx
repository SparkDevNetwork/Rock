<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VisitHistory.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.PastoralCare.VisitHistory" %>
<asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="panel panel-block panel-analytics">
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fas fa-users-medical"></i>Visit History
                </h1>
                <div class="panel-labels">
                    <%--label info--%>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>
            <Rock:NotificationBox ID="nbInfo" runat="server" Visible="false" />
            <div class="panel-body">
                <div class="row row-eq-height">
                    <div class="col-md-3 filter-options">
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Visit Type" AutoPostBack="true" OnSelectedIndexChanged="ddlWorkflowType_SelectedIndexChanged" />
                        <Rock:NotificationBox ID="nbWorkflowTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please Select a Visit Type" Visible="false" />

                        <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="DateRange" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" EnabledSlidingDateRangeUnits="Day, Week, Month, Year" />
                        <Rock:NotificationBox ID="nbDateRangeWarning" runat="server" NotificationBoxType="Warning" Text="Date Range is Required" Visible="false" />

                        <Rock:DefinedValuesPickerEnhanced ID="dvpFacilityType" runat="server" Label="Hospital" Help="The facility to display visits for, Leave blank to not filter by facility." />
                        <Rock:PersonPicker ID="ppVisitor" runat="server" EnableSelfSelection="true" Label="Visitor" Help="The visitor to display visits for. Leave blank to not filter by visitor." />

                    </div>
                    <div class="col-md-9">
                        <div class="row analysis-types">
                            <div class="col-sm-8">
                                <div class="controls">
                                    <div class="js-show-by">
                                        <Rock:HiddenFieldWithClass ID="hfShowBy" CssClass="js-hidden-selected" runat="server" />
                                        <div class="btn-group">
                                            <%--show by here--%>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="actions text-right">
                                    <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" ToolTip="Update the Grid" OnClick="btnApply_Click">
                                        <i class="fa fa-refresh"></i> Update
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <div class="grid">
                                <Rock:Grid ID="gVisits" runat="server" AllowSorting="true" RowItemText="Visit" OnGridRebind="gVisits_GridRebind">
                                    <Columns>
                                        <Rock:SelectField />
                                        <Rock:RockBoundField DataField="FacilityName" HeaderText="Facility" Visible="true" ExcelExportBehavior="AlwaysInclude" SortExpression="Facility.Name" />
                                        <Rock:RockBoundField DataField="FacilityAddress" HeaderText="Address" Visible="true" />
                                        <Rock:DateField DataField="VisitDate" HeaderText="Visit Date" DataFormatString="{0:d}" SortExpression="VisitDate" />
                                        <Rock:RockBoundField DataField="PersonVisitedFullName" HeaderText="Person Visited" SortExpression="PersonvisitedFullNameReversed" />
                                        <Rock:RockBoundField DataField="VisitorFullName" HeaderText="Visitor" SortExpression="VisitorFullNameReversed" />
                                        <Rock:RockBoundField DataField="VisitNotes" HeaderText="Notes" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </asp:Panel>
                    </div>

                </div>
            </div>

        </div>

        <script>
            Sys.Application.add_load(function() {
                Rock.controls.fullScreen.initialize();
            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
