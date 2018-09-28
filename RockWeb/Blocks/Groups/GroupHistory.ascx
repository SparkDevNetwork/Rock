<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupHistory.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupHistory" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-history"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="pull-right">
                    <asp:HyperLink ID="hlMemberHistory" runat="server" CssClass="btn btn-xs btn-default">Member History</asp:HyperLink>
                    <a class="btn btn-xs btn-default margin-l-sm" onclick="javascript: toggleOptions()"><i title="Options" class="fa fa-gear"></i></a>
                </div>
            </div>

            <div class="panel-body js-options" style="display: none;">
                <div class="col-md-6">
                </div>
                <div class="col-md-6">
                    <asp:Panel ID="pnlGroupHistoryOptions" runat="server">
                        <div class="pull-right">
                            <span class="">Show Group Members in History</span>
                            <Rock:Toggle ID="tglShowGroupMembersInHistory" runat="server" CssClass="pull-right margin-l-sm" ButtonSizeCssClass="btn-xs" OnCssClass="btn-primary" Checked="true" OnCheckedChanged="tglShowGroupMembersInHistory_CheckedChanged" />
                        </div>
                    </asp:Panel>
                </div>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lTimelineHtml" runat="server" />
            </div>
        </div>
        </div>

        <script>
            function toggleOptions() {
                $('.js-options').slideToggle();
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
