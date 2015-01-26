<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinder.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupFinder" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">

            <asp:Panel ID="pnlSearch" runat="server" >

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:AddressControl ID="acAddress" runat="server" Required="true" RequiredErrorMessage="Address is Required" />
                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                </div>

            </asp:Panel>

            <asp:Panel ID="pnlResults" runat="server" Visible="false" >

                <asp:Literal ID="lMapStyling" runat="server" />

                <asp:Panel ID="pnlMap" runat="server">
                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlGrid" runat="server" CssClass="grid grid-panel" >
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockBoundField DataField="AverageAge" HeaderText="Average Age" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockBoundField DataField="Distance" HeaderText="Distance" DataFormatString="{0:N2} M" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

            </asp:Panel>

        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Group Finder Configuration" ValidationGroup="GroupFinderSettings" >
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

                            <asp:ValidationSummary ID="valSettings" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="GroupFinderSettings"  />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" Help="The type of groups that should be displayed."
                                        AutoPostBack="true" OnSelectedIndexChanged="gtpGroupType_SelectedIndexChanged" ValidationGroup="GroupFinderSettings" />
                                    <Rock:GroupTypePicker ID="gtpGeofenceGroupType" runat="server" Label="Geofence Group Type" 
                                        Help="The parent group type that defines the geofence boundaries." ValidationGroup="GroupFinderSettings" />
                                    <Rock:RockCheckBox ID="cbProximity" runat="server" Label="Filter Groups by Proximity" Text="Yes" 
                                        Help="Should groups be limited to the same geofenced area as the address entered." ValidationGroup="GroupFinderSettings" />
                                    <Rock:RockCheckBoxList ID="cblAttributes" runat="server" Label="Attribute Filters" RepeatDirection="Horizontal" 
                                        Help="The attributes that should be available for user to filter groups by." ValidationGroup="GroupFinderSettings" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbShowCount" runat="server" Label="Show Member Count" Text="Yes" 
                                        Help="Should the number of members in each group be displayed?" ValidationGroup="GroupFinderSettings" />
                                    <Rock:RockCheckBox ID="cbShowAge" runat="server" Label="Show Average Age" Text="Yes" 
                                        Help="Should the average group member age be displayed for each group?" ValidationGroup="GroupFinderSettings" />
                                    <Rock:RockCheckBoxList ID="cblGridAttributes" runat="server" Label="Display Attributes" RepeatDirection="Horizontal" 
                                        Help="The attributes that should be displayed in result grid." ValidationGroup="GroupFinderSettings" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbShowMap" runat="server" Label="Show Map" Text="Yes" 
                                        Help="Should groups be displayed on a map?" ValidationGroup="GroupFinderSettings" />
                                    <Rock:NumberBox ID="nbMapHeight" runat="server" Label="Map Height" 
                                        Help="The pixel height to use for the map." ValidationGroup="GroupFinderSettings" />
                                    <Rock:RockDropDownList ID="ddlMapStyle" runat="server" Label="Map Style" 
                                        Help="The map theme that should be used for styling the map." ValidationGroup="GroupFinderSettings" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbShowFence" runat="server" Label="Show Fence" Text="Yes" 
                                        Help="Should geofence boundary be displayed on the map (Requires a Geofence Group Type)?" ValidationGroup="GroupFinderSettings" />
                                    <Rock:ValueList ID="vlPolygonColors" runat="server" Label="Fence Polygon Colors"
                                        Help="List of colors to use when displaying multiple fences (normally will only have one fence)." ValidationGroup="GroupFinderSettings" />
                                </div>
                            </div>

                            <Rock:CodeEditor ID="ceMapInfo" runat="server" Label="Map Info Window Contents" EditorMode="Liquid" EditorTheme="Rock" Height="200" 
                                Help="The Lava template to use for formatting the group information on map. To suppress the window provide a blank template." 
                                ValidationGroup="GroupFinderSettings" />
 
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
