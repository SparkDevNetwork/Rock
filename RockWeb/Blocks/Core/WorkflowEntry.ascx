<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <p class="description"><asp:Literal ID="lheadingText" runat="server" /></p>

        <asp:PlaceHolder id="phAttributes" runat="server" />

        <p class="description"><asp:Literal ID="lFootingText" runat="server" /></p>

        <div class="actions">
            <asp:PlaceHolder ID="phActions" runat="server" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
