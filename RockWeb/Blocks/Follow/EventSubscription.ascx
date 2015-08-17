<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventSubscription.ascx.cs" Inherits="RockWeb.Blocks.Follow.EventSubscription" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class='fa fa-flag'></i> Following Events</h1>
            </div>

            <div class="panel-body">

                <asp:Repeater ID="rptEntityType" runat="server" OnItemDataBound="rptEntityType_ItemDataBound">
                    <ItemTemplate>
                        <h4><%# Eval("FriendlyName").ToString().Replace(" Alias", "") %> Events</h4>
                        <div class="clearfix margin-l-md">    
                            <ul class="list-unstyled">
                                <asp:Repeater ID="rptEvent" runat="server" >
                                    <ItemTemplate>
                                        <li class="margin-b-sm">
                                            <asp:HiddenField ID="hfEvent" runat="server" Value='<%# Eval("Id") %>' />
                                            <Rock:RockCheckBox ID="cbEvent" runat="server" Checked='<%# (bool)Eval("Selected") %>'  Enabled='<%# !(bool)Eval("IsNoticeRequired") %>'
                                                Text='<%# Eval("Name") %>' SelectedIconCssClass="fa fa-check-square-o fa-lg fa-fw" UnSelectedIconCssClass="fa fa-square-o fa-lg fa-fw" />
                                            <span class="margin-l-md"><small><%# Eval("description") %></small></span>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                
                <div class="actions margin-b-md">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

                <Rock:NotificationBox ID="nbSaved" runat="server" NotificationBoxType="Success" Text="Your settings have been saved." Dismissable="true" Visible="false" />
                
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
