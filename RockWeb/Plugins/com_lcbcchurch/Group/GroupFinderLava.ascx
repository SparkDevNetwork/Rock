<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinderLava.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.Groups.GroupFinderLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />
        <Rock:HiddenFieldWithClass ID="hfCampusIds" runat="server" CssClass="js-CampusId" />
        <Rock:HiddenFieldWithClass ID="hfFilterOneValues" runat="server" CssClass="js-FilterOneValue" />
        <Rock:HiddenFieldWithClass ID="hfFilterTwoValues" runat="server" CssClass="js-FilterTwoValue" />
        <Rock:HiddenFieldWithClass ID="hfPageNo" runat="server" CssClass="js-Page" />


        <asp:Literal ID="lFilter" runat="server"></asp:Literal>
        <div class="searchbox">
        <asp:TextBox ID="tbKeywords" runat="server" placeholder="Search Groups..." CssClass="form-control form-control-minimal js-search" />
        </div>
        <asp:Literal ID="lResults" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>