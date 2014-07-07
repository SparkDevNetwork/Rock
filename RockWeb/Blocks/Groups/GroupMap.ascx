<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMap.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lMapStyling" runat="server" />
        
        <div class="form-inline js-show-hide-options">

            <div class="form-group">
                <div class="checkbox">
                    <label>
                        <input type="checkbox" id="cbShowGroup" checked="checked" /> <span id="lGroupName">Show Group</span>
                    </label>
                </div>
            </div>

            <div class="form-group">
                <div class="checkbox">
                    <label>
                        <input type="checkbox" id="cbShowChildGroups" /> Child Groups
                    </label>
                </div>
            </div>

            <div class="form-group">
                <div class="checkbox">
                    <label>
                        <input type="checkbox" id="cbShowGroupMembers" /> Group Members
                    </label>
                </div>
            </div>

            <asp:Repeater ID="rptStatus" runat="server">
                <ItemTemplate>
                    <div class="form-group" style="display:none">
                        <div class="checkbox">
                            <label>
                                <input type="checkbox" class="js-connection-status-cb" data-item='<%# Eval("Id") %>' /> <%# Eval("Name") %>
                            </label>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            
        </div>

        <asp:Panel ID="pnlMap" runat="server">
            <div id="map_wrapper">
                <div id="map_canvas" class="mapping"></div>
            </div>
        </asp:Panel>

        <asp:Literal ID="lMessages" runat="server" />
        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
