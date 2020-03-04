<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchievementAttemptList.ascx.cs" Inherits="RockWeb.Blocks.Streaks.AchievementAttemptList" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-member-note').tooltip();

        // data view sync list popover
        $('.js-sync-popover').popover();
    });
</script>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlAttempts" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick" FieldLayout="Custom">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                        <Rock:StreakTypeAchievementTypePicker ID="statPicker" runat="server" Label="Achievement Type" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DateRangePicker ID="drpStartDate" runat="server" Label="Start Date" />
                                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                                    </div>
                                </div>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gAttempts" runat="server" AllowSorting="true" OnRowDataBound="gAttempts_RowDataBound" ExportSource="ColumnOutput" OnRowSelected="gAttempts_RowSelected" >
                                <Columns>
                                    <Rock:RockLiteralField ID="lExportFullName" HeaderText="Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lNameWithHtml" HeaderText="Name" SortExpression="LastName,NickName" ExcelExportBehavior="NeverInclude" />
                                    <Rock:RockBoundField DataField="AchievementName" HeaderText="Achievement" />
                                    <Rock:DateTimeField HeaderText="Start Date" DataField="StartDate" SortExpression="StartDate" DataFormatString="{0:d}" />
                                    <Rock:DateTimeField HeaderText="End Date" DataField="EndDate" SortExpression="EndDate" DataFormatString="{0:d}" />
                                    <Rock:BoolField DataField="IsSuccessful" HeaderText="Successful" SortExpression="IsSuccessful" />
                                    <Rock:BoolField DataField="IsClosed" HeaderText="Closed" SortExpression="IsClosed" />
                                    <Rock:RockLiteralField ID="lProgress" HeaderText="Progress" SortExpression="Progress" />
                                    <Rock:DeleteField OnClick="gAttempts_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

             <script>

                Sys.Application.add_load( function () {
                    $("div.photo-icon").lazyload({
                        effect: "fadeIn"
                    });

                    // person-link-popover
                    $('.js-person-popover').popover({
                        placement: 'right',
                        trigger: 'manual',
                        delay: 500,
                        html: true,
                        content: function() {
                            var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' +  $(this).attr('personid') + '/false';

                            var result = $.ajax({
                                                type: 'GET',
                                                url: dataUrl,
                                                dataType: 'json',
                                                contentType: 'application/json; charset=utf-8',
                                                async: false }).responseText;

                            var resultObject = JSON.parse(result);

                            return resultObject.PickerItemDetailsHtml;

                        }
                    }).on('mouseenter', function () {
                        var _this = this;
                        $(this).popover('show');
                        $(this).siblings('.popover').on('mouseleave', function () {
                            $(_this).popover('hide');
                        });
                    }).on('mouseleave', function () {
                        var _this = this;
                        setTimeout(function () {
                            if (!$('.popover:hover').length) {
                                $(_this).popover('hide')
                            }
                        }, 100);
                    });

                });
            </script>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
