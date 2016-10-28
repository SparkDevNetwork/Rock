<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ETLTest.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ETLTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <h1>ETL Test</h1>
            <asp:LinkButton ID="btnCreateDimPersonSQL" runat="server" CssClass="btn btn-primary" Text="CreateDimPersonSQL" OnClick="btnCreateDimPersonSQL_Click" />
            <Rock:RockTextBox ID="tbSQL" runat="server" TextMode="MultiLine" Rows="10" />

            <asp:LinkButton ID="btnCreateDimDefinedTypeViews1" runat="server" CssClass="btn btn-primary" Text="CreateDimDefinedTypeViews1" OnClick="btnCreateDimDefinedTypeViews1_Click" />
            <asp:LinkButton ID="btnCreateDimDefinedTypeViews2" runat="server" CssClass="btn btn-primary" Text="CreateDimDefinedTypeViews2" OnClick="btnCreateDimDefinedTypeViews2_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
