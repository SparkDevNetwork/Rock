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
    public partial class CommunicationUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // defined type of mediums
            RockMigrationHelper.AddDefinedType( "Communication", "Communication Mediums", "Used by Rock's communication features for selecting which mediums a user prefers.", "BCBE1494-23F5-3683-4EC5-C0B5CACE8A5A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BCBE1494-23F5-3683-4EC5-C0B5CACE8A5A", SystemGuid.FieldType.TEXT, "Icon CSS Class", "IconCssClass", "An icon to use for the communication medium.", 0, "", "CA8DF904-5281-86B2-433E-3C914F1A106A" );

            RockMigrationHelper.AddDefinedValue( "BCBE1494-23F5-3683-4EC5-C0B5CACE8A5A", "Email", "", "0A299F1D-D703-FDBA-4826-7D0CA03A71A6", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0A299F1D-D703-FDBA-4826-7D0CA03A71A6", "CA8DF904-5281-86B2-433E-3C914F1A106A", "fa fa-envelope-o" );

            RockMigrationHelper.AddDefinedValue( "BCBE1494-23F5-3683-4EC5-C0B5CACE8A5A", "SMS", "", "D5F097D9-0415-9584-4019-681C77FD1513", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D5F097D9-0415-9584-4019-681C77FD1513", "CA8DF904-5281-86B2-433E-3C914F1A106A", "fa fa-mobile" );

            // group type
            RockMigrationHelper.AddGroupType( "Communication List", "For groups used by Rock's communication tools for storing lists of people to communicate to.", "List", "Recipient", false, true, false, "fa fa-bullhorn", 0, null, 0, null, "D1D95777-FFA3-CBB3-4A6D-658706DAED33" );

            RockMigrationHelper.AddGroupTypeRole( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", "Recipient", "", 0, null, null, "9D85AB4E-59BC-B48A-494A-5684BA41578E", true, false, true );

            // group member attribute
            RockMigrationHelper.AddGroupTypeGroupAttribute( "D1D95777-FFA3-CBB3-4A6D-658706DAED33", SystemGuid.FieldType.DEFINED_VALUE, "Preferred Medium", "Used to store the user's preference for the sending medium for the communication list.", 0, "", "D7941908-1F65-CC9B-416C-CCFABE4221B9" );
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
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
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
        }
    }
}
