<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i>&nbsp;Dynamic Map</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lMapStyling" runat="server" />

                <asp:Panel ID="pnlMap" runat="server">
                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                </asp:Panel>

                <asp:Literal ID="lMessages" runat="server" />
                <asp:Literal ID="lDebug" runat="server" />

            </div>
        </div>

        <%-- Configuration Panel --%>
        <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdConfigure" runat="server" ValidationGroup="vgConfigure" OnSaveClick="mdConfigure_SaveClick">
                <Content>
                    <Rock:RockDropDownList ID="ddlDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the reults." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
