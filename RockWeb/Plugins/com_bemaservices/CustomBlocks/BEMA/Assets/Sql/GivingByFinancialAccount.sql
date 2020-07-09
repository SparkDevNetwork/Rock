{% assign daterange = PageParameter.DateRange | SanitizeSql %} 
{% assign accountIds = PageParameter.AccountIds | SanitizeSql %}
SELECT Distinct FA.Name,
                Sum(FTD.[Amount]) AS [Total $]
FROM [dbo].[FinancialAccount] FA
LEFT JOIN [dbo].[FinancialTransactionDetail] FTD ON FTD.[AccountId] = FA.[Id]
LEFT JOIN [dbo].[FinancialTransaction] FT ON FTD.[TransactionId] = FT.[Id]
LEFT JOIN [dbo].[FinancialPaymentDetail] FPD ON FT.[FinancialPaymentDetailId] = FPD.[Id]
WHERE 1=1

{% if daterange == null or daterange == '' %}
    AND FT.[TransactionDateTime] > DateAdd(Year, -1, GetDate() )
{% else %}
    AND Cast(FT.[TransactionDateTime] As Date) Between '{{ daterange | Split:',' | First | Date:'sd' }}' And '{{ daterange | Split:',' | Last | Date:'sd' }}'
{% endif %}

{% if accountIds != null and accountIds != '' %}
    AND FTD.[AccountId] in ({{accountIds}})
{% endif %}

Group By FA.Name
