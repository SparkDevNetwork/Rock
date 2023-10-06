
/********************************************************************************************************************
 Sign-Ups - View sign-up projects and supporting entities.
*********************************************************************************************************************/

DECLARE @DefinedTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'B7842AF3-6F04-495E-9A6C-F403D06C02F3');
SELECT 'DefinedType' AS Entity, * FROM [DefinedType] WHERE [Id] = @DefinedTypeId;
SELECT 'DefinedValue' AS Entity, * FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId;

DECLARE @GroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '499B1367-06B3-4538-9D56-56D53F55DCB1');
SELECT 'GroupType' AS Entity, * FROM [GroupType] WHERE [Id] = @GroupTypeId OR [InheritedGroupTypeId] = @GroupTypeId;
SELECT 'GroupTypeRole' AS Entity, * FROM [GroupTypeRole] WHERE [GroupTypeId] = @GroupTypeId;
SELECT 'GroupTypeAssociation' AS Entity, * FROM [GroupTypeAssociation] WHERE [GroupTypeId] = @GroupTypeId;

DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'GroupTypeId' AND [EntityTypeQualifierValue] = @GroupTypeId);
SELECT 'Attribute' AS Entity, * FROM [Attribute] WHERE [Id] = @AttributeId;
SELECT 'AttributeQualifier' AS Entity, * FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId;

SELECT 'Group' AS Entity, * FROM [Group] WHERE [GroupTypeId] = @GroupTypeId;

SELECT 'AttributeValue' as Entity, * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId;

DECLARE @SysCommCategoryId int = (SELECT [Id] FROM [Category] WHERE [Guid] = 'CB279EE1-9A12-4837-9A14-1F36B6F7CDAF');
SELECT 'Category' AS Entity, * FROM [Category] WHERE [Id] = @SysCommCategoryId;
SELECT 'System Communication' AS Entity, * FROM [SystemCommunication] WHERE [CategoryId] = @SysCommCategoryId;
