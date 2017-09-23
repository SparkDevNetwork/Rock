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
    public partial class Communications2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Data Views", "", "Rock", "Rock.Field.Types.DataViewsFieldType", "F739BF5D-3FDC-45EC-A03C-1AE7C47E3883" );
            RockMigrationHelper.UpdateFieldType( "Communication Preference", "", "Rock", "Rock.Field.Types.CommunicationPreferenceFieldType", "507C28F2-8BC0-4909-A4FE-9C2B1149E2B2" );

            DropForeignKey( "dbo.Communication", "MediumEntityTypeId", "dbo.EntityType");
            Sql( @"
IF object_id(N'[dbo].[FK_dbo.Communication_dbo.EntityType_ChannelEntityTypeId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[Communication] DROP CONSTRAINT [FK_dbo.Communication_dbo.EntityType_ChannelEntityTypeId]
" );
            DropForeignKey("dbo.CommunicationTemplate", "MediumEntityTypeId", "dbo.EntityType");
            Sql( @"
IF object_id(N'[dbo].[FK_dbo.CommunicationTemplate_dbo.EntityType_ChannelEntityTypeId]', N'F') IS NOT NULL
    ALTER TABLE [dbo].[CommunicationTemplate] DROP CONSTRAINT [FK_dbo.CommunicationTemplate_dbo.EntityType_ChannelEntityTypeId]
" );
            DropIndex("dbo.Communication", new[] { "MediumEntityTypeId" });
            DropIndex("dbo.CommunicationTemplate", new[] { "MediumEntityTypeId" });
            CreateTable(
                "dbo.CommunicationAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BinaryFileId = c.Int(nullable: false),
                        CommunicationId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.Communication", t => t.CommunicationId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CommunicationId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.CommunicationTemplateAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BinaryFileId = c.Int(nullable: false),
                        CommunicationTemplateId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.CommunicationTemplate", t => t.CommunicationTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CommunicationTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Person", "CommunicationPreference", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationRecipient", "MediumEntityTypeId", c => c.Int());
            AddColumn("dbo.Communication", "Name", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "CommunicationType", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "UrlReferrer", c => c.String(maxLength: 200));
            AddColumn("dbo.Communication", "ListGroupId", c => c.Int());
            AddColumn("dbo.Communication", "Segments", c => c.String());
            AddColumn("dbo.Communication", "SegmentCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "CommunicationTemplateId", c => c.Int());
            AddColumn("dbo.Communication", "FromName", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "FromEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "ReplyToEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "CCEmails", c => c.String());
            AddColumn("dbo.Communication", "BCCEmails", c => c.String());
            AddColumn("dbo.Communication", "Message", c => c.String());
            AddColumn("dbo.Communication", "MessageMetaData", c => c.String());
            AddColumn("dbo.Communication", "SMSFromDefinedValueId", c => c.Int());
            AddColumn("dbo.Communication", "SMSMessage", c => c.String());
            AddColumn("dbo.Communication", "PushTitle", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "PushMessage", c => c.String());
            AddColumn("dbo.Communication", "PushSound", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "IsSystem", c => c.Boolean(nullable: false));
            AddColumn("dbo.CommunicationTemplate", "ImageFileId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "FromName", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "FromEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "ReplyToEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "CCEmails", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "BCCEmails", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "Message", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "MessageMetaData", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "SMSFromDefinedValueId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "SMSMessage", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "PushTitle", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "PushMessage", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "PushSound", c => c.String(maxLength: 100));
            CreateIndex("dbo.CommunicationRecipient", "MediumEntityTypeId");
            CreateIndex("dbo.Communication", "CommunicationTemplateId");
            CreateIndex("dbo.Communication", "SMSFromDefinedValueId");
            CreateIndex("dbo.CommunicationTemplate", "SMSFromDefinedValueId");
            AddForeignKey("dbo.CommunicationTemplate", "SMSFromDefinedValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.CommunicationRecipient", "MediumEntityTypeId", "dbo.EntityType", "Id");

            // AddForeignKey("dbo.Communication", "CommunicationTemplateId", "dbo.CommunicationTemplate", "Id");
            // Instead of AddForeignKey, do it manually so it can be a ON DELETE SET NULL
            Sql( @"ALTER TABLE dbo.Communication ADD CONSTRAINT [FK_dbo.Communication_dbo.CommunicationTemplate_CommunicationTemplateId] 
                    FOREIGN KEY (CommunicationTemplateId) REFERENCES dbo.CommunicationTemplate (Id) ON DELETE SET NULL" );

            Sql( @"
    UPDATE C 
        SET [CommunicationType] = CASE E.[Name]
            WHEN 'Rock.Communication.Medium.Email' THEN 1
            WHEN 'Rock.Communication.Medium.Sms' THEN 2
            WHEN 'Rock.Communication.Medium.PushNotification' THEN 3
        END
    FROM [Communication] C
    INNER JOIN [EntityType] E ON E.[Id] = C.[MediumEntityTypeId]
" );

            DropColumn( "dbo.Communication", "MediumEntityTypeId");
            DropColumn("dbo.CommunicationTemplate", "MediumEntityTypeId");

            Sql( MigrationSQL._201709082257551_Communications2_AddCommunicationTemplates );

            // group type
            RockMigrationHelper.AddGroupType( "Communication List", "For groups used by Rock's communication tools for storing lists of people to communicate to.", "List", "Recipient", false, true, false, "fa fa-bullhorn", 0, null, 0, null, "D1D95777-FFA3-CBB3-4A6D-658706DAED33" );
            RockMigrationHelper.AddGroupTypeRole( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", "Recipient", "", 0, null, null, "9D85AB4E-59BC-B48A-494A-5684BA41578E", true, false, true );

            // group attribute
            RockMigrationHelper.AddGroupTypeGroupAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.DATAVIEWS, "Communication Segments", "Additional Communication Segments to be presented when this communication list is selected.", 0, "", "73A53BC1-2178-46A1-8413-C7A4DD49F0B4" );
            RockMigrationHelper.AddAttributeQualifier( "73A53BC1-2178-46A1-8413-C7A4DD49F0B4", "entityTypeName", "Rock.Model.Person", "37C5CD82-C4D2-4B58-BBCB-D2D59EF4B200" );

            // group member attribute
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.COMMUNICATION_PREFERENCE_TYPE, "Preferred Communication Type", "The preferred communication type for this group member. Select None to use the person's default communication preference.", 0, "0", "D7941908-1F65-CC9B-416C-CCFABE4221B9" );
            RockMigrationHelper.AddAttributeQualifier( "D7941908-1F65-CC9B-416C-CCFABE4221B9", "allowmultiple", "False", "AEB4720A-B053-8D92-407E-9B29564882D2" );
            RockMigrationHelper.AddAttributeQualifier( "D7941908-1F65-CC9B-416C-CCFABE4221B9", "displaydescription", "False", "3A85C857-43E1-6586-41E5-9E9DF7D7D6B0" );

            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D7941908-1F65-CC9B-416C-CCFABE4221B9' )
                  DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE[Guid] = 'BCBE1494-23F5-3683-4EC5-C0B5CACE8A5A' )

                  INSERT INTO[AttributeQualifier]
                  ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                  VALUES
                  ( 0, @AttributeId, 'definedtype', @DefinedTypeId, newid() )" );

            // group attribute of category
            RockMigrationHelper.AddGroupTypeGroupAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.CATEGORY, "Category", "The category for the communication list.", 0, "", "E3810936-182E-2585-4F8E-030A0E18B27A" );
            RockMigrationHelper.AddAttributeQualifier( "E3810936-182E-2585-4F8E-030A0E18B27A", "entityTypeName", "Rock.Model.Group", "20AA23EC-B732-B2A8-444E-60CA6FB3C986" );
            RockMigrationHelper.AddAttributeQualifier( "E3810936-182E-2585-4F8E-030A0E18B27A", "qualifierColumn", "GroupTypeId", "0DFD7DA3-1B68-9E9B-43D6-C415753D6718" );
            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E3810936-182E-2585-4F8E-030A0E18B27A' )
                  DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE[Guid] = 'D1D95777-FFA3-CBB3-4A6D-658706DAED33' )

                  INSERT INTO[AttributeQualifier]
                  ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                  VALUES
                  ( 0, @AttributeId, 'qualifierValue', @GroupTypeId, newid() )" );


            // pages
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication List Categories", "", "307570FD-9472-48D5-A67F-80B2056C5308", "fa fa-folder" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Lists", "", "002C9991-523A-478C-B19B-E9DF2B977481", "fa fa-bullhorn" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "002C9991-523A-478C-B19B-E9DF2B977481", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication List Detail", "", "60216406-5BD6-4253-B891-262717C07A00", "fa fa-bullhorn" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "60216406-5BD6-4253-B891-262717C07A00", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail", "", "FB3FCA8D-2011-42B5-A9F4-2657C4F856AC", "" ); // Site:Rock RMS

            // Add Block to Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "307570FD-9472-48D5-A67F-80B2056C5308", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", "", "", 0, "25F82ADE-BD0A-404C-A659-30874AFC50A1" );
            // Add Block to Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "002C9991-523A-478C-B19B-E9DF2B977481", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Group List", "Main", "", "", 0, "426EC86B-5784-411D-94ED-DD007E6DF783" );
            // Add Block to Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "60216406-5BD6-4253-B891-262717C07A00", "", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Group Detail", "Main", "", "", 0, "3FF79A87-ABC1-4DE3-B25A-8111E5D05607" );
            // Add Block to Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "60216406-5BD6-4253-B891-262717C07A00", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "Main", "", "", 1, "B906C477-BFA2-4617-BCE4-B7A1D3D8042C" );
            // Add Block to Page: Group Member Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "FB3FCA8D-2011-42B5-A9F4-2657C4F856AC", "", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "Main", "", "", 0, "550684A1-D34C-4198-B87E-5BC5C644A920" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '3FF79A87-ABC1-4DE3-B25A-8111E5D05607'" );  // Page: Communication List Detail,  Zone: Main,  Block: Group Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '3FF79A87-ABC1-4DE3-B25A-8111E5D05607'" );  // Page: Communication List Detail,  Zone: Main,  Block: Group Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B906C477-BFA2-4617-BCE4-B7A1D3D8042C'" );  // Page: Communication List Detail,  Zone: Main,  Block: Group Member List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B906C477-BFA2-4617-BCE4-B7A1D3D8042C'" );  // Page: Communication List Detail,  Zone: Main,  Block: Group Member List


            // Attrib Value for Block:Categories, Attribute:Entity Type Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "C405A507-7889-4287-8342-105B89710044", @"9bbfda11-0d22-40d5-902f-60adfbc88987" );
            // Attrib Value for Block:Categories, Attribute:Enable Hierarchy Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "F3370A76-E1D1-47FD-AE90-1D428183235C", @"True" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Column Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "E9E2BE91-5D5E-4688-A6AD-A4AAD3D629E2", @"GroupTypeId" );
            // Attrib Value for Block:Categories, Attribute:Entity Qualifier Value Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "25F82ADE-BD0A-404C-A659-30874AFC50A1", "E3CC4A91-697C-4269-8AA8-E1F1A63F04D8", @"31" );
            // Attrib Value for Block:Group List, Attribute:Display Member Count Column Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "FDD84597-E3E8-4E91-A72F-C6538B085310", @"True" );
            // Attrib Value for Block:Group List, Attribute:Limit to Active Status Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "B4133552-42B6-4053-90B9-33B882B72D2D", @"all" );
            // Attrib Value for Block:Group List, Attribute:Display Group Path Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "6F229535-B44E-44C2-A9AF-28244600E244", @"False" );
            // Attrib Value for Block:Group List, Attribute:Display Filter Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"True" );
            // Attrib Value for Block:Group List, Attribute:Include Group Types Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"d1d95777-ffa3-cbb3-4a6d-658706daed33" );
            // Attrib Value for Block:Group List, Attribute:Exclude Group Types Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"" );
            // Attrib Value for Block:Group List, Attribute:Display Group Type Column Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"False" );
            // Attrib Value for Block:Group List, Attribute:Display Description Column Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"True" );
            // Attrib Value for Block:Group List, Attribute:Limit to Security Role Groups Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );
            // Attrib Value for Block:Group List, Attribute:Detail Page Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"60216406-5bd6-4253-b891-262717c07a00" );
            // Attrib Value for Block:Group List, Attribute:Display System Column Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"False" );
            // Attrib Value for Block:Group List, Attribute:Display Active Status Column Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "426EC86B-5784-411D-94ED-DD007E6DF783", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"True" );
            // Attrib Value for Block:Group Detail, Attribute:Group Map Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "69F9C989-456D-4855-A420-050DB8B9FEB7", @"" );
            // Attrib Value for Block:Group Detail, Attribute:Map Style Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "E50B6C24-930C-4D9C-BD94-0AD6BC018C4D", @"fdc5d6ba-a818-4a06-96b1-9ef31b4087ac" );
            // Attrib Value for Block:Group Detail, Attribute:Group Types Exclude Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "85EE581F-D246-498A-B857-5AD33EC3CAEA", @"" );
            // Attrib Value for Block:Group Detail, Attribute:Registration Instance Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "36643FFE-C49F-443E-8C3D-E83324A45822", @"844dc54b-daec-47b3-a63a-712dd6d57793" );
            // Attrib Value for Block:Group Detail, Attribute:Event Item Occurrence Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "6114CE99-C97F-4394-93F5-B34D479AB54E", @"4b0c44ee-28e3-4753-a95b-8c57cd958fd1" );
            // Attrib Value for Block:Group Detail, Attribute:Content Item Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "45897721-F38C-4B4B-BCF9-A81D27DBB731", @"d18e837c-9e65-4a38-8647-dff04a595d97" );
            // Attrib Value for Block:Group Detail, Attribute:Show Edit Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "50C7E223-459E-4A1C-AE3C-2892CBD40D22", @"True" );

            // Attrib Value for Block:Group Detail, Attribute:Group Types Include Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", @"d1d95777-ffa3-cbb3-4a6d-658706daed33" );

            // Attrib Value for Block:Group Detail, Attribute:Limit to Security Role Groups Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "12295C7E-08F4-4AC5-8A34-C829620FC0B1", @"False" );
            // Attrib Value for Block:Group Detail, Attribute:Limit to Group Types that are shown in navigation Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607", "62B0099E-B1A3-4468-B821-B96AB088A861", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"fb3fca8d-2011-42b5-a9f4-2657c4f856ac" );
            // Attrib Value for Block:Group Member List, Attribute:Person Profile Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            // Attrib Value for Block:Group Member List, Attribute:Registration Page Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "EDF79295-04A4-42B4-B382-DDEF5888D565", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            // Attrib Value for Block:Group Member List, Attribute:Show Campus Filter Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "5796D8C1-0F65-48C2-8920-8C9521E974FF", @"True" );
            // Attrib Value for Block:Group Member List, Attribute:Show First/Last Attendance Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "3BFE216F-9CAC-42FF-AC62-427557351F31", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Date Added Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C", "7BC3F3B7-8354-4B1C-B8F8-DEDEC5D8A0BD", @"False" );

            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.DATAVIEW, "Communication Segments", string.Empty, "Dataviews that can be used to refine a communication recipient list when creating a communication", "FF7081F8-7223-43D4-BE28-CB030DC4E13B" );

            // Create [GroupAll] DataViewFilter for DataView: 35 and older
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: 35 and older
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '256|35|,' for Rock.Reporting.DataFilter.Person.AgeFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '90563175-E8C6-4845-9D60-78C0BBD5A9BE') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'256|35|,','90563175-E8C6-4845-9D60-78C0BBD5A9BE')
END
" );
            // Create DataView: 35 and older
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = '5537D54C-1B9B-4B81-AA63-F10D676FAE77') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'35 and older','A filter to help refine a communications recipient list to include only people that are 35 and older',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'5537D54C-1B9B-4B81-AA63-F10D676FAE77')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Female
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','01CDD06D-810E-4861-85EE-69FB8C97EA3C')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Female
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '[
              "Property_Gender",
              "Female"
            ]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'CD9ADB66-67E5-4F3E-A481-13EE257407EC') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""Property_Gender"",
  ""Female""
]','CD9ADB66-67E5-4F3E-A481-13EE257407EC')
END
" );
            // Create DataView: Female
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Female','A filter to help refine a communications recipient list to include only females',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Male
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','348FF1A5-2D80-4FC6-86AF-0FC3C117982A')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Male
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '[
              "Property_Gender",
              "Male"
            ]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '03D9CA62-4F58-43C6-A508-AE8597B27539') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""Property_Gender"",
  ""Male""
]','03D9CA62-4F58-43C6-A508-AE8597B27539')
END
" );
            // Create DataView: Male
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'C43983D7-1F22-4E94-9F5C-342DA3A0E168') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Male','A filter to help refine a communications recipient list to include only males',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'C43983D7-1F22-4E94-9F5C-342DA3A0E168')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Under 35
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','8760D688-01B9-4030-BF19-898D68CBA757')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Under 35
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '512|35|,' for Rock.Reporting.DataFilter.Person.AgeFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '9423C06E-C5D3-4589-8EA7-813A6F59F13B') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'512|35|,','9423C06E-C5D3-4589-8EA7-813A6F59F13B')
END
" );
            // Create DataView: Under 35
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'FF608842-BB10-4C9C-AA18-9D5C407590D3') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Under 35','A filter to help refine a communications recipient list to include only people that under age of 35',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'FF608842-BB10-4C9C-AA18-9D5C407590D3')
END
" );

            // Job for Migrating Interaction Data
            Sql( @"
    INSERT INTO [dbo].[ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus] ,[Guid] )
    VALUES ( 0, 1, 'Convert communication medium data', 'Converts communication medium data to field values.', 
        'Rock.Jobs.MigrateCommunicationMediumData', '0 0 3 1/1 * ? *', 3, 'E7C54AAB-451E-4E89-8083-CF398D37416E')" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteGroupTypeRole( "9D85AB4E-59BC-B48A-494A-5684BA41578E" );
            RockMigrationHelper.DeleteGroupType( "D1D95777-FFA3-CBB3-4A6D-658706DAED33" );
            RockMigrationHelper.DeleteAttribute( "E3810936-182E-2585-4F8E-030A0E18B27A" );
            RockMigrationHelper.DeleteAttribute( "D7941908-1F65-CC9B-416C-CCFABE4221B9" );
            RockMigrationHelper.DeleteAttribute( "73A53BC1-2178-46A1-8413-C7A4DD49F0B4" );

            // Delete DataView: 35 and older
            Sql( @"DELETE FROM DataView where [Guid] = '5537D54C-1B9B-4B81-AA63-F10D676FAE77'" );
            // Delete DataViewFilter for DataView: 35 and older
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '90563175-E8C6-4845-9D60-78C0BBD5A9BE'" );
            // Delete DataViewFilter for DataView: 35 and older
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'" );

            // Delete DataView: Female
            Sql( @"DELETE FROM DataView where [Guid] = 'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3'" );
            // Delete DataViewFilter for DataView: Female
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'CD9ADB66-67E5-4F3E-A481-13EE257407EC'" );
            // Delete DataViewFilter for DataView: Female
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'" );

            // Delete DataView: Male
            Sql( @"DELETE FROM DataView where [Guid] = 'C43983D7-1F22-4E94-9F5C-342DA3A0E168'" );
            // Delete DataViewFilter for DataView: Male
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '03D9CA62-4F58-43C6-A508-AE8597B27539'" );
            // Delete DataViewFilter for DataView: Male
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'" );

            // Delete DataView: Under 35
            Sql( @"DELETE FROM DataView where [Guid] = 'FF608842-BB10-4C9C-AA18-9D5C407590D3'" );
            // Delete DataViewFilter for DataView: Under 35
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '9423C06E-C5D3-4589-8EA7-813A6F59F13B'" );
            // Delete DataViewFilter for DataView: Under 35
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'" );


            // Remove Block: Group Member Detail, from Page: Group Member Detail, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "550684A1-D34C-4198-B87E-5BC5C644A920" );
            // Remove Block: Group Member List, from Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "B906C477-BFA2-4617-BCE4-B7A1D3D8042C" );
            // Remove Block: Group Detail, from Page: Communication List Detail, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "3FF79A87-ABC1-4DE3-B25A-8111E5D05607" );
            // Remove Block: Group List, from Page: Communication Lists, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "426EC86B-5784-411D-94ED-DD007E6DF783" );
            // Remove Block: Categories, from Page: Communication List Categories, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "25F82ADE-BD0A-404C-A659-30874AFC50A1" );


            RockMigrationHelper.DeletePage( "FB3FCA8D-2011-42B5-A9F4-2657C4F856AC" ); //  Page: Group Member Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "60216406-5BD6-4253-B891-262717C07A00" ); //  Page: Communication List Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "002C9991-523A-478C-B19B-E9DF2B977481" ); //  Page: Communication Lists, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "307570FD-9472-48D5-A67F-80B2056C5308" ); //  Page: Communication List Categories, Layout: Full Width, Site: Rock RMS


            AddColumn( "dbo.CommunicationTemplate", "MediumEntityTypeId", c => c.Int());
            AddColumn("dbo.Communication", "MediumEntityTypeId", c => c.Int());
            DropForeignKey("dbo.CommunicationRecipient", "MediumEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Communication", "CommunicationTemplateId", "dbo.CommunicationTemplate");
            DropForeignKey("dbo.CommunicationTemplate", "SMSFromDefinedValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.CommunicationTemplateAttachment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationTemplateAttachment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationTemplateAttachment", "CommunicationTemplateId", "dbo.CommunicationTemplate");
            DropForeignKey("dbo.CommunicationTemplateAttachment", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.CommunicationAttachment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationAttachment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationAttachment", "CommunicationId", "dbo.Communication");
            DropForeignKey("dbo.CommunicationAttachment", "BinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.CommunicationTemplateAttachment", new[] { "Guid" });
            DropIndex("dbo.CommunicationTemplateAttachment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationTemplateAttachment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationTemplateAttachment", new[] { "CommunicationTemplateId" });
            DropIndex("dbo.CommunicationTemplateAttachment", new[] { "BinaryFileId" });
            DropIndex("dbo.CommunicationTemplate", new[] { "SMSFromDefinedValueId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "Guid" });
            DropIndex("dbo.CommunicationAttachment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "CommunicationId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "BinaryFileId" });
            DropIndex("dbo.Communication", new[] { "SMSFromDefinedValueId" });
            DropIndex("dbo.Communication", new[] { "CommunicationTemplateId" });
            DropIndex("dbo.CommunicationRecipient", new[] { "MediumEntityTypeId" });
            DropColumn("dbo.CommunicationTemplate", "PushSound");
            DropColumn("dbo.CommunicationTemplate", "PushMessage");
            DropColumn("dbo.CommunicationTemplate", "PushTitle");
            DropColumn("dbo.CommunicationTemplate", "SMSMessage");
            DropColumn("dbo.CommunicationTemplate", "SMSFromDefinedValueId");
            DropColumn("dbo.CommunicationTemplate", "MessageMetaData");
            DropColumn("dbo.CommunicationTemplate", "Message");
            DropColumn("dbo.CommunicationTemplate", "BCCEmails");
            DropColumn("dbo.CommunicationTemplate", "CCEmails");
            DropColumn("dbo.CommunicationTemplate", "ReplyToEmail");
            DropColumn("dbo.CommunicationTemplate", "FromEmail");
            DropColumn("dbo.CommunicationTemplate", "FromName");
            DropColumn("dbo.CommunicationTemplate", "ImageFileId");
            DropColumn("dbo.CommunicationTemplate", "IsSystem");
            DropColumn("dbo.Communication", "PushSound");
            DropColumn("dbo.Communication", "PushMessage");
            DropColumn("dbo.Communication", "PushTitle");
            DropColumn("dbo.Communication", "SMSMessage");
            DropColumn("dbo.Communication", "SMSFromDefinedValueId");
            DropColumn("dbo.Communication", "MessageMetaData");
            DropColumn("dbo.Communication", "Message");
            DropColumn("dbo.Communication", "BCCEmails");
            DropColumn("dbo.Communication", "CCEmails");
            DropColumn("dbo.Communication", "ReplyToEmail");
            DropColumn("dbo.Communication", "FromEmail");
            DropColumn("dbo.Communication", "FromName");
            DropColumn("dbo.Communication", "CommunicationTemplateId");
            DropColumn("dbo.Communication", "SegmentCriteria");
            DropColumn("dbo.Communication", "Segments");
            DropColumn("dbo.Communication", "ListGroupId");
            DropColumn("dbo.Communication", "UrlReferrer");
            DropColumn("dbo.Communication", "CommunicationType");
            DropColumn("dbo.Communication", "Name");
            DropColumn("dbo.CommunicationRecipient", "MediumEntityTypeId");
            DropColumn("dbo.Person", "CommunicationPreference");
            DropTable("dbo.CommunicationTemplateAttachment");
            DropTable("dbo.CommunicationAttachment");
            CreateIndex("dbo.CommunicationTemplate", "MediumEntityTypeId");
            CreateIndex("dbo.Communication", "MediumEntityTypeId");
            AddForeignKey("dbo.CommunicationTemplate", "MediumEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.Communication", "MediumEntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
