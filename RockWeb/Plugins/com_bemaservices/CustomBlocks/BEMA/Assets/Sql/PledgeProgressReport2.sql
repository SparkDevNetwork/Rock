{%- if PageParameter['FinancialAccount'] -%}
    {%- assign account = PageParameter['FinancialAccount'] -%}
{%- endif -%}
 
{%- if PageParameter['AnalysisDate'] -%}
    {%- assign analysisdate = PageParameter['AnalysisDate'] -%}
{%- endif -%}
 
{%- if account -%}
{%- if analysisdate -%}
Declare @endDate as Date = '{{ analysisdate | Date: 'yyyy-MM-dd' }}' -- Date Set
{% else %}
Declare @endDate as Date = getDate() -- get Date
{% endif %}
{% if account != null and account != empty %}
Declare @account as Int = (Select Id From FinancialAccount Where [Guid] = '{{account}}')
{% endif %}

DECLARE @PledgeReport table (PledgeId int, AccountId int, StartDate date, EndDate date, PersonId int, PledgeAmount decimal, TotalGiven decimal,PercentPledgeGiven decimal, PledgeProgress decimal )
 
INSERT INTO @PledgeReport 
 
Select 
            fp.Id as PledgeId
  
            , fp.AccountId as AccountId
            , fp.StartDate
            , fp.EndDate
            , p.Id
            , fp.TotalAmount as PledgeAmount
            , Case When [TotalGiven] is Null  Then 0 Else [TotalGiven] End as TotalGiven
            , Case When TotalGiven is Null Then 0 Else Floor(Cast(TotalGiven as Decimal) / Cast(fp.TotalAmount as Decimal) * 100) End As PercentPledgeGiven
            , Case
            When Cast(DATEDIFF(day, fp.StartDate, @endDate) as Decimal) / Cast(DateDiff(day, fp.StartDate, fp.EndDate) as Decimal) > 1 Then 100
            When TotalGiven = 0 Then 0
            Else Floor(Cast(DATEDIFF(day, fp.StartDate, @endDate) as Decimal) / Cast(DateDiff(day, fp.StartDate, fp.EndDate) as Decimal) * 100)
            End as PledgeProgress
            
 
   
            
        From FinancialPledge fp
        Join PersonAlias pa
            on pa.Id = fp.PersonAliasId
        Inner Join Person p on pa.PersonId = p.Id
 
        Left Outer Join PhoneNumber pn
            On pn.PersonId = p.Id and NumberTypeValueId = 12
 
        Outer Apply (
            Select 
            Distinct
                Sum(aft.Amount) Over (Partition By fp.Id) as TotalGiven
 
        From AnalyticsFactFinancialTransaction aft
 
        Where 
            aft.TransactionDateTime >= fp.StartDate
            and aft.TransactionDateTime <= @endDate
            and p.GivingGroupId = aft.GivingGroupId
            and fp.AccountId = aft.AccountId
			and fp.TotalAmount > 0
        ) as [Sum]
 
        Where 1=1
		{% if amount != null and amount != empty %}
			And fp.AccountId = @account
		{% endif %}
            
                and fp.StartDate <= @endDate
                and fp.EndDate >= @endDate
 
Select 
        tpt.PledgeAmount
        ,fa.Name
        , tpt.[TotalGiven] as AmountGiven
        , tpt.PledgeAmount * (tpt.PledgeProgress / 100) as PledgeToDate
        , tpt.TotalGiven - (tpt.PledgeAmount * (tpt.PledgeProgress / 100)) as DifferenceToDate
        , tpt.TotalGiven - (tpt.PledgeAmount) as DifferenceFromTotalPledge
        , Case
            When tpt.PercentPledgeGiven > tpt.PledgeProgress then 'Over'
            When tpt.PercentPledgeGiven = tpt.PledgeProgress then 'Meets'
            ELse 'Under'   
        End as PledgeStatus
From @PledgeReport tpt 
 
Inner Join FinancialAccount fa
    on fa.Id = tpt.AccountId
 
 
{%- endif -%}