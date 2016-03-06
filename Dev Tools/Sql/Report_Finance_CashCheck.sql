/* ====================================================== */
-- Finance Cash/Check Report

-- @StartDate and @EndDate can be set by Rock using query params.
-- If they are not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

-- For testing in sql server management studio, uncomment here:
DECLARE @StartDate AS NVARCHAR(MAX) = '2015-01-04';
DECLARE @EndDate AS NVARCHAR(MAX) = '2015-01-04';

DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

SELECT
	b.Id AS BatchId,
	b.Name AS Batch,
	pa.Name AS Fund,
	c.Name AS SubFund,
	a.GlCode AS GeneralLedger,
	CONVERT(DATE, b.BatchStartDateTime) AS ReceivedDate,
	SUM(td.Amount) AS Amount,
	dv.Value
FROM
	FinancialBatch b
	JOIN FinancialTransaction t ON b.Id = t.BatchId
	JOIN FinancialTransactionDetail td ON t.Id = td.TransactionId
	JOIN FinancialAccount a ON td.AccountId = a.Id
	JOIN FinancialAccount pa ON pa.Id = a.ParentAccountId
	JOIN Campus c ON a.CampusId = c.Id
	JOIN FinancialPaymentDetail pd ON t.FinancialPaymentDetailId = pd.Id
	JOIN DefinedValue dv ON dv.Id = pd.CurrencyTypeValueId
WHERE
	dv.Value IN ('Cash', 'Check')
	AND CONVERT(DATE, b.BatchStartDateTime) >= CONVERT(DATE, @reportStartDate)
	AND CONVERT(DATE, b.BatchStartDateTime) <= CONVERT(DATE, @reportEndDate)
GROUP BY
	b.Id,
	b.Name,
	CONVERT(DATE, b.BatchStartDateTime),
	a.Id,
	a.GlCode,
	pa.Id,
	pa.Name,
	c.Id,
	c.Name,
	dv.Id,
	dv.Value
ORDER BY
	dv.Value,
	CONVERT(DATE, b.BatchStartDateTime),
	b.Name;


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
