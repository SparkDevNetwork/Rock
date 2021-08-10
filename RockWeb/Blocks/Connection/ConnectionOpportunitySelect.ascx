<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunitySelect.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionOpportunitySelect" %>

<style>
    .badge-legend.expand-on-hover .badge {
        width: auto;
        padding: 5px 0 0 24px;
        font-weight: normal;
        max-width: 24px;
        overflow: hidden;
        transition: all 250ms ease-in-out 25ms;
    }

    .badge-legend.expand-on-hover .badge:hover {
        max-width: 250px;
        padding: 5px 12px 0 12px;
    }

    .list-as-blocks ul li .follow-toggle {
        position: absolute;
        border: 0;
        background: transparent;
        min-height: 0;
        top: 0;
        height: 35px;
        width: 35px;
        right: 15px; /* padding of the list-as-block li */
    }

    .list-as-blocks ul li .follow-toggle:hover {
        background: transparent;
    }

    .list-as-blocks ul li .follow-toggle:hover i {
        color: #626262;
    }

    .list-as-blocks ul li .follow-toggle i {
        font-size: 16px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-plug"></i>
                    Connections
                </h1>
                <div class="panel-labels d-flex align-items-center">
                    <asp:Literal ID="lStatusBarContent" runat="server" />

                    <asp:LinkButton ID="lbConnectionTypes" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right" OnClick="lbConnectionTypes_Click" CausesValidation="false"> <i title="Options" class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>

            <div class="panel-body">
                <div class="small text-right">
                    <Rock:Switch ID="tglMyActiveOpportunities" runat="server" OnCheckedChanged="tglMyActiveOpportunities_CheckedChanged" AutoPostBack="true" Text="My Active Opportunities" />
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
                        <h5>
                            <%# Eval("IconMarkup") %>
                            <%# Eval("Name") %>
                        </h5>
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
