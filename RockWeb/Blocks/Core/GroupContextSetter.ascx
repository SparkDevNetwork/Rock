<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.GroupContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox runat="server" ID="nbSelectGroupTypeWarning" NotificationBoxType="Warning" Text="Select a group type or root group from the block settings" />

        <ul class="nav navbar-nav contextsetter group-context-setter">
            <li class="dropdown">

                <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                    <asp:Literal ID="lCurrentSelection" runat="server" />
                    <b class="fa fa-caret-down"></b>
                </a>

                <ul class="dropdown-menu">
                    <asp:Repeater runat="server" ID="rptGroups" OnItemCommand="rptGroups_ItemCommand">
                        <ItemTemplate>
                            <li>
                                <asp:LinkButton ID="btnGroup" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </li>
        </ul>
    </ContentTemplate>
</asp:UpdatePanel>