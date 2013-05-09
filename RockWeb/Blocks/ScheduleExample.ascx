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
                        <div id="reoccurrence-pattern-specific-date" class="reoccurrence-pattern-type">
                            <Rock:DatePicker runat="server" ID="dpAddSpecificDate" />

                            <div class="controls">
                                <ul>
                                    <li>2/13/2013</li>
                                    <li>3/13/2013</li>
                                </ul>

                                <a class="btn btn-small"><i class="icon-plus"></i>Add Date</a>
                            </div>
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

                        <div class="control-group">
                            <label class="control-label" for="inputEmail">Exclusions</label>
                            <div class="controls">
                                <ul>
                                    <li>2/13/2013 - 2/27/2013</li>
                                    <li>3/13/2013 - 3/13/2013</li>
                                </ul>

                                <a class="btn btn-small"><i class="icon-plus"></i>Add Date Range</a>
                            </div>
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
