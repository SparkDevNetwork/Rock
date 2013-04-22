<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Communication.ascx.cs" Inherits="RockWeb.Blocks.Core.Communication" %>

<asp:UpdatePanel id="upPanel" runat="server">
    <ContentTemplate>

        <ul class="nav nav-pills" >
            <asp:Repeater ID="rptChannels" runat="server" >
                <ItemTemplate >
                    <li class='<%# ChannelId.HasValue && (int)Eval("Key") == ChannelId.Value ? "active" : "" %>'>
                        <asp:LinkButton ID="lbChannel" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Key") %>' OnClick="lbChannel_Click">
                        </asp:LinkButton> 
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

        <asp:PlaceHolder ID="phContent" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>


