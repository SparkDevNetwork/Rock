<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExecutiveDashboard.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.ExecutiveDashboard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fas fa-hands-heart"></i>Executive/Campus Dashboard
                </h1>
                <div class="pull-right">
                    <Rock:RockDropDownList ID="ddlCampus" runat="server" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlCampus_SelectionChanged" DataTextField="Name" DataValueField="Id" />
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                <asp:Literal ID="lDashboardContent" runat="server"></asp:Literal>
            </div>
        </div>
        <asp:Literal ID="lActivitiesContent" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>
