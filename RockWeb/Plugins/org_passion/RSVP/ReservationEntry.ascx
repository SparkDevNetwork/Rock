<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationEntry.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.ServiceReservation.ReservationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalDialog ID="mdWarning" runat="server" />

        <asp:Panel ID="pnlCardSelection" runat="server" >
            <asp:Literal ID="lCardCaption" runat="server" />
            <div class="row margin-t-lg">
                <asp:Repeater ID="rptrCards" runat="server">
                    <ItemTemplate>
                        <div ID="dvItemTemplate" runat="server" class='<%# CardItemClass %>' >
                            <asp:LinkButton ID="lbCard" runat="server" CommandArgument='<%# Eval("Key") %>' CommandName="Select"><%# FormatCard( Container.DataItem ) %></asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="actions">
                <asp:LinkButton ID="lBack" runat="server" CssClass="btn btn-default btn-sm" Visible="false" OnClick="lBack_Click"><i class="fa fa-chevron-left"></i> Back</asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfCampusGuid" runat="server" />
        <asp:HiddenField ID="hfDate" runat="server" />
        <asp:HiddenField id="hfLocationGuid" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
