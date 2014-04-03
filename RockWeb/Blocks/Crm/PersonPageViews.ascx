<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonPageViews.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonPageViews" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Repeater ID="rptSessions" runat="server" OnItemDataBound="rptSessions_ItemDataBound">
                <ItemTemplate>
                    <div class="panel panel-widget pageviewsession">
                        <header class="panel-heading clearfix">
                            <div class="pull-left">
                                <h4 class="panel-title">Started <asp:Literal id="lRelativeDate" runat="server" />
                                    <small>
                                        Duration: <asp:Literal ID="lSessionDuration" runat="server" />
                                    </small>
                                </h4>
                                <span><%# Eval("Site") %></span>
                            </div> 
                            <asp:Literal ID="lClientIcon" runat="server" />
                        </header>
                        <div class="panel-body">
                            <ol>
                                <asp:Repeater ID="rptPageViews" runat="server" DataSource='<%# Eval("PageViews") %>' OnItemDataBound="rptPageViews_ItemDataBound">
                                    <ItemTemplate>
                                        <li><a href='<%# Eval("Url") %>'><%# Eval("PageTitle") %></a> <asp:Literal ID="lPageViewDuration" runat="server" /></li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ol>
                        </div>
                    </div>
                </ItemTemplate>
        </asp:Repeater>

        <asp:Literal ID="lMessages" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
