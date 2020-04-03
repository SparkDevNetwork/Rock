<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SearchBar.ascx.cs" Inherits="RockWeb.Plugins.org_hfbc.Legacy685.SearchBar" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>
        <div class="smartsearch searchinput">
            <Rock:BootstrapButton ID="btnSubmit" runat="server" CssClass="pull-right btn btn-primary" Text="Search" OnClick="btnSubmit_Click" />

            <span class="twitter-typeahead" style="position: relative; display: inline-block;">
                <Rock:RockTextBox ID="tbName" FormGroupCssClass="searchinput tt-query" runat="server" CssClass="input-width-md" />
            </span>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
