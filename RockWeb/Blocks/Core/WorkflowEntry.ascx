<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" />

        <asp:Panel ID="pnlForm" runat="server">

            <asp:Literal ID="lheadingText" runat="server" />

            <asp:PlaceHolder ID="phAttributes" runat="server" />
            
            <asp:Literal ID="lFootingText" runat="server" />

            <div class="actions">
                <asp:PlaceHolder ID="phActions" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
