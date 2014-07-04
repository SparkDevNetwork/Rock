<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMap.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lMapStyling" runat="server" />
        
        <div class="form-inline">

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
