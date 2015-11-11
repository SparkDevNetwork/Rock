/*

This script is intended to be run within the SQL runner in the Rock Power Tools.  

It will set the Apollos sync settings since these are sensitive and should not be
stored in a public repo as a migration.

*/


-- These are the values that will be written to all of the sync workflow attributes
DECLARE @syncUrl AS NVARCHAR(MAX) = 'PUT ROOT APOLLOS URL HERE';
DECLARE @tokenValue AS NVARCHAR(MAX) = 'PUT TOKEN VALUE HERE';


-- ========================================================================= --

IF object_id('tempdb..#actionToUrl') IS NOT NULL
BEGIN
	DROP TABLE #actionToUrl
END

CREATE TABLE #actionToUrl (
	actionName NVARCHAR(255),
	url NVARCHAR(255)
);

-- Check-in Area, GroupType, Inherited Type
INSERT #actionToUrl VALUES
	('FinancialPaymentDetail Delete Action', 'paymentDetails'),
	('FinancialPaymentDetail Save Action', 'paymentDetails'),
	('Location Delete Action', 'locations'),
	('Location Save Action', 'locations'),
	('GroupLocation Delete Action', 'groupLocations'),
	('GroupLocation Save Action', 'groupLocations'),
	('FinancialPersonSavedAccount Delete Action', 'savedAccounts'),
	('FinancialPersonSavedAccount Save Action', 'savedAccounts'),
	('GroupMember Delete Action', 'groupMembers'),
	('GroupMember Save Action', 'groupMembers'),
	('GroupType Delete Action', 'groupTypes'),
	('GroupType Save Action', 'groupTypes'),
	('Group Delete Action', 'groups'),
	('Group Save Action', 'groups'),
	('Campus Delete Action', 'campuses'),
	('Campus Save Action', 'campuses'),
	('UserLogin Delete Action', 'users'),
	('UserLogin Save Action', 'users'),
	('Person Delete Action', 'people'),
	('Person Save Action', 'people'),
	('FinancialTransaction Delete Action', 'transactions'),
	('FinancialTransaction Save Action', 'transactions'),
	('FinancialTransactionDetail Delete Action', 'transactionDetails'),
	('FinancialTransactionDetail Save Action', 'transactionDetails'),
	('FinancialAccount Delete Action', 'accounts'),
	('FinancialAccount Save Action', 'accounts'),
	('FinancialScheduledTransaction Delete Action', 'scheduledTransactions'),
	('FinancialScheduledTransaction Save Action', 'scheduledTransactions'),
	('FinancialScheduledTransactionDetail Delete Action', 'scheduledTransactionDetails'),
	('FinancialScheduledTransactionDetail Save Action', 'scheduledTransactionDetails'),
	('PersonAlias Save Action', 'aliases'),
	('PersonAlias Delete Action', 'aliases');

DECLARE @syncEntityName AS NVARCHAR(MAX) = 'cc.newspring.Apollos.Workflow.Action.APISync';
DECLARE @syncEntityId AS INT;

SELECT @syncEntityId = Id
FROM EntityType
WHERE Name = @syncEntityName;

DECLARE @categoryName AS NVARCHAR(MAX) = 'API Sync To Apollos';
DECLARE @categoryId AS INT;

SELECT @categoryId = Id
FROM Category
WHERE Name = @categoryName;

-- Update sync URL
UPDATE AttributeValue
SET Value = (
	SELECT CONCAT(@syncUrl, atu.url, '/')
	FROM WorkflowActionType wat
	JOIN #actionToUrl atu ON atu.actionName = wat.Name
	WHERE EntityId = wat.Id
)
WHERE 
	EntityId IN (
		SELECT Id FROM WorkflowActionType WHERE ActivityTypeId IN (
			SELECT Id FROM WorkflowActivityType WHERE WorkflowTypeId IN (
				SELECT Id FROM WorkflowType WHERE CategoryId = @categoryId
			)
		)
	AND AttributeId IN (
		SELECT Id
		FROM Attribute
		WHERE
			EntityTypeQualifierColumn = 'EntityTypeId'
			AND EntityTypeQualifierValue = @syncEntityId
			AND Name = 'Sync URL'
	)
);

-- Update token value
UPDATE AttributeValue
SET Value = @tokenValue
WHERE 
	EntityId IN (
		SELECT Id FROM WorkflowActionType WHERE ActivityTypeId IN (
			SELECT Id FROM WorkflowActivityType WHERE WorkflowTypeId IN (
				SELECT Id FROM WorkflowType WHERE CategoryId = @categoryId
			)
		)
	AND AttributeId IN (
		SELECT Id
		FROM Attribute
		WHERE
			EntityTypeQualifierColumn = 'EntityTypeId'
			AND EntityTypeQualifierValue = @syncEntityId
			AND Name = 'Token Value'
	)
);

SELECT 'END';