<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Communication.ascx.cs" Inherits="RockWeb.Blocks.Core.Communication" %>

<asp:UpdatePanel id="upPanel" runat="server">
    <ContentTemplate>

        <ul class="nav nav-pills" >
            <asp:Repeater ID="rptChannels" runat="server" >
                <ItemTemplate >
                    <li class='<%# (int)Eval("Id") == CurrentChannelId ? "active" : "" %>'>
                        <asp:LinkButton ID="lbChannel" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' OnClick="lbChannel_Click">
                        </asp:LinkButton> 
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

        <asp:PlaceHolder ID="phContent" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>


