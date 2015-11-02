<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">

            <div class="panel panel-block margin-t-md">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lResultsIconCssClass" runat="server" />
                        <asp:Literal ID="lResultsTitle" runat="server" />
                    </h1>
                </div>
                <asp:Panel ID="pnlMap" runat="server" CssClass="margin-v-sm">
                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                    <asp:Literal ID="lMapInfoDebug" runat="server" />
                </asp:Panel>
            </div>

        </asp:Panel>

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
