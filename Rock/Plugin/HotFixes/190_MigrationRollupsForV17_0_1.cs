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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 190, "1.16.1" )]
    public class MigrationRollupsForV17_0_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddAdaptiveMessageRelatedPagesUp();
            RemoveLavaEngineFrameworkGlobalAttribute_Up();
            RemoveLavaEngineWarningBlockInstance();
            AddRawHtmlLavaStrucuturedContentToolUp();
            SortCMSPagesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: 2) Add Adaptive Message related Pages
        /// </summary>
        private void AddAdaptiveMessageRelatedPagesUp()
        {
            RockMigrationHelper.AddPage( true, "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Attributes", "", "222ED9E3-06C0-438F-B520-C899B8835650", "fa fa-list-ul" );
            RockMigrationHelper.AddPage( true, "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Messages", "", "73112D38-E051-4452-AEF9-E473EEDD0BCB", "fa fa-comment" );
            RockMigrationHelper.AddPage( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Detail", "", "BEC30E90-0434-43C4-B839-09E11775E497", "" );
            RockMigrationHelper.AddPage( true, "BEC30E90-0434-43C4-B839-09E11775E497", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Adaptation Detail", "", "FE12A90C-C20F-4F23-A1B1-528E0C5FDA83", "" );
            RockMigrationHelper.AddPageRoute( "222ED9E3-06C0-438F-B520-C899B8835650", "admin/cms/adaptive-message/attributes", "E612018C-FD4B-4F6F-9BCD-3B76B58CC8AB" );

            RockMigrationHelper.AddBlock( true, "222ED9E3-06C0-438F-B520-C899B8835650".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "791DB49B-58A4-44E1-AEF5-ABFF2F37E197".AsGuid(), "Attributes", "Main", @"", @"", 0, "F1233621-77CC-4CBE-AE21-9221B2EC4034" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD".AsGuid(), "Adaptive Message List", "Main", @"", @"", 0, "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1" );
            RockMigrationHelper.AddBlock( true, "BEC30E90-0434-43C4-B839-09E11775E497".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A81FE4E0-DF9F-4978-83A7-EB5459F37938".AsGuid(), "Adaptive Message Detail", "Main", @"", @"", 0, "8535DA65-D941-4ECE-9FAD-AA887361DF0E" );
            RockMigrationHelper.AddBlock( true, "FE12A90C-C20F-4F23-A1B1-528E0C5FDA83".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "113C4223-19B9-46F2-AAE8-AC646BC5A3C7".AsGuid(), "Adaptive Message Adaptation Detail", "Main", @"", @"", 0, "39A91338-89BC-4149-B642-C39732247A9E" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "9434F17F-F28C-4CEF-B65A-1A42CB7A17DC", @"39753cce-184a-4f14-ae80-08241de8fc2e" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "B921CB38-9D1F-4717-B4F9-D370BB8B3219", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "9AE7890A-1ABD-409C-9F3F-26D8497EC8EA", @"0" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "F09CAD11-1232-4F12-8875-BC22BA2A7693", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87", @"bec30e90-0434-43c4-b839-09e11775e497" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "A46D73FB-6BEF-492B-97B5-4A0351CC8286", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "8535DA65-D941-4ECE-9FAD-AA887361DF0E", "E4F286BC-9338-4E17-ABFF-4578793B7A54", @"fe12a90c-c20f-4f23-a1b1-528e0c5fda83" );
        }

        /// <summary>
        /// DL: Data Migration to Remove Lava Engine Global Attribute
        /// </summary>
        private void RemoveLavaEngineFrameworkGlobalAttribute_Up()
        {
        Sql( @"
            DELETE FROM [AttributeValue] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework' );
        " );
 
        Sql( @"
            DELETE FROM [AttributeQualifier] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework' );
        " );
 
        Sql( @"
            DELETE FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework';
        " );
        }

        /// <summary>
        /// KA: Remove Lava Engine Warning Block Instance
        /// </summary>
        private void RemoveLavaEngineWarningBlockInstance()
        {
            RockMigrationHelper.DeleteSecurityAuthForBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
            RockMigrationHelper.DeleteBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
        }

        /// <summary>
        /// DH: Add Raw structured editor tool.
        /// </summary>
        private void AddRawHtmlLavaStrucuturedContentToolUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS,
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'
        ],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'
        ],
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
        }

        /// <summary>
        /// PA: Sort CMS Config Pages
        /// </summary>
        private void SortCMSPagesUp()
        {
            // Remove page display options for Section Pages in CMS Config
            var cmsSectionPageGuids = new string[] {
                "\'CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89\'", // Website Configurations Section Page
                "\'889D7F7F-EB0F-40CD-9E80-E58A00EE69F7\'", // Content Channels Section Page
                "\'B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4\'", // Personalization Section Page
                "\'04FE297E-D45E-44EC-B521-181423F05A1C\'", // Content Platform Section Page
                "\'82726ACD-3480-4514-A920-FE920A71C046\'" // Digital Media Applications Section Page
            }.JoinStrings( ", " );

            Sql( $@"UPDATE [dbo].[Page]
                    SET [PageDisplayBreadCrumb] = 0,
                        [PageDisplayDescription] = 0,
                        [PageDisplayIcon] = 0,
                        [PageDisplayTitle] = 0,
                        [BreadCrumbDisplayName] = 0
                WHERE [Guid] IN ( 
                    {cmsSectionPageGuids}
                )" );

            // Set the order of the pages within the Sections
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '7596D389-4EAB-4535-8BEE-229737F46F44'" ); // Sites Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E'" ); // Pages Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1'" ); // File Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA'" ); // Routes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D'" ); // Block Types Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = 'BC2AFAEF-712C-4173-895E-81347F6B0B1C'" ); // Themes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 7 WHERE [Guid] = '39F928A5-1374-4380-B807-EADF145F18A1'" ); // HTTP Modules Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '8ADCE4B2-8E95-4FA3-89C4-06A883E8145E'" ); // Content Channels Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55'" ); // Content Channel Types Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '40875E7E-B912-43FF-892B-6161C21F130B'" ); // Content Collections Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = 'F1ED10C2-A17D-4310-9F86-76E11A4A7ED2'" ); // Content Component Templates Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = 'BBDE39C3-01C9-4C9E-9506-C2205508BC77'" ); // Content Channel Item Attribute Categories Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = '0F1B45B8-032D-4306-834D-670FA3933589'" ); // Content Channel Categories Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '905F6132-AE1C-4C85-9752-18D22E604C3A'" ); // Personalization Segments Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '511FC29A-EAF2-4AC0-B9B3-8613739A9ACF'" ); // Request Filters Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '6CFF2C81-6303-4477-A7EC-156DDBF8BE64'" ); // Lava Shortcodes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '07CB7BB5-1465-4E75-8DD4-28FA6EA48222'" ); // Media Accounts Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '37C20B91-737B-42D1-907D-9868104DBA7B'" ); // Persisted Datasets Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = '8C0114FF-31CF-443E-9278-3F9E6087140C'" ); // Short Links Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = 'C206A96E-6926-4EB9-A30F-E5FCE559D180'" ); // Shared Links Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = '4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E'" ); // Cache Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 7 WHERE [Guid] = 'D2B919E2-3725-438F-8A86-AC87F81A72EB'" ); // Asset Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 8 WHERE [Guid] = '706C0584-285F-4014-BA61-EC42C8F6F76B'" ); // Control Gallery Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 9 WHERE [Guid] = 'BB2AF2B3-6D06-48C6-9895-EDF2BA254533'" ); // Font Awesome Settings Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '784259EC-46B7-4DE3-AC37-E8BFDB0B90A6'" ); // Mobile Applications Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = 'C8B81EBE-E98F-43EF-9E39-0491685145E2'" ); // Apple TV Apps Page

        }
    }
}
