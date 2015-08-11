-- FinancialTransaction
INSERT INTO [FinancialPaymentDetail] ( [CurrencyTypeValueId], [CreditCardTypeValueId], [Guid], [ForeignId] )
SELECT [CurrencyTypeValueId], [CreditCardTypeValueId], NEWID(), CAST( [Id] AS VARCHAR )
FROM [FinancialTransaction]

UPDATE F
SET [FinancialPaymentDetailId] = D.[Id]
FROM [FinancialPaymentDetail] D
INNER JOIN [FinancialTransaction] F ON CAST( F.[ID] AS VARCHAR ) = D.[ForeignId]
WHERE D.[ForeignId] IS NOT NULL

UPDATE [FinancialPaymentDetail]
SET [ForeignId] = NULL

-- FinancialScheduledTransaction
INSERT INTO [FinancialPaymentDetail] ( [CurrencyTypeValueId], [CreditCardTypeValueId], [Guid], [ForeignId] )
SELECT [CurrencyTypeValueId], [CreditCardTypeValueId], NEWID(), CAST( [Id] AS VARCHAR )
FROM [FinancialScheduledTransaction]

UPDATE F
SET [FinancialPaymentDetailId] = D.[Id]
FROM [FinancialPaymentDetail] D
INNER JOIN [FinancialScheduledTransaction] F ON CAST( F.[ID] AS VARCHAR ) = D.[ForeignId]
WHERE D.[ForeignId] IS NOT NULL

UPDATE [FinancialPaymentDetail]
SET [ForeignId] = NULL

-- FinancialPersonSavedAccount
INSERT INTO [FinancialPaymentDetail] ( [CurrencyTypeValueId], [CreditCardTypeValueId], [AccountNumberMasked], [Guid], [ForeignId] )
SELECT [CurrencyTypeValueId], [CreditCardTypeValueId], [MaskedAccountNumber], NEWID(), CAST( [Id] AS VARCHAR )
FROM [FinancialPersonSavedAccount]

UPDATE F
SET [FinancialPaymentDetailId] = D.[Id]
FROM [FinancialPaymentDetail] D
INNER JOIN [FinancialPersonSavedAccount] F ON CAST( F.[ID] AS VARCHAR ) = D.[ForeignId]
WHERE D.[ForeignId] IS NOT NULL

UPDATE [FinancialPaymentDetail]
SET [ForeignId] = NULL
