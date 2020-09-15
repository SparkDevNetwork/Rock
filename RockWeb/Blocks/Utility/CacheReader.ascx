<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CacheReader.ascx.cs" Inherits="RockWeb.Blocks.Utility.CacheReader" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i> 
                    Cache Reader
                </h1>

            </div>

            <div class="panel-body">

                <Rock:RockLiteral ID="lOutput" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>