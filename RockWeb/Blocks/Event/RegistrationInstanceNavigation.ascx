<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceNavigation.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceNavigation" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <ul class="nav nav-tabs margin-b-md">
                <asp:Repeater ID="rptPages" runat="server" OnItemDataBound="rptPages_ItemDataBound">
                    <ItemTemplate>
                        <li runat="server" id="liNavigationTab" class="">
                            <a runat="server" id="aPageLink">
                                <asp:Literal ID="lPageName" runat="server" /></a>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
