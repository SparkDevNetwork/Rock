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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class DataIntegrityFeatures : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // move current reports under new category 'Data Integrity'
            Sql( @"
                    DECLARE @RootCategoryId INT
                    SET @RootCategoryId = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'b88e45fc-c4f8-487f-ab16-9e30157da967')

                    INSERT INTO [Category] 
                      ([ParentCategoryId]
                      , [IsSystem]
                      , [EntityTypeId]
                      , [Name]
                      , [IconCssClass]
                      , [Guid]
                      , [Order]
                      , [Description])
                      VALUES (
                      @RootCategoryId
                      , 0
                      , 107
                      , 'Data Integrity'
                      , 'fa fa-magic'
                      , 'd738d12d-bc3b-47b0-8a90-f7924d137595'
                      , 100
                      , 'Category for grouping data integrity reports.')

                    DECLARE @NewCategoryId INT
                    SET @NewCategoryId = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'd738d12d-bc3b-47b0-8a90-f7924d137595')

                    UPDATE [Report]
                      SET [CategoryId] = @NewCategoryId
                    WHERE [Guid] = '87d3e118-ada8-4424-b63b-9482a7d9e609'" );


            // add data integrity user  security group
            Sql( @"
                INSERT INTO [Group]
                  (
                    [IsSystem]
                    , [GroupTypeId]
                    , [Name]
                    , [Description]
                    , [IsSecurityRole]
                    , [Guid]
                    , [IsActive]
                    , [Order]
                  )
                  VALUES (
                    1
                    , 1 
                    , 'Data Integrity User'
                    , 'Group of individuals who have access to Rock''s data integrity tools to help improve the quality of the data.'
                    , 1
                    , '40517e10-0f2d-4c61-aa8d-bde36d58c63a'
                    , 1
                    , 0
                  )" );

            // add admin user to the new role
            Sql( @"
                  DECLARE @DataIntegrityGroupId INT
                  SET @DataIntegrityGroupId = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '40517e10-0f2d-4c61-aa8d-bde36d58c63a')

                  DECLARE @GroupRoleId INT
                  SET @GroupRoleId = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '00f3ac1c-71b9-4ee5-a30e-4c48c8a0bf1f')

                  INSERT INTO [GroupMember]
                    ([IsSystem]
                     , [GroupId]
                     , [PersonId]
                     , [GroupRoleId]
                     , [GroupMemberStatus]
                     , [Guid]
                    )
                    VALUES (
                      1
                      , @DataIntegrityGroupId
                      , 1
                      , @GroupRoleId
                      , 1
                      , '3a1d0c2e-4ada-4b91-acbe-43bb161475cd'
                    )" );

            // secure report category
            Sql( @"
                DECLARE @EntityId INT
                SET @EntityId = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'd738d12d-bc3b-47b0-8a90-f7924d137595')

                DECLARE @EntityTypeId INT
                SET @EntityTypeId = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '1d68154e-ec76-44c8-9813-7736b27aecf9')

                DECLARE @DataIntegrityGroup INT
                SET @DataIntegrityGroup = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '40517e10-0f2d-4c61-aa8d-bde36d58c63a')

                INSERT INTO [Auth]
                  ([EntityTypeId]
                   , [EntityId]
                   , [Order]
                   , [Action]
                   , [AllowOrDeny]
                   , [SpecialRole]
                   , [PersonId]
                   , [GroupId]
                   , [Guid])
                  VALUES (
                    @EntityTypeId
                    , @EntityId
                    , 0
                    , 'View'
                    , 'A'
                    , 0
                    , NULL
                    , @DataIntegrityGroup
                    , '7b280913-8269-45af-a1a1-3a526c873e3f')

                  INSERT INTO [Auth]
                  ([EntityTypeId]
                   , [EntityId]
                   , [Order]
                   , [Action]
                   , [AllowOrDeny]
                   , [SpecialRole]
                   , [PersonId]
                   , [GroupId]
                   , [Guid])
                  VALUES (
                    @EntityTypeId
                    , @EntityId
                    , 1
                    , 'View'
                    , 'D'
                    , 1
                    , NULL
                    , NULL
                    , 'bee5445c-ee00-4b8b-b829-7bac5294d0e5')

                " );

            RockMigrationHelper.AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Data Integrity", "", "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "fa fa-magic" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Duplicate Finder", "", "21E94BF1-C594-44B6-AD91-939ABD04D36E", "fa fa-group" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reports", "", "134D8730-6AF5-4518-89EE-7370FA78676E", "fa fa-file-text-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "21E94BF1-C594-44B6-AD91-939ABD04D36E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Duplicate Detail", "", "6F9CE971-75DF-4F2A-BD5E-A12B149A442E", "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "134D8730-6AF5-4518-89EE-7370FA78676E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Report Detail", "", "DB58BC69-01FA-4F3E-832B-B1D0DE915C21", "fa fa-file-text-o" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Person Duplicate Detail", "Shows records that are possible duplicates of the selected person", "~/Blocks/Crm/PersonDuplicateDetail.ascx", "CRM", "A65CF2F8-93A4-4AC6-9018-D7C6996D9017" );
            RockMigrationHelper.UpdateBlockType( "Person Duplicate List", "List of person records that have possible duplicates", "~/Blocks/Crm/PersonDuplicateList.ascx", "CRM", "12D89810-23EB-4818-99A2-E076097DD979" );

            RockMigrationHelper.UpdateBlockType( "Report List", "Lists all reports under a specified report category.", "~/Blocks/Reporting/ReportList.ascx", "Reporting", "37D29989-F7CA-4D51-925A-378DB73ED53D" );

            // Add Block to Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlock( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "5A428636-5522-4AFD-8FDE-228F711E51C1" );

            // Add Block to Page: Duplicate Finder, Site: Rock RMS
            RockMigrationHelper.AddBlock( "21E94BF1-C594-44B6-AD91-939ABD04D36E", "", "12D89810-23EB-4818-99A2-E076097DD979", "Person Duplicate List", "Main", "", "", 0, "FE550CE4-26AA-469F-8B2E-B983350C68D5" );

            // Add Block to Page: Duplicate Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6F9CE971-75DF-4F2A-BD5E-A12B149A442E", "", "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "Person Duplicate Detail", "Main", "", "", 0, "427472B5-8D46-4344-B7F6-196CCFED7D0C" );

            // Add Block to Page: Data Integrity Reports, Site: Rock RMS
            RockMigrationHelper.AddBlock( "134D8730-6AF5-4518-89EE-7370FA78676E", "", "37D29989-F7CA-4D51-925A-378DB73ED53D", "Report List", "Main", "", "", 0, "2A8DB476-0DB5-4D2A-9E94-FB04E480750A" );

            // Add Block to Page: Report Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "DB58BC69-01FA-4F3E-832B-B1D0DE915C21", "", "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "Report Detail", "Main", "", "", 0, "9C86924D-FB2D-4C2D-9692-24F97C8F4A2E" );

            // Attrib for BlockType: Person Duplicate List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "F772A50B-DAEA-4501-84AD-AD7F1820AF0B" );

            // Attrib for BlockType: Person Duplicate List:Match Percent Low
            RockMigrationHelper.AddBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Match Percent Low", "MatchPercentLow", "", "The max percent score required to be considered an unlikely match", 0, @"40", "A2F381CC-3E61-453D-B6A4-747D41353DC8" );

            // Attrib for BlockType: Person Duplicate List:Match Percent High
            RockMigrationHelper.AddBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Match Percent High", "MatchPercentHigh", "", "The minimum percent score required to be considered a likely match", 0, @"80", "CB436515-4F2E-47E3-BAF1-8778345F2FD3" );

            // Attrib for BlockType: Person Duplicate Detail:Match Percent Low
            RockMigrationHelper.AddBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Match Percent Low", "MatchPercentLow", "", "The max percent score required to be considered an unlikely match", 0, @"40", "3BCDFEFB-42F9-4580-8B2C-03D1685047B5" );

            // Attrib for BlockType: Person Duplicate Detail:Match Percent High
            RockMigrationHelper.AddBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Match Percent High", "MatchPercentHigh", "", "The minimum percent score required to be considered a likely match", 0, @"80", "7A073396-CB37-4755-8EAB-DB60841EE98F" );

            // Attrib for BlockType: Report List:Report Category
            RockMigrationHelper.AddBlockTypeAttribute( "37D29989-F7CA-4D51-925A-378DB73ED53D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Report Category", "ReportCategory", "", "Category to use to list reports for.", 0, @"89e54497-5e98-4f1b-b83a-95bfb685da91", "3EADB984-8F0E-4C51-8ED5-DB7F592306A4" );

            // Attrib for BlockType: Report List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "37D29989-F7CA-4D51-925A-378DB73ED53D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Detail page to show report.", 1, @"", "4116F8D4-8D71-4B07-9073-6A9A733FB68B" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"84fd84df-f58b-4b9d-a407-96276c40ab7e" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5A428636-5522-4AFD-8FDE-228F711E51C1", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsBlocks' %}" );

            // Attrib Value for Block:Person Duplicate List, Attribute:Detail Page Page: Duplicate Finder, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FE550CE4-26AA-469F-8B2E-B983350C68D5", "F772A50B-DAEA-4501-84AD-AD7F1820AF0B", @"6f9ce971-75df-4f2a-bd5e-a12b149a442e" );

            // Attrib Value for Block:Person Duplicate List, Attribute:Match Percent Low Page: Duplicate Finder, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FE550CE4-26AA-469F-8B2E-B983350C68D5", "A2F381CC-3E61-453D-B6A4-747D41353DC8", @"40" );

            // Attrib Value for Block:Person Duplicate List, Attribute:Match Percent High Page: Duplicate Finder, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FE550CE4-26AA-469F-8B2E-B983350C68D5", "CB436515-4F2E-47E3-BAF1-8778345F2FD3", @"80" );

            // Attrib Value for Block:Report List, Attribute:Report Category Page: Data Integrity Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2A8DB476-0DB5-4D2A-9E94-FB04E480750A", "3EADB984-8F0E-4C51-8ED5-DB7F592306A4", @"d738d12d-bc3b-47b0-8a90-f7924d137595" );

            // Attrib Value for Block:Report List, Attribute:Detail Page Page: Data Integrity Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2A8DB476-0DB5-4D2A-9E94-FB04E480750A", "4116F8D4-8D71-4B07-9073-6A9A733FB68B", @"db58bc69-01fa-4f3e-832b-b1d0de915c21" );

            RockMigrationHelper.AddPage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflows", "", "90C32D5E-A5D5-4CE4-AAB0-E31B43B585E4", "fa fa-gears" ); // Site:Rock RMS
            // Add Block to Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlock( "90C32D5E-A5D5-4CE4-AAB0-E31B43B585E4", "", "DDC6B004-9ED1-470F-ABF5-041250082168", "Workflow Navigation", "Main", "", "", 0, "6AA6974C-4720-4A68-ABF4-84F488A383E1" );

            // Attrib Value for Block:Workflow Navigation, Attribute:Manage Page Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AA6974C-4720-4A68-ABF4-84F488A383E1", "6B8E6B05-87E6-4CA0-9A44-861184E3A34C", @"61e1b4b6-eace-42e8-a2fb-37465e6d0004" );

            // Attrib Value for Block:Workflow Navigation, Attribute:Entry Page Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AA6974C-4720-4A68-ABF4-84F488A383E1", "DABA0448-C967-4E9D-863E-59C95059935A", @"0550d2aa-a705-4400-81ff-ab124fdf83d7,88fbdde5-ef54-4202-9331-0d60c5df75d5" );

            // Attrib Value for Block:Workflow Navigation, Attribute:Categories Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AA6974C-4720-4A68-ABF4-84F488A383E1", "FB420F14-3D9D-4304-878F-124902E2CEAB", @"bbae05fd-8192-4616-a71e-903a927e0d90" );

            // Attrib Value for Block:Workflow Navigation, Attribute:Include Child Categories Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AA6974C-4720-4A68-ABF4-84F488A383E1", "61F01133-C84E-4380-ADE3-42EF894A3E2A", @"True" );


            // set security on data inetgrity pages
            Sql( @"
                    DECLARE @EntityId INT
                    SET @EntityId = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '84FD84DF-F58B-4B9D-A407-96276C40AB7E')

                    DECLARE @EntityTypeId INT
                    SET @EntityTypeId = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'E104DCDF-247C-4CED-A119-8CC51632761F')

                    DECLARE @DataIntegrityGroup INT
                    SET @DataIntegrityGroup = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '40517e10-0f2d-4c61-aa8d-bde36d58c63a')

                    INSERT INTO [Auth]
                        ([EntityTypeId]
                        , [EntityId]
                        , [Order]
                        , [Action]
                        , [AllowOrDeny]
                        , [SpecialRole]
                        , [PersonId]
                        , [GroupId]
                        , [Guid])
                        VALUES (
                        @EntityTypeId
                        , @EntityId
                        , 0
                        , 'View'
                        , 'A'
                        , 0
                        , NULL
                        , @DataIntegrityGroup
                        , 'A61FD7FB-823F-4382-BA17-90534DDE0836')

                        INSERT INTO [Auth]
                        ([EntityTypeId]
                        , [EntityId]
                        , [Order]
                        , [Action]
                        , [AllowOrDeny]
                        , [SpecialRole]
                        , [PersonId]
                        , [GroupId]
                        , [Guid])
                        VALUES (
                        @EntityTypeId
                        , @EntityId
                        , 1
                        , 'View'
                        , 'D'
                        , 1
                        , NULL
                        , NULL
                        , '4DCB8D77-959A-4EC1-A709-AB684F3FA519')
                " );

            // update the attributes shown on the workflow page to remove the data integrity ones
            Sql( @"UPDATE [AttributeValue]
                    SET [Value] = '78E38655-D951-41DB-A0FF-D6474775CFA1'
                    WHERE [Guid] = '751F6305-E49B-475C-AE44-23706E21D228'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {            
            // reset the categories of workflows show on the workflow page
            Sql( @"UPDATE [AttributeValue]
                    SET [Value] = 'bbae05fd-8192-4616-a71e-903a927e0d90,78e38655-d951-41db-a0ff-d6474775cfa1'
                    WHERE [Guid] = '751F6305-E49B-475C-AE44-23706E21D228'" );
            
            // delete security on data integrity pages
            Sql( @"DELETE FROM [Auth] 
                    WHERE [Guid] IN ('A61FD7FB-823F-4382-BA17-90534DDE0836','4DCB8D77-959A-4EC1-A709-AB684F3FA519')" );
            
            // delete security for report category
            Sql( @"
                DELETE FROM [Auth]
                    WHERE [Guid] IN ('bee5445c-ee00-4b8b-b829-7bac5294d0e5','7b280913-8269-45af-a1a1-3a526c873e3f')
                " );

            // delete data integrity security group
            Sql( @"
                DELETE FROM [Group] WHERE [Guid] = '40517e10-0f2d-4c61-aa8d-bde36d58c63a'
                " );

            // move data integrity report back to root category and delete the new category
            Sql( @"
                DECLARE @RootCategoryId INT
                    SET @RootCategoryId = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'b88e45fc-c4f8-487f-ab16-9e30157da967')

                
                    UPDATE [Report]
                      SET [CategoryId] = @RootCategoryId
                    WHERE [Guid] = '87d3e118-ada8-4424-b63b-9482a7d9e609'

                    DELETE FROM [Category] WHERE [Guid] = 'd738d12d-bc3b-47b0-8a90-f7924d137595'

                " );

            // Attrib for BlockType: Report List:Detail Page
            RockMigrationHelper.DeleteAttribute( "4116F8D4-8D71-4B07-9073-6A9A733FB68B" );

            // Attrib for BlockType: Report List:Report Category
            RockMigrationHelper.DeleteAttribute( "3EADB984-8F0E-4C51-8ED5-DB7F592306A4" );

            // Attrib for BlockType: Person Duplicate Detail:Match Percent High
            RockMigrationHelper.DeleteAttribute( "7A073396-CB37-4755-8EAB-DB60841EE98F" );

            // Attrib for BlockType: Person Duplicate Detail:Match Percent Low
            RockMigrationHelper.DeleteAttribute( "3BCDFEFB-42F9-4580-8B2C-03D1685047B5" );

            // Attrib for BlockType: Person Duplicate List:Match Percent High
            RockMigrationHelper.DeleteAttribute( "CB436515-4F2E-47E3-BAF1-8778345F2FD3" );

            // Attrib for BlockType: Person Duplicate List:Match Percent Low
            RockMigrationHelper.DeleteAttribute( "A2F381CC-3E61-453D-B6A4-747D41353DC8" );

            // Attrib for BlockType: Person Duplicate List:Detail Page
            RockMigrationHelper.DeleteAttribute( "F772A50B-DAEA-4501-84AD-AD7F1820AF0B" );

            // Remove Block: Report Detail, from Page: Report Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9C86924D-FB2D-4C2D-9692-24F97C8F4A2E" );

            // Remove Block: Report List, from Page: Data Integrity Reports, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2A8DB476-0DB5-4D2A-9E94-FB04E480750A" );

            // Remove Block: Person Duplicate Detail, from Page: Duplicate Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "427472B5-8D46-4344-B7F6-196CCFED7D0C" );

            // Remove Block: Person Duplicate List, from Page: Duplicate Finder, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FE550CE4-26AA-469F-8B2E-B983350C68D5" );

            // Remove Block: Page Menu, from Page: Data Integrity, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5A428636-5522-4AFD-8FDE-228F711E51C1" );
            RockMigrationHelper.DeleteBlockType( "37D29989-F7CA-4D51-925A-378DB73ED53D" ); // Report List
            RockMigrationHelper.DeleteBlockType( "12D89810-23EB-4818-99A2-E076097DD979" ); // Person Duplicate List
            RockMigrationHelper.DeleteBlockType( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017" ); // Person Duplicate Detail

            // Remove Block: Workflow Navigation, from Page: Workflows, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6AA6974C-4720-4A68-ABF4-84F488A383E1" );
            RockMigrationHelper.DeletePage( "90C32D5E-A5D5-4CE4-AAB0-E31B43B585E4" ); //  Page: Workflows, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeletePage( "DB58BC69-01FA-4F3E-832B-B1D0DE915C21" ); //  Page: Report Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6F9CE971-75DF-4F2A-BD5E-A12B149A442E" ); //  Page: Duplicate Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "134D8730-6AF5-4518-89EE-7370FA78676E" ); //  Page: Data Integrity Reports, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "21E94BF1-C594-44B6-AD91-939ABD04D36E" ); //  Page: Duplicate Finder, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E" ); //  Page: Data Integrity, Layout: Full Width, Site: Rock RMS
        }
    }
}
