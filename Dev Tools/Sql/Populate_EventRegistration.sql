-- Entity Types
DECLARE @TemplateEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.RegistrationTemplate' )
IF @TemplateEntityTypeId IS NULL
BEGIN
	INSERT INTO [EntityType] ( [Name], [AssemblyName], [FriendlyName], [IsEntity], [IsSecured], [IsCommon], [Guid] )
	VALUES ( 'Rock.Model.RegistrationTemplate', 'Rock.Model.RegistrationTemplate, Rock, Version=1.3.4.0, Culture=neutral, PublicKeyToken=null', 'Registration Template', 1, 1, 0, NEWID() )
	SET @TemplateEntityTypeId = SCOPE_IDENTITY()
END

DECLARE @RegistrationEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Registration' )
IF @TemplateEntityTypeId IS NULL
BEGIN
	INSERT INTO [EntityType] ( [Name], [AssemblyName], [FriendlyName], [IsEntity], [IsSecured], [IsCommon], [Guid] )
	VALUES ( 'Rock.Model.Registration', 'Rock.Model.Registration, Rock, Version=1.3.4.0, Culture=neutral, PublicKeyToken=null', 'Registration', 1, 1, 0, NEWID() )
	SET @RegistrationEntityTypeId = SCOPE_IDENTITY()
END

-- Delete existing data
DELETE [RegistrationRegistrant]
DELETE [Registration]
DELETE [RegistrationTemplate]
DELETE [Category] WHERE [EntityTypeId] = @TemplateEntityTypeId
DELETE [FinancialTransaction] WHERE [Id] IN ( SELECT DISTINCT [TransactionId] FROM [FinancialTransactionDetail] WHERE [EntityTypeId] = @RegistrationEntityTypeId )

-- Category
DECLARE @CategoryId int
INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Order], [IconCssClass], [Guid] )
VALUES ( 0, @TemplateEntityTypeId, '', '', 'Test Category', 0, '', NEWID() )
SET @CategoryId = SCOPE_IDENTITY()

-- Template
DECLARE @RegistrationTemplateId int
INSERT INTO [RegistrationTemplate] ( [Name], [CategoryId], [GroupMemberStatus], [NotifyGroupLeaders], [UseDefaultConfirmationEmail], [Cost], [MinimumInitialPayment], [LoginRequired], [RegistrantsSameFamily], [AllowMultipleRegistrants], [MaxRegistrants], [IsActive], [CreatedDateTime], [Guid] )
VALUES ( 'Test Registration Template', @CategoryId, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, GETDATE(), NEWID() ) 
SET @RegistrationTemplateId = SCOPE_IDENTITY()

-- Template Fee
DECLARE @BookFeeId int
INSERT INTO [RegistrationTemplateFee] ( [Name], [RegistrationTemplateId], [FeeType], [CostValue], [DiscountApplies], [AllowMultiple], [Order], [Guid] )
VALUES ( 'Book', @RegistrationTemplateId, 0, '20', 1, 1, 0, NEWID() ) 
SET @BookFeeId = SCOPE_IDENTITY()

DECLARE @ShirtFeeId int
INSERT INTO [RegistrationTemplateFee] ( [Name], [RegistrationTemplateId], [FeeType], [CostValue], [DiscountApplies], [AllowMultiple], [Order], [Guid] )
VALUES ( 'Shirt', @RegistrationTemplateId, 1, 'S^20|M^20|L^20|XL^20|XXL^25', 1, 1, 1, NEWID() ) 
SET @ShirtFeeId = SCOPE_IDENTITY()

-- Instance
DECLARE @AccountId int = ( SELECT TOP 1 [Id] FROM [FinancialAccount] WHERE [Guid] = '2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5' )
DECLARE @RegistrationInstanceId int
INSERT INTO [RegistrationInstance] ( [Name], [RegistrationTemplateId], [Details], [MaxAttendees], [AccountId], [IsActive], [ContactName], [ContactEmail], [AdditionalReminderDetails], [AdditionalConfirmationDetails], [CreatedDateTime], [Guid] )
VALUES ( 'Test Instance', @RegistrationTemplateId, '', 0, @AccountId, 1, '', '', '', '', GETDATE(), NEWID() ) 
SET @RegistrationInstanceId = SCOPE_IDENTITY()

-- Registration
DECLARE @RegistrationId int
INSERT INTO [Registration] ( [RegistrationInstanceId], [PersonAliasId], [CreatedDateTime], [Guid] )
VALUES ( @RegistrationInstanceId, 3, GETDATE(), NEWID() ) 
SET @RegistrationId = SCOPE_IDENTITY()

-- Registrants
DECLARE @RegistrationRegistrantId int
INSERT INTO [RegistrationRegistrant] ( [RegistrationId], [PersonAliasId], [Cost], [CreatedDateTime], [Guid] )
VALUES ( @RegistrationId, 5, 50, GETDATE(), NEWID() ) 
SET @RegistrationRegistrantId = SCOPE_IDENTITY()

INSERT INTO [RegistrationRegistrantFee] ( [RegistrationRegistrantId], [RegistrationTemplateFeeId], [Quantity], [Cost], [Guid] )
VALUES ( @RegistrationRegistrantId, @BookFeeId, 2, 40, NEWID() ) 
INSERT INTO [RegistrationRegistrantFee] ( [RegistrationRegistrantId], [RegistrationTemplateFeeId], [Quantity], [Cost], [Guid] )
VALUES ( @RegistrationRegistrantId, @ShirtFeeId, 1, 20, NEWID() ) 

INSERT INTO [RegistrationRegistrant] ( [RegistrationId], [PersonAliasId], [Cost], [CreatedDateTime], [Guid] )
VALUES ( @RegistrationId, 6, 50, GETDATE(), NEWID() ) 
SET @RegistrationRegistrantId = SCOPE_IDENTITY()

INSERT INTO [RegistrationRegistrantFee] ( [RegistrationRegistrantId], [RegistrationTemplateFeeId], [Quantity], [Cost], [Guid] )
VALUES ( @RegistrationRegistrantId, @BookFeeId, 1, 20, NEWID() ) 
INSERT INTO [RegistrationRegistrantFee] ( [RegistrationRegistrantId], [RegistrationTemplateFeeId], [Quantity], [Cost], [Guid] )
VALUES ( @RegistrationRegistrantId, @ShirtFeeId, 1, 20, NEWID() ) 

-- Payments
DECLARE @FinancialTransactionId int
INSERT INTO [dbo].[FinancialTransaction] ( [AuthorizedPersonAliasId] ,[TransactionDateTime], [Summary], [TransactionTypeValueId], [CurrencyTypeValueId], [CreditCardTypeValueId], [SourceTypeValueId], [Guid] )
VALUES ( 3, GETDATE(), 'Event Registration for Alex Decker and Noah Decker. Fees.... ', 54, 156, 7, 10, NEWID() )
SET @FinancialTransactionId = SCOPE_IDENTITY()
 
INSERT INTO [dbo].[FinancialTransactionDetail] ( [TransactionId], [Amount], [AccountId], [EntityTypeId], [EntityId] ,[Guid] )
VALUES ( @FinancialTransactionId, 100, @AccountId, @RegistrationEntityTypeId, @RegistrationId, NEWID() )
