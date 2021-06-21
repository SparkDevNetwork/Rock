﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionRequests.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.ConnectionRequests" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <section class="panel panel-persondetails">

            <div class="panel-heading clearfix">
                <h3 class="panel-title pull-left">
                    <i class="fa fa-plug"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Connection Requests" /></h3>
            </div>

            <div class="panel-body">
                <ul class="connectiontype-list list-unstyled">
                    <asp:Repeater ID="rConnectionTypes" runat="server" OnItemDataBound="rConnectionTypes_ItemDataBound">
                        <ItemTemplate>
                            <asp:Literal runat="server" ID="lConnectionOpportunityList" />
                        </ItemTemplate>
                        <FooterTemplate>
                                </ul>
                            </li>
                        </FooterTemplate>
                    </asp:Repeater>
                </ul>
            </div>

        </section>

    </ContentTemplate>
</asp:UpdatePanel>
