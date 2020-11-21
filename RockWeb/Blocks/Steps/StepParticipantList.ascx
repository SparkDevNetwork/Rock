<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepParticipantList.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepParticipantList" %>
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

            <div id="pnlSteps" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Steps" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" FieldLayout="Custom">
                                <div class="row">
                                    <div class="col-lg-6">
                                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                        <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" />
                                        <Rock:CampusPicker ID="cpCampusFilter" runat="server" Label="Campus" />
                                    </div>
                                    <div class="col-lg-6">
                                        <Rock:DateRangePicker ID="drpDateStarted" runat="server" Label="Date Started" />
                                        <Rock:DateRangePicker ID="drpDateCompleted" runat="server" Label="Date Completed" />
                                        <Rock:RockCheckBoxList ID="cblStepStatus" runat="server" Label="Step Status" RepeatDirection="Horizontal" />
                                    </div>
                                </div>
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gSteps" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gSteps_Edit" CssClass="js-grid-step-list" OnRowDataBound="gSteps_RowDataBound" ExportSource="ColumnOutput" >
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <Rock:RockLiteralField ID="lExportFullName" HeaderText="Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lNameWithHtml" HeaderText="Name" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" ExcelExportBehavior="NeverInclude" />
                                    <Rock:DateTimeField HeaderText="Date Started" DataField="StartedDateTime" SortExpression="StartedDateTime" DataFormatString="{0:d}"  />
                                    <Rock:DateTimeField HeaderText="Date Completed" DataField="CompletedDateTime" SortExpression="CompletedDateTime" DataFormatString="{0:d}" />
                                    <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="Campus.Name" ExcelExportBehavior="AlwaysInclude"/>
                                    <Rock:RockLiteralField ID="lStepStatusHtml" HeaderText="Status" SortExpression="StepStatus.Name" ExcelExportBehavior="NeverInclude" />
                                    <Rock:RockBoundField DataField="Note" HeaderText="Note" SortExpression="Note" ItemStyle-CssClass="small" />
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
