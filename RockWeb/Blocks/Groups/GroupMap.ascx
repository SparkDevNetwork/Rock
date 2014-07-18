<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMap.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i> Group Map</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lMapStyling" runat="server" />
        
                <div class="form-inline js-show-hide-options clearfix">

                    <div class="pull-left">
                        <div class="form-group">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" id="cbShowGroup" checked="checked" /> <i class="fa fa-circle" style="color:#<%=_groupColor %>"></i> <span id="lGroupName">Show Group</span>
                                </label>
                            </div>
                        </div>

                        <div class="form-group">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" id="cbShowChildGroups" /> <i class="fa fa-circle" style="color:#<%=_childGroupColor %>"></i> Groups
                                </label>
                            </div>
                        </div>

                        <div class="form-group">
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
                                <div class="form-group" style="display:none">
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

        

    </ContentTemplate>
</asp:UpdatePanel>
