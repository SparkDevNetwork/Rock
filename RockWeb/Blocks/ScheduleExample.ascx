<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleExample.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ScheduleExample" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <a href="#myModal" role="button" class="btn btn-small" data-toggle="modal">
            <i class="icon-calendar"></i>
            Schedule
        </a>

        <div id="myModal" class="modal hide fade" style="left:25%;" >
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                <h3>Schedule Builder</h3>
            </div>
            <div class="modal-body">

                <!-- modal body -->
                <div class="form-horizontal">

                    <div class="control-group">
                        <label class="control-label" for="inputEmail">Start Date / Time</label>
                        <div class="controls">
                            <input type="text" class="input-small" id="inputEmail" placeholder="">
                            <input type="text" class="input-small" id="Text1" placeholder="">
                        </div>
                    </div>

                    <div class="control-group">
                        <label class="control-label" for="inputEmail">Duration</label>
                        <div class="controls">
                            <input type="text" class="input-mini" id="Text2" placeholder="">
                            hrs
											<input type="text" class="input-mini" id="Text3" placeholder="">
                            mins
                        </div>
                    </div>

                    <div class="control-group">
                        <div class="controls">
                            <label class="radio inline">
                                <input type="radio" name="type" class="schedule-type" id="schedule-onetime" value="schedule-onetime" checked>
                                One Time
                            </label>
                            <label class="radio inline">
                                <input type="radio" name="type" class="schedule-type" id="schedule-reoccurring" value="schedule-reoccurring">
                                Reoccurring
                            </label>
                        </div>
                    </div>


                    <!-- reoccurrence panel -->
                    <div id="schedule-reoccurrence-panel" style="display: none;">

                        <legend class="legend-small">Reoccurrence </legend>



                        <div class="control-group">
                            <label class="control-label" for="inputEmail">Occurrence Pattern</label>
                            <div class="controls">
                                <label class="radio inline">
                                    <input type="radio" name="reoccurrence-pattern-radio" class="reoccurrence-pattern-radio" id="reoccurrence-pattern-specific-radio" value="reoccurrence-pattern-specific-date" checked>
                                    Specific Dates
                                </label>
                                <label class="radio inline">
                                    <input type="radio" name="reoccurrence-pattern-radio" class="reoccurrence-pattern-radio" id="reoccurrence-pattern-daily-radio" value="reoccurrence-pattern-daily">
                                    Daily
                                </label>
                                <label class="radio inline">
                                    <input type="radio" name="reoccurrence-pattern-radio" class="reoccurrence-pattern-radio" id="reoccurrence-pattern-weekly-radio" value="reoccurrence-pattern-weekly">
                                    Weekly
                                </label>
                                <label class="radio inline">
                                    <input type="radio" name="reoccurrence-pattern-radio" class="reoccurrence-pattern-radio" id="reoccurrence-pattern-monthly-radio" value="reoccurrence-pattern-monthly">
                                    Monthly
                                </label>
                            </div>
                        </div>

                        <!-- specific date panel -->
                        <div id="reoccurrence-pattern-specific-date" class="reoccurrence-pattern-type">

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
                <a href="#" class="btn btn-primary">Save Schedule</a>
                <a href="#" class="btn">Cancel</a>

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
