<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <ul class="nav navbar-nav contextsetter contextsetter-campus">
            <li class="dropdown">

                <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                    <asp:Literal ID="lCurrentSelection" runat="server" />
                    <b class="fa fa-caret-down"></b>
                </a>

                <ul class="dropdown-menu">
                    <asp:Repeater runat="server" ID="rptCampuses" OnItemCommand="rptCampuses_ItemCommand">
                        <ItemTemplate>
                            <li>
                                <asp:LinkButton ID="btnCampus" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </li>
        </ul>
    </ContentTemplate>
</asp:UpdatePanel>