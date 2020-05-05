<%@ Control AutoEventWireup="true" CodeFile="TestSQLConnectivity.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Testing.TestSQLConnectivity" Language="C#" %>

<asp:UpdatePanel ID="upnlMain" runat="server" Visible="true">
    <ContentTemplate>
        <asp:Literal runat="server" ID="lAlert" />
        <Rock:CodeEditor runat="server" ID="ceMain" Label="SQL You Want To Run" Wrap="true" EditorTheme="Rock" EditorMode="Sql" />
        <Rock:BootstrapButton runat="server" ID="btnRun" Text="Run" OnClick="btnRun_Click" CssClass="btn btn-primary" />
        <Rock:RockTextBox runat="server" ID="tbResult" Label="Result" Visible="false"/>

    </ContentTemplate>
</asp:UpdatePanel>
