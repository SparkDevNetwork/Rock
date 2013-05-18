<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleExample.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ScheduleExample" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <a href='#myModal' role='button' class='btn btn-small' data-toggle='modal'>
            <i class='icon-calendar'></i>
            Schedule
        </a>

        <div id='myModal' class='modal hide fade schedule-builder '>
            <div class='modal-header'>
                <asp:LinkButton ID='LinkButton1' runat='server' Text='&times;' CssClass='close modal-control-close' OnClick='btnCancelSchedule_Click' />
                <h3>Schedule Builder</h3>
            </div>
            <div class='modal-body'>
                <div id='modal-scroll-container' class='scroll-container '>
                    <div class='scrollbar'>
                        <div class='track'>
                            <div class='thumb'>
                                <div class='end'></div>
                            </div>
                        </div>
                    </div>
                    <div class='viewport'>
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
                                        <asp:RadioButton runat='server' GroupName='ScheduleTypeGroup' CssClass='schedule-type' ID='radrecurring' Text='Recurring' data-schedule-type='schedule-recurring' />
                                    </div>
                                </div>

                                <!-- recurrence panel -->
                                <div id='schedule-recurrence-panel' style='display: none;'>

                                    <legend class='legend-small'>Recurrence </legend>

                                    <div class='control-group'>
                                        <asp:Label ID='lblOccurrencePattern' runat='server' CssClass='control-label' Text='Occurrence Pattern' />
                                        <div class='controls'>
                                            <asp:RadioButton runat='server' GroupName='recurrence-pattern-radio' CssClass='recurrence-pattern-radio' ID='radSpecificDates' Text='Specific Dates' data-recurrence-pattern='recurrence-pattern-specific-date' Checked='true' />
                                            <asp:RadioButton runat='server' GroupName='recurrence-pattern-radio' CssClass='recurrence-pattern-radio' ID='radDaily' Text='Daily' data-recurrence-pattern='recurrence-pattern-daily' />
                                            <asp:RadioButton runat='server' GroupName='recurrence-pattern-radio' CssClass='recurrence-pattern-radio' ID='radWeekly' Text='Weekly' data-recurrence-pattern='recurrence-pattern-weekly' />
                                            <asp:RadioButton runat='server' GroupName='recurrence-pattern-radio' CssClass='recurrence-pattern-radio' ID='radMonthly' Text='Monthly' data-recurrence-pattern='recurrence-pattern-monthly' />
                                        </div>
                                    </div>

                                    <!-- specific date panel -->
                                    <div id='recurrence-pattern-specific-date' class='recurrence-pattern-type control-group controls'>
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

                                    <!-- daily recurrence -->
                                    <div id='recurrence-pattern-daily' class='recurrence-pattern-type' style='display: none;'>
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

                                    <!-- weekly recurrence -->
                                    <div id='recurrence-pattern-weekly' class='recurrence-pattern-type' style='display: none;'>
                                        <div class='control-group controls'>

                                            <!--not needed <input type='radio' name='weekly-options' id='Radio2' value='option1' checked> -->
                                            <span>Every</span>
                                            <Rock:NumberBox runat='server' ID='tbWeeklyEveryX' CssClass='input-mini' MinimumValue='1' MaximumValue='52' />
                                            <span>weeks on</span>
                                            <br>
                                        </div>
                                        <div class='control-group controls'>
                                            <asp:CheckBox runat="server" ID="cbWeeklySunday" Text="Sun" />
                                            <asp:CheckBox runat="server" ID="cbWeeklyMonday" Text="Mon" />
                                            <asp:CheckBox runat="server" ID="cbWeeklyTuesday" Text="Tue" />
                                            <asp:CheckBox runat="server" ID="cbWeeklyWednesday" Text="Wed" />
                                            <asp:CheckBox runat="server" ID="cbWeeklyThursday" Text="Thu" />
                                            <asp:CheckBox runat="server" ID="cbWeeklyFriday" Text="Fri" />
                                            <asp:CheckBox runat="server" ID="cbWeeklySaturday" Text="Sat" />
                                        </div>
                                    </div>

                                    <!-- monthly recurrence -->
                                    <div id='recurrence-pattern-monthly' class='recurrence-pattern-type' style='display: none;'>
                                        <div class='control-group controls'>
                                            <div>
                                                <asp:RadioButton runat="server" GroupName="monthly-options" ID="radMonthlyDayX" Checked="true" />
                                                <span>Day</span>
                                                <Rock:NumberBox runat='server' ID='tbMonthlyDayX' CssClass='input-mini' MinimumValue='1' MaximumValue='31' />
                                                <span>of every</span>
                                                <Rock:NumberBox runat='server' ID='tbMonthlyXMonths' CssClass='input-mini' MinimumValue='1' MaximumValue='12' />
                                                <span>months</span>
                                            </div>
                                            <div>
                                                <asp:RadioButton runat="server" GroupName="monthly-options" ID="radMonthlyNth" />
                                                <span>The</span>
                                                <asp:DropDownList runat="server" ID="ddlMonthlyNth" CssClass="input-small" >
                                                    <asp:ListItem Text="" Value="" />
                                                    <asp:ListItem Text="First" Value="1" />
                                                    <asp:ListItem Text="Second" Value="2" />
                                                    <asp:ListItem Text="Third" Value="3" />
                                                    <asp:ListItem Text="Fourth" Value="4" />
                                                    <asp:ListItem Text="Last" Value="5" />
                                                </asp:DropDownList>
                                                <asp:DropDownList runat="server" ID="ddlMonthlyDayName" CssClass="input-medium" >
                                                    <asp:ListItem Text="" Value="" />
                                                    <asp:ListItem Text="Sunday" Value="0" />
                                                    <asp:ListItem Text="Monday" Value="1" />
                                                    <asp:ListItem Text="Tuesday" Value="2" />
                                                    <asp:ListItem Text="Wednesday" Value="3" />
                                                    <asp:ListItem Text="Thursday" Value="4" />
                                                    <asp:ListItem Text="Friday" Value="5" />
                                                    <asp:ListItem Text="Saturday" Value="6" />
                                                </asp:DropDownList>
                                            </div>    
                                        </div>
                                    </div>

                                    <!-- end date panel -->
                                    <div class='controls'><hr /></div>
                                    <div class='control-group'>
                                        <label class='control-label'>Continue Until</label>
                                        <div class='controls '>
                                            <div>
                                                <asp:RadioButton runat="server" GroupName="end-by" ID="radEndByNone" Checked="true" />
                                                <span>No End</span>
                                            </div>
                                            <div>
                                                <asp:RadioButton runat="server" GroupName="end-by" ID="radEndByDate"  />
                                                <span>End by</span>
                                                <Rock:DatePicker runat='server' ID='dpEndBy' />
                                            </div>
                                            <div>
                                                <asp:RadioButton runat="server" GroupName="end-by" ID="radEndByOccurrenceCount"  />
                                                <span>End after</span>
                                                <Rock:NumberBox runat="server" ID="tbEndByOccurrenceCount" CssClass="input-mini" />
                                                <span>occurrences</span>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- exclusions panel -->
                                    <div class='controls'><hr /></div>
                                    <label class='control-label'>Exclusions</label>

                                    <div id='recurrence-pattern-exclusions' class='control-group controls'>
                                        <asp:HiddenField ID='hfExclusionDateRangeListValues' runat='server' ClientIDMode='Static' />
                                        <ul id='lstExclusionDateRanges'></ul>
                                        <a class='btn btn-small' id='add-exclusion-daterange'><i class='icon-plus'></i>
                                            <span> Add Date Range</span>
                                        </a>
                                        <div id='add-exclusion-daterange-group' style='display: none'>
                                            <Rock:DatePicker runat='server' ID='dpExclusionDateStart' ClientIDMode='Static' />
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

        <asp:ScriptManagerProxy runat="server">
            <Scripts>
                <asp:ScriptReference Path="~/Scripts/Rock/Rock.schedulebuilder.js" />
            </Scripts>
        </asp:ScriptManagerProxy>

    </ContentTemplate>
</asp:UpdatePanel>
