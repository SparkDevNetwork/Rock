/* ====================================================== */
-- KidSpring Ratio Report

-- @Date can be set by Rock using a query param.
-- If not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

-- For testing in sql server management studio, uncomment here:
DECLARE @Date AS NVARCHAR(MAX) = '2016-04-17';

DECLARE @today AS DATE = GETDATE() - 1;
DECLARE @reportDate AS DATE = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today);

IF ISDATE(@Date) = 1
BEGIN
    SELECT @reportDate = @Date;
END;

WITH cteSchedules AS (
	SELECT
		Id AS ScheduleId,
		Name AS ScheduleName
	FROM
		Schedule s
	WHERE 
		Name LIKE 'Sunday [0-1]%'
), cteLocations AS (
	SELECT
		c.Id AS CampusId,
		c.Name AS CampusName, 
		l.Name AS LocationName,
		l.Id AS LocationId
	FROM 
		Location rl
		JOIN DefinedValue dv ON dv.Id = rl.LocationTypeValueId
		JOIN Location ksl ON ksl.ParentLocationId = rl.Id
		JOIN Location l ON ksl.Id = l.ParentLocationId
		JOIN Campus c ON rl.Name = c.Name
	WHERE
		rl.ParentLocationId IS NULL
		AND rl.Name <> 'Central'
		AND dv.Value = 'Campus'
		AND ksl.Name = 'KidSpring'
		AND l.Name NOT IN (
			'Check-In Volunteer', 
			'KidSpring Production',
			'Next Steps Childcare',
			'Sunday Support Volunteer',
			'Advocate',
			'First Time Team Volunteer',
			'Guest Services Area Leader',
			'Guest Services Service Leader',
			'KidSpring Greeter',
			'KidSpring Office Team',
			'Elementary Area Leader',
			'Elementary Early Bird Volunteer',
			'Elementary Production',
			'Elementary Production Service Leader',
			'Elementary Service Leader',
			'Load In',
			'Load Out',
			'New Serve Area Leader',
			'New Serve Team',
			'Nursery Early Bird Volunteer',
			'Preschool Area Leader',
			'Preschool Early Bird Volunteer',
			'Preschool Production',
			'Preschool Production Service Leader',
			'Preschool Service Leader',
			'Production Area Leader',
			'Production Service Leader',
			'Spring Zone Area Leader',
			'Spring Zone Service Leader',
			'Wonder Way Area Leader',
			'Wonder Way Service Leader',
			'CHANGE ROOM'
		)
), cteAttendances AS (
	SELECT
		Id, 
		LocationId, 
		ScheduleId
	FROM
		Attendance
	WHERE
		CONVERT(DATE, StartDateTime) = @reportDate
		AND DidAttend = 1
)
SELECT
	c.CampusName AS Campus,
	c.LocationName AS Location,
	s.ScheduleName AS [Service],
	COUNT(a.Id) AS Attendees
FROM
	cteLocations c
	CROSS JOIN cteSchedules s
	LEFT JOIN cteAttendances a ON a.ScheduleId = s.ScheduleId AND a.LocationId = c.LocationId
GROUP BY
	c.CampusId,
	c.CampusName,
	c.LocationId,
	c.LocationName,
	s.ScheduleId,
	s.ScheduleName
ORDER BY
	c.CampusName,
	c.LocationName,
	s.ScheduleId;

/*

<div class="form-group date-range-picker ">
    <label class="control-label" for="ctl00_main_ctl23_ctl01_ctl06_gfTransactions_drpDates">Date</label>
    <div id="ctl00_main_ctl23_ctl01_ctl06_gfTransactions_drpDates">
        <div class="form-control-group">
            <div class="input-group input-width-md date input-group-lower input-width-md js-date-picker date">
                <input name="drpDates_lower" type="text" id="drpDates_lower" class="form-control" value="{{ PageParameter.StartDate }}" />
                <span class="input-group-addon">
                    <i class="fa fa-calendar"></i>
                </span>
            </div>
        </div>
    </div>
</div>
<div class='actions margin-b-md'>
    <a id="aGo" class="btn btn-primary" href="javascript:showReport();">Go</a>
</div>

<script language='javascript'>
 
$(function() {
    $('#drpDates_lower').datepicker({ format: 'mm/dd/yy', todayHighlight: true }).on('changeDate', function (ev) {
        if (event && event.type == 'click') {
            $('#drpDates_lower').data('datepicker').hide();
            $('#drpDates_lower')[0].focus();
        }
    });
    
    $('.date-range-picker').find('.input-group-addon').on('click', function () {
        $(this).siblings('.form-control').select();
    });
    
    var getParameterByName = function (name) {
        var url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)");
        var results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    };
    
    window.showReport = function () {
        var url = [location.protocol, '//', location.host, location.pathname].join('');
        window.location = url + '?Date=' + $('#drpDates_lower').val();
    };
    
    $(document).ready(function () {
        var theDate = getParameterByName("Date");
    
        if(theDate) {
            $('#drpDates_lower').datepicker("setDate", theDate);
        }
    });
});

</script>

*/