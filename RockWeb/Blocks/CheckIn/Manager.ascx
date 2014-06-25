<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Manager.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="checkin-manager">

            <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a Check-in type in the block settings." Dismissable="true" />

            <div class="row">
                <div class="col-xs-8 col-sm-9 col-md-10 col-lg-11">
                    <Rock:RockTextBox ID="tbSearch" runat="server" Placeholder="Person Search..." />
                </div>
                <div class="col-xs-4 col-sm-3 col-md-2 col-lg-1">
                    <span class="pull-right">
                        <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-default"><i class="fa fa-search"></i> Search</asp:LinkButton>
                    </span>
                </div>
            </div>

            <br />

            <div class="panel panel-default">

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading">
                    <asp:LinkButton ID="lbNavHeading" runat="server" OnClick="lbNavigate_Click">
                        <asp:PlaceHolder runat="server">
                            <i class="fa fa-chevron-left"></i> <asp:Literal ID="lNavHeading" runat="server" />
                        </asp:PlaceHolder>
                    </asp:LinkButton>
                </asp:Panel>

                <ul class="list-group">
                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li class="list-group-item">
                                <asp:LinkButton ID="lbNavItem" runat="server" OnClick="lbNavigate_Click" CommandArgument='<%# Eval("TypeKey").ToString() + Eval("Id").ToString() %>'>
                                    <asp:PlaceHolder runat="server">
                                        <%# Eval("Name") %>
                                        <span class="pull-right">
                                            <span class="badge"><%# ((int)Eval("CurrentCount")).ToString("N0") %></span>
                                            &nbsp;&nbsp;
                                            <i class='fa fa-fw fa-chevron-right'></i>
                                        </span>
                                    </asp:PlaceHolder>
                                </asp:LinkButton>
                            </li>
                        </ItemTemplate>
                        <AlternatingItemTemplate>
                            <li class="list-group-item">
                                <asp:LinkButton ID="lbNavItem" runat="server" OnClick="lbNavigate_Click" CommandArgument='<%# Eval("TypeKey").ToString() + Eval("Id").ToString() %>'>
                                    <asp:PlaceHolder runat="server">
                                        <%# Eval("Name") %>
                                        <span class="pull-right">
                                            <span class="badge"><%# ((int)Eval("CurrentCount")).ToString("N0") %></span>
                                            &nbsp;&nbsp;
                                            <i class="fa fa-fw"></i>
                                        </span>
                                    </asp:PlaceHolder>
                                </asp:LinkButton>
                            </li>
                        </AlternatingItemTemplate>
                    </asp:Repeater>
                </ul>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
