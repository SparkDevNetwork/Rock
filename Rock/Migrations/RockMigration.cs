using System;
using System.Data.Entity.Migrations;
using System.Linq;

using Rock;
using Rock.Cms;

namespace Rock.Migrations
{
	public abstract class RockMigration : DbMigration
	{
		#region Block Methods

		public void AddBlock( string name, string description, string path, string guid )
		{
			Sql( string.Format( @"
				
				INSERT INTO [cmsBlock] (
					[IsSystem],[Path],[Name],[Description],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					1,'{0}','{1}','{2}',
					GETDATE(),GETDATE(),1,1,
					'{3}')
",
					path,
					name,
					description,
					guid
					) );
		}

		public void AddBlock( BlockDto block )
		{

			Sql( string.Format( @"
				INSERT INTO [cmsBlock] (
					[IsSystem],[Path],[Name],[Description],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					{0},'{1}','{2}','{3}'
					'{4}','{5}',{6},{7},
					'{8}')
",
					block.IsSystem.Bit(),
					block.Path,
					block.Name,
					block.Description,
					block.CreatedDateTime,
					block.ModifiedDateTime,
					block.CreatedByPersonId,
					block.ModifiedByPersonId,
					block.Guid ) );
		}

		public void DeleteBlock( string guid )
		{
			Sql( string.Format( @"
				DELETE [cmsBlock] WHERE [Guid] = '{0}'
",
					guid
					) );
		}

		public BlockDto DefaultSystemBlock( string name, string description, Guid guid )
		{
			var block = new BlockDto();

			block.IsSystem = true;
			block.Name = name;
			block.Description = description;
			block.CreatedDateTime = DateTime.Now;
			block.ModifiedDateTime = DateTime.Now;
			block.CreatedByPersonId = 1;
			block.ModifiedByPersonId = 1;
			block.Guid = guid;

			return block;
		}

		#endregion		
		
		#region Page Methods

		public void AddPage( string parentPageGuid, string name, string description, string guid )
		{
			Sql( string.Format( @"
				
				DECLARE @ParentPageId int
				SET @ParentPageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

				DECLARE @Order int
				SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = @ParentPageId;

				INSERT INTO [cmsPage] (
					[Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
					[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
					[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[IconUrl],[Guid])
				VALUES(
					'{1}','{1}',1,@ParentPageId,1,'Default',
					0,1,1,0,1,0,
					@Order,0,'{2}',1,
					GETDATE(),GETDATE(),1,1,
					'','{3}')
",
					parentPageGuid,
					name,
					description,
					guid
					) );
		}

		public void AddPage( string parentPageGuid, PageDto page )
		{

			Sql( string.Format( @"

				DECLARE @ParentPageId int
				SET @ParentPageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

				DECLARE @Order int
				SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = @ParentPageId;

				INSERT INTO [cmsPage] (
					[Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
					[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
					[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[IconUrl],[Guid])
				VALUES(
					'{1}','{2}',{3},@ParentPageId,{4},'{5}',
					{6},{7},{8},{9},{10},{11},
					@Order,{12},'{13}',{14},
					'{15}','{16}',{17},{18},
					'{19}','{20}')
",
					parentPageGuid,
					page.Name,
					page.Title,
					page.IsSystem.Bit(),
					page.SiteId,
					page.Layout,
					page.RequiresEncryption.Bit(),
					page.EnableViewState.Bit(),
					page.MenuDisplayDescription.Bit(),
					page.MenuDisplayIcon.Bit(),
					page.MenuDisplayChildPages.Bit(),
					(int)page.DisplayInNavWhen,
					page.OutputCacheDuration,
					page.Description,
					page.IncludeAdminFooter.Bit(),
					page.CreatedDateTime,
					page.ModifiedDateTime,
					page.CreatedByPersonId,
					page.ModifiedByPersonId,
					page.IconUrl,
					page.Guid ) );
		}

		public void DeletePage( string guid )
		{
			Sql( string.Format( @"
				DELETE [cmsPage] WHERE [Guid] = '{0}'
",
					guid
					) );
		}

		public PageDto DefaultSystemPage( string name, string description, Guid guid )
		{
			var page = new PageDto();

			page.Name = name;
			page.Title = name;
			page.IsSystem = true;
			page.SiteId = 1;
			page.Layout = "Default";
			page.EnableViewState = true; ;
			page.MenuDisplayDescription = true;
			page.MenuDisplayChildPages = true;
			page.DisplayInNavWhen = DisplayInNavWhen.WhenAllowed;
			page.Description = description;
			page.IncludeAdminFooter = true;
			page.CreatedDateTime = DateTime.Now;
			page.ModifiedDateTime = DateTime.Now;
			page.CreatedByPersonId = 1;
			page.ModifiedByPersonId = 1;
			page.Guid = guid;

			return page;
		}

		#endregion

		#region BlockInstance Methods

		public void AddBlockInstance( string pageGuid, string blockGuid, string name, string zone, string guid, int order = 0 )
		{
			Sql( string.Format( @"
				
				DECLARE @PageId int
				SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

				DECLARE @BlockId int
				SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{1}')

				INSERT INTO [cmsBlockInstance] (
					[IsSystem],[PageId],[Layout],[BlockId],[Zone],
					[Order],[Name],[OutputCacheDuration],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					1,@PageId,NULL,@BlockId,'{2}',
					{3},'{4}',0,
					GETDATE(),GETDATE(),1,1,
					'{5}')
",
					pageGuid,
					blockGuid,
					zone,
					order,
					name,
					guid )
			);
		}

		public void AddBlockInstance( string pageGuid, string blockGuid, BlockInstanceDto blockInstance )
		{

			Sql( string.Format( @"

				DECLARE @PageId int
				SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

				DECLARE @BlockId int
				SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{1}')

				INSERT INTO [cmsBlockInstance] (
					[IsSystem],[PageId],[Layout],[BlockId],[Zone],
					[Order],[Name],[OutputCacheDuration],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					{2},@PageId,{3},@BlockId,'{4}',
					{5},'{6}',{7},
					'{8}','{9}',{10},{11},
					'{12}')
",
					pageGuid,
					blockGuid,
 					blockInstance.IsSystem,
					blockInstance.Layout == null ? "NULL" : "'" + blockInstance.Layout + "'",
					blockInstance.Zone,
					blockInstance.Order,
					blockInstance.Name,
					blockInstance.OutputCacheDuration,
					blockInstance.CreatedDateTime,
					blockInstance.ModifiedDateTime,
					blockInstance.CreatedByPersonId,
					blockInstance.ModifiedByPersonId,
					blockInstance.Guid )
			);
		}

		public BlockInstanceDto DefaultSystemBlockInstance( string name, string description, Guid guid )
		{
			var blockInstance = new BlockInstanceDto();

			blockInstance.IsSystem = true;
			blockInstance.Zone = "Content";
			blockInstance.Name = name;
			blockInstance.CreatedDateTime = DateTime.Now;
			blockInstance.ModifiedDateTime = DateTime.Now;
			blockInstance.CreatedByPersonId = 1;
			blockInstance.ModifiedByPersonId = 1;
			blockInstance.Guid = guid;

			return blockInstance;
		}

		#endregion

		#region Attribute Methods

		public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, string name, string category, string description, int order, string defaultValue, string guid)
		{
			Sql( string.Format( @"
				
				DECLARE @BlockId int
				SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

				DECLARE @FieldTypeId int
				SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

				INSERT INTO [coreAttribute] (
					[IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
					[Key],[Name],[Category],[Description],
					[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockId',CAST(@BlockId as varchar),
					'{2}','{3}','{4}','{5}',
					{6},0,'{7}',0,0,
					GETDATE(),GETDATE(),1,1,
					'{8}')
",
					blockGuid,
					fieldTypeGuid,
					name.Replace( " ", string.Empty ),
					name,
					category,
					description,
					order,
					defaultValue,
					guid)
			);
		}

		public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, Rock.Core.AttributeDto attribute )
		{

			Sql( string.Format( @"

				DECLARE @BlockId int
				SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

				DECLARE @FieldTypeId int
				SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

				INSERT INTO [coreAttribute] (
					[IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
					[Key],[Name],[Category],[Description],
					[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockId',CAST(@BlockId as varchar),
					'{2}','{3}','{4}','{5}',
					{6},{7},'{8}',{9},{10},
					'{11}','{12}',{13},{14},
					'{15}')
",
					blockGuid,
					fieldTypeGuid,
					attribute.Key,
					attribute.Name,
					attribute.Category,
					attribute.Description,
					attribute.Order,
					attribute.IsGridColumn.Bit(),
					attribute.DefaultValue,
					attribute.IsMultiValue.Bit(),
					attribute.IsRequired.Bit(),
					attribute.CreatedDateTime,
					attribute.ModifiedDateTime,
					attribute.CreatedByPersonId,
					attribute.ModifiedByPersonId,
					attribute.Guid )
			);
		}

		public void DeleteBlockAttribute( string guid )
		{
			Sql( string.Format( @"
				DELETE [coreAttribute] WHERE [Guid] = '{0}'
",
					guid
					) );
		}

		public Rock.Core.AttributeDto DefaultBlockAttribute( string name, string category, string description, int order, string defaultValue, Guid guid )
		{
			var attribute = new Rock.Core.AttributeDto();

			attribute.IsSystem = true;
			attribute.Key = name.Replace( " ", string.Empty );
			attribute.Name = name;
			attribute.Category = category;
			attribute.Description = description;
			attribute.Order = order;
			attribute.CreatedDateTime = DateTime.Now;
			attribute.ModifiedDateTime = DateTime.Now;
			attribute.CreatedByPersonId = 1;
			attribute.ModifiedByPersonId = 1;
			attribute.Guid = guid;

			return attribute;
		}

		#endregion

		#region Block Attribute Value Methods

		public void AddBlockAttributeValue( string blockInstanceGuid, string attributeGuid, string value )
		{
			Sql( string.Format( @"
				
				DECLARE @BlockInstanceId int
				SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '{0}')

				DECLARE @AttributeId int
				SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

				INSERT INTO [coreAttributeValue] (
					[IsSystem],[AttributeId],[EntityId],
					[Order],[Value],
					[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
					[Guid])
				VALUES(
					1,@AttributeId,@BlockInstanceId,
					0,'{2}',
					GETDATE(),GETDATE(),1,1,
					NEWID())
",
					blockInstanceGuid,
					attributeGuid,
					value)
			);
		}

		public void DeleteBlockAttributeValue( string blockInstanceGuid, string attributeGuid )
		{
			Sql( string.Format( @"

				DECLARE @BlockInstanceId int
				SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '{0}')

				DECLARE @AttributeId int
				SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

				DELETE [coreAttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockInstanceId
",
					blockInstanceGuid,
					attributeGuid)
			);
		}

		#endregion
	}
}