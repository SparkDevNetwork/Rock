<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunitySelect.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionOpportunitySelect" %>

<style>
    .badge-legend.expand-on-hover .badge {
        transition-duration: 200ms;
        border-radius: 12px;
        width: auto;
        padding: 5px 0 0 24px;
        font-weight: normal;
        max-width: 24px;
        overflow: hidden;
    }

    .badge-legend.expand-on-hover .badge:hover {
        max-width: 250px;
        padding: 5px 12px 0 12px;
    }

    .list-as-blocks ul li .follow-toggle {
        position: absolute;
        border: none;
        background: none;
        min-height: 0;
        top: 0;
        height: 35px;
        width: 35px;
        right: 15px; /* padding of the list-as-block li */
    }

    .list-as-blocks ul li .follow-toggle i {
        font-size: 20px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class='fa fa-plug'></i>
                    Connections
                </h1>
                <div class="pull-right">
                    <div class="pull-left">
                        <Rock:Switch ID="tglMyActiveOpportunities" runat="server" OnCheckedChanged="tglMyActiveOpportunities_CheckedChanged" AutoPostBack="true" />
                    </div>
                    <span class="padding-r-lg">My Active Opportunities</span>
                    <asp:LinkButton ID="lbConnectionTypes" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right" OnClick="lbConnectionTypes_Click" CausesValidation="false"> <i title="Options" class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>

            <div class="panel-body">
                <div class="text-right">
                    <asp:Literal ID="lStatusBarContent" runat="server" />
                </div>

                <Rock:NotificationBox ID="nbNoOpportunities" runat="server" NotificationBoxType="Info" CssClass="margin-t-md" Text="There are no current connection requests." />

                <asp:Panel ID="pnlFavorites" runat="server">
                    <h4>
                        <i class="far fa-star"></i>
                        Favorites
                    </h4>
                    <div class="list-as-blocks clearfix margin-b-lg">
                        <ul>
                            <asp:Repeater ID="rptFavoriteOpportunities" runat="server" OnItemCommand="rptConnectionOpportunities_ItemCommand">
                                <ItemTemplate>
                                    <li class="block-status">
                                        <asp:LinkButton ID="lbConnectionOpportunity" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Select">
                                            <%# this.GetOpportunitySummaryHtml( Container.DataItem as OpportunitySummary ) %>
                                        </asp:LinkButton>
                                        <Rock:BootstrapButton ID="btnToggleFollow" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="ToggleFollow" CssClass="follow-toggle" data-toggle="tooltip" data-placement="top" title="Click to Follow">
                                            <%# Eval("FollowIconHtml") %>
                                        </Rock:BootstrapButton>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                    <hr class="margin-b-lg" />
                </asp:Panel>

                <asp:Repeater ID="rptConnnectionTypes" runat="server" OnItemDataBound="rptConnnectionTypes_ItemDataBound">
                    <ItemTemplate>
                        <h4>
                            <%# Eval("IconMarkup") %>
                            <%# Eval("Name") %>
                        </h4>
                        <div class="list-as-blocks clearfix margin-b-lg">
                            <ul>
                                <asp:Repeater ID="rptConnectionOpportunities" runat="server" OnItemCommand="rptConnectionOpportunities_ItemCommand">
                                    <ItemTemplate>
                                        <li class="block-status">
                                            <asp:LinkButton ID="lbConnectionOpportunity" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Select">
                                                <%# this.GetOpportunitySummaryHtml( Container.DataItem as OpportunitySummary ) %>
                                            </asp:LinkButton>
                                            <Rock:BootstrapButton ID="btnToggleFollow" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="ToggleFollow" CssClass="follow-toggle" data-toggle="tooltip" data-placement="top" title="Click to Follow">
                                                <%# Eval("FollowIconHtml") %>
                                            </Rock:BootstrapButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
