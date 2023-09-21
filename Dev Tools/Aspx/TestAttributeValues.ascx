<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestAttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Examples.TestAttributeValues" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div>Status:</div>
        <div>
            <asp:Literal ID="ltMessage" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>