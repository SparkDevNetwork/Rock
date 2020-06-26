/*
Pledge Export Report
Basically gives the same information as the Pledge Analytics block, but with a bunch more columns

Allows a Sliding Date Range and Account(Pledge) selector
To enable Sliding Date Range picker, make sure lava's Execute method is enabled
*/

Declare @startDate datetime = DateAdd(year,-10, getDate())
Declare @endDate datetime = getDate()
Declare @accountId int = 0
Declare @accountTable TABLE ( Id int )
 
{% assign dates = PageParameter['DateRange']%}
{% if dates and dates != '' %}
    {% capture startdate %}{% execute import:'Rock.Web.UI.Controls' %}
        return SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( "{{dates}}" ).Start.Value.ToString();
    {% endexecute %}{% endcapture %}
    {% capture enddate %}{% execute import:'Rock.Web.UI.Controls' %}
        return SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( "{{dates}}" ).End.Value.ToString();
    {% endexecute %}{% endcapture %}
    
    {% if startdate and startdate != '' %}
    Set @startDate = '{{startdate}}'
    {% endif %}
    {% if enddate and enddate != '' %}
    Set @endDate = '{{enddate}}'
    {% endif %}
{% endif %}

{% assign accountId = 'Global' | PageParameter:'Pledge' | AsInteger %}
{% if accountId and accountId != '' %}
    Set @accountId = {{ accountId }}
{% endif %}

;WITH CTE AS (
	Select Id as Id
	From [FinancialAccount]
	Where Id = @accountId
 
	Union All 
	Select A.Id as Id
	From [FinancialAccount] A
	Join CTE on CTE.Id = A.ParentAccountId
)

INSERT INTO @accountTable
SELECT Id FROM CTE

SELECT ISNULL(P.NickName, ISNULL(P.FirstName, '')) + ' ' + P.LastName as Person
        ,DV1.Value as ConnectionStatus
        ,DV2.Value as RecordStatus
        ,P.Id as PersonId
        ,P.GivingGroupId as GivingGroupId
        , FP.TotalAmount as PledgeTotal
        , SUM(FTD.Amount) as GivingTotal
        , IIF( SUM(FTD.Amount) > 0 and FP.TotalAmount > 0, ( ( SUM(FTD.Amount) * 100 ) / FP.TotalAmount ) , 0 ) as PercentageComplete
        , COUNT( FTD.Id ) as GivingCount
FROM 
    Person P
    INNER JOIN  
    PersonAlias PA ON PA.PersonId = P.Id
    INNER JOIN
    Person P1 ON P1.GivingLeaderId = P.Id -- gives all people who give with pledge giver
    INNER JOIN
    PersonAlias PA1 ON P1.Id = PA1.PersonId
    LEFT JOIN
    FinancialPledge FP ON FP.PersonAliasId = PA.Id
    LEFT JOIN 
    FinancialTransaction FT ON FT.AuthorizedPersonAliasId = PA1.Id AND FT.TransactionDateTime >= @startDate AND FT.TransactionDateTime <= @endDate
    LEFT JOIN
    FinancialTransactionDetail FTD ON FTD.TransactionId = FT.Id AND FTD.AccountId IN ( Select Id FROM @accountTable )
    LEFT JOIN
    DefinedValue DV1 ON P.ConnectionStatusValueId = DV1.Id
    LEFT JOIN
    DefinedValue DV2 ON P.RecordStatusValueId = DV2.Id
WHERE
    ( FP.AccountId IS NULL AND FTD.Amount > 0 )
    Or 
    ( FP.AccountId IN ( Select Id FROM @accountTable ) )
GROUP BY
    P.GivingGroupId, P.Id, P.NickName, P.FirstName, P.LastName, FP.Id, FP.TotalAmount, DV1.Value, DV2.Value
ORDER BY P.LastName, P.FirstName
    
        