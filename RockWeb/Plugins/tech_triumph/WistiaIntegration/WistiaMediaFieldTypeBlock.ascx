<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WistiaMediaFieldTypeBlock.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.WistiaMediaFieldTypeBlock" %>

<asp:UpdatePanel ID="upnlWistiaMedia" runat="server">
    <ContentTemplate>

        <Rock:RockDropDownList ID="ddlAccount" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlAccount_SelectedIndexChanged" Label = "Wistia Account" Help = "The Wistia Account to use for the video. If one is not provided it will be asked for when uploading the video." ></Rock:RockDropDownList>
        <Rock:RockDropDownList ID="ddlProject" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" EnhanceForLongLists="true" Label="Wistia Project" Help="The Wistia Project to use for the video. If one is not provided it will be asked for when uploading the video." ></Rock:RockDropDownList>
        <Rock:RockDropDownList ID="ddlMedia" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMedia_SelectedIndexChanged" EnhanceForLongLists="true" Label="Media" Help="" ></Rock:RockDropDownList>

    </ContentTemplate>
</asp:UpdatePanel>