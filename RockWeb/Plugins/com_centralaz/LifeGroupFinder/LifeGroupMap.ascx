<%@ Control AutoEventWireup="true" CodeFile="LifeGroupMap.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupMap" Language="C#" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i>Group Map</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lMapStyling" runat="server" />

                <div class="form-inline clearfix">
                    <div class="lifegroupmap-campusselector">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" Label="Select Campus" AutoPostBack="true" />
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
