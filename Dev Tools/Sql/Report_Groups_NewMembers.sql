/* ====================================================== */
-- New Group Members for a given timeframe

-- @StartDate and @EndDate can be set by Rock using query params.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

-- For testing in sql server management studio, uncomment here:
--DECLARE @groupId AS NVARCHAR(MAX) = '-1';
--DECLARE @StartDate AS NVARCHAR(MAX) = '2016-03-6';
--DECLARE @EndDate AS NVARCHAR(MAX) = '2016-03-13';

DECLARE @rootGroupId AS INT = NULL;

IF @groupId <> '-1' AND LEN(@groupId) > 0
BEGIN
	SELECT @rootGroupId = @groupId;
END;

IF ISDATE(@StartDate) <> 1 AND ISDATE(@EndDate) <> 1
BEGIN
    SELECT 'Please pick a date range' AS [Message];
	RETURN;
END;

WITH GroupsUnderRoot(Id, Name, ParentGroupId) AS (
	SELECT
		Id, Name, ParentGroupId
	FROM
		[Group]
	WHERE
		((@rootGroupId IS NULL AND ParentGroupId IS NULL) OR Id = @rootGroupId)
		AND IsSystem = 0
		AND IsSecurityRole = 0
		AND GroupTypeId NOT IN (1, 11, 12)

	UNION ALL

	SELECT
		g.Id, g.Name, g.ParentGroupId
	FROM
		[Group] g
		INNER JOIN GroupsUnderRoot a ON a.Id = g.ParentGroupId
	WHERE
		IsSystem = 0
		AND IsSecurityRole = 0
		AND GroupTypeId NOT IN (1, 11, 12)
)
SELECT 
	gm.PersonId,
	p.FirstName,
	p.LastName,
	p.Email,
	g.Name AS GroupName,
	gm.CreatedDateTime,
	CONCAT(l.Street1, CASE WHEN l.Street2 IS NOT NULL AND LEN(l.Street2) > 0 THEN '<br />' END, l.Street2, '<br />', l.City, ', ', l.[State], ' ', l.PostalCode) AS [Address],
	hpn.NumberFormatted AS HomePhone,
	mpn.NumberFormatted AS MobilePhone,
	fc.Name AS Campus
FROM 
	GroupsUnderRoot g
	INNER JOIN GroupMember gm ON g.Id = gm.GroupId
	INNER JOIN Person p ON p.Id = gm.PersonId
	LEFT JOIN [GroupMember] fm ON fm.PersonId = p.Id
	LEFT JOIN [Group] f ON f.Id = fm.GroupId
	LEFT JOIN [GroupLocation] gl ON f.Id = gl.GroupId
	LEFT JOIN Location l ON l.Id = gl.LocationId
	LEFT JOIN PhoneNumber hpn ON hpn.PersonId = p.Id
	LEFT JOIN PhoneNumber mpn ON mpn.PersonId = p.Id
	LEFT JOIN Campus fc ON fc.Id = f.CampusId
WHERE
	CONVERT(DATE, gm.CreatedDateTime) BETWEEN @StartDate AND @EndDate
	AND p.IsDeceased = 0
	AND f.GroupTypeId = 10
	AND gl.IsMailingLocation = 1
	AND l.IsActive = 1
	AND gl.GroupLocationTypeValueId = 19
	AND hpn.NumberTypeValueId = 13
	AND mpn.NumberTypeValueId = 12
ORDER BY 
	gm.CreatedDateTime DESC,
	p.LastName,
	p.FirstName;

/*
-- HTML Content block to mimic filters in Rock
<div class="form-group date-range-picker ">
    <label class="control-label" for="ctl00_main_ctl23_ctl01_ctl06_gfTransactions_drpDates">Date Range</label>
    <div id="ctl00_main_ctl23_ctl01_ctl06_gfTransactions_drpDates">
        <div class="form-control-group">
            <div class="input-group input-width-md date input-group-lower input-width-md js-date-picker date">
                <input name="drpDates_lower" type="text" id="drpDates_lower" class="form-control" value="{{ PageParameter.StartDate }}" />
                <span class="input-group-addon">
                    <i class="fa fa-calendar"></i>
                </span>
            </div>
            <div class="input-group form-control-static">
                to
            </div>
            <div class="input-group input-width-md date input-group-upper input-width-md js-date-picker date">
                <input name="drpDates_upper" type="text" id="drpDates_upper" class="form-control" value="{{ PageParameter.EndDate }}">
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
        if (ev.date.valueOf() > $('#drpDates_upper').data('datepicker').dates[0]) {
            var newDate = new Date(ev.date)
            newDate.setDate(newDate.getDate() + 1);
            $('#drpDates_upper').datepicker('update', newDate);
            $('#drpDates_upper').datepicker('setStartDate', ev.date);
        }
        if (event && event.type == 'click') {
            $('#drpDates_lower').data('datepicker').hide();
            $('#drpDates_lower')[0].focus();
        }
    });
    $('#drpDates_upper').datepicker({ format: 'mm/dd/yy', todayHighlight: true }).on('changeDate', function (ev) {
        $('#drpDates_upper').data('datepicker').hide();
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
        window.location = url + '?StartDate=' + $('#drpDates_lower').val() + '&EndDate=' + $('#drpDates_upper').val();
    };
    $(document).ready(function () {
        var start = getParameterByName("StartDate");
        var end = getParameterByName("EndDate");
        if(start) {
            $('#drpDates_lower').datepicker("setDate", start);
        }
        if(end) {
            $('#drpDates_upper').datepicker("setDate", end);
        }
    });
});
</script>
*/