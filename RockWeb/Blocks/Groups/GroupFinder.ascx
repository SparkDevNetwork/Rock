<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinder.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupFinder" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-map-marker"></i> Group Finder
                </h1>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lTitle" runat="server" />

                <asp:Panel ID="pnlSearch" runat="server">

                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <Rock:AddressControl ID="acAddress" runat="server" Required="true" RequiredErrorMessage="Your Address is Required" />
                    <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                    <asp:PlaceHolder ID="phFilterControls" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                        <asp:LinkButton ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-link" OnClick="btnClear_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlResults" runat="server" Visible="false">

                    <asp:Literal ID="lMapStyling" runat="server" />

                    <Rock:RockDropDownList ID="ddlPageSize" runat="server" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" AutoPostBack="true" Label="Number of groups to show" />

                    <asp:Panel ID="pnlMap" runat="server" CssClass="margin-v-sm">
                        <div id="map_wrapper">
                            <div id="map_canvas" class="mapping"></div>
                        </div>
                        <asp:Literal ID="lMapInfoDebug" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlLavaOutput" runat="server" CssClass="margin-v-sm">
                        <asp:Literal ID="lLavaOverview" runat="server" />
                        <asp:Literal ID="lLavaOutputDebug" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlGrid" runat="server" CssClass="margin-v-sm">
                        <div class="grid">
                            <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_RowSelected">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                    <Rock:RockBoundField DataField="Schedule" HeaderText="Schedule" />
                                    <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    <Rock:RockBoundField DataField="AverageAge" HeaderText="Average Age" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                    <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus.Name" />
                                    <Rock:RockBoundField DataField="Distance" HeaderText="Distance" DataFormatString="{0:N2} M" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </asp:Panel>

                </asp:Panel>
            </div>
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Group Finder Configuration" ValidationGroup="GroupFinderSettings">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <asp:ValidationSummary ID="valSettings" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupFinderSettings" />

                            <Rock:PanelWidget ID="wpFilter" runat="server" Title="Filter Settings" Expanded="true">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" Help="The type of groups to look for."
                                            AutoPostBack="true" OnSelectedIndexChanged="gtpGroupType_SelectedIndexChanged" ValidationGroup="GroupFinderSettings" />
                                        <Rock:GroupTypePicker ID="gtpGeofenceGroupType" runat="server" Label="Geofence Group Type"
                                            Help="An optional group type that contains groups with geographic boundary (fence). If specified, user will be prompted for their address, and only groups that are located in the same geographic boundary ( as defined by one or more groups of this type ) will be displayed."
                                            ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockTextBox ID="tbDayOfWeekLabel" runat="server" Label="Day of Week Filter Label" Help="The text above the day of week filter" AutoPostBack="true" Required="true" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockTextBox ID="tbTimeOfDayLabel" runat="server" Label="Time of Day Filter Label" Help="The text above the time of day filter" AutoPostBack="true" Required="true" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockTextBox ID="tbCampusLabel" runat="server" Label="Campus Filter Label" Help="The text above the campus filter" AutoPostBack="true" Required="true" ValidationGroup="GroupFinderSettings" />
                                     </div>
                                    <div class="col-md-6">
                                        <Rock:RockRadioButtonList ID="rblFilterDOW" runat="server" Label="Display Day of Week Filter" RepeatDirection="Horizontal"
                                            Help="Flag indicating if and how the Day of Week filter should be displayed to filter groups with 'Weekly' schedules." ValidationGroup="GroupFinderSettings">
                                            <asp:ListItem Text="None" Value="" />
                                            <asp:ListItem Text="Single-Select Dropdown" Value="Day" />
                                            <asp:ListItem Text="Multi-Select Checkboxes" Value="Days" />
                                        </Rock:RockRadioButtonList>
                                        <Rock:RockCheckBox ID="cbFilterTimeOfDay" runat="server" Label="Display Time of Day Filter" Text="Yes"
                                            Help="Display a Time of Day filter to filter groups with 'Weekly' schedules." ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbFilterCampus" runat="server" Label="Display Campus Filter" Text="Yes"
                                            Help="Display the campus filter" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbCampusContext" runat="server" Label="Enable Campus Context" Text="Yes"
                                            Help="If the page has a campus context its value will be used as a filter" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBoxList ID="cblAttributes" runat="server" Label="Display Attribute Filters" RepeatDirection="Horizontal"
                                            Help="The group attributes that should be available for user to filter results by." ValidationGroup="GroupFinderSettings" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                            <Rock:PanelWidget ID="wpMap" runat="server" Title="Map">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbShowMap" runat="server" Label="Map" Text="Yes"
                                            Help="Should a map be displayed that shows the location of each group?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:DefinedValuePicker ID="dvpMapStyle" runat="server" Label="Map Style"
                                            Help="The map theme that should be used for styling the map." ValidationGroup="GroupFinderSettings" />
                                        <Rock:NumberBox ID="nbMapHeight" runat="server" Label="Map Height"
                                            Help="The pixel height to use for the map." ValidationGroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbShowFence" runat="server" Label="Show Fence(s)" Text="Yes"
                                            Help="If a Geofence group type was selected, should that group's boundary be displayed on the map?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:ValueList ID="vlPolygonColors" runat="server" Label="Fence Polygon Colors"
                                            Help="The list of colors to use when displaying multiple fences ( there should normally be only one fence)." ValidationGroup="GroupFinderSettings" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:CodeEditor ID="ceMapInfo" runat="server" Label="Group Window Contents" EditorMode="Lava" EditorTheme="Rock" Height="300"
                                            Help="The Lava template to use for formatting the group information that is displayed when user clicks the group marker on the map."
                                            ValidationGroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                            <Rock:PanelWidget ID="wpLavaOutput" runat="server" Title="Lava">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbShowLavaOutput" runat="server" Label="Show Formatted Output" Text="Yes"
                                            Help="Should the matching groups be merged with a Lava template and displayed to the user as formatted output?" ValidationGroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:CodeEditor ID="ceLavaOutput" runat="server" Label="Lava Template" EditorMode="Lava" EditorTheme="Rock" Height="300"
                                            Help="The Lava template to use for formatting the matching groups."
                                            ValidationGroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                            <Rock:PanelWidget ID="wpGrid" runat="server" Title="Grid">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbShowGrid" runat="server" Label="Show Grid" Text="Yes"
                                            Help="Should a grid be displayed showing the matching groups?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbShowSchedule" runat="server" Label="Show Schedule" Text="Yes"
                                            Help="Should the schedule for each group be displayed?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbShowDescription" runat="server" Label="Show Description" Text="Yes"
                                            Help="Should the description for each group be displayed?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbShowCount" runat="server" Label="Show Member Count" Text="Yes"
                                            Help="Should the number of members in each group be displayed in the result grid?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbShowAge" runat="server" Label="Show Average Age" Text="Yes"
                                            Help="Should the average group member age be displayed for each group in the result grid?" ValidationGroup="GroupFinderSettings" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbShowCampus" runat="server" Label="Show Campus" Text="Yes"
                                            Help="Should the campus column be displayed? If selected, the Campus column will still only be displayed if one or more of the groups has a campus." ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbProximity" runat="server" Label="Show Distance" Text="Yes"
                                            Help="Should the distance to each group be displayed? Using this option will require the user to enter their address when searching for groups." ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockCheckBox ID="cbSortByDistance" runat="server" Label="Sort by Distance" Text="Yes"
                                            Help="Should the results be sorted from closest to furthest distance?" ValidationGroup="GroupFinderSettings" />
                                        <Rock:RockTextBox ID="tbPageSizes" runat="server" Label="Page Sizes" Help="To limit the number of groups displayed and to show a dropdown of page sizes, enter a comma delimited list of page sizes. For example: 10,20 will present a drop down with 10,20,All as options with the default as 10" />
                                        <Rock:RockCheckBoxList ID="cblGridAttributes" runat="server" Label="Show Attribute Columns" RepeatDirection="Horizontal"
                                            Help="The group attribute values that should be displayed in the result grid." ValidationGroup="GroupFinderSettings" />
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                            <Rock:PanelWidget ID="wpLinkedPages" runat="server" Title="Linked Pages">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:PagePicker ID="ppGroupDetailPage" runat="server" Label="Group Detail Page" Help="If showing the Grid, this is the page user will be redirected to when they click on the row. If using Formatted ouput, a url to this page will be included as a 'GroupDetailPage' property of the 'LinkedPages' merge field." Required="false" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:PagePicker ID="ppRegisterPage" runat="server" Label="Register Page" Help="If this value is set and the block is configured to show the Grid, a 'Register' button will be added to each row for user to click and be redirected to this page. If using Formatted ouput, a url to this page will be included as a 'RegisterPage' property of the 'LinkedPages' merge field." Required="false" />
                                    </div>
                                </div>
                            </Rock:PanelWidget>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
