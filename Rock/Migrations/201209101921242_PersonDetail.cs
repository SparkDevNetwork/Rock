namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

	using Rock.Cms;

    public partial class PersonDetail : RockMigration
    {
		public override void Up()
		{
			// Add Container page for all person related pages
			var page = DefaultSystemPage( "Person Pages", "Container page for person related pages", new Guid( "BF04BB7E-BE3A-4A38-A37C-386B55496303" ) );
			page.DisplayInNavWhen = DisplayInNavWhen.Never;
			AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", page );

			// Blocks for person detail page
			AddBlock( "Person Bio", "Person biographic/demographic information and picture (Person detail page)", "~/Blocks/Crm/PersonDetail/Bio.ascx", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" );
			AddBlock( "Person Contact Info", "Person contact information(Person Detail Page)", "~/Blocks/Crm/PersonDetail/ContactInfo.ascx", "D0D57B02-6EE7-4E4B-B80B-A8640D326572" );
			AddBlock( "Person Dates", "Person important dates (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Dates.ascx", "7ECA4A0B-4506-4F38-B889-E1D659D74930" );
			AddBlock( "Person Documents", "Person documents (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Documents.ascx", "26C043E6-39FC-48B9-8A19-3FBD06664E20" );
			AddBlock( "Person Family", "Person familiy members (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Family.ascx", "3E14B410-22CB-49CC-8A1F-C30ECD0E816A" );
			AddBlock( "Person Implied Relationships", "Person implied relationships (Person Detail Page)", "~/Blocks/Crm/PersonDetail/ImpliedRelationships.ascx", "AC43A72D-F04D-44E5-9E04-019925C4A825" );
			AddBlock( "Person Key Attributes", "Person key attributes (Person Detail Page)", "~/Blocks/Crm/PersonDetail/KeyAttributes.ascx", "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178" );
			AddBlock( "Person Known Relationships", "Person known relationships (Person Detail Page)", "~/Blocks/Crm/PersonDetail/KnownRelationships.ascx", "93BC538E-35CE-409F-89FB-4E3D9D00BEEC" );
			AddBlock( "Person Notes", "Person notes (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Notes.ascx", "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708" );
			AddBlock( "Tags", "Add tags to current context object", "~/Blocks/Core/Tags.ascx", "351004FF-C2D6-4169-978F-5888BEFF982F" );

			AddBlockAttribute( "351004FF-C2D6-4169-978F-5888BEFF982F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity", "Filter", "Entity Name", 0, "", "E7E7A6E4-445B-4A19-874F-2AEB510C8D7D" );
			AddBlockAttribute( "351004FF-C2D6-4169-978F-5888BEFF982F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "Filter", "The entity column to evaluate when determining if this attribute applies to the entity", 1, "", "237A123C-BFD4-43F6-860F-3C61815AC8AB" );
			AddBlockAttribute( "351004FF-C2D6-4169-978F-5888BEFF982F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "Filter", "The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, "", "75FA3234-76CE-45DC-82BA-14615B3E60C4" );

			// Add Person Detail page
			page = DefaultSystemPage( "Person Detail", "Displays information about a person", new Guid( "08DBD8A5-2C35-4146-B4A8-0F7652348B25" ) );
			page.Layout = "PersonDetail";
			AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", page );

			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "Bio", "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", 0 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "Family", "BioDetails", "44BDB19E-9967-45B9-A272-81F9C12FFE20", 0 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "D0D57B02-6EE7-4E4B-B80B-A8640D326572", "Contact Info", "BioDetails", "47B76E96-5641-486F-94B2-1E799A092BE0", 1 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "7ECA4A0B-4506-4F38-B889-E1D659D74930", "Dates", "BioDetails", "64366596-3301-4247-8819-4086EA86D1B6", 2 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "26C043E6-39FC-48B9-8A19-3FBD06664E20", "Documents", "BioDetails", "8ABD1FBF-3FE7-4E2D-A74D-00C78C53FE3F", 3 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "351004FF-C2D6-4169-978F-5888BEFF982F", "Tags", "Tags", "961623AC-1243-44A2-9ECB-685A7EDE2424", 0 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708", "Notes", "Notes", "CCEB85C0-45B4-4508-8331-DA59B7F573B6", 0 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178", "Key Attributes", "SupplementalInfo", "C9386DF7-8ACB-46E3-89DD-B00CB0648184", 0 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "93BC538E-35CE-409F-89FB-4E3D9D00BEEC", "Know Relationships", "SupplementalInfo", "192B987F-94C0-4FA6-8795-FF1CEE89FDB0", 1 );
			AddBlockInstance( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "AC43A72D-F04D-44E5-9E04-019925C4A825", "Implied Relationships", "SupplementalInfo", "72DD0749-1298-4C12-A336-9E1F49852BD4", 2 );

			AddBlockAttributeValue( "961623AC-1243-44A2-9ECB-685A7EDE2424", "E7E7A6E4-445B-4A19-874F-2AEB510C8D7D", "Rock.Crm.Person" );
			AddBlockAttributeValue( "961623AC-1243-44A2-9ECB-685A7EDE2424", "237A123C-BFD4-43F6-860F-3C61815AC8AB", "" );
			AddBlockAttributeValue( "961623AC-1243-44A2-9ECB-685A7EDE2424", "75FA3234-76CE-45DC-82BA-14615B3E60C4", "" );

			Sql( @"
	-- Get Person Detail Page
	DECLARE @PageId int
	SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25')

	-- Add context for person to person detail page
	INSERT INTO [cmsPageContext] ([IsSystem], [PageId], [Entity], [IdParameter], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonId], [ModifiedByPersonId], [Guid])
		VALUES (1, @PageId, 'Rock.Crm.Person', 'PersonId', GETDATE(), GETDATE(), 1, 1, '68E043C8-C7C8-4C1F-B260-8A5AB8E6B3CB');

	-- Update person route to point to new page
	UPDATE [cmsPageRoute] SET [PageId] = @PageId WHERE [Guid] = '7E97823A-78A8-4E8E-A337-7A20F2DA9E52'
" );
		}
        
        public override void Down()
        {
			Sql( @"
	-- Delete context for person detail page
	DELETE [cmsPageContext] WHERE [Guid] = '68E043C8-C7C8-4C1F-B260-8A5AB8E6B3CB'

	-- Update person route to point to old person detail page
	DECLARE @PageId int
	SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = 'F8657CB3-C97B-4F24-82C4-B93579A38B4F')
	UPDATE [cmsPageRoute] SET [PageId] = @PageId WHERE [Guid] = '7E97823A-78A8-4E8E-A337-7A20F2DA9E52'
" );

			// Person Detail Page
			DeletePage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25" );

			// Person Detail Blocks
			DeleteBlockAttribute( "75FA3234-76CE-45DC-82BA-14615B3E60C4" );
			DeleteBlockAttribute( "237A123C-BFD4-43F6-860F-3C61815AC8AB" );
			DeleteBlockAttribute( "E7E7A6E4-445B-4A19-874F-2AEB510C8D7D" );

			// Delete person detail blocks
			DeleteBlock( "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708" );
			DeleteBlock( "93BC538E-35CE-409F-89FB-4E3D9D00BEEC" );
			DeleteBlock( "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178" );
			DeleteBlock( "AC43A72D-F04D-44E5-9E04-019925C4A825" );
			DeleteBlock( "3E14B410-22CB-49CC-8A1F-C30ECD0E816A" );
			DeleteBlock( "26C043E6-39FC-48B9-8A19-3FBD06664E20" );
			DeleteBlock( "7ECA4A0B-4506-4F38-B889-E1D659D74930" );
			DeleteBlock( "D0D57B02-6EE7-4E4B-B80B-A8640D326572" );
			DeleteBlock( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" );
			DeleteBlock( "351004FF-C2D6-4169-978F-5888BEFF982F" );

			// Delete person container page
			DeletePage( "BF04BB7E-BE3A-4A38-A37C-386B55496303" );
		}
    }
}
