<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CurrentPerson.ascx.cs" Inherits="RockWeb.Blocks.Cms.CurrentPerson" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <h3>
            <asp:Literal ID="lblPersonName" runat="server" />
        </h3>
    </ContentTemplate>
</asp:UpdatePanel>
