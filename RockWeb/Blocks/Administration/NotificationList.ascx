<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NotificationList.ascx.cs" Inherits="RockWeb.Blocks.Utility.NotificationList" %>

<asp:UpdatePanel ID="upnlContent" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bell"></i> Notifications</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">
                <asp:Repeater ID="rptNotifications" OnItemDataBound="rptNotifications_ItemDataBound" OnItemCommand="rptProjects_ItemCommand" runat="server" >
                    <ItemTemplate>
                        <div class="alert" id="rptNotificationAlert" runat="server">
                            <div class="pull-right"><asp:LinkButton runat="server" CssClass="action" CommandName="Close" CommandArgument='<%#Eval("Guid") %>' CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                            <h4>
                                <i id="iIconCssClass" runat="server" class='<%# Eval("Notification.IconCssClass") %>'></i>
                                <span><%# Eval("Notification.Title") %></span>
                            </h4>
                            <div class="notification-content"><%# Eval("Notification.Message") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
