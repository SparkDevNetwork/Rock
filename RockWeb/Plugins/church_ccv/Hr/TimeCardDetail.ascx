<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Hr.TimeCardDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfTimeCardId" runat="server" />
        <asp:HiddenField ID="hfEditMode" runat="server" />

        <Rock:NotificationBox ID="nbMessage" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-clock-o"></i>
                    <asp:Literal ID="lTimeCardPersonName" runat="server" />
                </h1>

                (<asp:Literal ID="lTitle" runat="server" Text="Pay Period" />)

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblSubTitle" runat="server" LabelType="Info" Text="Status" />
                </div>
            </div>
            <div class="panel-body">

                <div class="row gridresponsive-row gridresponsive-header">
                    <div class="col-xs-3 col-md-2 col-lg-1">
                        <strong>Date</strong>
                    </div>
                    <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                        <div class="row">
                            <div class="col-md-3"><strong>Time In</strong></div>
                            <div class="col-md-3"><strong>Lunch Out</strong></div>
                            <div class="col-md-3"><strong>Lunch In</strong></div>
                            <div class="col-md-3"><strong>Time Out</strong></div>
                        </div>
                    </div>
                    <div class="col-xs-4 col-md-2">
                        <div class="row">
                            <div class="col-xs-6"><strong>Hrs Worked</strong></div>
                            <div class="col-xs-6"><strong>Overtime Hrs</strong></div>
                        </div>
                    </div>
                    <div class="col-xs-2 col-md-2 col-lg-2"><strong>Other Hrs</strong></div>
                    <div class="col-xs-2 col-md-1 col-lg-1"><strong>Total Hrs</strong></div>
                    <div class="col-md-4 col-lg-2 hidden-xs hidden-sm"><strong>Note</strong></div>
                </div>

                <asp:Repeater runat="server" ID="rptTimeCardDay" OnItemDataBound="rptTimeCardDay_ItemDataBound">
                    <ItemTemplate>

                        <div class="row gridresponsive-row gridresponsive-item">
                            <div class="gridresponsive-item-view clickable">
                                <div class="col-xs-3 col-md-2 col-lg-1">
                                    <div class="row">
                                        <div class="col-xs-5">
                                            <asp:Literal runat="server" ID="lTimeCardDayName" />
                                        </div>
                                        <div class="col-xs-7">
                                            <Rock:Badge runat="server" ID="lTimeCardDate" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                                    <div class="row">
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lStartDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lLunchStartDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lLunchEndDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lEndDateTime" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-4 col-md-2">
                                    <div class="row">
                                        <div class="col-xs-6">
                                            <asp:Literal runat="server" ID="lWorkedRegularHours" />
                                        </div>
                                        <div class="col-xs-6">
                                            <Rock:Badge runat="server" ID="lWorkedOvertimeHours" BadgeType="Danger" ToolTip="Overtime" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-2 col-md-2 col-lg-2">
                                    <Rock:Badge runat="server" ID="lPaidVacationHours" BadgeType="Success" ToolTip="Vacation" />
                                    <Rock:Badge runat="server" ID="lPaidHolidayHours" BadgeType="Info" ToolTip="Paid Holiday" />
                                    <Rock:Badge runat="server" ID="lPaidSickHours" BadgeType="Warning" ToolTip="Sick" />
                                </div>
                                <div class="col-xs-2 col-md-1 col-lg-1">
                                    <asp:Literal runat="server" ID="lTotalHours" />
                                </div>
                                <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">
                                    <asp:Literal runat="server" ID="lNotes" />
                                </div>
                                <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
                            </div>
                            <div class="gridresponsive-item-edit padding-b-md" style="display: none;">

                                <div class="col-xs-4 col-md-2">
                                    <div class="row">
                                        <div class="col-xs-7">
                                            <asp:Literal runat="server" ID="lTimeCardDayNameEdit" />
                                        </div>
                                        <div class="col-xs-5">
                                            <Rock:Badge runat="server" ID="lTimeCardDateEdit" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-8 col-md-8">
                                    <asp:Panel ID="pnlEditRow" runat="server" CssClass="row js-time-edit-group">
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpTimeIn" Placeholder="Enter Time" Label="Time In" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpLunchOut" Placeholder="Enter Time" Label="Lunch Out" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpLunchIn" Placeholder="Enter Time" Label="Lunch In" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpTimeOut" Placeholder="Enter Time" Label="Time Out" />
                                        </div>

                                        <div class="col-md-3">
                                            <Rock:RockDropDownList runat="server" ID="ddlVacationHours" Label="Vacation Hrs" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Label ID="lEarnedHolidayHours" runat="server" CssClass="js-earned-holiday-hours badge badge-info" ToolTip="Earned Holiday Hours based on 50% of hours worked" Style="float: right" />
                                            <Rock:RockDropDownList runat="server" ID="ddlHolidayHours" Label="Holiday Hrs" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:RockDropDownList runat="server" ID="ddlSickHours" Label="Sick Hrs" />
                                        </div>

                                        <div class="col-md-3">
                                            <Rock:RockTextBox runat="server" ID="tbNotes" Label="Notes" MaxLength="200" />
                                        </div>
                                    </asp:Panel>
                                </div>

                                <div class="col-md-2 pull-right gridresponsive-commandcolumn">
                                    <asp:LinkButton runat="server" ID="lbSave" CssClass="btn btn-success btn-sm js-item-save margin-b-sm" OnClick="lbSave_Click"><i class="fa fa-check"></i></asp:LinkButton>
                                    <a runat="server" id="lbCancel" class="btn btn-warning btn-sm js-item-cancel margin-b-sm"><i class="fa fa-minus"></i></a>
                                </div>

                            </div>

                        </div>
                        <asp:Panel ID="pnlTimeCardSummaryRow" runat="server">
                            <div class="row gridresponsive-row timecard-subtotal-row">
                                <div class="col-xs-3 col-md-2 col-lg-1">
                                    Subtotal:
                                </div>
                                <div class="col-lg-3 hidden-xs hidden-sm hidden-md"></div>
                                <div class="col-xs-4 col-md-2">
                                    <div class="row">
                                        <div class="col-xs-6">
                                            <asp:Literal runat="server" ID="lWorkedRegularHoursSummary" />
                                        </div>
                                        <div class="col-xs-6">
                                            <asp:Literal runat="server" ID="lWorkedOvertimeHoursSummary" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-2 col-md-2 col-lg-2">
                                    <asp:Literal runat="server" ID="lOtherHoursSummary" />
                                </div>
                                <div class="col-xs-2 col-md-1 col-lg-1">
                                    <asp:Literal runat="server" ID="lTotalHoursSummary" />
                                </div>
                                <div class="col-md-4 col-lg-2 hidden-xs hidden-sm"></div>
                            </div>
                        </asp:Panel>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="margin-t-md margin-b-md hidden-sm hidden-xs clearfix">
                    <div class=" pull-right">
                        <span class="label label-success">Vacation</span> <span class="label label-info">Holiday</span> <span class="label label-warning">Sick</span> <span class="label label-danger">Overtime</span>
                    </div>
                </div>

                <div class="row">

                    <div class="col-md-8">

                        <!-- Actions/Submit Panel -->
                        <asp:Panel ID="pnlPersonActions" CssClass="js-submit-panel" runat="server">
                            <h4>Submit</h4>

                            <Rock:NotificationBox ID="nbSubmittedSuccessMessage" runat="server" NotificationBoxType="Success" />
                            <asp:ValidationSummary ID="valSummarySubmit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vgSubmit" />
                            <Rock:RockDropDownList ID="ddlSubmitTo" runat="server" Label="Submit to" Required="true" ValidationGroup="vgSubmit" />

                            <asp:CustomValidator ID="validateAgree" runat="server" ValidationGroup="vgSubmit" ClientValidationFunction="validateCheckbox" ErrorMessage="Agree must be checked" />
                            <Rock:RockCheckBox ID="cbAgree" runat="server" CssClass="js-agree-checkbox" Text="I certify and agree that:" />

                            <ol>
                                <li>All of the entries on this Time Card for this payroll period are both accurate and complete;</li>
                                <li>The entries include all hours worked plus paid time off hours, if any;</li>
                                <li>The entries do not include unpaid meal periods or other non-work time; and,</li>
                                <li>I have not worked either more or fewer hours than entered.</li>
                            </ol>


                            <div class="actions margin-t-sm">
                                <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-action" ValidationGroup="vgSubmit" CausesValidation="true" OnClientClick="maintainScrollPosition($('.js-submit-panel'))" OnClick="lbSubmit_Click" Text="Submit" />
                            </div>

                        </asp:Panel>

                    </div>

                    <div class="col-md-4">
                        <!-- Totals Panel -->
                        <asp:Panel ID="pnlTotals" runat="server">

                            <h4>Totals:</h4>

                            <div>
                                <strong>Worked Hours</strong>
                                <div class="row">
                                    <div class="col-xs-8">&nbsp;Regular</div>
                                    <div class="col-xs-4">
                                        <asp:Literal ID="lTotalRegularWorked" runat="server" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-8">&nbsp;Overtime</div>
                                    <div class="col-xs-4">
                                        <asp:Literal ID="lTotalOvertimeWorked" runat="server" />
                                    </div>
                                </div>
                            </div>

                            <div class="margin-t-sm">
                                <div class="row">
                                    <div class="col-xs-8">Vacation Hours</div>
                                    <div class="col-xs-4">
                                        <asp:Literal ID="lTotalVacationPaid" runat="server" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-8">Holiday Hours</div>
                                    <div class="col-xs-4">
                                        <asp:Literal ID="lTotalHolidayPaid" runat="server" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-8">Sick Hours</div>
                                    <div class="col-xs-4">
                                        <asp:Literal ID="lTotalSickPaid" runat="server" />
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <h4>
                                    <div class="col-xs-8">All Hours</div>
                                    <div class="col-xs-4">
                                        <strong>
                                            <asp:Literal ID="lTotalHours" runat="server" />
                                        </strong>
                                    </div>
                                </h4>
                            </div>
                        </asp:Panel>
                    </div>

                </div>





                <!-- Actions/Approver-->
                <asp:Panel ID="pnlApproverActions" CssClass="js-approve-panel" runat="server">
                    <h2>Approve</h2>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbApprovedSuccessMessage" runat="server" NotificationBoxType="Success" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton runat="server" ID="btnApprove" CssClass="btn btn-success" ValidationGroup="vgApprove" CausesValidation="true" OnClientClick="maintainScrollPosition($('.js-approve-panel'))" OnClick="btnApprove_Click" Text="<i class='fa fa-check'> Approve</i>" />
                    </div>
                </asp:Panel>

                <!-- History Panel -->
                <asp:Panel ID="pnlHistory" runat="server">

                    <h2>History</h2>
                    <Rock:Grid ID="gHistory" runat="server" AllowPaging="false" AllowSorting="false" DataKeyNames="Id" DisplayType="Light">
                        <Columns>
                            <Rock:DateTimeField HeaderText="Date/Time" DataField="HistoryDateTime" />
                            <Rock:EnumField HeaderText="Status" DataField="TimeCardStatus" />
                            <Rock:RockBoundField HeaderText="Person" DataField="StatusPersonAlias" />
                            <Rock:RockBoundField HeaderText="Notes" DataField="Notes" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>

                </asp:Panel>

            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('.badge').tooltip();

                $(".js-item-edit, .js-item-cancel, .gridresponsive-item-view").on("click", function (a) {

                    // prevent the .js-item-edit from bubbling up to .gridresponsive-item-view
                    a.stopImmediatePropagation();

                    var $parent = $(this).closest(".gridresponsive-item");
                    $parent.find(".gridresponsive-item-view").slideToggle();
                    $parent.find(".gridresponsive-item-edit").slideToggle();
                });


                $(".bootstrap-timepicker").timepicker().on('changeTime.timepicker', function (a, b, c) {

                    var $earnedHolidayBadge = $(this).closest('.js-time-edit-group').find('.js-earned-holiday-hours');

                    try {
                        var startTime = parseTime(null, $(this).closest('.js-time-edit-group').find("[id$='tpTimeIn']").val());
                        var lunchStartTime = parseTime(startTime, $(this).closest('.js-time-edit-group').find("[id$='tpLunchOut']").val());
                        var lunchEndTime = parseTime(startTime, $(this).closest('.js-time-edit-group').find("[id$='tpLunchIn']").val());
                        var endTime = parseTime(startTime, $(this).closest('.js-time-edit-group').find("[id$='tpTimeOut']").val());
                        var totalWorkedMS = 0;
                        if (!startTime) {
                            totalWorkedMS = 0;
                        }
                        else if (endTime) {
                            var lunchMS = lunchEndTime && lunchStartTime ? (lunchEndTime - lunchStartTime) : 0;
                            totalWorkedMS = (endTime - startTime) - lunchMS;
                        }
                        else if (lunchStartTime) {
                            totalWorkedMS = lunchStartTime - startTime;
                        }

                        // convert to hours and divide in half
                        var earnedHours = (totalWorkedMS / 1000 / 60 / 60 / 2);

                        // round to nearest .25
                        earnedHours = Math.round(earnedHours * 4) / 4;

                        if (earnedHours && $earnedHolidayBadge.data('is-holiday')) {
                            $earnedHolidayBadge.text("+ " + earnedHours.toFixed(2));
                        }
                        else {
                            $earnedHolidayBadge.text('');
                        }
                    }
                    catch (e) {
                        $earnedHolidayBadge.text("+ 50%");
                    }
                });
            });

            function parseTime(startTime, str) {
                if (!str)
                    return null;

                var d = new Date();
                var time = str.match(/(\d+)(?::(\d\d))?\s*(p?)/i);
                d.setHours(parseInt(time[1]) + ((parseInt(time[1]) < 12 && time[3]) ? 12 : 0));
                d.setMinutes(parseInt(time[2]) || 0);
                if (startTime && startTime > d) {
                    d.setDate(d.getDate() + 1);
                }
                return d;
            }

            // make the window scroll to the specified element.  use setTimeout to have it do it after any other scrolls that happen (like validation)
            function maintainScrollPosition($element) {
                setTimeout(function () {
                    var scrollPosition = $element.offset().top;
                    $(window).scrollTop(scrollPosition);
                }, 0)
            }

            // custom validate function to check if Checkbox is Checked=True
            function validateCheckbox(a, b) {
                b.IsValid = $('.js-agree-checkbox').prop('checked');
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
