<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i> 
                    Blank Detail Block
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:LinkButton ID="btnGetReport" runat="server" CssClass="btn btn-primary" Text="Get Plugin DLLS Report" OnClick="btnGetReport_Click" />

                <Rock:Grid ID="gPluginsReport" runat="server" AllowSorting="true">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Plugin" SortExpression="Name" />
                        <asp:BoundField DataField="PluginVersion" HeaderText="Plugin Version" SortExpression="PluginVersion" />
                        <asp:BoundField DataField="RockVersion" HeaderText="Rock Version" SortExpression="RockVersion" />
                        <asp:BoundField DataField="QuartzJobTypes" HeaderText="Quartz Job Types" HtmlEncode="false" SortExpression="QuartzJobTypes" />
                        <asp:BoundField DataField="IRockEntities" HeaderText="IRockEntity Types" HtmlEncode="false" SortExpression="IRockEntities" />
                        <asp:BoundField DataField="RestEndPointTypes" HeaderText="Rest Controllers" HtmlEncode="false" SortExpression="RestEndPointTypes" />
                    </Columns>
                </Rock:Grid>
                <asp:Literal ID="lPluginDllsReport" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>