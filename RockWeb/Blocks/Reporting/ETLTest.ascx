<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ETLTest.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ETLTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <h1>ETL Test</h1>
            <asp:LinkButton ID="btnGo" runat="server" CssClass="btn btn-primary" Text="GO" OnClick="btnGo_Click" />

            
            <div class="code">
                <pre><asp:Literal ID="lSql" runat="server" /></pre>
            </div>
            
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
