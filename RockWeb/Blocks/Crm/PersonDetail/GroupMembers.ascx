<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMembers.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GroupMembers" %>

<asp:UpdatePanel ID="upGroupMembers" runat="server">
    <ContentTemplate>

        <asp:Repeater ID="rptrGroups" runat="server">
            <ItemTemplate>

                <div class="persondetails-group rollover-container">

                    <div class="actions rollover-item">
                        <asp:HyperLink ID="hlEditGroup" runat="server" AccessKey="O" CssClass="edit btn btn-action btn-xs"><i class="fa fa-pencil"></i> <asp:Literal ID="lEditGroup" runat="server" /></asp:HyperLink>
                    </div>

                    <div class="row">
                
                        <div class="col-md-8 clearfix">

                            <header class="title"><%# FormatAsHtmlTitle(Eval("Name").ToString()) %></header>

                            <ul class="groupmembers">

                                <asp:Repeater ID="rptrMembers" runat="server">
                                    <ItemTemplate>
                                        <li class='<%# FormatPersonCssClass( (bool)Eval("Person.IsDeceased") ) %>'>
                                            <a href='<%# FormatPersonLink(Eval("PersonId").ToString()) %>'>
                                                <div class="person-image" id="divPersonImage" runat="server"></div>
                                                <div class="person-info">
                                                    <h4><%# FormatPersonName(Eval("Person.NickName") as string, Eval("Person.LastName") as string) %></h4>
                                                    <small><%# FormatPersonDetails( Container.DataItem ) %></small>
                                                </div>
                                            </a>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                        <div class="col-md-4 addresses clearfix">

                            <ul class="list-unstyled">

                                <asp:Repeater ID="rptrAddresses" runat="server">
                                    <ItemTemplate>
                                        <li class="address rollover-container clearfix">
                                            <h4><%# FormatAddressType(Eval("GroupLocationTypeValue.Value")) %></h4>
                                            <a id="aMap" runat="server" title="Map This Address" class="map" target="_blank">
                                                <i class="fa fa-map-marker"></i>
                                            </a>
                                            <div class="address">
                                                <%# Eval("Location.FormattedHtmlAddress") %>
                                            </div>
                                            <div class="pull-left rollover-item">
                                                <asp:LinkButton ID="lbVerify" runat="server" CommandName="verify" ToolTip="Verify Address">
                                                    <i class="fa fa-globe"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="lbLocationSettings" runat="server" CommandName="settings" ToolTip="Configure Location">
                                                    <i class="fa fa-gear"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>

                        </div>

                    </div>

                    <asp:panel ID="pnlGroupAttributes" runat="server" CssClass="margin-l-md js-family-attributes" style="min-height: 22px;" >
                        <div class="actions pull-right">
                            <asp:HyperLink ID="hlShowMoreAttributes" runat="server" Visible="false" CssClass="btn btn-xs btn-default js-show-more-family-attributes"><i class="fa fa-chevron-down"></i> More</asp:HyperLink>
                        </div>
                        <ul class="list-inline">
                            <asp:PlaceHolder ID="phGroupAttributes" runat="server" />
                        </ul>
                        <ul class="list-inline js-more-family-attributes" style="display:none">
                            <asp:PlaceHolder ID="phMoreGroupAttributes" runat="server" />
                        </ul>
                    </asp:panel>

                </div>

            </ItemTemplate>
        </asp:Repeater>

        <script type="text/javascript">

            Sys.Application.add_load(function () {
                $('a.js-show-more-family-attributes').click(function (e) {
                    var $pnl = $(this).closest('div.js-family-attributes');
                    var $ul = $pnl.find('ul.js-more-family-attributes').first();
                    if ( $ul.is(':visible') ) {
                        $ul.slideUp();
                        $(this).html('<i class="fa fa-chevron-down"></i> More');
                    } else {
                        $ul.slideDown();
                        $(this).html('<i class="fa fa-chevron-up"></i> Less');
                    }
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
