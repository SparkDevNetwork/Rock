//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class UpdatePersonDetail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Update Component attribute to use guid instead of id
    UPDATE AV SET [Value] = E.[Guid]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'A7486B0E-4CA2-4E00-A987-5544C7DABA76'
    INNER JOIN [EntityType] E ON E.[Id] = AV.[Value]
    AND AV.[Value] <> ''
   
    -- Update PageXslt page attribute to use guid instead of id
    UPDATE AV SET [Value] = P.[Guid]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [Page] P ON P.[Id] = AV.[Value]
    WHERE A.[Guid] = 'DD516FA7-966E-4C80-8523-BEAC91C8EEDA'
    AND AV.[Value] <> ''
   
    -- Update icons for relationship group types
    UPDATE [GroupType] SET [IconCssClass] = 'icon-pushpin' WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E'
    UPDATE [GroupType] SET [IconCssClass] = 'icon-exchange' WHERE [Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'

    -- Move person pages to root
    UPDATE [Page] SET 
         [ParentPageId] = null 
        ,[DisplayInNavWhen] = 0
    WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303'

    DECLARE @PageId int
    SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25')
    DELETE [Block] WHERE [PageId] = @PageId
" );

            UpdateFieldType( "Components Field Type", "", "Rock", "Rock.Field.Types.ComponentsFieldType", "039E2E97-3682-4B29-8748-7132287A2059" );

            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Extended Attributes", "", "PersonDetail", "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Groups", "", "PersonDetail", "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Staff Details", "", "PersonDetail", "0E56F56E-FB32-4827-A69A-B90D43CB47F5" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Contributions", "", "PersonDetail", "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "History", "", "PersonDetail", "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418" );
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Person Profile Badges", "The badges available for display on the person profile page", "Default", "26547B83-A92D-4D7E-82ED-691F403F16B6", "icon-shield" );

            AddPageRoute( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "Person/{PersonId}/ExtendedAttributes" );
            AddPageRoute( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D", "Person/{PersonId}/Groups" );
            AddPageRoute( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "Person/{PersonId}/StaffDetails" );
            AddPageRoute( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "Person/{PersonId}/Contributions" );
            AddPageRoute( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "Person/{PersonId}/History" );

            AddPageContext( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "Rock.Model.Person", "PersonId" );
            AddPageContext( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D", "Rock.Model.Person", "PersonId" );
            AddPageContext( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "Rock.Model.Person", "PersonId" );
            AddPageContext( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "Rock.Model.Person", "PersonId" );
            AddPageContext( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "Rock.Model.Person", "PersonId" );

            AddBlockType( "CRM - Person Detail - Family Members", "", "~/Blocks/CRM/PersonDetail/FamilyMembers.ascx", "FC137BDA-4F05-4ECE-9899-A249C90D11FC" );
            AddBlockType( "CRM - Person Detail - Relationships", "", "~/Blocks/CRM/PersonDetail/Relationships.ascx", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD" );
            AddBlockType( "CRM - Person Detail - Badges", "", "~/Blocks/CRM/PersonDetail/Badges.ascx", "FC8AF928-C4AF-40C7-A667-4B24390F03A1" );

            // Bio Block Type
            AddBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "039E2E97-3682-4B29-8748-7132287A2059", "Badges", "Badges", "", "", 0, "", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE" );

            // Family Members Block Type
            AddBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Xslt File", "XsltFile", "", "XSLT File to use.", 0, "PersonDetail/FamilyMembers.xslt", "4B85F5F3-755E-4B14-91AD-56089BCD15B9" );

            // Sub Page Menu Block Type
            AddBlockTypeAttribute( "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current Parameters", "IncludeCurrentParameters", "", "Flag indicating if current page's parameters should be used when building url for child pages", 0, "False", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" );

            // Notes Block Type
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Entity Type", "ContextEntityType", "Filter", "Context Entity Type", 0, "", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" );
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Type", "NoteType", "", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", 0, "Notes", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6" );

            // Relationship Block type
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The type of group to display.  Any group of this type that person belongs to will be displayed", 0, "", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E" );
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Role Filter", "GroupRoleFilter", "", "Delimited list of group role id's that if entered, will only show groups where selected person is one of the roles.", 0, "", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B" );
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Role", "ShowRole", "", "Should the member's role be displayed with their name", 0, "False", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6" );

            // Badges
            AddBlockTypeAttribute( "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "039E2E97-3682-4B29-8748-7132287A2059", "Badges", "Badges", "", "", 0, "", "F5AB231E-3836-4D52-BD03-BF79773C237A" );

            AddBlock( "", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "PersonDetail", "HeaderZone", 0, "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" );
            AddBlock( "", "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "PersonDetail", "FamilyZone", 0, "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" );
            AddBlock( "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Sub Page Menu", "PersonDetail", "TabsZone", 0, "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "TimeLine", "", "ContentZoneLeft", 0, "0B2B550C-B0C9-420E-9CF3-BEC8979108F2" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178", "Bookmarked Attributes", "", "ContentZoneRight", 0, "BCFCB62D-9B83-4AF7-B884-DEB232A72506" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "Relationships", "", "ContentZoneRight", 1, "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "Peer Network", "", "ContentZoneRight", 2, "32847AAF-15F5-4F8B-9F84-92D6AE827857" );
            AddBlock( "26547B83-A92D-4D7E-82ED-691F403F16B6", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Badge Components", "", "Content", 0, "C5B56466-6EAF-404A-A803-C2314B36C38F" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "badges 1", "", "BadgBarZone1", 0, "98A30DD7-8665-4C6D-B1BB-A8380E862A04" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 2", "", "BadgBarZone2", 0, "AA588E23-D34C-433A-BA3D-B0B82797A22F" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges 3", "", "BadgBarZone3", 0, "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B" );

            // Attrib Value for Badge Components:Component Container
            AddBlockAttributeValue( "C5B56466-6EAF-404A-A803-C2314B36C38F", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.PersonProfile.BadgeContainer, Rock" );

            // Attrib Value for Bio:Badges
            AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE", "3d093330-d547-454b-8956-b76d8f9b8536|d4b2ba9b-4f2c-47cb-a5bb-f3ff53a68f39|09e8cd24-be34-4dc0-8b43-a7ace0549ce0" );

            // Attrib Value for badges 1:Badges
            AddBlockAttributeValue( "98A30DD7-8665-4C6D-B1BB-A8380E862A04", "F5AB231E-3836-4D52-BD03-BF79773C237A", "5ee996cb-ea8a-4eef-88d2-f11510aee5cd|d046a808-06a2-4131-88af-a97166840e97|7adba2d4-663f-4039-aa04-e0aec81b5a21|208a2fe8-9cc8-4608-891a-a62ce29be05b" );
            // Attrib Value for Badges 2:Badges
            AddBlockAttributeValue( "AA588E23-D34C-433A-BA3D-B0B82797A22F", "F5AB231E-3836-4D52-BD03-BF79773C237A", "78f5527e-0e90-4ac9-aaab-f8f2f061bdfb" );
            // Attrib Value for Badges 3:Badges
            AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", "F5AB231E-3836-4D52-BD03-BF79773C237A", "13e3b42c-7e7f-41a6-940b-fa0ddc5e647f" );

            // Attrib Value for Sub Page Menu:Number of Levels
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );
            // Attrib Value for Sub Page Menu:Root Page
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "BF04BB7E-BE3A-4A38-A37C-386B55496303" );
            // Attrib Value for Sub Page Menu:XSLT File
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/PersonDetail/SubPageNav.xslt" );
            // Attrib Value for Sub Page Menu:Include Current Parameters
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for TimeLine:Context Entity Type
            AddBlockAttributeValue( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", "Rock.Model.Person" );
            // Attrib Value for TimeLine:Note Type
            AddBlockAttributeValue( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6", "TimeLine" );

            // Attrib Value for Relationships:Group Type
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E", "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF" );
            // Attrib Value for Relationships:Group Role Filter
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B", "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42" );
            // Attrib Value for Relationships:Show Role
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6", "True" );

            // Attrib Value for Peer Network:Group Type
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E", "8C0E5852-F08F-4327-9AA5-87800A6AB53E" );
            // Attrib Value for Peer Network:Group Role Filter
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B", "CB9A0E14-6FCF-4C07-A49A-D7873F45E196" );
            // Attrib Value for Peer Network:Show Role
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6", "False" );

            #region Add Relationship Groups for all persons

            Sql( @"
        --Adds a Known Relationship group for each person in DB that does not have one

        DECLARE @RelationshipId int
        DECLARE @GroupTypeId int
        DECLARE @OwnerRoleId int

        SET @GroupTypeId = (
	        SELECT [Id] 
	        FROM [GroupType] WITH (NOLOCK)
	        WHERE [Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
        )

        SET @OwnerRoleId = (
	        SELECT [Id]
	        FROM [GroupRole] WITH (NOLOCK)
	        WHERE [Guid] = '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'
        )

        -- Find all the people that aren't owners of an Known Relationship group
        SELECT
	         RP.[id]
	        ,RP.[guid]
        INTO #Persons
        FROM [Person] RP WITH(NOLOCK)
        LEFT OUTER JOIN [GroupMember] GM WITH (NOLOCK)
	        ON GM.[PersonId] = RP.[Id]
	        AND GM.[GroupRoleId] = @OwnerRoleId
        WHERE GM.[Id] IS NULL

        -- Create an implied Relationships group for each person 
        -- (Use person's guid so we know what group to add owner to in next step)
        INSERT INTO [Group] (
	         [IsSystem]
	        ,[GroupTypeId]
	        ,[Name]
	        ,[IsSecurityRole]
	        ,[IsActive]
	        ,[Guid]
        )
        SELECT
	         0
	        ,@GroupTypeId
	        ,'Relationships'
	        ,0
	        ,1
	        ,[guid]
        FROM #Persons WITH (NOLOCK)

        -- Add the owner to the group
        INSERT INTO [GroupMember] (
	         [IsSystem]
	        ,[GroupId]
	        ,[PersonId]
	        ,[GroupRoleId]
	        ,[Guid]
        )
        SELECT
	        0
	        ,G.[Id]
	        ,P.[Id]
	        ,@OwnerRoleId
	        ,NEWID()
        FROM #Persons P WITH (NOLOCK)
        INNER JOIN [Group] G WITH (NOLOCK)
	        ON G.[GroupTypeId] = @GroupTypeId
	        AND G.[Guid] = P.[Guid]

        -- Reset the Group Guids
        UPDATE G
        SET [Guid] = NEWID()
        FROM #Persons P WITH (NOLOCK)
        INNER JOIN [Group] G WITH (NOLOCK)
	        ON G.[Guid] = P.[Guid]

        DROP TABLE #Persons


        --Adds an implied Relationship group for each person in DB that does not have one

        DECLARE @RelatedRoleId int

        SET @GroupTypeId = (
	        SELECT [Id] 
	        FROM [GroupType] WITH (NOLOCK)
	        WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E'
        )

        SET @OwnerRoleId = (
	        SELECT [Id]
	        FROM [GroupRole] WITH (NOLOCK)
	        WHERE [Guid] = 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'
        )

        SET @RelatedRoleId = (
	        SELECT [Id]
	        FROM [GroupRole] WITH (NOLOCK)
	        WHERE [Guid] = 'FEA75948-97CB-4DE9-8A0D-43FA2646F55B'
        )

        IF @RelatedRoleId IS NULL
        BEGIN
	        INSERT INTO [GroupRole] (
		         [IsSystem]
		        ,[GroupTypeId]
		        ,[Name]
		        ,[Description]
		        ,[Guid]
		        ,[IsLeader]
	        ) VALUES (
		         1
		        ,@GroupTypeId
		        ,'Related'
		        ,'Related person in an implied relationship'
		        ,'FEA75948-97CB-4DE9-8A0D-43FA2646F55B'
		        ,0
	        )
	        SET @RelatedRoleId = SCOPE_IDENTITY()
        END

        -- Find all the people that aren't owners of an Implied Relationship group
        SELECT
	         RP.[id]
	        ,RP.[guid]
        INTO #Persons2
        FROM [Person] RP WITH(NOLOCK)
        LEFT OUTER JOIN [GroupMember] GM WITH (NOLOCK)
	        ON GM.[PersonId] = RP.[Id]
	        AND GM.[GroupRoleId] = @OwnerRoleId
        WHERE GM.[Id] IS NULL

        -- Create an implied Relationships group for each person
        -- (Use person's guid so we know what group to add owner to in next step)
        INSERT INTO [Group] (
	         [IsSystem]
	        ,[GroupTypeId]
	        ,[Name]
	        ,[IsSecurityRole]
	        ,[IsActive]
	        ,[Guid]
        )
        SELECT
	         0
	        ,@GroupTypeId
	        ,'Peer Network'
	        ,0
	        ,1
	        ,[guid]
        FROM #Persons2 WITH (NOLOCK)

        -- Add the owner to the group
        INSERT INTO [GroupMember] (
	         [IsSystem]
	        ,[GroupId]
	        ,[PersonId]
	        ,[GroupRoleId]
	        ,[Guid]
        )
        SELECT
	        0
	        ,G.[Id]
	        ,P.[Id]
	        ,@OwnerRoleId
	        ,NEWID()
        FROM #Persons2 P WITH (NOLOCK)
        INNER JOIN [Group] G WITH (NOLOCK)
	        ON G.[GroupTypeId] = @GroupTypeId
	        AND G.[Guid] = P.[Guid]

        -- Reset the Group Guids
        UPDATE G
        SET [Guid] = NEWID()
        FROM #Persons2 P WITH (NOLOCK)
        INNER JOIN [Group] G WITH (NOLOCK)
	        ON G.[Guid] = P.[Guid]

        DROP TABLE #Persons2

" );
            #endregion

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Relationships Block Type
            DeleteAttribute( "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B" ); // Group Role Filter
            DeleteAttribute( "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E" ); // Group Type
            DeleteAttribute( "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6" ); // Show Role

            // Notes
            DeleteAttribute( "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" ); // Context Entity Type
            DeleteAttribute( "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6" ); // Note Type

            // Subpage
            DeleteAttribute( "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" ); // Include Current Parameters

            // Family Members Block Type
            DeleteAttribute( "4B85F5F3-755E-4B14-91AD-56089BCD15B9" ); // Xslt File

            // Badges
            DeleteAttribute( "8E11F65B-7272-4E9F-A4F1-89CE08E658DE" ); // Badges

            // Bio
            DeleteAttribute( "F5AB231E-3836-4D52-BD03-BF79773C237A" ); // Badges

            DeleteBlock( "C5B56466-6EAF-404A-A803-C2314B36C38F" ); // Badge Components

            DeleteBlock( "98A30DD7-8665-4C6D-B1BB-A8380E862A04" ); // badges 1
            DeleteBlock( "AA588E23-D34C-433A-BA3D-B0B82797A22F" ); // Badges 2
            DeleteBlock( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B" ); // Badges 3
            DeleteBlock( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136" ); // Relationships
            DeleteBlock( "32847AAF-15F5-4F8B-9F84-92D6AE827857" ); // Peer Network
            DeleteBlock( "BCFCB62D-9B83-4AF7-B884-DEB232A72506" ); // Bookmarked Attributes
            DeleteBlock( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2" ); // Timeline
            DeleteBlock( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" ); // Sub Page Menu
            DeleteBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" ); // Family Members
            DeleteBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" ); // Bio

            DeleteBlockType( "FC8AF928-C4AF-40C7-A667-4B24390F03A1" ); // CRM - Person Detail - Badges
            DeleteBlockType( "FC137BDA-4F05-4ECE-9899-A249C90D11FC" ); // CRM - Person Detail - Family Members
            DeleteBlockType( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD" ); // CRM - Person Detail - Relationships

            DeletePage( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE" ); // Extended Attributes
            DeletePage( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D" ); // Groups
            DeletePage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5" ); // Staff Details
            DeletePage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" ); // Contributions
            DeletePage( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418" ); // History

            DeletePage( "26547B83-A92D-4D7E-82ED-691F403F16B6" ); // Person Profile Badges

            Sql( @"
    -- Update PageXslt page attribute to use id instead of guid
    UPDATE AV SET [Value] = P.[Id]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId] 
    INNER JOIN [Page] P ON P.[Guid] = AV.[Value]
    WHERE A.[Guid] = 'DD516FA7-966E-4C80-8523-BEAC91C8EEDA'
    AND AV.[Value] <> ''

    -- Update Component attribute to use id instead of guid
    UPDATE AV SET [Value] = E.[Id]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'A7486B0E-4CA2-4E00-A987-5544C7DABA76'
    INNER JOIN [EntityType] E ON E.[Guid] = AV.[Value]
    AND AV.[Value] <> ''

" );

        }
    }
}
