<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlForm" runat="server">

            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <asp:Literal ID="lheadingText" runat="server" />

            <asp:PlaceHolder ID="phAttributes" runat="server" />
            
            <asp:Literal ID="lFootingText" runat="server" />

            <div class="actions">
                <asp:PlaceHolder ID="phActions" runat="server" />
            </div>
        
        </asp:Panel>

        <br />

        <Rock:NotificationBox ID="nbMessage" runat="server" Dismissable="true" />

    </ContentTemplate>
</asp:UpdatePanel>
