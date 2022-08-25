<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CacheManager.ascx.cs" Inherits="RockWeb.Blocks.Cms.CacheManager" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<script type="text/javascript">
<%--
    This script overrides the default click handling of rcbEnableStatistics control
    to force the user to confirm the settings change before it takes effect, as
    updating the web.config value will immediately cause Rock to restart.
    --%>
    $(document).ready(function () {
        var confirmText = 'Changing this setting will cause Rock to restart.  Are you sure you want to modify this setting?';
        $('.js-enable-statistics')
            .removeAttr('onclick')
            .off('click')
            .on('click', function (e) {
                e.preventDefault();
                Rock.dialogs.confirm(confirmText, function (result) {
                    if (result) {
                        $('.js-enable-statistics').each(function () { this.checked = !this.checked });
                        bootbox.alert('The Rock application will be restarted.  Please wait while this page is reloaded.')
                        __doPostBack('<%= rcbEnableStatistics.UniqueID %>', '');
                    }
                });
            });
    });
</script>

<asp:HiddenField ID="hfActiveDialog" runat="server" />

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="rcbEnableStatistics" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tachometer"></i>Cache Manager</h1>
                <div class="panel-labels">
                    <asp:LinkButton ID="btnClearCache" runat="server" CssClass="btn btn-primary btn-xs" OnClick="btnClearCache_Click" CausesValidation="false">
                        <i class="fa fa-repeat"></i> Clear Cache
                    </asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" Dismissable="true" />

                <div class="row">

                    <div class="col-md-6">
                        <h4 class="mt-0 pull-left">Cache Tags </h4>
                        <div class="pull-right"><Rock:HelpBlock ID="hbCacheTag" runat="server" Text="These Cache Tags values are written to the 'Cache Tags' Defined Type.  You can delete the values there, but you should make sure they are not being used in any HTML content or Lava templates." /></div>
                        <Rock:Grid ID="gCacheTagList" runat="server" AllowSorting="true" EmptyDataText="No Tags Found" DisplayType="Light" OnRowSelected="gCacheTagList_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="TagName" HeaderText="Tag Name" SortExpression="TagName" />
                                <Rock:RockBoundField DataField="TagDescription" HeaderText="Description" SortExpression="TagDescription" TruncateLength="255" HtmlEncode="false" />
                                <Rock:RockBoundField DataField="LinkedKeys" HeaderText="Linked Keys" SortExpression="LinkedKeys" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:LinkButtonField ToolTip="Flushes all items from cache that are tied to this tag." Text="<i class='fa fa-eraser'></i>" CssClass="btn btn-default btn-sm btn-square" OnClick="gCacheTagList_ClearCacheTag" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                    <div class="col-md-6">
                        <div class="row d-flex">
                            <div class="col-md-6">
                                <h4>Cache Statistics</h4>
                            </div>
                            <div class="col-md-6 my-auto">
                                <Rock:RockCheckBox ID="rcbEnableStatistics" CssClass="js-enable-statistics" runat="server" Text="Enable Statistics" AutoPostBack="true" OnCheckedChanged="rcbEnableStatistics_CheckedChanged" />
                            </div>
                        </div>
                        <Rock:RockDropDownList ID="ddlCacheTypes" runat="server" DataTextField="Name" DataValueField="Id" Label="Cache Types" OnSelectedIndexChanged="ddlCacheTypes_SelectedIndexChanged" AutoPostBack="true" />
                        <table class="grid-table table table-condensed table-light">
                            <thead><tr><td colspan="2"></td></tr><tr><th>Stat</th><th>Count</th></tr></thead>
                            <asp:Literal ID="lCacheStatistics" runat="server" /><tr><td colspan="2"></td></tr>
                        </table>

                        <br />
                    </div>
                </div>

            </div>
        </div>

        <Rock:ModalDialog ID="dlgAddTag" runat="server" Title="Add Tag" OnSaveClick="dlgAddTag_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <Rock:NotificationBox ID="nbModalMessage" runat="server" Visible="false" />
                <asp:HiddenField ID="hfTagId" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTagName" runat="server" Label="Tag Name" onkeypress="this.value = this.value.toLowerCase().replace(' ', '-');" Style="text-transform: lowercase;" Width="100%"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTagDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" Width="100%"/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>