<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleExample.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ScheduleExample" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <a href='#myModal' role='button' class='btn btn-small' data-toggle='modal'>
            <i class='icon-calendar'></i>
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

        <div id='myModal' class='modal hide fade modal-control'>
            <div class='modal-header'>
                <asp:LinkButton ID='LinkButton1' runat='server' Text='&times;' CssClass='close modal-control-close' OnClick='btnCancelSchedule_Click' />
                <h3>Schedule Builder</h3>
            </div>
            <div class='modal-body'>
                <div id='modal-scroll-container' class='scroll-container scroll-container-picker'>
                    <div class='scrollbar'>
                        <div class='track'>
                            <div class='thumb'>
                                <div class='end'></div>
                            </div>
                        </div>
                    </div>
                    <div class='viewport' style='height: auto; min-height: 400px'>
                        <div class='overview'>

                            <!-- modal body -->
                            <div class='form-horizontal'>

                                <Rock:DateTimePicker runat='server' ID='dpStartDateTime' LabelText='Start Date / Time' />

                                <div class='control-group'>
                                    <asp:Label ID='lblDuration' runat='server' CssClass='control-label' Text='Duration' />
                                    <div class='controls'>
                                        <Rock:NumberBox runat='server' ID='tbDurationHours' CssClass='input-mini' MinimumValue='0' MaximumValue='24' />
                                        <asp:Label ID='lblDurationHours' runat='server' Text=' hrs ' />
                                        <Rock:NumberBox runat='server' ID='tbDurationMinutes' CssClass='input-mini' MinimumValue='0' MaximumValue='59' />
                                        <asp:Label ID='lblDurationMinutes' runat='server' Text=' mins' />
                                    </div>
                                </div>

                                <div class='control-group'>
                                    <div class='controls'>
                                        <asp:RadioButton runat='server' GroupName='ScheduleTypeGroup' CssClass='schedule-type' ID='radOneTime' Text='One Time' data-schedule-type='schedule-onetime' Checked='true' />
                                        <asp:RadioButton runat='server' GroupName='ScheduleTypeGroup' CssClass='schedule-type' ID='radReoccurring' Text='Reoccurring' data-schedule-type='schedule-reoccurring' />
                                    </div>
                                </div>

                                <!-- reoccurrence panel -->
                                <div id='schedule-reoccurrence-panel' style='display: none;'>

                                    <legend class='legend-small'>Reoccurrence </legend>

                                    <div class='control-group'>
                                        <asp:Label ID='lblOccurrencePattern' runat='server' CssClass='control-label' Text='Occurrence Pattern' />
                                        <div class='controls'>
                                            <asp:RadioButton runat='server' GroupName='reoccurrence-pattern-radio' CssClass='reoccurrence-pattern-radio' ID='radSpecificDates' Text='Specific Dates' data-reoccurrence-pattern='reoccurrence-pattern-specific-date' Checked='true' />
                                            <asp:RadioButton runat='server' GroupName='reoccurrence-pattern-radio' CssClass='reoccurrence-pattern-radio' ID='radDaily' Text='Daily' data-reoccurrence-pattern='reoccurrence-pattern-daily' />
                                            <asp:RadioButton runat='server' GroupName='reoccurrence-pattern-radio' CssClass='reoccurrence-pattern-radio' ID='radWeekly' Text='Weekly' data-reoccurrence-pattern='reoccurrence-pattern-weekly' />
                                            <asp:RadioButton runat='server' GroupName='reoccurrence-pattern-radio' CssClass='reoccurrence-pattern-radio' ID='radMonthly' Text='Monthly' data-reoccurrence-pattern='reoccurrence-pattern-monthly' />
                                        </div>
                                    </div>

                                    <!-- specific date panel -->
                                    <div id='reoccurrence-pattern-specific-date' class='reoccurrence-pattern-type control-group controls'>
                                        <asp:HiddenField runat='server' ID='hfSpecificDateListValues' ClientIDMode='Static' />
                                        <ul id='lstSpecificDates'>
                                        </ul>
                                        <a class='btn btn-small' id='add-specific-date'><i class='icon-plus'></i>
                                            <asp:Label ID='lblAddSpecificDate' runat='server' Text=' Add Date' />
                                        </a>
                                        <div id='add-specific-date-group' style='display: none'>
                                            <Rock:DatePicker runat='server' ID='dpSpecificDate' ClientIDMode='Static' />

                                            <a class='btn btn-primary btn-mini' id='add-specific-date-ok'></i>
                                                <span>OK</span>
                                            </a>
                                            <a class='btn btn-mini' id='add-specific-date-cancel'></i>
                                                <span>Cancel</span>
                                            </a>
                                        </div>
                                    </div>

                                    <!-- daily reoccurence -->
                                    <div id='reoccurrence-pattern-daily' class='reoccurrence-pattern-type' style='display: none;'>
                                        <div class='control-group'>
                                            <div class='controls'>
                                                <div>
                                                    <asp:RadioButton ID='radDailyEveryXDays' runat='server' GroupName='daily-options' />
                                                    <span>Every </span>
                                                    <asp:TextBox ID='txtDailyEveryXDays' runat='server' CssClass='input-mini' />
                                                    <span>days</span>
                                                </div>
                                                <div>
                                                    <asp:RadioButton ID='radDailyEveryWeekday' runat='server' GroupName='daily-options' />
                                                    Every weekday
                                                </div>
                                                <div>
                                                    <asp:RadioButton ID='radDailyEveryWeekendDay' runat='server' GroupName='daily-options' />
                                                    Every weekend day
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- weekly reoccurence -->
                                    <div id='reoccurrence-pattern-weekly' class='reoccurrence-pattern-type' style='display: none;'>
                                        <div class='control-group'>
                                            <div class='controls'>
                                                <div>
                                                    <input type='radio' name='weekly-options' id='Radio2' value='option1' checked>
                                                    Every
                                        <input type='text' class='input-mini' id='Text5' placeholder=''>
                                                    weeks on
                                        <br>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='inlineCheckbox1' value='option1'>
                                                        Sun
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='Checkbox1' value='option1'>
                                                        Mon
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='inlineCheckbox2' value='option2'>
                                                        Tue
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='inlineCheckbox3' value='option3'>
                                                        Wed
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='Checkbox2' value='option3'>
                                                        Thu
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='Checkbox3' value='option3'>
                                                        Fri
                                                    </label>
                                                    <label class='checkbox inline'>
                                                        <input type='checkbox' id='Checkbox4' value='option1'>
                                                        Sat
                                                    </label>
                                                </div>

                                            </div>
                                        </div>
                                    </div>

                                    <!-- monthly reoccurence -->
                                    <div id='reoccurrence-pattern-monthly' class='reoccurrence-pattern-type' style='display: none;'>

                                        <div class='control-group'>

                                            <div class='controls'>
                                                <div>
                                                    <input type='radio' name='daily-options' id='Radio3' value='option1' checked>
                                                    Day
                                        <input type='text' class='input-mini' id='Text6' placeholder=''>
                                                    of every
                                        <input type='text' class='input-mini' id='Text7' placeholder=''>
                                                    months
                                                </div>
                                                <div>
                                                    <input type='radio' name='daily-options' id='Radio4' value='option2'>
                                                    The 
															<select class='input-small'>
                                                                <option></option>
                                                                <option>First</option>
                                                                <option>Second</option>
                                                                <option>Third</option>
                                                                <option>Fourth</option>
                                                                <option>Last</option>
                                                            </select>

                                                    <select class='input-medium'>
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
                                        <div class='controls'>
                                            <hr />
                                        </div>
                                    </div>

                                    <!-- end date panel -->
                                    <div class='control-group'>
                                        <label class='control-label'>Continue Until</label>
                                        <div class='controls end-date-options'>
                                            <div>
                                                <input type='radio' name='end-by' id='Radio5' value='no-end' checked>
                                                No End
                                            </div>
                                            <div>
                                                <input type='radio' name='end-by' id='Radio6' value='end-by'>
                                                End by
                                    <Rock:DatePicker runat='server' ID='dpEndBy' />
                                            </div>
                                            <div>
                                                <input type='radio' name='end-by' id='Radio7' value='end-after'>
                                                End after
                                    <input type='text' class='input-mini' id='Text9' placeholder=''>
                                                occurrences
                                            </div>
                                        </div>
                                    </div>

                                    <!-- exclusions panel -->
                                    <div>

                                        <div class='controls'>
                                            <hr />
                                        </div>
                                    </div>

                                    <label class='control-label'>Exclusions</label>

                                    <div id='reoccurrence-pattern-exclusions' class='control-group controls'>
                                        <asp:HiddenField ID='hfExclusionDateRangeListValues' runat='server' ClientIDMode='Static' />
                                        <ul id='lstExclusionDateRanges'>
                                        </ul>
                                        <a class='btn btn-small' id='add-exclusion-daterange'><i class='icon-plus'></i>
                                            <asp:Label ID='Label6' runat='server' Text=' Add Date Range' />
                                        </a>
                                        <div id='add-exclusion-daterange-group' style='display: none'>
                                            <Rock:DatePicker runat='server' ID='dpExclusionDateStart' ClientIDMode='Static' CssClass='input-mini' />
                                            <span>to</span>
                                            <Rock:DatePicker runat='server' ID='dpExclusionDateEnd' ClientIDMode='Static' />

                                            <a class='btn btn-primary btn-mini' id='add-exclusion-daterange-ok'></i>
                                    <span>OK</span>
                                            </a>
                                            <a class='btn btn-mini' id='add-exclusion-daterange-cancel'></i>
                                    <span>Cancel</span>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class='modal-footer'>
                <asp:LinkButton ID='btnCancelSchedule' runat='server' Text='Cancel' CssClass='btn modal-control-close' OnClick='btnCancelSchedule_Click' />
                <asp:LinkButton ID='btnSaveSchedule' runat='server' Text='Save Schedule' CssClass='btn btn-primary modal-control-close' OnClick='btnSaveSchedule_Click' />
            </div>
        </div>

        <style type='text/css'>
            .legend-small {
                font-size: 14px;
                line-height: 26px;
            }

            div.controls div {
                padding-bottom: 5px;
            }
        </style>

        <asp:ScriptManagerProxy runat="server">
            <Scripts>
                <asp:ScriptReference Path="~/Scripts/Rock/Rock.schedulebuilder.js" />
            </Scripts>
        </asp:ScriptManagerProxy>

    </ContentTemplate>
</asp:UpdatePanel>
