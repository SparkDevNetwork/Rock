<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkPersonInactivator.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.BulkPersonInactivator" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (percent, resultText) {

            $('.js-progress-container').fadeIn();
            $('.js-inactivate-btn').hide();

            var $progressBar = $('.js-notification').find('.js-progress-bar');
            $progressBar.attr('aria-valuenow', percent).css('width', percent + '%');
            $progressBar.find('.sr-only').text(percent + '% complete');
            $('.js-notification').find('.js-result-text').html(resultText);

            if (percent == 'completed') {
                $('.js-progress-container').fadeOut();
                $('.js-notification').find('.js-result-text').html(resultText);
                $('.js-inactivate-btn').show();
            }

            if (percent == 'exception')
            {
                $('.js-notification').find('.js-result-text').html(resultText);
            }
        }

        $.connection.hub.start().done(function () { });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h4 class="panel-title pull-left">Bulk Person Record Status Updater</h4>
            </div>
            <div class="panel-body">
                <Rock:DataViewPicker ID="dvpDataview" runat="server" Label="DataView" Help="Choose the Dataview that contains the list of people that will get their record status updated."  Required="true" AutoPostBack="true" OnSelectedIndexChanged="dvpDataview_SelectedIndexChanged" />
                <Rock:RockLiteral ID="lRecordCount" runat="server" Label="Record Count" />

                <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" Label="Update Record Status" Required="true" AutoPostBack="true" OnSelectedIndexChanged="dvpRecordStatus_SelectedIndexChanged" />
                <Rock:DefinedValuePicker ID="dvpRecordStatusReason" runat="server" Label="Update Record Status Reason" Required="true" />
                <Rock:RockTextBox ID="tbInactiveReasonNote" runat="server" Label="Inactive Reason Note" Text="Inactive for 8+ months" />

                <div class="js-notification">
                    <div class="js-result-text"></div>
                    <div class='progress js-progress-container' style="display:none">
                        <div class='progress-bar js-progress-bar' role='progressbar' aria-valuenow='0' aria-valuemin='0' aria-valuemax='100' style='width: 0%;'>
                            <span class='sr-only'>0% Complete</span>
                        </div>
                    </div>
                </div>

                <asp:LinkButton ID="btnUpdateRecords" runat="server" CssClass="btn btn-primary js-inactivate-btn" Text="Update Records" OnClick="btnUpdateRecords_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-danger js-cancel-btn" Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
