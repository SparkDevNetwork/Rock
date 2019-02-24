/* ====================================================== */
-- Finance First Time Givers Report

-- @StartDate and @EndDate can be set by Rock using query params.
-- If they are not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

-- For testing in sql server management studio, uncomment here:
DECLARE @StartDate AS NVARCHAR(MAX) = '2016-03-13';
DECLARE @EndDate AS NVARCHAR(MAX) = '2016-03-13';

DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

-- GivingIds in this set of transactions
IF object_id('tempdb..#givingIds') IS NOT NULL
BEGIN
	drop table #givingIds
END

SELECT 
	p.GivingId,
	ft.Id AS TransactionId
INTO #givingIds
FROM
	FinancialTransaction ft
	JOIN PersonAlias pa ON ft.AuthorizedPersonAliasId = pa.Id
	JOIN Person p ON pa.PersonId = p.Id
WHERE
	CONVERT(DATE, ft.TransactionDateTime) BETWEEN @reportStartDate AND @reportEndDate;

-- Filter ids that gave before
DELETE FROM #givingIds WHERE GivingId IN (
	SELECT 
		p.GivingId
	FROM
		FinancialTransaction ft
		JOIN PersonAlias pa ON ft.AuthorizedPersonAliasId = pa.Id
		JOIN Person p ON pa.PersonId = p.Id
	WHERE
		CONVERT(DATE, ft.TransactionDateTime) < @reportStartDate
);

SELECT 
	ft.TransactionDateTime,
	p.Id AS PersonId,
	gid.TransactionId,
	CONVERT(DATE, ft.TransactionDateTime) AS TransactionDate,
	CONCAT(p.FirstName, ' ', p.LastName) AS ContributorName,
	p.Email,
	l.Street1,
	l.Street2,
	l.City,
	l.[State],
	l.PostalCode,
	fap.PublicName AS Fund,
	fa.PublicName AS SubFund,
	c.Name AS Campus,
	ftd.Amount
FROM 
	#givingIds gid
	JOIN FinancialTransaction ft ON ft.Id = gid.TransactionId
	JOIN FinancialTransactionDetail ftd ON ft.Id = ftd.TransactionId
	JOIN PersonAlias pa ON pa.Id = ft.AuthorizedPersonAliasId
	JOIN Person p ON pa.PersonId = p.Id
	LEFT JOIN FinancialAccount fa ON fa.Id = ftd.AccountId
	LEFT JOIN FinancialAccount fap ON fap.Id = fa.ParentAccountId
	LEFT JOIN GroupMember gm ON gm.PersonId = p.Id
	LEFT JOIN [Group] g ON g.Id = gm.GroupId
	LEFT JOIN GroupLocation gl ON gl.GroupId = g.Id
	LEFT JOIN Location l ON l.Id = gl.LocationId
	LEFT JOIN Campus c ON c.Id = fa.CampusId
WHERE
	(g.Id IS NULL OR g.GroupTypeId = 10)
	AND (g.Id IS NULL OR g.IsActive = 1)
	AND (gl.Id IS NULL OR gl.IsMailingLocation = 1)
	AND (gl.Id IS NULL OR gl.GroupLocationTypeValueId = 19)
ORDER BY
	CONVERT(DATE, ft.TransactionDateTime) DESC,
	ftd.Amount DESC;

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
