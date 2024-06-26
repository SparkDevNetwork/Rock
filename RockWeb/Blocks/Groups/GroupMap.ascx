<%@ Control AutoEventWireup="true" CodeFile="GroupMap.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMap" Language="C#" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnApplyOptions" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i> Group Map</h1>
                 <div class="pull-right"><a class="btn btn-xs btn-default btn-square" onclick="javascript: toggleOptions()"><i title="Options" class="fa fa-gear"></i></a></div>
            </div>

            <asp:Panel ID="pnlOptions" runat="server" Title="Options" CssClass="panel-body js-options" Style="display: none">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbShowAllGroups" runat="server" Text="Show Child Groups" Help="Show all the child groups that are within the selected geofence(s)" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupTypesPicker ID="gtpGroupType" runat="server" RepeatDirection="Horizontal" Label="Child Group Types" Help="Select Group Types to limit what type of child groups are shown" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnApplyOptions" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApplyOptions_Click" />
                </div>
            </asp:Panel>

            <div class="panel-body">

                <asp:Literal ID="lMapStyling" runat="server" />

                <Rock:CampusesPicker ID="cpCampuses" runat="server" FormGroupCssClass="js-campuses-picker" Label="Campuses Filter" Help="Select the campuses to narrow the results down to families with that home campus." Required="false" ForceVisible="true" Visible="false" />

                <div class="form-inline clearfix">

                    <div class="pull-left">
                        <div class="form-group margin-r-sm js-show-group">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" id="cbShowGroup" /> <i class="fa fa-circle" style="color:#<%=_groupColor %>"></i> <span id="lGroupName">Show Group</span>
                                </label>
                            </div>
                        </div>

                        <div class="form-group margin-r-sm js-show-child-groups">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" id="cbShowChildGroups" /> <i class="fa fa-circle" style="color:#<%=_childGroupColor %>"></i> Groups
                                </label>
                            </div>
                        </div>

                        <div class="form-group margin-r-sm js-show-group-members">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" id="cbShowGroupMembers" /> <i class="fa fa-circle" style="color:#<%=_memberColor %>"></i> Group Members
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="pull-right">
                        <asp:Repeater ID="rptStatus" runat="server">
                            <ItemTemplate>
                                <div class="form-group margin-l-sm js-connection-status" style="display:none">
                                    <div class="checkbox">
                                        <label>
                                            <input type="checkbox" class="js-connection-status-cb" data-item='<%# Eval("Id") %>' data-color='<%# Eval("Color") %>' /> <i class="fa fa-circle" style='color:#<%# Eval("Color") %>'></i> <%# Eval("Name") %>
                                        </label>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>


                </div>

                <asp:Panel ID="pnlMap" runat="server">
                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                </asp:Panel>

                <asp:Literal ID="lMessages" runat="server" />
                <asp:Literal ID="lDebug" runat="server" />

            </div>
        </div>

        <script>
            function toggleOptions() {
                $('.js-options').slideToggle();
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
