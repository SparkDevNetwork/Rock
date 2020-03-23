-- change for each environment:
DECLARE @workflowTypeId int = 9999;  -- workflow type that sends email
DECLARE @currencyTypes table (id int);
INSERT @currencyTypes(id) values (6),(9),(763);  

-- lava assigns from pageparameter workflow/block
{% assign daysBack = 'Global' | PageParameter:'DaysWithoutGiving' %}
{% assign startDate = 'Global' | PageParameter:'StartDate' %}
{% assign endDate = 'Global' | PageParameter:'EndDate' %}
{% assign givenOnline = 'Global' | PageParameter:'GivenOnline' %}
{% assign threshold = 'Global' | PageParameter:'Threshold' %}
{% assign connectionStatus = 'Global' | PageParameter:'ConnectionStatus' %}

--Variable Declares
DECLARE @daysBack int = 7;
DECLARE @startDate date = CURRENT_TIMESTAMP - 90;
DECLARE @endDate date = CURRENT_TIMESTAMP;
DECLARE @givenOnline int = 0;
DECLARE @threshold decimal = 100.00;
DECLARE @connectionStatus table (id int);

--set variables from lava assigns
{% if daysBack and daysBack != '' %}
SET @daysBack = {{daysBack}};
{% endif %}
{% if startDate and startDate != '' %}
SET @startDate = '{{ startDate }}';
{% endif %}
{% if endDate and endDate != '' %}
SET @endDate = '{{ endDate }}';
{% endif %}
{% if givenOnline and givenOnline != '' %}
SET @givenOnline = {{ givenOnline }};
{% endif %}
{% if threshold and threshold != '' %}
SET @threshold = {{ threshold }};
{% endif %}
{% if connectionStatus and connectionStatus != '' %}
INSERT INTO @connectionStatus(id) SELECT * FROM STRING_SPLIT( '{{ connectionStatus }}',',' )
{% endif %}


-- temp index
Select pa.Id, p.GivingLeaderId Into #tempPA 
From PersonAlias pa
Inner Join Person p on p.Id = pa.PersonId

-- workflows completed
SELECT PA.GivingLeaderId as GivingLeaderId, MAX(W.CompletedDateTime) as CompletedDateTime, COUNT(W.Id) as [Count]
INTO #workflows
FROM Workflow W
INNER JOIN WorkflowActivity WA ON WA.WorkflowId = W.Id
INNER JOIN #tempPA PA ON WA.AssignedPersonAliasId = PA.Id
WHERE W.WorkflowTypeId = @workflowTypeId
AND W.CompletedDateTime IS NOT NULL
AND WA.CompletedDateTime IS NOT NULL
GROUP BY PA.GivingLeaderId

-- select distinct people who have given cash or check in past 180 days but not online
Select
    gl.Id
    , gl.NickName
    , gl.LastName
    , dv1.Value as ConnectionStatus
    , FORMAT(SUM( ftd.Amount ),'C') as CashGiven
    , CASE WHEN SUM(online.OnlineCount) > 0 THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END as GaveAnyOnline
    , '<a class="btn btn-sm" target="_blank" href="/Person/' + CONVERT(varchar(20), gl.Id) + '/Contributions"><i class="fa fa-user"></i></a>' as Person
    , (SELECT TOP 1 CompletedDateTime FROM #workflows W WHERE W.GivingLeaderId = gl.Id) as [SentEmail]
    , '<a class="btn btn-sm" target="_blank" href="/WorkflowEntry/' + CONVERT(varchar(20), @workflowTypeId) + '?PersonId=' + CONVERT(varchar(20), gl.Id) + '"><i class="fa fa-envelope"></i></a>' as SendEmail
From FinancialTransaction ft
Inner Join FinancialPaymentDetail fpd on ft.FinancialPaymentDetailId = fpd.Id
Inner Join #tempPA tp on tp.Id = ft.AuthorizedPersonAliasId
Inner Join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
Inner Join Person gl on tp.GivingLeaderId = gl.Id
Inner Join DefinedValue dv1 on dv1.Id = gl.ConnectionStatusValueId
Outer Apply (Select Count(fpd2.Id) As OnlineCount From FinancialPaymentDetail fpd2
  Inner Join FinancialTransaction ft2 on ft2.FinancialPaymentDetailId = fpd2.Id
  Inner Join #tempPA tp on tp.Id = ft2.AuthorizedPersonAliasId
  Where ft2.TransactionDateTime >= @startDate
  AND ft2.TransactionDateTime <= @endDate
  And ft2.TransactionTypeValueId = 53 -- contribution
  And fpd2.CurrencyTypeValueId not in ( select id from @currencyTypes )
  And tp.GivingLeaderId = gl.GivingLeaderId
) online
Where ft.TransactionDateTime >= @startDate
and ft.TransactionDateTime <= @endDate
and ft.TransactionTypeValueId = 53
And fpd.CurrencyTypeValueId IN ( select id from @currencyTypes )
And 1 = CASE WHEN @givenOnline = 1 AND online.OnlineCount != 0 THEN 0 WHEN @givenOnline = 2 AND online.OnlineCount = 0 THEN 0 ELSE 1 END
AND 1 = CASE WHEN (SELECT COUNT(*) FROM @connectionStatus) = 0 THEN 1 WHEN EXISTS( SELECT 1 FROM @connectionStatus WHERE id = gl.ConnectionStatusValueId) THEN 1 ELSE 0 END
And gl.LastName != 'Anonymous'
GROUP BY gl.Id, gl.NickName, gl.LastName, dv1.Value
HAVING SUM(ftd.Amount) >= @threshold
AND DATEDIFF( day, MAX(ft.TransactionDateTime), @endDate ) >= @daysBack
ORDER BY SUM(ftd.Amount) DESC

-- delete temp tables
Drop Table #tempPA
DROP TABLE #workflows