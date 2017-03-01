<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeMap.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Core.Groups.GroupTypeMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lMapStyling" runat="server" />
        
        <asp:Panel ID="pnlMap" runat="server">
            <div id="map_wrapper">
                <div id="map_canvas" class="mapping"></div>
            </div>
        </asp:Panel>
        <div class="row margin-t-md">
          <div class="col-md-4"><asp:HyperLink ID="hlPrev" runat="server" CssClass="btn btn-default" Text="< prev"></asp:HyperLink> </div>
          <div class="col-md-4 text-center"><asp:Label ID="lShowing" runat="server" CssClass="text-center"></asp:Label> </div>
          <div class="col-md-4"><asp:HyperLink ID="hlNext" runat="server" Text="next >" CssClass="btn btn-primary pull-right"></asp:HyperLink></div>
        </div>

        <asp:Literal ID="lMessages" runat="server" />
        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
