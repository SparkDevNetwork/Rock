<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarContentChannelItemList.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarContentChannelItemList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bullhorn"></i> Content Channels
                </h1>
            </div>
            
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <asp:PlaceHolder ID="phContentChannelGrids" runat="server" />
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
