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
    public partial class Rollup_1027 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateForgotUserNameTemplateUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "F00236DA-EC96-4890-8ED0-7275AAFDB220");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "F90E1662-AD75-4FB1-9744-4341E6A4DC50");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "6D8E3BD7-244F-4663-AE2D-8B8CEC33693A");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "01BDFF03-C28C-443F-9898-C7C2C096697A");

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F90E1662-AD75-4FB1-9744-4341E6A4DC50", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "7FF95EE2-8A9B-4F76-8E8F-E36D286B9B90" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F90E1662-AD75-4FB1-9744-4341E6A4DC50", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "8AED70AC-16EA-4FFD-B36B-9FB1B1891CE4" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6D8E3BD7-244F-4663-AE2D-8B8CEC33693A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "5231EB5E-6C26-4390-AE2C-AC827FBC3670" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6D8E3BD7-244F-4663-AE2D-8B8CEC33693A", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "E82BBE74-E1CB-4328-A2DB-EE4CA92BE33E" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "FAE05684-2534-45FB-B1A9-1E0EBB5AB686" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "DFCF2ED4-2360-4B22-BA9C-116C2D60E045" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "16AA7993-2442-4734-949A-5C3F7702591D" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "30A22B1B-6283-42A2-838B-DC0F98A6ED64" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "1705F20C-5FD4-43C8-9AF8-61857E163FC6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "24D80186-DDD9-4253-947D-74061D30BAC2" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "14BF3131-7F7C-48DD-B708-5894E684C47C" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "2209B73C-0E2E-4113-851A-5A9694DFC065" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "6C460725-50AD-4D05-B600-2D8E5F113723" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01BDFF03-C28C-443F-9898-C7C2C096697A", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "75CA8B3D-0CE1-4117-A209-20CE8843AE15" );
            RockMigrationHelper.UpdateFieldType("Location List","","Rock","Rock.Field.Types.LocationListFieldType","A58A0CBF-C3E6-4054-85D7-05118035980B");
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("75CA8B3D-0CE1-4117-A209-20CE8843AE15");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6C460725-50AD-4D05-B600-2D8E5F113723");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("2209B73C-0E2E-4113-851A-5A9694DFC065");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("14BF3131-7F7C-48DD-B708-5894E684C47C");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("24D80186-DDD9-4253-947D-74061D30BAC2");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("1705F20C-5FD4-43C8-9AF8-61857E163FC6");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("30A22B1B-6283-42A2-838B-DC0F98A6ED64");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("16AA7993-2442-4734-949A-5C3F7702591D");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("DFCF2ED4-2360-4B22-BA9C-116C2D60E045");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("FAE05684-2534-45FB-B1A9-1E0EBB5AB686");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("E82BBE74-E1CB-4328-A2DB-EE4CA92BE33E");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("5231EB5E-6C26-4390-AE2C-AC827FBC3670");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("8AED70AC-16EA-4FFD-B36B-9FB1B1891CE4");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("7FF95EE2-8A9B-4F76-8E8F-E36D286B9B90");

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("01BDFF03-C28C-443F-9898-C7C2C096697A"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("6D8E3BD7-244F-4663-AE2D-8B8CEC33693A"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("F90E1662-AD75-4FB1-9744-4341E6A4DC50"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("F00236DA-EC96-4890-8ED0-7275AAFDB220"); // Structured Content View
        }
    
        /// <summary>
        /// SK: Update Forgot User Names Communication Template
        /// </summary>
        private void UpdateForgotUserNameTemplateUp()
        {
            string oldValue = @"<table class=""tiny-button button reset-password"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100px; overflow: hidden; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; display: block; width: auto !important; color: #ffffff; background: #2795b6; padding: 5px 0 4px; border: 1px solid #2284a1;"" align=""center"" bgcolor=""#00acee"" valign=""top"">
                                  <a href=""{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset"" style=""color: #ffffff; text-decoration: none; font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 12px;"">Reset Password</a>
                                </td>
                              </tr>
		 </table>".Replace( "'", "''" );
            string newValue = @"{% assign isChangeable =  SupportsChangePassword | Contains: User.UserName %}
{% if isChangeable %}
                     <table class=""tiny-button button reset-password"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100px; overflow: hidden; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; display: block; width: auto !important; color: #ffffff; background: #2795b6; padding: 5px 0 4px; border: 1px solid #2284a1;"" align=""center"" bgcolor=""#00acee"" valign=""top"">
                                  <a href=""{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset"" style=""color: #fffffe; text-decoration: none; font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 12px;"">Reset Password</a>
                                </td>
                              </tr>
		 </table>{% endif %}".Replace( "'", "''" );
            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE 
                        {targetColumn} LIKE '%{oldValue}%'
                        AND [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'" );
        }
    }
}
