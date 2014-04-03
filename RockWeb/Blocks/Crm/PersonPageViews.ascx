<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonPageViews.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonPageViews" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lMessages" runat="server" />

        <asp:Panel ID="pnlHeader" CssClass="clearfix" runat="server">
            <div class="banner">
                <h1><asp:Literal ID="lPersonName" runat="server" /></h1>
            </div>

            <div class="form-horizontal pull-right">
                <Rock:DateRangePicker ID="drpDateFilter" CssClass="pull-left" runat="server" Label="Date Filter"  /> 
                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-action pull-left" OnClick="btnFilter_Click" />
            </div>
        </asp:Panel>

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

        <div class="nav-paging">
            <asp:HyperLink ID="hlPrev" CssClass="btn btn-primary btn-prev" Visible="false" runat="server" Text="<i class='fa fa-chevron-left'></i> Prev" />
            <asp:HyperLink ID="hlNext" CssClass="btn btn-primary btn-next" Visible="false" runat="server" Text="Next <i class='fa fa-chevron-right'></i>" />
        </div>

        

    </ContentTemplate>
</asp:UpdatePanel>
