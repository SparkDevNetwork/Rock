<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemOccurrenceLava.ascx.cs" Inherits="RockWeb.Blocks.Event.EventItemOccurrenceLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
  <div class="container">
            <div class="row">
        <asp:Literal ID="lOutput" runat="server"></asp:Literal>
    </div>
        </div>

        <div class="container">
            <div class="row">
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
