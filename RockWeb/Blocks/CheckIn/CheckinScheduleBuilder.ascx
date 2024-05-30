<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinScheduleBuilder.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinScheduleBuilder" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Schedule Builder</h1>
            </div>
            <div class="panel-body">

                <div class="form-inline margin-b-md row">
                    <asp:Panel ID="pnlGroupType" runat="server" CssClass="col-sm-3">
                        <Rock:GroupTypePicker ID="ddlGroupType" runat="server" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>
                    <div class="col-sm-3">
                        <Rock:LocationItemPicker ID="pkrParentLocation" runat="server" Label="Parent Location" OnSelectItem="pkrParentLocation_SelectItem" />
                    </div>
                     <div class="col-sm-3">
                         <Rock:GroupTypePicker ID="ddlArea" runat="server" Label="Area" OnSelectedIndexChanged="ddlArea_SelectedIndexChanged" AutoPostBack="true" />
                     </div>
                    <div class="col-sm-3">
                        <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false" Label="Schedule Category" OnSelectItem="pCategory_SelectItem"/>
                    </div>
                </div>

                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Warning" />

                <div class="grid grid-panel">
                    <Rock:Grid ID="gGroupLocationSchedule" runat="server" AllowSorting="true" AllowPaging="false" OnRowDataBound="gGroupLocationSchedule_RowDataBound" >
                        <Columns>
		                    <Rock:RockLiteralField ID="lGroupName" HeaderText="Group" SortExpression="Group.Name" />
		                    <Rock:RockLiteralField ID="lLocationName" HeaderText="Location" SortExpression="Location.Name" />
                        </Columns>
                    </Rock:Grid>
                </div>

                <div class="actions">
                        <Rock:BootstrapButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"
                            DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                            CompletedText ="Done" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"/>
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </div>

        <script>
            function updateScheduleBuilderHeaderCheckboxes($table)
            {
                $table.find('thead > tr > th').each(function (columnIndex) {
                    if ($(this).find('a.fa').length > 0) {
                        columnIndex += 1;
                        var $cbs = $table.find('tbody > tr > td:nth-child(' + columnIndex + ') input');
                        if ($cbs.length == $cbs.filter(':checked').length) {
                            $(this).find('a.fa').addClass('fa-check-square-o').removeClass('fa-square-o');
                        }
                        else {
                            $(this).find('a.fa').addClass('fa-square-o').removeClass('fa-check-square-o');
                        }
                    }
                });
            }

            Sys.Application.add_load(function () {
                // set the default state of the header checkbox based on if all item row checkboxes are checked
                var $table = $('#<%=gGroupLocationSchedule.ClientID%>');
                updateScheduleBuilderHeaderCheckboxes($table);

                $table.find('tbody > tr > td input[type="checkbox"]').on('click', function () {
                    updateScheduleBuilderHeaderCheckboxes($table);
                });

                // toggle all check boxes when the user clicks the header checkbox.
                $('.js-sched-select-all').on('click', function (e) {
                    e.preventDefault();
                    var $th = $(this).closest('th');
                    var $table = $(this).closest('table');
                    var columnIndex = $th.parent().children().index($th) + 1;
                    var $cbs = $table.find('tbody > tr > td:nth-child(' + columnIndex + ') input');
                    if ($(this).hasClass('fa-square-o')) {
                        $(this).addClass('fa-check-square-o').removeClass('fa-square-o');
                        $cbs.prop('checked', true);
                    } else {
                        $(this).addClass('fa-square-o').removeClass('fa-check-square-o');
                        $cbs.prop('checked', false);
                    }
                });
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

