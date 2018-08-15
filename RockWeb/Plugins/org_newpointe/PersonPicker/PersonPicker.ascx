<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonPicker.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.PersonPicker.PersonPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" OnSelectPerson="ppPerson_SelectPerson" />
    </ContentTemplate>
</asp:UpdatePanel>
