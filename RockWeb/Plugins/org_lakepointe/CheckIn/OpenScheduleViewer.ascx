<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OpenScheduleViewer.ascx.cs" Inherits="RockBlocks.Plugins.org_lakepointe.Checkin.OpenScheduleViewer" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Panel ID="pnlFilter" runat="server">
                <div class="panel panel-block margin-t-md">
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>Open Schedules
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbFiltersError" runat="server" NotificationBoxType="Danger" Visible="false" />
                        <div class="filter-list">
                            <div class="row">
                                <asp:ValidationSummary runat="server" HeaderText="Please correct the following to run report" CssClass="alert alert-validation" ValidationGroup="ReportFilter" />
                            </div>

                            <div class="row">
                                <asp:Panel ID="pnlCampus" runat="server" CssClass="filterfield">
                                    <div class="col-md-2">
                                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" AutoPostBack="true" IncludeInactive="false" />
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" Label="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />
                                        <Rock:RockDropDownList ID="ddlCheckinType" runat="server" CssClass="input-xlarge" Label="Check-in Configuration" DataTextField="Name" DataValueField="Id"/>
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:DateTimePicker ID="dpStart" runat="server" Label="Start Date" Required="true" RequiredErrorMessage="Check in Start Date is required" ValidationGroup="ReportFilter" Help="Start Date/Time to Report on" />
                                        <Rock:DateTimePicker ID="dpEnd" runat="server" Label="End Date" Help="End Date to report on ." />
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12 actions margin-t-md">
                                <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Filter" CssClass="btn btn-primary btn-sm" CausesValidation="true" ValidationGroup="ReportFilter" OnClick="btnFilter_Click" />
                                <asp:LinkButton ID="btnFilterSetDefault" runat="server" Text="Reset Filters" ToolTip="Set the filter to its default values" CssClass="btn btn-link btn-sm pull-right" OnClick="btnFilterSetDefault_Click" />

                            </div>
                        </div>
                        <div style="width:100%;height:300px;text-align:center;margin:10px">        
                            <div id="flot-placeholder" style="width:100%;height:100%;"></div>        
                        </div>
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <Rock:NotificationBox ID="nbGroupsError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                            <Rock:RockLiteral ID="rlScheduleName" runat="server" Label="List of Groups and Locations" />
                            <Rock:Grid ID="gGroupLocations" runat="server" AllowSorting="true" RowItemText="Group" >
                                <Columns>
                                    <Rock:SelectField />
                                    <Rock:RockBoundField DataField="LocationName" HeaderText="Location" SortExpression="Location" />
                                    <Rock:RockBoundField DataField="OpenClose" HeaderText="Open/Closed" SortExpression="OpenClose" />
                                    <Rock:RockBoundField DataField="GroupName" HeaderText="Group" SortExpression="GroupName" />
                                    <Rock:RockBoundField DataField="GroupType" HeaderText="Group Type" SortExpression="GroupType" />
                                </Columns>
                            </Rock:Grid>
                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>