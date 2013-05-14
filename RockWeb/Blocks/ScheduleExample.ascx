<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleExample.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ScheduleExample" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <a href="#myModal" role="button" class="btn btn-small" data-toggle="modal">
            <i class="icon-calendar"></i>
            Schedule
        </a>

        <style>
            .modal-control {
                /* modal is left:50%, but might be getting ignored since most of our modals are in iframe's */
                left: 25%;
                /* override modal's background-color of grey, which always gets overridden in all our other modals by the iframe's body to be white again */
                background-color: white;
                /* override modal's z-index to be bigger than modal backdrop, but less than datepicker, so that datepicker is on top  */
                z-index: 1041;
            }
        </style>

        <div id="myModal" class="modal hide fade modal-control">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                <h3>Schedule Builder</h3>
            </div>
            <div class="modal-body">

                <!-- modal body -->
                <div class="form-horizontal">

                    <asp:ValidationSummary runat="server" />

                    <Rock:DateTimePicker runat="server" ID="dpStartDateTime" LabelText="Start Date / Time" />

                    <div class="control-group">
                        <asp:Label ID="lblDuration" runat="server" CssClass="control-label" Text="Duration" />
                        <div class="controls">
                            <Rock:NumberBox runat="server" ID="tbDurationHours" CssClass="input-mini" MinimumValue="0" MaximumValue="24" />
                            <asp:Label ID="lblDurationHours" runat="server" Text=" hrs " />
                            <Rock:NumberBox runat="server" ID="tbDurationMinutes" CssClass="input-mini" MinimumValue="0" MaximumValue="59" />
                            <asp:Label ID="lblDurationMinutes" runat="server" Text=" mins" />
                        </div>
                    </div>

                    <div class="control-group">
                        <div class="controls">
                            <asp:RadioButton runat="server" GroupName="ScheduleTypeGroup" CssClass="schedule-type" ID="radOneTime" Text="One Time" value="schedule-onetime" Checked="true" />
                            <asp:RadioButton runat="server" GroupName="ScheduleTypeGroup" CssClass="schedule-type" ID="radReoccurring" Text="Reoccurring" value="schedule-reoccurring" />
                        </div>
                    </div>

                    <!-- reoccurrence panel -->
                    <div id="schedule-reoccurrence-panel" style="display: none;">

                        <legend class="legend-small">Reoccurrence </legend>

                        <div class="control-group">
                            <asp:Label ID="lblOccurrencePattern" runat="server" CssClass="control-label" Text="Occurrence Pattern" />
                            <div class="controls">
                                <asp:RadioButton runat="server" GroupName="reoccurrence-pattern-radio" CssClass="reoccurrence-pattern-radio" ID="radSpecificDates" Text="Specific Dates" value="reoccurrence-pattern-specific-date" Checked="true" />
                                <asp:RadioButton runat="server" GroupName="reoccurrence-pattern-radio" CssClass="reoccurrence-pattern-radio" ID="radDaily" Text="Daily" value="reoccurrence-pattern-daily" />
                                <asp:RadioButton runat="server" GroupName="reoccurrence-pattern-radio" CssClass="reoccurrence-pattern-radio" ID="radWeekly" Text="Weekly" value="reoccurrence-pattern-weekly" />
                                <asp:RadioButton runat="server" GroupName="reoccurrence-pattern-radio" CssClass="reoccurrence-pattern-radio" ID="radMonthly" Text="Monthly" value="reoccurrence-pattern-monthly" />
                            </div>
                        </div>

                        <!-- specific date panel -->
                        <div id="reoccurrence-pattern-specific-date" class="reoccurrence-pattern-type control-group controls">
                            <input id="specific-date-list-values" type="hidden" value="4/23/2013,5/23/2013,6/23/2013,7/23/2013" />
                            <ul id="specific-date-list">
                                <li>
                                    <span>4/23/2013</span>
                                    <a id='A7' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>5/23/2013</span>
                                    <a id='A1' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>6/23/2013</span>
                                    <a id='A2' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>7/23/2013</span>
                                    <a id='A3' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                            </ul>
                            <a class="btn btn-small" id="add-specific-date"><i class="icon-plus"></i>
                                <asp:Label ID="Label9" runat="server" Text=" Add Date" />
                            </a>
                            <div id="add-specific-date-group" style="display: none">
                                <Rock:DatePicker runat="server" ID="dpSpecificDate" ClientIDMode="Static" SelectedDate="12/25/2013" />

                                <a class="btn btn-primary btn-mini" id="add-specific-date-ok"></i>
                                   <span>OK</span>
                                </a>
                                <a class="btn btn-mini" id="add-specific-date-cancel"></i>
                                   <span>Cancel</span>
                                </a>
                            </div>

                            <script>
                                // show datepicker, ok, cancel so that new date can be added to the list
                                $('#add-specific-date').click(function () {
                                    $('#add-specific-date').hide();
                                    $('#add-specific-date-group').show();
                                });

                                // add new date to list when ok is clicked
                                $('#add-specific-date-ok').click(function () {

                                    // get date list from hidden field
                                    var dateList = $('#specific-date-list-values').val().split(",");
                                    var newDate = $('#dpSpecificDate').val();

                                    // delete newDate from list in case it is already there
                                    var index = dateList.indexOf(newDate);
                                    if (index > 0) {
                                        dateList.splice(index, 1);
                                    }

                                    // add new date to list
                                    dateList.push(newDate);

                                    // save list back to hidden field
                                    $('#specific-date-list-values').val(dateList);

                                    // rebuild the UL
                                    $('#specific-date-list').children().remove();
                                    $.each(dateList, function (index, value) {
                                        // add to ul
                                        var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
                                        $('#specific-date-list').append(newLi);
                                    });

                                    $('#add-specific-date-group').hide();
                                    $('#add-specific-date').show();
                                });

                                // cancel out of adding a new date
                                $('#add-specific-date-cancel').click(function () {
                                    $('#add-specific-date-group').hide();
                                    $('#add-specific-date').show();
                                });

                                // fadeIn/fadeOut the X buttons to delete dates
                                $('#specific-date-list').hover(
                                    function () {
                                        $(this).find('li a').stop(true, true).show();
                                    },
                                    function () {
                                        $(this).find('li a').stop(true, true).fadeOut(500);
                                    }
                               );

                                // delete specific date from list
                                $('#specific-date-list').on('click', 'li a', function (event) {
                                    var selectedDate = $(this).siblings().text();

                                    // get date list from hidden field
                                    var dateList = $('#specific-date-list-values').val().split(",");

                                    // delete selectedDate
                                    var index = dateList.indexOf(selectedDate);
                                    if (index > 0) {
                                        dateList.splice(index, 1);
                                    }

                                    // save list back to hidden field
                                    $('#specific-date-list-values').val(dateList);

                                    // remove date from ul list
                                    var liItem = $(this).parent();
                                    liItem.remove();
                                });
                            </script>
                        </div>

                        <!-- daily reoccurence -->
                        <div id="reoccurrence-pattern-daily" class="reoccurrence-pattern-type" style="display: none;">

                            <div class="control-group">

                                <div class="controls">
                                    <div>
                                        <input type="radio" name="daily-options" id="optionsRadios1" value="option1" checked>
                                        Every
                                        <input type="text" class="input-mini" id="Text4" placeholder="">
                                        days
                                    </div>
                                    <div>
                                        <input type="radio" name="daily-options" id="optionsRadios2" value="option2">
                                        Every weekday
                                    </div>
                                    <div>
                                        <input type="radio" name="daily-options" id="Radio1" value="option2">
                                        Every weekend day
                                    </div>
                                </div>
                            </div>

                        </div>

                        <!-- weekly reoccurence -->
                        <div id="reoccurrence-pattern-weekly" class="reoccurrence-pattern-type" style="display: none;">

                            <div class="control-group">

                                <div class="controls">
                                    <div>
                                        <input type="radio" name="weekly-options" id="Radio2" value="option1" checked>
                                        Every
                                        <input type="text" class="input-mini" id="Text5" placeholder="">
                                        weeks on
                                        <br>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="inlineCheckbox1" value="option1">
                                            Sun
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="Checkbox1" value="option1">
                                            Mon
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="inlineCheckbox2" value="option2">
                                            Tue
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="inlineCheckbox3" value="option3">
                                            Wed
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="Checkbox2" value="option3">
                                            Thu
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="Checkbox3" value="option3">
                                            Fri
                                        </label>
                                        <label class="checkbox inline">
                                            <input type="checkbox" id="Checkbox4" value="option1">
                                            Sat
                                        </label>
                                    </div>

                                </div>
                            </div>

                        </div>

                        <!-- monthly reoccurence -->
                        <div id="reoccurrence-pattern-monthly" class="reoccurrence-pattern-type" style="display: none;">

                            <div class="control-group">

                                <div class="controls">
                                    <div>
                                        <input type="radio" name="daily-options" id="Radio3" value="option1" checked>
                                        Day
                                        <input type="text" class="input-mini" id="Text6" placeholder="">
                                        of every
                                        <input type="text" class="input-mini" id="Text7" placeholder="">
                                        months
                                    </div>
                                    <div>
                                        <input type="radio" name="daily-options" id="Radio4" value="option2">
                                        The 
															<select class="input-small">
                                                                <option></option>
                                                                <option>First</option>
                                                                <option>Second</option>
                                                                <option>Third</option>
                                                                <option>Fourth</option>
                                                                <option>Last</option>
                                                            </select>

                                        <select class="input-medium">
                                            <option></option>
                                            <option>Sunday</option>
                                            <option>Monday</option>
                                            <option>Tuesday</option>
                                            <option>Wednesday</option>
                                            <option>Thursday</option>
                                            <option>Friday</option>
                                            <option>Saturday</option>
                                        </select>
                                    </div>
                                </div>
                            </div>

                        </div>

                        <div>

                            <div class="controls">
                                <hr />
                            </div>
                        </div>

                        <!-- end date panel -->
                        <div class="control-group">
                            <label class="control-label" for="inputEmail">Continue Until</label>
                            <div class="controls">
                                <div>
                                    <input type="radio" name="end-by" id="Radio5" value="option1" checked>
                                    No End
                                </div>
                                <div>
                                    <input type="radio" name="end-by" id="Radio6" value="option2">
                                    End by
                                    <input type="text" class="input-small" id="Text8" placeholder="">
                                </div>
                                <div>
                                    <input type="radio" name="end-by" id="Radio7" value="option2">
                                    End after
                                    <input type="text" class="input-mini" id="Text9" placeholder="">
                                    occurrences
                                </div>
                            </div>
                        </div>

                        <!-- exclusions panel -->
                        <div>

                            <div class="controls">
                                <hr />
                            </div>
                        </div>

                        <label class="control-label">Exclusions</label>

                        <div id="reoccurrence-pattern-exclusions" class="reoccurrence-pattern-type control-group controls">

                            <input id="exclusion-daterange-list-values" type="hidden" value="4/23/2013 - 5/23/2014,8/23/2013 - 9/23/2014,11/23/2013 - 5/23/2015,3/23/2013 - 3/24/2014" />
                            <ul id="exclusion-daterange-list">
                                <li>
                                    <span>4/23/2013 - 5/23/2014</span>
                                    <a id='A12' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>8/23/2013 - 9/23/2014</span>
                                    <a id='A13' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>11/23/2013 - 5/23/2015</span>
                                    <a id='A14' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                                <li>
                                    <span>3/23/2013 - 3/24/2014</span>
                                    <a id='A15' href='#' style="display: none"><i class='icon-remove'></i></a>
                                </li>
                            </ul>
                            <a class="btn btn-small" id="add-exclusion-daterange"><i class="icon-plus"></i>
                                <asp:Label ID="Label6" runat="server" Text=" Add Date Range" />
                            </a>
                            <div id="add-exclusion-daterange-group" style="display: none">
                                <Rock:DatePicker runat="server" ID="dpExclusionDateStart" ClientIDMode="Static" SelectedDate="12/25/2013" />
                                <span>to</span>
                                <Rock:DatePicker runat="server" ID="dpExclusionDateEnd" ClientIDMode="Static" SelectedDate="12/26/2013" />

                                <a class="btn btn-primary btn-mini" id="add-exclusion-daterange-ok"></i>
                                    <span>OK</span>
                                </a>
                                <a class="btn btn-mini" id="add-exclusion-daterange-cancel"></i>
                                    <span>Cancel</span>
                                </a>
                            </div>

                            <script>
                                // show daterangepicker, ok, cancel so that new daterange can be added to the list
                                $('#add-exclusion-daterange').click(function () {
                                    $('#add-exclusion-daterange').hide();
                                    $('#add-exclusion-daterange-group').show();
                                });

                                // add new date to list when ok is clicked
                                $('#add-exclusion-daterange-ok').click(function () {

                                    // get date list from hidden field
                                    var dateList = $('#exclusion-daterange-list-values').val().split(",");
                                    var newDateRange = $('#dpExclusionDateStart').val() + ' - ' + $('#dpExclusionDateEnd').val();

                                    // delete newDateRange from list in case it is already there
                                    var index = dateList.indexOf(newDateRange);
                                    if (index > 0) {
                                        dateList.splice(index, 1);
                                    }

                                    // add new daterange to list
                                    dateList.push(newDateRange);

                                    // save list back to hidden field
                                    $('#exclusion-daterange-list-values').val(dateList);

                                    // rebuild the UL
                                    $('#exclusion-daterange-list').children().remove();
                                    $.each(dateList, function (index, value) {
                                        // add to ul
                                        var newLi = "<li><span>" + value + "</span><a href='#' style='display: none'><i class='icon-remove'></i></a></li>";
                                        $('#exclusion-daterange-list').append(newLi);
                                    });

                                    $('#add-exclusion-daterange-group').hide();
                                    $('#add-exclusion-daterange').show();
                                });

                                // cancel out of adding a new daterange
                                $('#add-exclusion-daterange-cancel').click(function () {
                                    $('#add-exclusion-daterange-group').hide();
                                    $('#add-exclusion-daterange').show();
                                });

                                // fadeIn/fadeOut the X buttons to delete dateranges
                                $('#exclusion-daterange-list').hover(
                                    function () {
                                        $(this).find('li a').stop(true, true).show();
                                    },
                                    function () {
                                        $(this).find('li a').stop(true, true).fadeOut(500);
                                    }
                               );

                                // delete daterange from list
                                $('#exclusion-daterange-list').on('click', 'li a', function (event) {
                                    var selectedDateRange = $(this).siblings().text();

                                    // get daterange list from hidden field
                                    var dateRangeList = $('#exclusion-daterange-list-values').val().split(",");

                                    // delete selectedDateRange
                                    var index = dateRangeList.indexOf(selectedDateRange);
                                    if (index > 0) {
                                        dateRangeList.splice(index, 1);
                                    }

                                    // save list back to hidden field
                                    $('#exclusion-daterange-list-values').val(dateRangeList);

                                    // remove daterange from ul list
                                    var liItem = $(this).parent();
                                    liItem.remove();
                                });
                            </script>
                        </div>

                    </div>

                </div>

            </div>
            <div class="modal-footer">
                <a href="#" class="btn" data-dismiss="modal">Cancel</a>
                <a href="#" class="btn btn-primary">Save Schedule</a>
            </div>
        </div>

        <style type="text/css">
            .legend-small {
                font-size: 14px;
                line-height: 26px;
            }

            div.controls div {
                padding-bottom: 5px;
            }
        </style>

        <script>
            $('.schedule-type').click(function () {
                var reoccurrenceState = $('input[class=schedule-type]:checked').val();

                if (reoccurrenceState == 'schedule-onetime') {
                    $('#schedule-reoccurrence-panel').slideUp();
                } else {
                    $('#schedule-reoccurrence-panel').slideDown();
                }

            });

            $('.reoccurrence-pattern-radio').click(function () {

                var reoccurrencePattern = '#' + $('input[name=reoccurrence-pattern-radio]:checked').val();

                if ($(reoccurrencePattern).css('display') == 'none') {

                    $('.reoccurrence-pattern-type').slideUp();
                    $(reoccurrencePattern).slideDown();
                }
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
