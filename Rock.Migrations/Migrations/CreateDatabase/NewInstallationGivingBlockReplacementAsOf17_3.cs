// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    /*
        07/17/2025 - SMC
        This migration was updated to replace the TransactionEntry block with the newer TransactionEntryV2 block in new
        Rock installations, intentionally leaving the block alone for existing instances.

        The next time the database is "squished" into the CreateDatabase script, this extra migration should be removed,
        as it will already be included.

        See Rock.Migrations.CreateDatabase.

        Reason: Use new TransactionEntryV2 block for new Rock installations.
     */

    internal class NewInstallationGivingBlockReplacementAsOf17_3
    {
        public const string MigrationSql = @"
DECLARE @EntityTypeId_Block						INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65');
DECLARE @BlockTypeId_TransactionEntry			INT = (SELECT [Id] FROM [BlockType]  WHERE [Guid] = '74EE3481-3E5A-4971-A02E-D463ABB45591');
DECLARE @BlockTypeId_TransactionEntryV2			INT = (SELECT [Id] FROM [BlockType]  WHERE [Guid] = '6316D801-40C0-4EED-A2AD-55C13870664D');
DECLARE @BlockTypeId_ScheduledTransactionEdit	INT = (SELECT [Id] FROM [BlockType]  WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE');
DECLARE @BlockTypeId_ScheduledTransactionEditV2	INT = (SELECT [Id] FROM [BlockType]  WHERE [Guid] = 'F1ADF375-7442-4B30-BAC3-C387EA9B6C18');




-- Contributions block (TransactionEntry -> TransactionEntryV2)
DECLARE @PageId_GiveNow INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '1615E090-1889-42FF-AB18-5F7BE9F24498');
DECLARE @BlockId_Contributions INT = (SELECT [Id] FROM [Block] WHERE [PageId] = @PageId_GiveNow AND [BlockTypeId] = @BlockTypeId_TransactionEntry);

-- Contributions block: Clear the existing Block attributes.
DELETE FROM	[AttributeValue]
WHERE	[EntityId] = @BlockId_Contributions
	AND	[AttributeId] IN (
	SELECT	[Id]
	FROM	[Attribute]
	WHERE
			[EntityTypeId] = @EntityTypeId_Block
		AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
		AND	[EntityTypeQualifierValue] = @BlockTypeId_TransactionEntry);

-- Contributions block: Swap the Block ID.
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId_TransactionEntryV2 WHERE [Id] = @BlockId_Contributions;

-- Contributions block: Set new Block attributes.
INSERT INTO [AttributeValue]
	(IsSystem, AttributeId, EntityId, Value, Guid)
SELECT
	1,
	a.Id,
	@BlockId_Contributions,
	v.Value,
	v.Guid
FROM
	(VALUES
		('0C58E545-6F1F-40C6-BEA2-589E3A49233C', '17aaceef-15ca-4c30-9a3a-11e6cf7e6411', 'F2713934-827C-4409-BA5B-2768367E0083'), -- ConfirmAccountEmailTemplate
		('1182E3BF-0946-439C-9C7D-5CC7549B8209', '6432d2d2-32ff-443d-b5b3-fb6c8414c3ad', 'F84E3C62-D3EB-4BEB-ADE6-C95E540B902B'), -- FinancialGateway (set to test gateway by default)
		('6ED591F8-8D55-4ED5-9D89-38495B9DF7A6', '2072f4bc-53b4-4481-bc15-38f14425c6c9', 'FF933047-466A-45C8-A313-07D3264DDD3D'), -- ScheduledTransactionEditPage
		('1551C5EA-44B4-4B61-92A8-F91A54CB9561', '4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8', 'A1BC7734-5E55-4EA7-836B-77768FC49AA5') -- Accounts
	) AS v([AttributeGuid], [Value], [Guid])
JOIN [Attribute] a ON a.Guid = v.AttributeGuid;




-- Giving Profile - ScheduledTransactionEdit block (ScheduledTransactionEdit -> ScheduledTransactionEditV2)
DECLARE @PageId_EditGivingProfile INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '2072F4BC-53B4-4481-BC15-38F14425C6C9');
DECLARE @BlockId_ScheduledTransactionProfile INT = (SELECT [Id] FROM [Block] WHERE [PageId] = @PageId_EditGivingProfile AND [BlockTypeId] = @BlockTypeId_ScheduledTransactionEdit);

-- Giving Profile - ScheduledTransactionEdit block: Clear the existing Block attributes.
DELETE FROM	[AttributeValue]
WHERE	[EntityId] = @BlockId_ScheduledTransactionProfile
	AND	[AttributeId] IN (
	SELECT	[Id]
	FROM	[Attribute]
	WHERE
			[EntityTypeId] = @EntityTypeId_Block
		AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
		AND	[EntityTypeQualifierValue] = @BlockTypeId_ScheduledTransactionEdit);

-- Giving Profile - ScheduledTransactionEdit block: Swap the Block ID.
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId_ScheduledTransactionEditV2 WHERE [Id] = @BlockId_ScheduledTransactionProfile;

-- Giving Profile - ScheduledTransactionEdit block: This block uses default attribute values.




-- People Section - ScheduledTransactionEdit block (ScheduledTransactionEdit -> ScheduledTransactionEditV2)
DECLARE @PageId_EditScheduledTransactionPeople INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D360B64F-1267-4518-95CD-99CD5AB87D88');
DECLARE @BlockId_ScheduledTransactionEditPeople INT = (SELECT [Id] FROM [Block] WHERE [PageId] = @PageId_EditScheduledTransactionPeople AND [BlockTypeId] = @BlockTypeId_ScheduledTransactionEdit);

-- People Section - ScheduledTransactionEdit block: Clear the existing Block attributes.
DELETE FROM	[AttributeValue]
WHERE	[EntityId] = @BlockId_ScheduledTransactionEditPeople
	AND	[AttributeId] IN (
	SELECT	[Id]
	FROM	[Attribute]
	WHERE
			[EntityTypeId] = @EntityTypeId_Block
		AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
		AND	[EntityTypeQualifierValue] = @BlockTypeId_ScheduledTransactionEdit);

-- People Section - ScheduledTransactionEdit block: Swap the Block ID.
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId_ScheduledTransactionEditV2 WHERE [Id] = @BlockId_ScheduledTransactionEditPeople;

-- People Section - ScheduledTransactionEdit block: Set new Block attributes.
INSERT INTO [AttributeValue]
	(IsSystem, AttributeId, EntityId, Value, Guid)
SELECT
	1,
	a.Id,
	@BlockId_ScheduledTransactionEditPeople,
	v.Value,
	v.Guid
FROM
	(VALUES
		('2E528219-770B-4992-A8E7-F4D11CF9D943', 'True', '892F7B89-36F7-43A1-8788-3433B19000EE'), -- Impersonation
		('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', '<p>The transaction has been updated.</p>', '9E5F3B45-1249-4E85-B51E-4F63A6F731F4'), -- -- FinishLavaTemplate
		('BE3C72F7-32A7-4D4F-B49E-814496597B7D', '4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8', '27AE8BBF-1C87-46E7-8B3C-9D825F06366E') -- Accounts
	) AS v([AttributeGuid], [Value], [Guid])
JOIN [Attribute] a ON a.Guid = v.AttributeGuid;




-- Finance Section - ScheduledTransactionEdit block (ScheduledTransactionEdit -> ScheduledTransactionEditV2)
DECLARE @PageId_EditScheduledTransactionFinance INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F1C3BBD3-EE91-4DDD-8880-1542EBCD8041');
DECLARE @BlockId_ScheduledTransactionEditFinance INT = (SELECT [Id] FROM [Block] WHERE [PageId] = @PageId_EditScheduledTransactionFinance AND [BlockTypeId] = @BlockTypeId_ScheduledTransactionEdit);

-- Finance Section - ScheduledTransactionEdit block: Clear the existing Block attributes.
DELETE FROM	[AttributeValue]
WHERE	[EntityId] = @BlockId_ScheduledTransactionEditFinance
	AND	[AttributeId] IN (
	SELECT	[Id]
	FROM	[Attribute]
	WHERE
			[EntityTypeId] = @EntityTypeId_Block
		AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
		AND	[EntityTypeQualifierValue] = @BlockTypeId_ScheduledTransactionEdit);

-- Finance Section - ScheduledTransactionEdit block: Swap the Block ID.
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId_ScheduledTransactionEditV2 WHERE [Id] = @BlockId_ScheduledTransactionEditFinance;

-- Finance Section - ScheduledTransactionEdit block: Set new Block attributes.
INSERT INTO [AttributeValue]
	(IsSystem, AttributeId, EntityId, Value, Guid)
SELECT
	1,
	a.Id,
	@BlockId_ScheduledTransactionEditFinance,
	v.Value,
	v.Guid
FROM
	(VALUES
		('2E528219-770B-4992-A8E7-F4D11CF9D943', 'True', '26392577-5269-47DD-9C21-3047F58AA521'), -- Impersonation
		('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', '<p>The transaction has been updated.</p>', '78ADC306-982A-44F0-9BD5-FF3B375BAAC9'), -- -- FinishLavaTemplate
		('BE3C72F7-32A7-4D4F-B49E-814496597B7D', '4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8', '214CEADE-7BD1-42A3-988D-0E763A770397') -- Accounts
	) AS v([AttributeGuid], [Value], [Guid])
JOIN [Attribute] a ON a.Guid = v.AttributeGuid;




-- Update the Scheduled Transaction List Liquid block to include the update page for hosted gateways.  This enables the Edit button.
DECLARE @BlockId_ScheduledTransactionListLiquid INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '0D91DD2F-519C-4A4A-AB03-0933FC12BE7E')
DECLARE @AttributeId_ScheduledTransactionEditPageHosted INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DC84D98C-F53D-4C91-A0B1-D9AAD46395C1');

INSERT INTO [AttributeValue]
	([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
VALUES
	(1,@AttributeId_ScheduledTransactionEditPageHosted,@BlockId_ScheduledTransactionListLiquid,'2072F4BC-53B4-4481-BC15-38F14425C6C9','5EAA8233-F75B-407F-B5B4-715D9848A8E5');




-- Update the Scheduled Transaction View block to include the update page for hosted gateways.  This enables the Edit button.
-- See also:  Rock.Migrations.ScheduleTransactionPersonPage
DECLARE @BlockId_ScheduledTransactionView INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '909E5FAE-F8B9-4D3D-BFDC-68DD4F9ECEF2')
DECLARE @AttributeId_UpdatePageHosted INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '0C612E1C-8205-40A2-8F83-801E5816B2F2');

INSERT INTO [AttributeValue]
	([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
VALUES
	(1,@AttributeId_UpdatePageHosted,@BlockId_ScheduledTransactionView,'D360B64F-1267-4518-95CD-99CD5AB87D88','B70B50CD-F05E-44FD-BDE3-ADA474D13481');
";
    }
}
