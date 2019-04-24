/* ====================================================== */
-- Finance First Time Givers Report

-- @StartDate and @EndDate can be set by Rock using query params.
-- If they are not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

SELECT
    p.Id AS PersonId,
	CONCAT(p.LastName, ', ', p.FirstName) AS ContributorName,
	p.Email,
	fb.Name AS BatchName,
	fap.PublicName AS Fund,
	c.Name AS Campus,
	fa.GlCode AS GeneralLedger,
	stv.Value AS OriginatingSource,
	CONVERT(DATE, ft.TransactionDateTime) AS RecievedDate,
	CONVERT(TIME, ft.TransactionDateTime) AS ReceivedTime,
	ctv.Value AS [Type],
	cctv.Value AS [BankCardType],
	ftd.Amount,
	ft.TransactionCode
FROM
	FinancialTransaction ft
	LEFT JOIN FinancialBatch fb ON ft.BatchId = fb.Id
	LEFT JOIN FinancialPaymentDetail pd ON ft.FinancialPaymentDetailId = pd.Id
	JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
	LEFT JOIN FinancialAccount fa ON fa.Id = ftd.AccountId
	LEFT JOIN FinancialAccount fap ON fap.Id = fa.ParentAccountId
	JOIN PersonAlias pa ON pa.Id = ft.AuthorizedPersonAliasId
	JOIN Person p ON p.Id = pa.PersonId
	LEFT JOIN DefinedValue stv ON stv.Id = ft.SourceTypeValueId
	LEFT JOIN DefinedValue ctv ON ctv.Id = pd.CurrencyTypeValueId
	LEFT JOIN DefinedValue cctv ON cctv.Id = pd.CreditCardTypeValueId
	LEFT JOIN Campus c ON c.Id = fa.CampusId
WHERE
	CONVERT(DATE, ft.TransactionDateTime) BETWEEN @reportStartDate AND @reportEndDate
ORDER BY
	ft.TransactionDateTime DESC

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
