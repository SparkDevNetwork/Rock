<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationEntry.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment"></i> New Communication</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">

                

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>