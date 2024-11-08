<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinder.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupFinder" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <rock:notificationbox id="nbNotice" runat="server" visible="false" />

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-map-marker"></i>Group Finder
                </h1>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lTitle" runat="server" />

                <asp:Panel ID="pnlSearch" runat="server">

                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <rock:addresscontrol id="acAddress" runat="server" required="true" requirederrormessage="Your Address is Required" />
                    <rock:rockcheckboxlist id="cblCampus" runat="server" label="Campuses" datatextfield="Name" datavaluefield="Id" repeatdirection="Horizontal" />
                    <asp:PlaceHolder ID="phFilterControls" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                        <asp:LinkButton ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-link" OnClick="btnClear_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlResults" runat="server" Visible="false">

                    <asp:Literal ID="lMapStyling" runat="server" />

                    <rock:rockdropdownlist id="ddlPageSize" runat="server" onselectedindexchanged="ddlPageSize_SelectedIndexChanged" autopostback="true" label="Number of groups to show" />

                    <asp:Panel ID="pnlMap" runat="server" CssClass="margin-v-sm">
                        <div id="map_wrapper">
                            <div id="map_canvas" class="mapping"></div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlLavaOutput" runat="server" CssClass="margin-v-sm">
                        <asp:Literal ID="lLavaOverview" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlGrid" runat="server" CssClass="margin-v-sm">
                        <div class="grid">
                            <rock:grid id="gGroups" runat="server" rowitemtext="Group" allowsorting="true" onrowselected="gGroups_RowSelected">
                                <columns>
                                    <rock:rockboundfield datafield="Name" headertext="Name" sortexpression="Name" />
                                    <rock:rockboundfield datafield="Description" headertext="Description" sortexpression="Description" />
                                    <rock:rockboundfield datafield="Schedule" headertext="Schedule" />
                                    <rock:rockboundfield datafield="MemberCount" headertext="Members" dataformatstring="{0:N0}" itemstyle-horizontalalign="Right" headerstyle-horizontalalign="Right" />
                                    <rock:rockboundfield datafield="AverageAge" headertext="Average Age" dataformatstring="{0:N0}" itemstyle-horizontalalign="Right" headerstyle-horizontalalign="Right" />
                                    <rock:rockboundfield datafield="Campus" headertext="Campus" sortexpression="Campus.Name" />
                                    <rock:rockboundfield datafield="Distance" headertext="Distance" dataformatstring="{0:N2} M" itemstyle-horizontalalign="Right" headerstyle-horizontalalign="Right" />
                                </columns>
                            </rock:grid>
                        </div>
                    </asp:Panel>

                </asp:Panel>
            </div>
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <rock:modaldialog id="mdEdit" runat="server" onsaveclick="lbSave_Click" title="Group Finder Configuration" validationgroup="GroupFinderSettings">
                <content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <contenttemplate>

                            <asp:ValidationSummary ID="valSettings" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupFinderSettings" />

                            <rock:panelwidget id="wpGroups" runat="server" title="Groups" expanded="true">
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rocklistbox
                                            id="gtpGroupType"
                                            runat="server"
                                            label="Group Type"
                                            help="The type of groups to look for."
                                            autopostback="true"
                                            onselectedindexchanged="gtpGroupType_SelectedIndexChanged"
                                            validationgroup="GroupFinderSettings"
                                            datatextfield="text"
                                            datavaluefield="value" />

                                        <rock:rockcheckbox
                                            id="cbHideOvercapacityGroups"
                                            runat="server"
                                            label="Hide Overcapacity Groups"
                                            help="When set to true, groups that are at capacity or whose default GroupTypeRole are at capacity are hidden."
                                            validationgroup="GroupFinderSettings" />

                                        <rock:rockcheckbox
                                            id="cbLoadInitialResults"
                                            runat="server"
                                            label="Load Results on Initial Page Load"
                                            help="When enabled the group finder will load with all configured groups (no filters enabled)."
                                            validationgroup="GroupFinderSettings" />

                                        <rock:grouptypepicker id="gtpGeofenceGroupType" runat="server" label="Geofence Group Type"
                                            help="An optional group type that contains groups with geographic boundary (fence). If specified, user will be prompted for their address, and only groups that are located in the same geographic boundary ( as defined by one or more groups of this type ) will be displayed."
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <label class="control-label">Location Types</label>
                                        <rock:grid
                                            runat="server"
                                            id="gGroupTypeLocation"
                                            displaytype="Light"
                                            onrowdatabound="gGroupTypeLocation_RowDataBound">
                                            <columns>
                                                <rock:rockboundfield datafield="Name" />
                                                <asp:TemplateField>
                                                    <itemtemplate>
                                                        <rock:rockdropdownlist id="lLocationList" runat="server" autopostback="true" onselectedindexchanged="lLocationList_SelectedIndexChanged" />
                                                    </itemtemplate>
                                                </asp:TemplateField>
                                            </columns>
                                        </rock:grid>
                                    </div>
                                </div>
                            </rock:panelwidget>
                            <rock:panelwidget id="wpFilter" runat="server" title="Filters" expanded="true">
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rocktextbox id="tbDayOfWeekLabel" runat="server" label="Day of Week Filter Label" help="The text above the day of week filter" autopostback="true" required="true" validationgroup="GroupFinderSettings" />
                                        <rock:rocktextbox id="tbTimeOfDayLabel" runat="server" label="Time of Day Filter Label" help="The text above the time of day filter" autopostback="true" required="true" validationgroup="GroupFinderSettings" />
                                        <rock:rocktextbox id="tbCampusLabel" runat="server" label="Campus Filter Label" help="The text above the campus filter" autopostback="true" required="true" validationgroup="GroupFinderSettings" />
                                        <Rock:DefinedValuesPicker ID="dvpCampusTypes" runat="server" Label="Campus Types" Help="The campus types to filter the list of campuses on." />
                                        <Rock:DefinedValuesPicker ID="dvpCampusStatuses" runat="server" Label="Campus Statuses" Help="The campus statuses to filter the list of campuses on." />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:rockradiobuttonlist id="rblFilterDOW" runat="server" label="Display Day of Week Filter" repeatdirection="Horizontal"
                                            help="Flag indicating if and how the Day of Week filter should be displayed to filter groups with 'Weekly' schedules." validationgroup="GroupFinderSettings">
                                            <asp:ListItem Text="None" Value="" />
                                            <asp:ListItem Text="Single-Select Dropdown" Value="Day" />
                                            <asp:ListItem Text="Multi-Select Checkboxes" Value="Days" />
                                        </rock:rockradiobuttonlist>
                                        <rock:rockcheckbox id="cbFilterTimeOfDay" runat="server" label="Display Time of Day Filter"
                                            help="Display a Time of Day filter to filter groups with 'Weekly' schedules." validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbFilterCampus" runat="server" label="Display Campus Filter"
                                            help="Display the campus filter" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbCampusContext" runat="server" label="Enable Campus Context"
                                            help="If the page has a campus context its value will be used as a filter" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckboxlist id="cblAttributes" runat="server" label="Display Attribute Filters" repeatdirection="Horizontal"
                                            help="The group attributes that should be available for user to filter results by." validationgroup="GroupFinderSettings" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                            </rock:panelwidget>

                            <rock:panelwidget id="wpMap" runat="server" title="Map">
                                <div class="row">
                                    <div class="col-md-12">
                                        <rock:rockcheckbox
                                            id="cbShowMap"
                                            runat="server"
                                            label="Show Map"
                                            help="Should a map be displayed that shows the location of each group?"
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:definedvaluepicker
                                            id="dvpMapStyle"
                                            runat="server"
                                            label="Map Style"
                                            help="The map theme that should be used for styling the map."
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:numberbox
                                            id="nbMapHeight"
                                            runat="server"
                                            label="Map Height"
                                            help="The pixel height to use for the map."
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:definedvaluepicker id="ddlMapMarker" label="Map Marker" runat="server" validationgroup="GroupFinderSettings" help="The map marker shape to show on the map." />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:colorpicker runat="server" id="cpMarkerColor" label="Marker Color" validationgroup="GroupFinderSettings" help="The color to use for the map marker. If no color is provided the color will come from the group type's color setting." />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:rockdropdownlist id="ddlMinZoomLevel" label="Minimum Zoom Level" runat="server" validationgroup="GroupFinderSettings" help="Determines the minimum zoom level that the map will allow.">
                                            <asp:ListItem Text="" Value="" />
                                            <asp:ListItem Text="0" Value="0" />
                                            <asp:ListItem Text="1 - World" Value="1" />
                                            <asp:ListItem Text="2" Value="2" />
                                            <asp:ListItem Text="3" Value="3" />
                                            <asp:ListItem Text="4" Value="4" />
                                            <asp:ListItem Text="5 - Continent" Value="5" />
                                            <asp:ListItem Text="6" Value="6" />
                                            <asp:ListItem Text="7" Value="7" />
                                            <asp:ListItem Text="8" Value="8" />
                                            <asp:ListItem Text="9" Value="9" />
                                            <asp:ListItem Text="10 - City" Value="10" />
                                            <asp:ListItem Text="11" Value="11" />
                                            <asp:ListItem Text="12" Value="12" />
                                            <asp:ListItem Text="13" Value="13" />
                                            <asp:ListItem Text="14" Value="14" />
                                            <asp:ListItem Text="15 - Streets" Value="15" />
                                            <asp:ListItem Text="16" Value="16" />
                                            <asp:ListItem Text="17" Value="17" />
                                            <asp:ListItem Text="18" Value="18" />
                                            <asp:ListItem Text="19" Value="19" />
                                            <asp:ListItem Text="20 - Buildings" Value="20" />
                                        </rock:rockdropdownlist>
                                    </div>
                                    <div class="col-md-6">
                                        <rock:rockdropdownlist id="ddlMaxZoomLevel" label="Maxium Zoom Level" runat="server" validationgroup="GroupFinderSettings" help="Determines the maximum zoom level that the map will allow.">
                                            <asp:ListItem Text="" Value="" />
                                            <asp:ListItem Text="0" Value="0" />
                                            <asp:ListItem Text="1 - World" Value="1" />
                                            <asp:ListItem Text="2" Value="2" />
                                            <asp:ListItem Text="3" Value="3" />
                                            <asp:ListItem Text="4" Value="4" />
                                            <asp:ListItem Text="5 - Continent" Value="5" />
                                            <asp:ListItem Text="6" Value="6" />
                                            <asp:ListItem Text="7" Value="7" />
                                            <asp:ListItem Text="8" Value="8" />
                                            <asp:ListItem Text="9" Value="9" />
                                            <asp:ListItem Text="10 - City" Value="10" />
                                            <asp:ListItem Text="11" Value="11" />
                                            <asp:ListItem Text="12" Value="12" />
                                            <asp:ListItem Text="13" Value="13" />
                                            <asp:ListItem Text="14" Value="14" />
                                            <asp:ListItem Text="15 - Streets" Value="15" />
                                            <asp:ListItem Text="16" Value="16" />
                                            <asp:ListItem Text="17" Value="17" />
                                            <asp:ListItem Text="18" Value="18" />
                                            <asp:ListItem Text="19" Value="19" />
                                            <asp:ListItem Text="20 - Buildings" Value="20" />
                                        </rock:rockdropdownlist>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rockdropdownlist id="ddlInitialZoomLevel" label="Initial Zoom Level" runat="server" validationgroup="GroupFinderSettings" help="Determines the initial zoom level the map should use.">
                                            <asp:ListItem Text="Automatic" Value="" />
                                            <asp:ListItem Text="0" Value="0" />
                                            <asp:ListItem Text="1 - World" Value="1" />
                                            <asp:ListItem Text="2" Value="2" />
                                            <asp:ListItem Text="3" Value="3" />
                                            <asp:ListItem Text="4" Value="4" />
                                            <asp:ListItem Text="5 - Continent" Value="5" />
                                            <asp:ListItem Text="6" Value="6" />
                                            <asp:ListItem Text="7" Value="7" />
                                            <asp:ListItem Text="8" Value="8" />
                                            <asp:ListItem Text="9" Value="9" />
                                            <asp:ListItem Text="10 - City" Value="10" />
                                            <asp:ListItem Text="11" Value="11" />
                                            <asp:ListItem Text="12" Value="12" />
                                            <asp:ListItem Text="13" Value="13" />
                                            <asp:ListItem Text="14" Value="14" />
                                            <asp:ListItem Text="15 - Streets" Value="15" />
                                            <asp:ListItem Text="16" Value="16" />
                                            <asp:ListItem Text="17" Value="17" />
                                            <asp:ListItem Text="18" Value="18" />
                                            <asp:ListItem Text="19" Value="19" />
                                            <asp:ListItem Text="20 - Buildings" Value="20" />
                                        </rock:rockdropdownlist>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rockdropdownlist id="ddlMarkerZoomLevel" label="Marker Auto Scale Zoom Level" runat="server" validationgroup="GroupFinderSettings" help="The zoom level threshold that will cause the markers to auto resize to keep from showing precise locations on the map. Once this threshold is passed, the marker will begin to auto scale.">
                                            <asp:ListItem Text="" Value="" />
                                            <asp:ListItem Text="0" Value="0" />
                                            <asp:ListItem Text="1 - World" Value="1" />
                                            <asp:ListItem Text="2" Value="2" />
                                            <asp:ListItem Text="3" Value="3" />
                                            <asp:ListItem Text="4" Value="4" />
                                            <asp:ListItem Text="5 - Continent" Value="5" />
                                            <asp:ListItem Text="6" Value="6" />
                                            <asp:ListItem Text="7" Value="7" />
                                            <asp:ListItem Text="8" Value="8" />
                                            <asp:ListItem Text="9" Value="9" />
                                            <asp:ListItem Text="10 - City" Value="10" />
                                            <asp:ListItem Text="11" Value="11" />
                                            <asp:ListItem Text="12" Value="12" />
                                            <asp:ListItem Text="13" Value="13" />
                                            <asp:ListItem Text="14" Value="14" />
                                            <asp:ListItem Text="15 - Streets" Value="15" />
                                            <asp:ListItem Text="16" Value="16" />
                                            <asp:ListItem Text="17" Value="17" />
                                            <asp:ListItem Text="18" Value="18" />
                                            <asp:ListItem Text="19" Value="19" />
                                            <asp:ListItem Text="20 - Buildings" Value="20" />
                                        </rock:rockdropdownlist>
                                    </div>
                                    <div class="col-md-6">
                                        <rock:numberbox runat="server" id="nbMarkerAutoScaleAmount" label="Marker Auto Scale Amount" validationgroup="GroupFinderSettings" help="The amount relative to the zoom level that the markers should scale themselves. A value of 2 means the scale will be 2 times the zoom level." />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:rockdropdownlist id="ddlLocationPrecisionLevel" label="Location Precision Level" runat="server" validationgroup="GroupFinderSettings" help="Determines how precise of a latitude/longitude to provide to the map. ">
                                            <asp:ListItem Text="Precise" Value="Precise" />
                                            <asp:ListItem Text="Narrow" Value="Narrow" />
                                            <asp:ListItem Text="Close" Value="Close" />
                                            <asp:ListItem Text="Wide" Value="Wide" />
                                        </rock:rockdropdownlist>
                                    </div>
                                    <div class="col-md-12">
                                        <rock:codeeditor id="ceMapInfo" runat="server" label="Group Window Contents" editormode="Lava" editortheme="Rock" height="300"
                                            help="The Lava template to use for formatting the group information that is displayed when user clicks the group marker on the map."
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-12">
                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbShowAdditionalMapSettings"
                                            Text="Additional Geofence Settings"
                                            OnClick="lbShowAdditionalMapSettings_Click"
                                            CausesValidation="false"
                                            CssClass="pull-right" />
                                    </div>
                                </div>

                                <div runat="server" id="dMapAdditionalSettings" visible="false" class="row">
                                    <div class="col-md-6">
                                        <rock:rockcheckbox id="cbShowFence" runat="server" label="Show Fence(s)"
                                            help="If a Geofence group type was selected, should that group's boundary be displayed on the map?" validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:valuelist id="vlPolygonColors" runat="server" label="Fence Polygon Colors"
                                            help="The list of colors to use when displaying multiple fences ( there should normally be only one fence)." validationgroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </rock:panelwidget>

                            <rock:panelwidget id="wpLavaOutput" runat="server" title="Lava">
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rockcheckbox id="cbShowLavaOutput" runat="server" label="Show Formatted Output"
                                            help="Should the matching groups be merged with a Lava template and displayed to the user as formatted output?" validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <rock:codeeditor id="ceLavaOutput" runat="server" label="Lava Template" editormode="Lava" editortheme="Rock" height="300"
                                            help="The Lava template to use for formatting the matching groups."
                                            validationgroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </rock:panelwidget>

                            <rock:panelwidget id="wpGrid" runat="server" title="Grid">
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:rockcheckbox id="cbShowGrid" runat="server" label="Show Grid"
                                            help="Should a grid be displayed showing the matching groups?" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbShowSchedule" runat="server" label="Show Schedule"
                                            help="Should the schedule for each group be displayed?" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbShowDescription" runat="server" label="Show Description"
                                            help="Should the description for each group be displayed?" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbShowCount" runat="server" label="Show Member Count"
                                            help="Should the number of active members in each group be displayed in the result grid?" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbShowAge" runat="server" label="Show Average Age"
                                            help="Should the average active group member age be displayed for each group in the result grid?" validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbIncludePending" runat="server" label="Include Pending"
                                            help="Should Pending members be included in the member count and average age calculations?" validationgroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:rockcheckbox id="cbShowCampus" runat="server" label="Show Campus"
                                            help="Should the campus column be displayed? If selected, the Campus column will still only be displayed if one or more of the groups has a campus." validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbProximity" runat="server" label="Show Distance"
                                            help="Should the distance to each group be displayed? Using this option will require the user to enter their address when searching for groups." validationgroup="GroupFinderSettings" />
                                        <rock:rockcheckbox id="cbSortByDistance" runat="server" label="Sort by Distance"
                                            help="Should the results be sorted from closest to furthest distance?" validationgroup="GroupFinderSettings" />
                                        <rock:rocktextbox id="tbPageSizes" runat="server" label="Page Sizes" help="To limit the number of groups displayed and to show a dropdown of page sizes, enter a comma delimited list of page sizes. For example: 10,20 will present a drop down with 10,20,All as options with the default as 10" />
                                        <rock:rockcheckboxlist id="cblGridAttributes" runat="server" label="Show Attribute Columns" repeatdirection="Horizontal"
                                            help="The group attribute values that should be displayed in the result grid." validationgroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </rock:panelwidget>

                            <rock:panelwidget id="wpLinkedPages" runat="server" title="Linked Pages">
                                <div class="row">
                                    <div class="col-md-6">
                                        <rock:pagepicker id="ppGroupDetailPage" runat="server" label="Group Detail Page" help="If showing the Grid, this is the page user will be redirected to when they click on the row. If using Formatted ouput, a URL to this page will be included as a 'GroupDetailPage' property of the 'LinkedPages' merge field." required="false" />
                                    </div>
                                    <div class="col-md-6">
                                        <rock:pagepicker id="ppRegisterPage" runat="server" label="Register Page" help="If this value is set and the block is configured to show the Grid, a 'Register' button will be added to each row for user to click and be redirected to this page. If using Formatted ouput, a URL to this page will be included as a 'RegisterPage' property of the 'LinkedPages' merge field." required="false" />
                                    </div>
                                </div>
                            </rock:panelwidget>

                        </contenttemplate>
                    </asp:UpdatePanel>
                </content>
            </rock:modaldialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
