namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePersonDetail : RockMigration
    {
        public override void Up()
        {
			// Addd the AttributeCategoryView block type
			DeleteBlockType( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF" ); // Delete the blocktype if it might have been created already by Rock engine.
			AddBlockType( "Attribute Category View", "Displays attributes and values for the selected entity and category", "~/Blocks/Core/AttributeCategoryView.ascx", "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF" );

			Sql( @"
	-- Swith the order of the contact info and family members blocks
	UPDATE [cmsBlock] SET [Order] = 0 WHERE [Guid] = '47B76E96-5641-486F-94B2-1E799A092BE0'
	UPDATE [cmsBlock] SET [Order] = 1 WHERE [Guid] = '44BDB19E-9967-45B9-A272-81F9C12FFE20'

	-- Update the Dates block to use the Attribute Category View block
	UPDATE [cmsBlock] 
	SET [BlockTypeId] = (
		SELECT TOP 1 [Id] 
		FROM [cmsBlockType] 
		WHERE [Guid] = 'DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF'
	)
	WHERE [Guid] = '64366596-3301-4247-8819-4086EA86D1B6';

	-- Update the Known Relationships and Implied Relationships blocks to use the GroupMember block
	UPDATE [cmsBlock] 
	SET [BlockTypeId] = (
		SELECT TOP 1 [Id] 
		FROM [cmsBlockType] 
		WHERE [Guid] = '3E14B410-22CB-49CC-8A1F-C30ECD0E816A'
	)
	WHERE [Guid] IN ('192B987F-94C0-4FA6-8795-FF1CEE89FDB0', '72DD0749-1298-4C12-A336-9E1F49852BD4');
" );
			// Delete the Documents block (Documents will get displayed with the Dates)
			DeleteBlock("8ABD1FBF-3FE7-4E2D-A74D-00C78C53FE3F");

			// Delete the Dates block type
			DeleteBlockType( "7ECA4A0B-4506-4F38-B889-E1D659D74930" );

			// Delete the Documents block type
			DeleteBlockType( "26C043E6-39FC-48B9-8A19-3FBD06664E20" );

			// Delete the KnownRelationships block type
			DeleteBlockType("93BC538E-35CE-409F-89FB-4E3D9D00BEEC");

			// Delete the ImpliedRelationships block type
			DeleteBlockType("AC43A72D-F04D-44E5-9E04-019925C4A825");

			// Add attributes for GroupMembers block
			AddBlockAttribute( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "GroupRoleFilter", "Behavior", "Delimited list of group role id's that if entered, will only show groups where selected person is one of the roles.", 1, "", "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B" );
			AddBlockAttribute( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "IncludeSelf", "Behavior", "Should the current person be included in list of group members?", 2, "false", "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA" );
			AddBlockAttribute( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "IncludeLocations", "Behavior", "Should locations be included?", 3, "false", "0504FB69-C7EE-432F-B232-A705AACD4858" );
			AddBlockAttribute( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "XsltFile", "Behavior", "XSLT File to use.", 4, "GroupMembers.xslt", "69A88BCD-02DA-4600-AE9B-ADF30D41EE58" );

			// Add attributes for KeyAttributes block
			AddBlockAttribute( "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178", "9C204CD0-1233-41C5-818A-C5DA439445AA", "XsltFile", "Behavior", "XSLT File to use.", 1, "AttributeValues.xslt", "D06FAD77-7233-4EB9-B25B-3FD641C1DFF0" );

			// Add attributes for AttributeCategoryView
			AddBlockAttribute( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity", "Filter", "Entity Name", 0, "", "DF7E74C5-37E1-4AE9-9C8A-A5636D3D645D" );
			AddBlockAttribute( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "Filter", "The entity column to evaluate when determining if this attribute applies to the entity", 1, "", "9226E136-850C-4B50-B435-A54984F6A761" );
			AddBlockAttribute( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "Filter", "The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, "", "13034637-BD75-49F1-B0BB-CC035F817EE9" );
			AddBlockAttribute( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Categories", "Filter", "Delimited List of Attribute Category Names", 3, "", "9626C22F-12F5-4854-BBC5-F8983C73F1DA" );
			AddBlockAttribute( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "XsltFile", "Behavior", "XSLT File to use.", 4, "AttributeValues.xslt", "458CF961-C9EF-4CDD-9A71-527CF9DB7D0E" );

			// Dates bock
			AddBlockAttributeValue( "64366596-3301-4247-8819-4086EA86D1B6", "DF7E74C5-37E1-4AE9-9C8A-A5636D3D645D", "Rock.Crm.Person" );
			AddBlockAttributeValue( "64366596-3301-4247-8819-4086EA86D1B6", "9226E136-850C-4B50-B435-A54984F6A761", "" );
			AddBlockAttributeValue( "64366596-3301-4247-8819-4086EA86D1B6", "13034637-BD75-49F1-B0BB-CC035F817EE9", "" );
			AddBlockAttributeValue( "64366596-3301-4247-8819-4086EA86D1B6", "9626C22F-12F5-4854-BBC5-F8983C73F1DA", "Membership,Documents" );
			AddBlockAttributeValue( "64366596-3301-4247-8819-4086EA86D1B6", "458CF961-C9EF-4CDD-9A71-527CF9DB7D0E", "AttributeValues_ValueFirst.xslt" );

			// Family Members
			AddBlockAttributeValue( "44BDB19E-9967-45B9-A272-81F9C12FFE20", "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B", "" );
			AddBlockAttributeValue( "44BDB19E-9967-45B9-A272-81F9C12FFE20", "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA", "True" );
			AddBlockAttributeValue( "44BDB19E-9967-45B9-A272-81F9C12FFE20", "0504FB69-C7EE-432F-B232-A705AACD4858", "True" );
			AddBlockAttributeValue( "44BDB19E-9967-45B9-A272-81F9C12FFE20", "69A88BCD-02DA-4600-AE9B-ADF30D41EE58", "PersonDetail/FamilyMembers.xslt" );

			// Implied Relationships
			AddBlockAttributeValue( "72DD0749-1298-4C12-A336-9E1F49852BD4", "B84EB1CB-E719-4444-B739-B0112AA20BBA", "129A76BE-374B-44BE-98D0-32B5FED07331" );
			AddBlockAttributeValue( "72DD0749-1298-4C12-A336-9E1F49852BD4", "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B", "" );
			AddBlockAttributeValue( "72DD0749-1298-4C12-A336-9E1F49852BD4", "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA", "False" );
			AddBlockAttributeValue( "72DD0749-1298-4C12-A336-9E1F49852BD4", "0504FB69-C7EE-432F-B232-A705AACD4858", "False" );
			AddBlockAttributeValue( "72DD0749-1298-4C12-A336-9E1F49852BD4", "69A88BCD-02DA-4600-AE9B-ADF30D41EE58", "PersonDetail/ImpliedRelationships.xslt" );

			// Known Relationships
			AddBlockAttributeValue( "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", "B84EB1CB-E719-4444-B739-B0112AA20BBA", "C8D76E72-21EC-4651-AC57-4DA6B82575C4" );
			AddBlockAttributeValue( "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B", "" );
			AddBlockAttributeValue( "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA", "False" );
			AddBlockAttributeValue( "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", "0504FB69-C7EE-432F-B232-A705AACD4858", "False" );
			AddBlockAttributeValue( "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", "69A88BCD-02DA-4600-AE9B-ADF30D41EE58", "GroupMembers.xslt" );

			Sql( @"
	-- Add Known Relationships Group Type
	DECLARE @KnownRelationshipGroupTypeId int
	INSERT INTO [groupsGroupType] ([IsSystem],[Name],[Description],[DefaultGroupRoleId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,'Known Relationships','Manually configured relationships',NULL,GETDATE(),GETDATE(),1,1,'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF')
	SET @KnownRelationshipGroupTypeId = SCOPE_IDENTITY()
	UPDATE [coreAttributeValue] SET [Value] = CAST(@KnownRelationshipGroupTypeId as varchar) WHERE [Value] = 'C8D76E72-21EC-4651-AC57-4DA6B82575C4' 

	-- Add Implied Relationships Group Type
	DECLARE @ImpliedRelationshipGroupTypeId int
	INSERT INTO [groupsGroupType] ([IsSystem],[Name],[Description],[DefaultGroupRoleId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
		VALUES(1,'Implied Relationships','System discovered relationships',NULL,GETDATE(),GETDATE(),1,1,'8C0E5852-F08F-4327-9AA5-87800A6AB53E')
	SET @ImpliedRelationshipGroupTypeId = SCOPE_IDENTITY()
	UPDATE [coreAttributeValue] SET [Value] = CAST(@ImpliedRelationshipGroupTypeId as varchar) WHERE [Value] = '129A76BE-374B-44BE-98D0-32B5FED07331' 

	-- Update the name/description on the GroupMembers block
	UPDATE [cmsBlockType] SET 
		[Name] = 'Person Group Members',
		[Description] = 'Person Group Members (Person Detail Page)'
	WHERE [Guid] = '3E14B410-22CB-49CC-8A1F-C30ECD0E816A'
" );
		}
        
        public override void Down()
        {
			Sql( @"
	-- Delete Known Relationships
	DECLARE @KnownRelationshipGroupTypeId int
	SET @KnownRelationshipGroupTypeId = (SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = 'e0c5a0e2-b7b3-4ef4-820d-bbf7f9a374ef')
	DELETE [groupsGroup] WHERE [GroupTypeId] = @KnownRelationshipGroupTypeId
	DELETE [groupsGroupType] WHERE [Id] = @KnownRelationshipGroupTypeId

	-- Delete Implied Relationships
	DECLARE @ImpliedRelationshipGroupTypeId int
	SET @ImpliedRelationshipGroupTypeId = (SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = '8c0e5852-f08f-4327-9aa5-87800a6ab53e')
	DELETE [groupsGroup] WHERE [GroupTypeId] = @ImpliedRelationshipGroupTypeId
	DELETE [groupsGroupType] WHERE [Id] = @ImpliedRelationshipGroupTypeId
" );
			// Delete AttributeCategoryView attributes
			DeleteBlockAttribute( "458CF961-C9EF-4CDD-9A71-527CF9DB7D0E" );
			DeleteBlockAttribute( "9626C22F-12F5-4854-BBC5-F8983C73F1DA" );
			DeleteBlockAttribute( "13034637-BD75-49F1-B0BB-CC035F817EE9" );
			DeleteBlockAttribute( "9226E136-850C-4B50-B435-A54984F6A761" );
			DeleteBlockAttribute( "DF7E74C5-37E1-4AE9-9C8A-A5636D3D645D" );

			// Delete KeyAttributes attributes
			DeleteBlockAttribute( "D06FAD77-7233-4EB9-B25B-3FD641C1DFF0" );

			// Delete GroupMembers attributes
			DeleteBlockAttribute( "69A88BCD-02DA-4600-AE9B-ADF30D41EE58" ); 
			DeleteBlockAttribute( "0504FB69-C7EE-432F-B232-A705AACD4858" );
			DeleteBlockAttribute( "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA" );
			DeleteBlockAttribute( "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B" );
			
			AddBlockType( "Person Dates", "Person important dates (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Dates.ascx", "7ECA4A0B-4506-4F38-B889-E1D659D74930" );
			AddBlockType( "Person Documents", "Person documents (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Documents.ascx", "26C043E6-39FC-48B9-8A19-3FBD06664E20" );
			AddBlockType( "Person Implied Relationships", "Person implied relationships (Person Detail Page)", "~/Blocks/Crm/PersonDetail/ImpliedRelationships.ascx", "AC43A72D-F04D-44E5-9E04-019925C4A825" );
			AddBlockType( "Person Known Relationships", "Person known relationships (Person Detail Page)", "~/Blocks/Crm/PersonDetail/KnownRelationships.ascx", "93BC538E-35CE-409F-89FB-4E3D9D00BEEC" );

			AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "26C043E6-39FC-48B9-8A19-3FBD06664E20", "Documents", "BioDetails", "8ABD1FBF-3FE7-4E2D-A74D-00C78C53FE3F", 3 );

			Sql( @"
	-- Swith the order of the contact info and family members blocks
	UPDATE [cmsBlock] SET [Order] = 1 WHERE [Guid] = '47B76E96-5641-486F-94B2-1E799A092BE0'
	UPDATE [cmsBlock] SET [Order] = 0 WHERE [Guid] = '44BDB19E-9967-45B9-A272-81F9C12FFE20'

	-- Update the Dates block to use the dates block
	UPDATE [cmsBlock] 
	SET [BlockTypeId] = (
		SELECT TOP 1[Id] 
		FROM [cmsBlockType] 
		WHERE [Guid] = '7ECA4A0B-4506-4F38-B889-E1D659D74930'
	)
	WHERE [Guid] = '64366596-3301-4247-8819-4086EA86D1B6';

	-- Update the Known Relationships block type
	UPDATE [cmsBlock] 
	SET [BlockTypeId] = (
		SELECT TOP 1 [Id] 
		FROM [cmsBlockType] 
		WHERE [Guid] = '93BC538E-35CE-409F-89FB-4E3D9D00BEEC'
	)
	WHERE [Guid] = '192B987F-94C0-4FA6-8795-FF1CEE89FDB0';

	-- Update the Implied Relationships block type
	UPDATE [cmsBlock] 
	SET [BlockTypeId] = (
		SELECT TOP 1 [Id] 
		FROM [cmsBlockType] 
		WHERE [Guid] = 'AC43A72D-F04D-44E5-9E04-019925C4A825'
	)
	WHERE [Guid] = '72DD0749-1298-4C12-A336-9E1F49852BD4';

" );
			// Delete the AttributeCategoryView block type
			DeleteBlockType( "DAE0F74C-DD0D-4CD4-B30F-EB487F3DD7FF" );
		}
    }
}
