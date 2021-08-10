<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusStatus.ascx.cs" Inherits="RockWeb.Blocks.Bus.BusStatus" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bus"></i> 
                    Message Bus
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlActive" runat="server" />
                </div>
            </div>
            <div class="panel-body">

                <div>
                    <asp:Literal ID="lDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnTransport" runat="server" Text="Configure Transports" CssClass="btn btn-default" OnClick="btnTransport_Click" CausesValidation="false" />
                </div>
                
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>