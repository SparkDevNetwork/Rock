<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ImportCheckInConfig.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.ImportCheckInConfig" %>

<asp:UpdatePanel ID="pnlMain" runat="server">
    <ContentTemplate>
        <Rock:FileUploader ID="fuSource" runat="server" Label="Source JSON" />
        <asp:LinkButton ID="lbImport" runat="server" Text="Import" CssClass="btn btn-primary" OnClick="Import_Click" />

        <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="mt-4 well">
            <asp:Literal ID="ltMessages" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
