CREATE NONCLUSTERED INDEX [IX_AccountId_TransactionId_Amount]
ON [dbo].[FinancialTransactionDetail] ([AccountId])
INCLUDE ([TransactionId],[Amount])
GO

CREATE NONCLUSTERED INDEX [IX_TransactionDateTime_SourceType_AuthorizedPerson_PaymentDetails]
ON [dbo].[FinancialTransaction] ([TransactionDateTime])
INCLUDE ([Id],[SourceTypeValueId],[AuthorizedPersonAliasId],[FinancialPaymentDetailId])
GO