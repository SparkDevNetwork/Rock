<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SampleData.ascx.cs" Inherits="RockWeb.Blocks.Examples.SampleData" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="true" NotificationBoxType="Danger" Title="Important!" Text="Never load sample data into your production (real) Rock system.  This sample data for training and testing purposes only."></Rock:NotificationBox>
        <Rock:BootstrapButton ID="bbtnLoadData" runat="server" CssClass="btn btn-primary" OnClick="bbtnLoadData_Click" Text="Load Sample Data" DataLoadingText="Loading...(this may take a few minutes)"></Rock:BootstrapButton>
    </ContentTemplate>
</asp:UpdatePanel>
