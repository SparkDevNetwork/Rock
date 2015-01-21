<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinder.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupFinder" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">

            <asp:Panel ID="pnlSearch" runat="server" >

                <Rock:AddressControl ID="acAddress" runat="server" />
                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                </div>

            </asp:Panel>

            <asp:Panel ID="pnlGridResults" runat="server" Visible="false" CssClass="grid grid-panel" >

                <div class="grid grid-panel">
                    <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="AverageAge" HeaderText="Average Age" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="Distance" HeaderText="Distance" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </asp:Panel>

        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Group Finder Configuration">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" Help="The type of groups that should be displayed."
                                        AutoPostBack="true" OnSelectedIndexChanged="gtpGroupType_SelectedIndexChanged"/>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBoxList ID="cblAttributes" runat="server" Label="Allow Filtering By" RepeatDirection="Horizontal" Help="The attributes that should be available for user to filter groups by." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbShowCount" runat="server" Label="Show Member Count" Text="Yes" Help="Should the number of members in each group be displayed?" />
                                    <Rock:RockCheckBox ID="cbShowMap" runat="server" Label="Show Map" Text="Yes" Help="Should groups be displayed on a map?" />
                                    <Rock:NumberBox ID="nbMapHeight" runat="server" Label="Map Height" Help="The pixel height to use for the map." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbShowAge" runat="server" Label="Show Average Age" Text="Yes" Help="Should the average group member age be displayed for each group?" />
                                    <Rock:RockCheckBox ID="cbProximity" runat="server" Label="Filter Groups by Proximity" Text="Yes" Help="Should groups be limited to the same geofenced area as the address entered." />
                                    <Rock:GroupTypePicker ID="gtpGeofenceGroupType" runat="server" Help="The parent group type that defines the geofence boundaries." />
                                </div>
                            </div>

                            <Rock:CodeEditor ID="ceMapInfo" runat="server" Label="Map Info Window" Help="The Lava template to use for formatting the group information on map." EditorMode="Liquid" EditorTheme="Rock" Height="200" />
 
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
