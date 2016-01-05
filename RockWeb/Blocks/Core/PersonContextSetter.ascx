<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.PersonContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbSelectGroupWarning" NotificationBoxType="Warning" Text="Select a group in the block settings" />

        <ul class="nav navbar-nav contextsetter person-context-setter">
            <li class="dropdown">

                <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                    <asp:Literal ID="lCurrentSelection" runat="server" />
                    <b class="fa fa-caret-down"></b>
                </a>

                <ul class="dropdown-menu">
                    <asp:Repeater runat="server" ID="rptPersons" OnItemCommand="rptPersons_ItemCommand">
                        <ItemTemplate>
                            <li>
                                <asp:LinkButton ID="btnPerson" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </li>
        </ul>
    </ContentTemplate>
</asp:UpdatePanel>
