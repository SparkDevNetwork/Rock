// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupToolboxGroupFinder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // First let's make the changes to the group types
            //

            // add topic defined type and attribute to the small group type
            RockMigrationHelper.AddDefinedType( "Group", "Small Group Topic", "Used to manage the topic options for small groups.", "d4111631-6b42-1cbd-4019-427d6bc6f475" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Topic", "The group's study topic.", 0, "d4111631-6b42-1cbd-4019-427d6bc6f475", "04bfac9b-6d1a-e089-446a-b2c604c76764" );
            
            // add attribute qualifiers
            Sql( @"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '04bfac9b-6d1a-e089-446a-b2c604c76764')
  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'd4111631-6b42-1cbd-4019-427d6bc6f475')

  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'definedtype', @DefinedTypeId, '6B2F37DB-7237-BDA2-4297-0010040ED95D')

  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'allowmultiple', 'False', '1F4E58C7-BC92-6BB0-4E38-F0A0BAB06535')

    INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'displaydescription', 'False', '102C4AFE-FF12-B890-46C8-362CFBDB9E87')" );


            // update the topic attribute and meeting time to not be a system ones
            Sql( @"
    
                UPDATE [Attribute] 
                    SET [IsSystem] = 0
                    WHERE [Guid] IN ('04bfac9b-6d1a-e089-446a-b2c604c76764', 'E439FBC6-0098-4419-A89F-0465569A5BEE')
    
            " );


            //
            // Update the "Small Group" group type
            //

            Sql( @"	  
                -- update the small group group type to use the simple schedule and enable attendance
                UPDATE [GroupType]
	            SET [AllowedScheduleTypes] = 1
                    , [TakesAttendance] = 1
	            WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" );


            //
            // Migrate the current data over
            //

            Sql( @"
                
                -- if they have not created any group delete the meeting time attribute
                DECLARE @SmallGroupCount int = (SELECT COUNT(*) FROM [Group] WHERE [Guid] NOT IN ('10B60F8D-0F23-4FAA-B35F-9A5F19F5F995', '62DC3753-01D5-48B5-B22D-D2825D92900B') AND [GroupTypeId] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'))
                IF (@SmallGroupCount = 0)
                BEGIN
	                DELETE FROM [Attribute] WHERE [Guid] = 'E439FBC6-0098-4419-A89F-0465569A5BEE'
                END

                -- declare some variables for use in inserts
                DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'd4111631-6b42-1cbd-4019-427d6bc6f475')
                DECLARE @OldTopicAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'A73D3FE8-FC1C-4474-B68D-9A145D9E4A15')
                DECLARE @NewTopicAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '04bfac9b-6d1a-e089-446a-b2c604c76764')

                -- create topic defined values from the current text based topics
                INSERT INTO [DefinedValue]
                ([IsSystem], [Order], [DefinedTypeId], [Value], [Guid])
                SELECT DISTINCT 0, 1, @DefinedTypeId, [Value], newid() FROM [AttributeValue] WHERE [AttributeId] = @OldTopicAttributeId and isnull([Value], '') != '' 

                -- assign new defined value topics based on their current text topics
                INSERT INTO [AttributeValue]
                ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                SELECT 0, @NewTopicAttributeId, [EntityId], dv.[Guid], newid() FROM [AttributeValue] av
	                INNER JOIN [DefinedValue] dv ON dv.DefinedTypeId = @DefinedTypeId and dv.[Value] = av.[Value] 
	                WHERE [AttributeId] = @OldTopicAttributeId

                -- delete old group topic attribute
                DELETE FROM [Attribute] WHERE [Id] = @OldTopicAttributeId

            " );

            //
            // Add Group Toolbox Pages
            //
            RockMigrationHelper.AddPage( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Group Toolbox", "", "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38", "" ); // Site:External Website
            RockMigrationHelper.AddPage( "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Group Attendance", "", "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Group Detail Lava", "Presents the details of a group using Lava", "~/Blocks/Groups/GroupDetailLava.ascx", "Groups", "218B057F-B214-4317-8E84-7A95CF88067E" );
            RockMigrationHelper.UpdateBlockType( "Group Finder", "Block for people to find a group that matches their search parameters.", "~/Blocks/Groups/GroupFinder.ascx", "Groups", "9F8F2D68-DEEA-4686-810F-AB32923F855E" );
            RockMigrationHelper.UpdateBlockType( "Group List Personalized Lava", "Lists all group that the person is a member of using a Lava template.", "~/Blocks/Groups/GroupListPersonalizedLava.ascx", "Groups", "1B172C33-8672-4C98-A995-8E123FF316BD" );
            RockMigrationHelper.UpdateBlockType( "Group Registration", "Allows a person to register for a group.", "~/Blocks/Groups/GroupRegistration.ascx", "Groups", "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7" );
            // Add Block to Page: My Account, Site: External Website
            RockMigrationHelper.AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "1B172C33-8672-4C98-A995-8E123FF316BD", "Group List Personalized Lava", "Sidebar1", "", "", 1, "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8" );

            // Add Block to Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlock( "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38", "", "1B172C33-8672-4C98-A995-8E123FF316BD", "Group List Personalized Lava", "Sidebar1", "", "", 0, "FA8760D1-D158-412D-ADAC-57362420C296" );

            // Add Block to Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlock( "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38", "", "218B057F-B214-4317-8E84-7A95CF88067E", "Group Detail Lava", "Main", "", "", 0, "1EDDD8D0-5B7A-4148-8AE6-182863048AC5" );

            // Add Block to Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlock( "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "", "5C547728-38C2-420A-8602-3CDAAC369247", "Group Attendance List", "Main", "", "", 1, "82B107C3-AF97-4476-879C-198C56100C73" );

            // Add Block to Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlock( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "", "9F8F2D68-DEEA-4686-810F-AB32923F855E", "Group Finder", "Main", "", "", 1, "7332DD26-D5F5-4736-BFCF-FC4AD97DD571" );

            // Add Block to Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlock( "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "", "1B172C33-8672-4C98-A995-8E123FF316BD", "Group List Personalized Lava", "Sidebar1", "", "", 0, "F66FF6E0-B345-426A-B2C0-A2AA59E11771" );

            // Add Block to Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlock( "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "", "218B057F-B214-4317-8E84-7A95CF88067E", "Group Detail Lava", "Main", "", "", 0, "4E915F97-D7BA-46CF-8A96-F42C1B4F9304" );

            // Attrib for BlockType: Group List Personalized Lava:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "13921BE2-C0D4-4FD6-841F-36022B56DB54" );

            // Attrib for BlockType: Group List Personalized Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group list.", 6, @"{% include '~~/Assets/Lava/GroupListSidebar.lava' %}", "C7EC3847-7419-4364-98E8-09FE42A04A76" );

            // Attrib for BlockType: Group List Personalized Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 7, @"False", "89B144BE-8ECD-42AC-97A6-C76C8E403422" );

            // Attrib for BlockType: Group List Personalized Lava:Include Group Types
            RockMigrationHelper.AddBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "", "The group types to display in the list.  If none are selected, all group types will be included.", 4, @"", "81D7C7A0-5469-419A-9A4D-149511DB7271" );

            // Attrib for BlockType: Group List Personalized Lava:Exclude Group Types
            RockMigrationHelper.AddBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Exclude Group Types", "ExcludeGroupTypes", "", "The group types to exclude from the list (only valid if including all groups).", 5, @"", "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E" );

            // Attrib for BlockType: Group Detail:Group Types Exclude
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Exclude", "GroupTypesExclude", "", "Select group types to exclude from this block.", 1, @"", "85EE581F-D246-498A-B857-5AD33EC3CAEA" );

            // Attrib for BlockType: Group Detail Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group details.", 5, @"{% include '~~/Assets/Lava/GroupDetail.lava' %}", "AEB81535-5E99-4AAB-9379-25C0C68111EC" );

            // Attrib for BlockType: Group Detail Lava:Enable Location Edit
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Location Edit", "EnableLocationEdit", "", "Enables changing locations when editing a group.", 6, @"False", "7B080222-D905-4A4A-A1F7-8175D511F6E5" );

            // Attrib for BlockType: Group Detail Lava:Edit Group Pre-HTML
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Pre-HTML", "EditGroupPre-HTML", "", "HTML to display before the edit group panel.", 8, @"", "29BCAE3E-7C98-4765-8804-AE6F7D326A18" );

            // Attrib for BlockType: Group Detail Lava:Edit Group Post-HTML
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Post-HTML", "EditGroupPost-HTML", "", "HTML to display after the edit group panel.", 9, @"", "B8273C24-A51C-4FCD-A29C-302133F7D236" );

            // Attrib for BlockType: Group Detail Lava:Edit Group Member Pre-HTML
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Member Pre-HTML", "EditGroupMemberPre-HTML", "", "HTML to display before the edit group member panel.", 10, @"", "0162D261-2E91-4855-8FE4-572F6F7BBDDB" );

            // Attrib for BlockType: Group Detail Lava:Edit Group Member Post-HTML
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Group Member Post-HTML", "EditGroupMemberPost-HTML", "", "HTML to display after the edit group member panel.", 11, @"", "0DADE5FA-9FD2-4C95-B30C-4DE4A1FB66B5" );

            // Attrib for BlockType: Group Detail Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 7, @"False", "E8BD8616-33CE-4017-B4BA-E04F0FDE231A" );

            // Attrib for BlockType: Group Detail Lava:Group Member Add Page
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Add Page", "GroupMemberAddPage", "", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", 1, @"", "C1AFBB38-7C45-4391-B332-ABB0F66F8F14" );

            // Attrib for BlockType: Group Detail Lava:Person Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "", "Page to link to for more information on a group member.", 0, @"", "6B23DC5D-3E1B-41CE-912E-7DE231B5B2F3" );

            // Attrib for BlockType: Group Finder:Attribute Filters
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Attribute Filters", "AttributeFilters", "", "", 0, @"", "380D0978-AF4F-4EEC-B872-56F0FB9F91E4" );

            // Attrib for BlockType: Group Finder:Attribute Columns
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Attribute Columns", "AttributeColumns", "", "", 0, @"", "CCF6E7CA-F3EA-490F-B910-DD1049B75B5A" );

            // Attrib for BlockType: Group Finder:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page to navigate to for group details.", 0, @"", "ED4809CD-1F8A-491D-91F3-ED4C04E36F96" );

            // Attrib for BlockType: Group Finder:Register Page
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Register Page", "RegisterPage", "", "The page to navigate to when registering for a group.", 1, @"", "DACB3F11-AA38-4470-9D81-4BB0F7D43AA8" );

            // Attrib for BlockType: Group Finder:Group Type
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "", 0, @"", "1097A3DD-5A58-414B-A17C-DF679FBF12D6" );

            // Attrib for BlockType: Group Finder:Geofenced Group Type
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Geofenced Group Type", "GeofencedGroupType", "", "", 0, @"", "B6F965A8-0EF8-48DC-B599-D9131EC640CA" );

            // Attrib for BlockType: Group Finder:Map Info Debug
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Map Info Debug", "MapInfoDebug", "", "", 0, @"False", "6748E6B3-45B0-4FA1-8405-3FBEDF514BB1" );

            // Attrib for BlockType: Group Finder:Show Age
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "", "", 0, @"False", "CCDE5B4C-D195-4E9C-9212-36068E8406C8" );

            // Attrib for BlockType: Group Finder:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "", 0, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "FA863C93-132D-4888-BB51-E46603585199" );

            // Attrib for BlockType: Group Finder:Map Height
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "", 0, @"600", "3CE83B17-1E68-4FC2-BDC3-33920C0BC99C" );

            // Attrib for BlockType: Group Finder:Show Fence
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Fence", "ShowFence", "", "", 0, @"False", "2F71D27F-8EC1-4A13-A349-59229173F88B" );

            // Attrib for BlockType: Group Finder:Polygon Colors
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "7BDAE237-6E49-47AC-9961-A45AFB69E240", "Polygon Colors", "PolygonColors", "", "", 0, @"#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc", "16CC2B4B-3BA5-4B76-8601-8D5B91637B06" );

            // Attrib for BlockType: Group Finder:Map Info
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map Info", "MapInfo", "", "", 0, @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}

{% if LinkedPages.RegisterPage != '' %}
    <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}?GroupId={{ Group.Id }}'>Register</a>
{% endif %}
", "9DA54BB6-986C-4723-8FE1-E3EF53119C6A" );

            // Attrib for BlockType: Group Finder:Show Map
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map", "ShowMap", "", "", 0, @"False", "1846B075-E3B2-4220-A458-8F74B67272B7" );

            // Attrib for BlockType: Group Finder:Show Lava Output
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Lava Output", "ShowLavaOutput", "", "", 0, @"False", "AFB2F4FD-772C-4238-9805-13897A600DCC" );

            // Attrib for BlockType: Group Finder:Lava Output
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Output", "LavaOutput", "", "", 0, @"
", "71140493-AB2A-4040-8724-EA69F95FE264" );

            // Attrib for BlockType: Group Finder:Lava Output Debug
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Lava Output Debug", "LavaOutputDebug", "", "", 0, @"False", "6A113765-5FEA-4714-93A1-6675AD5DF8FE" );

            // Attrib for BlockType: Group Finder:Show Grid
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid", "ShowGrid", "", "", 0, @"False", "9EFF53E3-256D-4251-8A7A-629AAAEB856D" );

            // Attrib for BlockType: Group Finder:Show Proximity
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Proximity", "ShowProximity", "", "", 0, @"False", "7437E1B6-CCF4-4A00-8004-B6074F79C107" );

            // Attrib for BlockType: Group Finder:Show Count
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Count", "ShowCount", "", "", 0, @"False", "4D326BF5-EF92-455E-9B15-C4D5094D76FB" );

            // Attrib for BlockType: Transaction Entry:Success Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Title", "SuccessTitle", "", "The text to display as heading of section for displaying details of gift.", 21, @"Gift Information", "6CFD636A-F614-42A1-ACE3-390AB73625AE" );

            // Attrib for BlockType: Transaction Entry:Save Account Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Account Title", "SaveAccountTitle", "", "The text to display as heading of section for saving payment information.", 24, @"Make Giving Even Easier", "7697364F-C7F8-4FC2-8664-F9581C326B85" );

            // Attrib for BlockType: Transaction Entry:Panel Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "", "The text to display in panel heading", 13, @"Gifts", "DC96A27B-A0F6-43C6-AF97-04DD65457A31" );

            // Attrib for BlockType: Transaction Entry:Contribution Info Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Info Title", "ContributionInfoTitle", "", "The text to display as heading of section for selecting account and amount.", 14, @"Contribution Information", "983C0903-1AFC-4686-8358-6245F1BBE8B2" );

            // Attrib for BlockType: Transaction Entry:Personal Info Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal Info Title", "PersonalInfoTitle", "", "The text to display as heading of section for entering personal information.", 16, @"Personal Information", "DD6A5846-E5F5-4B31-89DB-D89449A1C5AD" );

            // Attrib for BlockType: Transaction Entry:Payment Info Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Info Title", "PaymentInfoTitle", "", "The text to display as heading of section for entering credit card or bank account information.", 17, @"Payment Information", "9A9D054D-7AD9-4BD6-B5AD-2D66BF03F54A" );

            // Attrib for BlockType: Transaction Entry:Confirmation Title
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmation Title", "ConfirmationTitle", "", "The text to display as heading of section for confirming information entered.", 18, @"Confirm Information", "E6F83950-B4C7-47CB-9BE3-3C1AED78292B" );

            // Attrib for BlockType: Person Bio:Display Country Code
            RockMigrationHelper.AddBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Country Code", "DisplayCountryCode", "", "When enabled prepends the country code to all phone numbers.", 0, @"False", "879ED630-23C8-429D-A064-32168DB8057C" );

            // Attrib for BlockType: Group Detail Lava:Roster Page
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roster Page", "RosterPage", "", "The page to link to to view the roster.", 2, @"", "A66FF680-1814-4B9C-B4B2-1B24F0C39AE1" );

            // Attrib for BlockType: Group Detail Lava:Attendance Page
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "", "The page to link to to manage the group's attendance.", 3, @"", "9ECD9F72-5CDE-406F-99D1-5278CD3D0683" );

            // Attrib for BlockType: Group Detail Lava:Communication Page
            RockMigrationHelper.AddBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", "The communication page to use for sending emails to the group members.", 4, @"", "A79C6325-80B6-4F79-9495-8764BAE61382" );

            // Attrib for BlockType: Login:Prompt Message
            RockMigrationHelper.AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Prompt Message", "PromptMessage", "", "Optional text (HTML) to display above username and password fields.", 8, @"", "B2ABA418-32EF-4310-A1EA-3C76A2375979" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Detail Page Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8", "13921BE2-C0D4-4FD6-841F-36022B56DB54", @"4d84e1b1-6ba0-4f04-a9f3-dd07a6cf3f38" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Lava Template Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8", "C7EC3847-7419-4364-98E8-09FE42A04A76", @"{% include '~~/Assets/Lava/GroupListSidebar.lava' %}" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Enable Debug Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8", "89B144BE-8ECD-42AC-97A6-C76C8E403422", @"False" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Include Group Types Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8", "81D7C7A0-5469-419A-9A4D-149511DB7271", @"2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Exclude Group Types Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8", "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E", @"" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Include Group Types Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "FA8760D1-D158-412D-ADAC-57362420C296", "81D7C7A0-5469-419A-9A4D-149511DB7271", @"2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Lava Template Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "FA8760D1-D158-412D-ADAC-57362420C296", "C7EC3847-7419-4364-98E8-09FE42A04A76", @"{% include '~~/Assets/Lava/GroupListSidebar.lava' %}" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Enable Debug Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "FA8760D1-D158-412D-ADAC-57362420C296", "89B144BE-8ECD-42AC-97A6-C76C8E403422", @"False" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Exclude Group Types Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "FA8760D1-D158-412D-ADAC-57362420C296", "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Member Post-HTML Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "0DADE5FA-9FD2-4C95-B30C-4DE4A1FB66B5", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Enable Debug Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "E8BD8616-33CE-4017-B4BA-E04F0FDE231A", @"False" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Attendance Page Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "9ECD9F72-5CDE-406F-99D1-5278CD3D0683", @"00d2dce6-d9c0-47a0-bae1-4591779ae2e1" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Communication Page Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "A79C6325-80B6-4F79-9495-8764BAE61382", @"60002bc0-790a-4052-8f8d-b08c2c5d261c" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Roster Page Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "A66FF680-1814-4B9C-B4B2-1B24F0C39AE1", @"4d84e1b1-6ba0-4f04-a9f3-dd07a6cf3f38" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Enable Location Edit Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "7B080222-D905-4A4A-A1F7-8175D511F6E5", @"False" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Pre-HTML Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "29BCAE3E-7C98-4765-8804-AE6F7D326A18", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Post-HTML Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "B8273C24-A51C-4FCD-A29C-302133F7D236", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Member Pre-HTML Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "0162D261-2E91-4855-8FE4-572F6F7BBDDB", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Lava Template Page: Group Toolbox, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5", "AEB81535-5E99-4AAB-9379-25C0C68111EC", @"{% include '~~/Assets/Lava/GroupDetail.lava' %}" );

            // Attrib Value for Block:Group Finder, Attribute:Attribute Filters Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "380D0978-AF4F-4EEC-B872-56F0FB9F91E4", @"6a6ac383-77ab-41ae-8d67-784f640dddbf" );

            // Attrib Value for Block:Group Finder, Attribute:Attribute Columns Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "CCF6E7CA-F3EA-490F-B910-DD1049B75B5A", @"" );

            // Attrib Value for Block:Group Finder, Attribute:Group Detail Page Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "ED4809CD-1F8A-491D-91F3-ED4C04E36F96", @"" );

            // Attrib Value for Block:Group Finder, Attribute:Register Page Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "DACB3F11-AA38-4470-9D81-4BB0F7D43AA8", @"7D24FE9A-710C-4B25-B1C7-76161ED78DB8" );

            // Attrib Value for Block:Group Finder, Attribute:Group Type Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "1097A3DD-5A58-414B-A17C-DF679FBF12D6", @"50fcfb30-f51a-49df-86f4-2b176ea1820b" );

            // Attrib Value for Block:Group Finder, Attribute:Geofenced Group Type Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "B6F965A8-0EF8-48DC-B599-D9131EC640CA", @"" );

            // Attrib Value for Block:Group Finder, Attribute:Map Info Debug Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "6748E6B3-45B0-4FA1-8405-3FBEDF514BB1", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Show Age Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "CCDE5B4C-D195-4E9C-9212-36068E8406C8", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Map Style Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "FA863C93-132D-4888-BB51-E46603585199", @"223" );

            // Attrib Value for Block:Group Finder, Attribute:Map Height Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "3CE83B17-1E68-4FC2-BDC3-33920C0BC99C", @"600" );

            // Attrib Value for Block:Group Finder, Attribute:Show Fence Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "2F71D27F-8EC1-4A13-A349-59229173F88B", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Polygon Colors Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "16CC2B4B-3BA5-4B76-8601-8D5B91637B06", @"#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc" );

            // Attrib Value for Block:Group Finder, Attribute:Map Info Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "9DA54BB6-986C-4723-8FE1-E3EF53119C6A", @"<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}

{% if LinkedPages.RegisterPage != '' %}
    <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}?GroupId={{ Group.Id }}'>Register</a>
{% endif %}
" );

            // Attrib Value for Block:Group Finder, Attribute:Show Map Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "1846B075-E3B2-4220-A458-8F74B67272B7", @"True" );

            // Attrib Value for Block:Group Finder, Attribute:Show Lava Output Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "AFB2F4FD-772C-4238-9805-13897A600DCC", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Lava Output Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "71140493-AB2A-4040-8724-EA69F95FE264", @"" );

            // Attrib Value for Block:Group Finder, Attribute:Lava Output Debug Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "6A113765-5FEA-4714-93A1-6675AD5DF8FE", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Show Grid Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "9EFF53E3-256D-4251-8A7A-629AAAEB856D", @"True" );

            // Attrib Value for Block:Group Finder, Attribute:Show Proximity Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "7437E1B6-CCF4-4A00-8004-B6074F79C107", @"False" );

            // Attrib Value for Block:Group Finder, Attribute:Show Count Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "4D326BF5-EF92-455E-9B15-C4D5094D76FB", @"False" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Lava Template Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F66FF6E0-B345-426A-B2C0-A2AA59E11771", "C7EC3847-7419-4364-98E8-09FE42A04A76", @"{% include '~~/Assets/Lava/GroupListSidebar.lava' %}" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Enable Debug Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F66FF6E0-B345-426A-B2C0-A2AA59E11771", "89B144BE-8ECD-42AC-97A6-C76C8E403422", @"False" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Include Group Types Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F66FF6E0-B345-426A-B2C0-A2AA59E11771", "81D7C7A0-5469-419A-9A4D-149511DB7271", @"2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20" );

            // Attrib Value for Block:Group List Personalized Lava, Attribute:Exclude Group Types Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "F66FF6E0-B345-426A-B2C0-A2AA59E11771", "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Lava Template Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "AEB81535-5E99-4AAB-9379-25C0C68111EC", @"{% include '~~/Assets/Lava/GroupDetail.lava' %}" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Enable Location Edit Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "7B080222-D905-4A4A-A1F7-8175D511F6E5", @"False" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Attendance Page Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "9ECD9F72-5CDE-406F-99D1-5278CD3D0683", @"00d2dce6-d9c0-47a0-bae1-4591779ae2e1" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Enable Debug Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "E8BD8616-33CE-4017-B4BA-E04F0FDE231A", @"False" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Member Pre-HTML Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "0162D261-2E91-4855-8FE4-572F6F7BBDDB", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Member Post-HTML Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "0DADE5FA-9FD2-4C95-B30C-4DE4A1FB66B5", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Pre-HTML Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "29BCAE3E-7C98-4765-8804-AE6F7D326A18", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Edit Group Post-HTML Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "B8273C24-A51C-4FCD-A29C-302133F7D236", @"" );

            // Attrib Value for Block:Group Detail Lava, Attribute:Roster Page Page: Group Attendance, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304", "A66FF680-1814-4B9C-B4B2-1B24F0C39AE1", @"4d84e1b1-6ba0-4f04-a9f3-dd07a6cf3f38" );

            //
            // Add Group Registration Pages
            //

            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Group Registration", "", "7D24FE9A-710C-4B25-B1C7-76161ED78DB8", "" ); // Site:External Website
            // Add Block to Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlock( "7D24FE9A-710C-4B25-B1C7-76161ED78DB8", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "02ABF2C1-0D34-45C2-9F0F-228954B13A52" );

            // Add Block to Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlock( "7D24FE9A-710C-4B25-B1C7-76161ED78DB8", "", "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "Group Registration", "Main", "", "", 0, "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1" );

            // Attrib for BlockType: Group Finder:ScheduleFilters
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "ScheduleFilters", "ScheduleFilters", "", "", 0, @"", "B9C26CFD-CCC8-43DA-9DB6-6EAAF6792A82" );

            // Attrib for BlockType: Group Finder:Show Schedule
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Schedule", "ShowSchedule", "", "", 0, @"False", "E13FC044-C674-4059-AAAB-099B0B636F0A" );

            // Attrib for BlockType: Group Simple Register:Success Message
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Message", "SuccessMessage", "", "The message to display when user is succesfully added to the group", 0, @"Please check your email to verify your registration", "76112491-8806-4081-AA89-FDBEF5B27BBB" );

            // Attrib for BlockType: Group Simple Register:Group
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", "The group to add people to", 0, @"", "F9DD30B1-DC64-4782-803C-D35D34807F3D" );

            // Attrib for BlockType: Group Simple Register:Save Button Text
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Button Text", "SaveButtonText", "", "The text to use for the Save button", 0, @"Save", "318E9C84-62F9-4666-86FA-EF13BDF77E49" );

            // Attrib for BlockType: Group Simple Register:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "16B0D253-F2E8-40B8-A99F-9DA4F815A644" );

            // Attrib for BlockType: Group Simple Register:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 0, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "86AEDC3E-37A7-42EE-8451-911FF127BFE2" );

            // Attrib for BlockType: Group Simple Register:Confirmation Page
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationPage", "", "The page that user should be directed to to confirm their registration", 0, @"", "7255CA32-9D1E-40D8-98CD-325613F2AD33" );

            // Attrib for BlockType: Group Simple Register:Confirmation Email
            RockMigrationHelper.AddBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Confirmation Email", "ConfirmationEmail", "", "The email to send the person to confirm their registration.  If not specified, the user will not need to confirm their registration", 0, @"", "BA40D736-AB27-4972-99FD-6B2C64C28B21" );

            // Attrib for BlockType: Group Registration:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 5, @"False", "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A" );

            // Attrib for BlockType: Group Registration:Workflow
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "C4287E3F-D2D8-413E-A3AE-F9A3EE7A5021" );

            // Attrib for BlockType: Group Registration:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group details.", 6, @"
", "8E37CB4A-AF69-4671-9EC6-2ED72380B749" );

            // Attrib for BlockType: Group Registration:Result Page
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "", "An optional page to redirect user to after they have been registered for the group.", 7, @"", "BAD40ACB-CC0B-4CE4-A3B8-E7C5134AE0E2" );

            // Attrib for BlockType: Group Registration:Result Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Result Lava Template", "ResultLavaTemplate", "", "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", 8, @"
", "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1" );

            // Attrib for BlockType: Group Registration:Mode
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "", "The mode to use when displaying registration details.", 0, @"Simple", "EDEC82F3-C9E1-4B26-862D-E896F6C26376" );

            // Attrib for BlockType: Group Registration:Group Member Status
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", "The group member status to use when adding person to group (default: 'Pending'.)", 1, @"2", "B61174DE-6E7D-4171-9067-6A7981F888E8" );

            // Attrib for BlockType: Group Registration:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 2, @"368DD475-242C-49C4-A42C-7278BE690CC2", "E3544F7A-0E7A-421B-9142-AE858E9CCFBB" );

            // Attrib for BlockType: Group Registration:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 3, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC" );

            // Attrib Value for Block:Group Finder, Attribute:ScheduleFilters Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "B9C26CFD-CCC8-43DA-9DB6-6EAAF6792A82", @"" );

            // Attrib Value for Block:Group Finder, Attribute:Show Schedule Page: Small Groups, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571", "E13FC044-C674-4059-AAAB-099B0B636F0A", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "02ABF2C1-0D34-45C2-9F0F-228954B13A52", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Group Registration, Attribute:Enable Debug Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A", @"False" );

            // Attrib Value for Block:Group Registration, Attribute:Lava Template Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "8E37CB4A-AF69-4671-9EC6-2ED72380B749", @"<div class='alert alert-info'>
    Please complete the form below to register for {{ Group.Name }}. 
</div>" );

            // Attrib Value for Block:Group Registration, Attribute:Result Lava Template Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1", @"<div class='alert alert-success'>
    You have been registered for {{ Group.Name }}. You should be hearing from the leader soon.
</div>" );

            // Attrib Value for Block:Group Registration, Attribute:Mode Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "EDEC82F3-C9E1-4B26-862D-E896F6C26376", @"FullSpouse" );

            // Attrib Value for Block:Group Registration, Attribute:Group Member Status Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "B61174DE-6E7D-4171-9067-6A7981F888E8", @"2" );

            // Attrib Value for Block:Group Registration, Attribute:Connection Status Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "E3544F7A-0E7A-421B-9142-AE858E9CCFBB", @"368dd475-242c-49c4-a42c-7278be690cc2" );

            // Attrib Value for Block:Group Registration, Attribute:Record Status Page: Group Registration, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1", "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC", @"283999ec-7346-42e3-b807-bce9b2babb49" );


            //
            // Add page for group attendance detail
            //

            RockMigrationHelper.AddPage( "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Group Attendance Detail", "", "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7", "" ); // Site:External Website
            // Add Block to Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlock( "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7", "", "1B172C33-8672-4C98-A995-8E123FF316BD", "Group List", "Sidebar1", "", "", 0, "D9E42DA3-1139-4D5E-83B6-12882BD403AE" );

            // Add Block to Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlock( "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7", "", "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "Group Attendance Detail", "Main", "", "", 0, "129EF990-DB27-462C-A33E-368E5A84EF4A" );

            // Attrib Value for Block:Group List, Attribute:Lava Template Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "D9E42DA3-1139-4D5E-83B6-12882BD403AE", "C7EC3847-7419-4364-98E8-09FE42A04A76", @"{% include '~~/Assets/Lava/GroupListSidebar.lava' %}" );

            // Attrib Value for Block:Group List, Attribute:Enable Debug Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "D9E42DA3-1139-4D5E-83B6-12882BD403AE", "89B144BE-8ECD-42AC-97A6-C76C8E403422", @"False" );

            // Attrib Value for Block:Group List, Attribute:Include Group Types Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "D9E42DA3-1139-4D5E-83B6-12882BD403AE", "81D7C7A0-5469-419A-9A4D-149511DB7271", @"2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4,50fcfb30-f51a-49df-86f4-2b176ea1820b,fab75ec6-0402-456a-be34-252097de4f20" );

            // Attrib Value for Block:Group List, Attribute:Exclude Group Types Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "D9E42DA3-1139-4D5E-83B6-12882BD403AE", "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E", @"" );

            // add attribute value for attendance detail
            RockMigrationHelper.AddBlockAttributeValue( "82B107C3-AF97-4476-879C-198C56100C73", "747E9320-C85D-42F2-8298-52E65A5F9F5C", "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete topic defined type and attribute
            RockMigrationHelper.DeleteAttribute( "04bfac9b-6d1a-e089-446a-b2c604c76764" );
            RockMigrationHelper.DeleteDefinedType( "d4111631-6b42-1cbd-4019-427d6bc6f475" );


            //
            // Remove the pages
            //

            // delete page for attendance detail

            // Remove Block: Group Attendance Detail, from Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "129EF990-DB27-462C-A33E-368E5A84EF4A" );
            // Remove Block: Group List, from Page: Group Attendance Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "D9E42DA3-1139-4D5E-83B6-12882BD403AE" );
            RockMigrationHelper.DeletePage( "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7" ); //  Page: Group Attendance Detail, Layout: LeftSidebar, Site: External Website

            // Attrib for BlockType: Login:Prompt Message
            RockMigrationHelper.DeleteAttribute( "B2ABA418-32EF-4310-A1EA-3C76A2375979" );
            // Attrib for BlockType: Group Detail Lava:Communication Page
            RockMigrationHelper.DeleteAttribute( "A79C6325-80B6-4F79-9495-8764BAE61382" );
            // Attrib for BlockType: Group Detail Lava:Attendance Page
            RockMigrationHelper.DeleteAttribute( "9ECD9F72-5CDE-406F-99D1-5278CD3D0683" );
            // Attrib for BlockType: Group Detail Lava:Roster Page
            RockMigrationHelper.DeleteAttribute( "A66FF680-1814-4B9C-B4B2-1B24F0C39AE1" );
            // Attrib for BlockType: Person Bio:Display Country Code
            RockMigrationHelper.DeleteAttribute( "879ED630-23C8-429D-A064-32168DB8057C" );
            // Attrib for BlockType: Transaction Entry:Confirmation Title
            RockMigrationHelper.DeleteAttribute( "E6F83950-B4C7-47CB-9BE3-3C1AED78292B" );
            // Attrib for BlockType: Transaction Entry:Payment Info Title
            RockMigrationHelper.DeleteAttribute( "9A9D054D-7AD9-4BD6-B5AD-2D66BF03F54A" );
            // Attrib for BlockType: Transaction Entry:Personal Info Title
            RockMigrationHelper.DeleteAttribute( "DD6A5846-E5F5-4B31-89DB-D89449A1C5AD" );
            // Attrib for BlockType: Transaction Entry:Contribution Info Title
            RockMigrationHelper.DeleteAttribute( "983C0903-1AFC-4686-8358-6245F1BBE8B2" );
            // Attrib for BlockType: Transaction Entry:Panel Title
            RockMigrationHelper.DeleteAttribute( "DC96A27B-A0F6-43C6-AF97-04DD65457A31" );
            // Attrib for BlockType: Transaction Entry:Save Account Title
            RockMigrationHelper.DeleteAttribute( "7697364F-C7F8-4FC2-8664-F9581C326B85" );
            // Attrib for BlockType: Transaction Entry:Success Title
            RockMigrationHelper.DeleteAttribute( "6CFD636A-F614-42A1-ACE3-390AB73625AE" );
            // Attrib for BlockType: Group Finder:Show Count
            RockMigrationHelper.DeleteAttribute( "4D326BF5-EF92-455E-9B15-C4D5094D76FB" );
            // Attrib for BlockType: Group Finder:Show Proximity
            RockMigrationHelper.DeleteAttribute( "7437E1B6-CCF4-4A00-8004-B6074F79C107" );
            // Attrib for BlockType: Group Finder:Show Grid
            RockMigrationHelper.DeleteAttribute( "9EFF53E3-256D-4251-8A7A-629AAAEB856D" );
            // Attrib for BlockType: Group Finder:Lava Output Debug
            RockMigrationHelper.DeleteAttribute( "6A113765-5FEA-4714-93A1-6675AD5DF8FE" );
            // Attrib for BlockType: Group Finder:Lava Output
            RockMigrationHelper.DeleteAttribute( "71140493-AB2A-4040-8724-EA69F95FE264" );
            // Attrib for BlockType: Group Finder:Show Lava Output
            RockMigrationHelper.DeleteAttribute( "AFB2F4FD-772C-4238-9805-13897A600DCC" );
            // Attrib for BlockType: Group Finder:Show Map
            RockMigrationHelper.DeleteAttribute( "1846B075-E3B2-4220-A458-8F74B67272B7" );
            // Attrib for BlockType: Group Finder:Map Info
            RockMigrationHelper.DeleteAttribute( "9DA54BB6-986C-4723-8FE1-E3EF53119C6A" );
            // Attrib for BlockType: Group Finder:Polygon Colors
            RockMigrationHelper.DeleteAttribute( "16CC2B4B-3BA5-4B76-8601-8D5B91637B06" );
            // Attrib for BlockType: Group Finder:Show Fence
            RockMigrationHelper.DeleteAttribute( "2F71D27F-8EC1-4A13-A349-59229173F88B" );
            // Attrib for BlockType: Group Finder:Map Height
            RockMigrationHelper.DeleteAttribute( "3CE83B17-1E68-4FC2-BDC3-33920C0BC99C" );
            // Attrib for BlockType: Group Finder:Map Style
            RockMigrationHelper.DeleteAttribute( "FA863C93-132D-4888-BB51-E46603585199" );
            // Attrib for BlockType: Group Finder:Show Age
            RockMigrationHelper.DeleteAttribute( "CCDE5B4C-D195-4E9C-9212-36068E8406C8" );
            // Attrib for BlockType: Group Finder:Map Info Debug
            RockMigrationHelper.DeleteAttribute( "6748E6B3-45B0-4FA1-8405-3FBEDF514BB1" );
            // Attrib for BlockType: Group Finder:Geofenced Group Type
            RockMigrationHelper.DeleteAttribute( "B6F965A8-0EF8-48DC-B599-D9131EC640CA" );
            // Attrib for BlockType: Group Finder:Group Type
            RockMigrationHelper.DeleteAttribute( "1097A3DD-5A58-414B-A17C-DF679FBF12D6" );
            // Attrib for BlockType: Group Finder:Register Page
            RockMigrationHelper.DeleteAttribute( "DACB3F11-AA38-4470-9D81-4BB0F7D43AA8" );
            // Attrib for BlockType: Group Finder:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "ED4809CD-1F8A-491D-91F3-ED4C04E36F96" );
            // Attrib for BlockType: Group Finder:Attribute Columns
            RockMigrationHelper.DeleteAttribute( "CCF6E7CA-F3EA-490F-B910-DD1049B75B5A" );
            // Attrib for BlockType: Group Finder:Attribute Filters
            RockMigrationHelper.DeleteAttribute( "380D0978-AF4F-4EEC-B872-56F0FB9F91E4" );
            // Attrib for BlockType: Group Attendance List:Detail Page
            RockMigrationHelper.DeleteAttribute( "747E9320-C85D-42F2-8298-52E65A5F9F5C" );
            // Attrib for BlockType: Group Detail Lava:Person Detail Page
            RockMigrationHelper.DeleteAttribute( "6B23DC5D-3E1B-41CE-912E-7DE231B5B2F3" );
            // Attrib for BlockType: Group Detail Lava:Group Member Add Page
            RockMigrationHelper.DeleteAttribute( "C1AFBB38-7C45-4391-B332-ABB0F66F8F14" );
            // Attrib for BlockType: Group Detail Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "E8BD8616-33CE-4017-B4BA-E04F0FDE231A" );
            // Attrib for BlockType: Group Detail Lava:Edit Group Member Post-HTML
            RockMigrationHelper.DeleteAttribute( "0DADE5FA-9FD2-4C95-B30C-4DE4A1FB66B5" );
            // Attrib for BlockType: Group Detail Lava:Edit Group Member Pre-HTML
            RockMigrationHelper.DeleteAttribute( "0162D261-2E91-4855-8FE4-572F6F7BBDDB" );
            // Attrib for BlockType: Group Detail Lava:Edit Group Post-HTML
            RockMigrationHelper.DeleteAttribute( "B8273C24-A51C-4FCD-A29C-302133F7D236" );
            // Attrib for BlockType: Group Detail Lava:Edit Group Pre-HTML
            RockMigrationHelper.DeleteAttribute( "29BCAE3E-7C98-4765-8804-AE6F7D326A18" );
            // Attrib for BlockType: Group Detail Lava:Enable Location Edit
            RockMigrationHelper.DeleteAttribute( "7B080222-D905-4A4A-A1F7-8175D511F6E5" );
            // Attrib for BlockType: Group Detail Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "AEB81535-5E99-4AAB-9379-25C0C68111EC" );
            // Attrib for BlockType: Group Detail:Group Types Exclude
            RockMigrationHelper.DeleteAttribute( "85EE581F-D246-498A-B857-5AD33EC3CAEA" );
            // Attrib for BlockType: Group List Personalized Lava:Exclude Group Types
            RockMigrationHelper.DeleteAttribute( "04AA4BAE-9E55-45C2-ABAB-F85B1F81596E" );
            // Attrib for BlockType: Group List Personalized Lava:Include Group Types
            RockMigrationHelper.DeleteAttribute( "81D7C7A0-5469-419A-9A4D-149511DB7271" );
            // Attrib for BlockType: Group List Personalized Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "89B144BE-8ECD-42AC-97A6-C76C8E403422" );
            // Attrib for BlockType: Group List Personalized Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "C7EC3847-7419-4364-98E8-09FE42A04A76" );
            // Attrib for BlockType: Group List Personalized Lava:Detail Page
            RockMigrationHelper.DeleteAttribute( "13921BE2-C0D4-4FD6-841F-36022B56DB54" );
            // Remove Block: Group Detail Lava, from Page: Group Attendance, Site: External Website
            RockMigrationHelper.DeleteBlock( "4E915F97-D7BA-46CF-8A96-F42C1B4F9304" );
            // Remove Block: Group List Personalized Lava, from Page: Group Attendance, Site: External Website
            RockMigrationHelper.DeleteBlock( "F66FF6E0-B345-426A-B2C0-A2AA59E11771" );
            // Remove Block: Group Finder, from Page: Small Groups, Site: External Website
            RockMigrationHelper.DeleteBlock( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571" );
            // Remove Block: Group Attendance List, from Page: Group Attendance, Site: External Website
            RockMigrationHelper.DeleteBlock( "82B107C3-AF97-4476-879C-198C56100C73" );
            // Remove Block: Group Detail Lava, from Page: Group Toolbox, Site: External Website
            RockMigrationHelper.DeleteBlock( "1EDDD8D0-5B7A-4148-8AE6-182863048AC5" );
            // Remove Block: Group List Personalized Lava, from Page: Group Toolbox, Site: External Website
            RockMigrationHelper.DeleteBlock( "FA8760D1-D158-412D-ADAC-57362420C296" );
            // Remove Block: Group List Personalized Lava, from Page: My Account, Site: External Website
            RockMigrationHelper.DeleteBlock( "8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8" );
            RockMigrationHelper.DeleteBlockType( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7" ); // Group Registration
            RockMigrationHelper.DeleteBlockType( "1B172C33-8672-4C98-A995-8E123FF316BD" ); // Group List Personalized Lava
            RockMigrationHelper.DeleteBlockType( "9F8F2D68-DEEA-4686-810F-AB32923F855E" ); // Group Finder
            RockMigrationHelper.DeleteBlockType( "218B057F-B214-4317-8E84-7A95CF88067E" ); // Group Detail Lava
            RockMigrationHelper.DeletePage( "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1" ); //  Page: Group Attendance, Layout: LeftSidebar, Site: External Website
            RockMigrationHelper.DeletePage( "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38" ); //  Page: Group Toolbox, Layout: LeftSidebar, Site: External Website

            // Attrib for BlockType: Group Registration:Record Status
            RockMigrationHelper.DeleteAttribute( "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC" );
            // Attrib for BlockType: Group Registration:Connection Status
            RockMigrationHelper.DeleteAttribute( "E3544F7A-0E7A-421B-9142-AE858E9CCFBB" );
            // Attrib for BlockType: Group Registration:Group Member Status
            RockMigrationHelper.DeleteAttribute( "B61174DE-6E7D-4171-9067-6A7981F888E8" );
            // Attrib for BlockType: Group Registration:Mode
            RockMigrationHelper.DeleteAttribute( "EDEC82F3-C9E1-4B26-862D-E896F6C26376" );
            // Attrib for BlockType: Group Registration:Result Lava Template
            RockMigrationHelper.DeleteAttribute( "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1" );
            // Attrib for BlockType: Group Registration:Result Page
            RockMigrationHelper.DeleteAttribute( "BAD40ACB-CC0B-4CE4-A3B8-E7C5134AE0E2" );
            // Attrib for BlockType: Group Registration:Lava Template
            RockMigrationHelper.DeleteAttribute( "8E37CB4A-AF69-4671-9EC6-2ED72380B749" );
            // Attrib for BlockType: Group Registration:Workflow
            RockMigrationHelper.DeleteAttribute( "C4287E3F-D2D8-413E-A3AE-F9A3EE7A5021" );
            // Attrib for BlockType: Group Registration:Enable Debug
            RockMigrationHelper.DeleteAttribute( "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A" );
            // Attrib for BlockType: Group Simple Register:Confirmation Email
            RockMigrationHelper.DeleteAttribute( "BA40D736-AB27-4972-99FD-6B2C64C28B21" );
            // Attrib for BlockType: Group Simple Register:Confirmation Page
            RockMigrationHelper.DeleteAttribute( "7255CA32-9D1E-40D8-98CD-325613F2AD33" );
            // Attrib for BlockType: Group Simple Register:Record Status
            RockMigrationHelper.DeleteAttribute( "86AEDC3E-37A7-42EE-8451-911FF127BFE2" );
            // Attrib for BlockType: Group Simple Register:Connection Status
            RockMigrationHelper.DeleteAttribute( "16B0D253-F2E8-40B8-A99F-9DA4F815A644" );
            // Attrib for BlockType: Group Simple Register:Save Button Text
            RockMigrationHelper.DeleteAttribute( "318E9C84-62F9-4666-86FA-EF13BDF77E49" );
            // Attrib for BlockType: Group Simple Register:Group
            RockMigrationHelper.DeleteAttribute( "F9DD30B1-DC64-4782-803C-D35D34807F3D" );
            // Attrib for BlockType: Group Simple Register:Success Message
            RockMigrationHelper.DeleteAttribute( "76112491-8806-4081-AA89-FDBEF5B27BBB" );
            // Attrib for BlockType: Group Finder:Show Schedule
            RockMigrationHelper.DeleteAttribute( "E13FC044-C674-4059-AAAB-099B0B636F0A" );
            // Attrib for BlockType: Group Finder:ScheduleFilters
            RockMigrationHelper.DeleteAttribute( "B9C26CFD-CCC8-43DA-9DB6-6EAAF6792A82" );
            // Remove Block: Group Registration, from Page: Group Registration, Site: External Website
            RockMigrationHelper.DeleteBlock( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1" );
            // Remove Block: Page Menu, from Page: Group Registration, Site: External Website
            RockMigrationHelper.DeleteBlock( "02ABF2C1-0D34-45C2-9F0F-228954B13A52" );
            RockMigrationHelper.DeletePage( "7D24FE9A-710C-4B25-B1C7-76161ED78DB8" ); //  Page: Group Registration, Layout: LeftSidebar, Site: External Website



        }
    }
}
