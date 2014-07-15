<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SelectArea.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.SelectArea" %>

<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="checkin-manager">

            <ul class="list-group">

                <asp:Repeater ID="rptNavItems" runat="server">
                    <ItemTemplate>
                        <li id="liNavItem" runat="server" class="list-group-item clickable" >
                            <asp:PlaceHolder runat="server">
                                <%# Eval("Name") %>
                            </asp:PlaceHolder>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>

            </ul>

        </div>

    </ContentTemplate>
</Rock:RockUpdatePanel>
