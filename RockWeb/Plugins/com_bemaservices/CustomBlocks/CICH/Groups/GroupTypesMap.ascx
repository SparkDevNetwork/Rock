<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypesMap.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Groups.GroupTypesMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 id="lbBlockText" runat="server" class="panel-title">Filter</h4>
            </div>
            <div class="panel-body">
                <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" />

                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnFilter" Text="Filter" runat="server" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>
        <asp:Literal ID="lMapStyling" runat="server" />
        <asp:Panel ID="pnlMap" runat="server">
            <div id="map_wrapper">
                <div id="map_canvas" class="mapping"></div>
            </div>
        </asp:Panel>

        <asp:Literal ID="lMessages" runat="server" />
        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
