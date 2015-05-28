<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinGroupList.ascx.cs" Inherits="RockWeb.Blocks.Checkin.CheckinGroupList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> Check-in Areas</h1>
            </div>
            <div class="panel-body">
                <asp:Literal ID="lWarnings" runat="server" />
                <asp:Literal ID="lContent" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
