<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceBadges.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Groups.AttendanceBadges" %>

<script>
    Sys.Application.add_load(function () {
        $('.attendance-badge').tooltip({ 'html': 'true' });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lBlockIcon" runat="server" />
                    <asp:Literal ID="lBlockTitle" runat="server" />
                <div class="panel-labels">
                    
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbError" runat="server" Visible="false" />

                <div class="row margin-b-md">
                    <asp:Repeater ID="rptIcons" runat="server">
                        <ItemTemplate>
                            <div class="col-sm-2">
                                <image src="<%# Eval( "IconUrl" ) %>" class="attendance-badge" data-toggle='tooltip' data-placement='top' title='<%# Eval( "Title" ) %>' />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

            </div>
        </asp:Panel>

        

    </ContentTemplate>
</asp:UpdatePanel>

