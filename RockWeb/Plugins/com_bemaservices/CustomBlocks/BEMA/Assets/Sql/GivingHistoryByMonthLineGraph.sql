{% assign Month = 'Now' | Date:'M' | AsInteger %}
{% if Month < 9 %}
    {% assign 3year = 'Now' | Date:'yyyy' | Minus:4 %}
    {% assign currentyear = 'Now' | Date:'yyyy' %}
{% else %}
    {% assign 3year = 'Now' | Date:'yyyy' | Minus:3 %}
    {% assign currentyear = 'Now' | Date:'yyyy' | Plus:1 %}
{% endif %}

Select 
    Cast([Year] as VarChar(8)) as Year
     ,[January]
     ,[February]
     ,[March]
     ,[April]
     ,[May]
     ,[June]
     ,[July]
     ,[August]
	 ,[September]
     ,[October]
     ,[November]
     ,[December]
    ,
    (
        IsNULL([January], 0)
        +IsNULL([February], 0)
        +IsNULL([March], 0)
        +IsNULL([April], 0)
        +IsNULL([May], 0)
        +IsNULL([June], 0)
        +IsNULL([July], 0)
        +IsNULL([August], 0)
		+ISNULL([September], 0)
        +ISNULL([October], 0)
        +IsNULL([November], 0)
        +IsNULL([December], 0)
    ) as Total
Into #tempdata
 FROM
(
    Select 
        DATEPART(Year,ft.TransactionDateTime) as Year
        ,DateName(Month,ft.TransactionDateTime) as [Month]
        ,ftd.Amount


        From FinancialTransaction ft
        left Join FinancialTransactionDetail ftd
            on ftd.TransactionId = ft.Id
        left Join FinancialAccount fa
            on fa.Id = ftd.AccountId


        where
            TransactionDateTime > '01/01/{{3year}}'
            and TransactionDateTime < '12/31/{{currentyear}}'
            and DATEADD(MONTH, DATEDIFF(MONTH, 0, TransactionDateTime), 0) != DATEADD(MONTH, DATEDIFF(MONTH, 0, getdate()), 0) -- ignore current month
            and ft.TransactionTypeValueId = 53 -- contribution
            {% if accounts != null and accounts != empty %}
               And fa.Id in({{accounts}})
            {% endif %}
) as src
Pivot
(
    Sum(Amount)
    For [Month] In (
     [January]
     ,[February]
     ,[March]
     ,[April]
     ,[May]
     ,[June]
     ,[July]
     ,[August]
	 ,[September]
     ,[October]
     ,[November]
     ,[December])
) as pvt ;

--- Total over Years
WITH data_test ([Year], January, February, March, April, May, June, July, August, September, October, November, December, Total)  
AS  
(  
    Select 
    [Year]
     ,[January]
     ,[February]
     ,[March]
     ,[April]
     ,[May]
     ,[June]
     ,[July]
     ,[August]
	 ,[September]
     ,[October]
     ,[November]
     ,[December]
    ,
     (  +IsNULL([January], 0)
        +IsNULL([February], 0)
        +IsNULL([March], 0)
        +IsNULL([April], 0)
        +IsNULL([May], 0)
        +IsNULL([June], 0)
        +IsNULL([July], 0)
        +IsNULL([August], 0)
		+ISNULL([September], 0)
        +ISNULL([October], 0)
        +IsNULL([November], 0)
        +IsNULL([December], 0)) as Total

 FROM
(
    Select 
        DatePart(Year,ft.TransactionDateTime) as Year
        ,DateName(Month,ft.TransactionDateTime) as [Month]
        ,ftd.Amount
        ,fa.Name

        From FinancialTransaction ft
        left Join FinancialTransactionDetail ftd
            on ftd.TransactionId = ft.Id
        left Join FinancialAccount fa
            on fa.Id = ftd.AccountId


        where 
            TransactionDateTime > '01/01/{{3year}}'
            and TransactionDateTime < getdate()
            and DATEADD(MONTH, DATEDIFF(MONTH, 0, TransactionDateTime), 0) != DATEADD(MONTH, DATEDIFF(MONTH, 0, getdate()), 0) -- ignore current month
            and ft.TransactionTypeValueId = 53 -- contribution
            {% if accounts != null and accounts != empty %}
               And fa.Id in({{accounts}})
            {% endif %}
) as src
Pivot
(
    Sum(Amount)
    For [Month] In (
    [January]
    ,[February]
    ,[March]
    ,[April]
    ,[May]
    ,[June]
    ,[July]
    ,[August]
	,[September]
    ,[October]
    ,[November]
    ,[December])
) as pvt )

Insert Into #tempdata
Select 
    'Total' as [Year]
    ,Sum(January) as January
    ,Sum(February) as February
    ,Sum(March) as March
    ,Sum(April) as April
    ,Sum(May) as May
    ,Sum(June) as June
    ,Sum(July) as July
    ,Sum(August) as August
    ,Sum(September) as September
    ,Sum(October) as October
    ,Sum(November) as November
    ,Sum(December) as December
    ,Sum(Total) as Total
 From data_test

-------------

Select * from #tempdata Order By Year


drop table #tempdata