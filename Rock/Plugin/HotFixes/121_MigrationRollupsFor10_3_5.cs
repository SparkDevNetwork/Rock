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
    [MigrationNumber( 121, "1.10.2" )]
    public class MigrationRollupsFor10_3_5 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //FixMarriedDefinedValue();
            //AddOnlineWatcherKnownRelationshipType();
            //UpdateCSSClassesInMobileTemplates();
            //MobileEventDetailBlock();
            //UpdateCardLavaTemplateInStepType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// GJ: Married Defined Value Typo
        /// </summary>
        private void FixMarriedDefinedValue()
        {
            Sql( @"
                UPDATE [DefinedValue]
                SET [Description]=N'Used when an individual is married.'
                WHERE ([Description] = 'Used with an individual is married.'
                    AND [Guid] = '5FE5A540-7D9F-433E-B47E-4229D1472248')" );
        }

        /// <summary>
        /// Items needed for the Attendance Self Entry block.
        /// </summary>
        private void AddOnlineWatcherKnownRelationshipType()
        {
            RockMigrationHelper.AddGroupTypeRole( "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF", "Online Watcher", "", 0, null, null, "6B05F48E-5235-45DE-970E-FE145BD28E1D", true );

            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Attendance Self Entry", "", "7863E418-A2C9-450B-943A-C3F34994C28E", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Attendance Self Entry", "Allows quick self service attendance recording.", "~/Blocks/CheckIn/AttendanceSelfEntry.ascx", "Check -in", "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3" );
            // Add Block to Page: Attendance Self Entry Site: External Website  
            RockMigrationHelper.AddBlock( true, "7863E418-A2C9-450B-943A-C3F34994C28E".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3".AsGuid(), "Attendance Self Entry","Main",@"",@"",0,"C7A5EC8D-6BDB-4E60-ACE4-AAB8BD77166F");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Shown", "PrimaryPersonAddressShown", "Primary Person Address Shown", @"Should address be displayed for primary person?", 3, @"True", "0395939D-1621-4C8C-A12F-CE42589EE50C" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Configuration            
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Configuration", "CheckinConfiguration", "Check-in Configuration", @"This will be the group type that we will use to determine where to check them in.", 0, @"77713830-AE5E-4B1A-94FA-E145DFF85035", "F63AD083-BAA7-4CB2-A3F0-DD3B0060921F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Shown      
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Shown", "PrimaryPersonBirthdayShown", "Primary Person Birthday Shown", @"Should birthday be displayed for primary person?", 1, @"True", "A81792AB-15B1-400F-905F-61D6A34BBF7B" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Required      
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Required", "PrimaryPersonBirthdayRequired", "Primary Person Birthday Required", @"Determine if birthday for primary person is required.", 2, @"False", "8528B4F1-85F5-40BE-8C42-81DB755387C1" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 2 Intro Text", "KnownIndividualPanel2IntroText", "Known Individual Panel 2 Intro Text", @"The intro text to display on the success panel.", 24, @"Thank you for connecting with us today. We hope that your enjoy the service!", "51ABBB3A-DD00-4112-B8EE-D097226207F0" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Required", "PrimaryPersonAddressRequired", "Primary Person Address Required", @"Determine if address for primary person is required.", 4, @"False", "AA1D4B29-2EF5-4085-85C5-B7DDF15250DD" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Shown", "OtherPersonBirthdayShown", "Other Person Birthday Shown", @"Should birthday be displayed for other person?", 7, @"True", "38F58816-17C2-4337-8A80-5B48768614B1" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Required 
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Required", "OtherPersonBirthdayRequired", "Other Person Birthday Required", @"Determine if birthday for other person is required.", 8, @"False", "DDDF9334-75CD-46C2-BBC0-67363BDEDE40" );
            // Attrib for BlockType: Attendance Self Entry:Known Relationship Types    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Known Relationship Types", "KnownRelationshipTypes", "Known Relationship Types", @"A checkbox list of Known Relationship types that should be included in the Relation dropdown.", 11, @"6b05f48e-5235-45de-970e-fe145bd28e1d", "08AA04A0-0657-4FE6-9FA1-12664A290111" );
            // Attrib for BlockType: Attendance Self Entry:Redirect URL    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Redirect URL", "RedirectURL", "Redirect URL", @"The URL to redirect the individual to when they check-in. The merge fields that are available includes 'PersonAliasGuid'.", 12, @"", "33A5C4FF-11E6-43EE-B0F3-C516DB0FC9BC" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Button Text  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Button Text", "CheckinButtonText", "Check-in Button Text", @"The text that should be shown on the check-in button.", 13, @"Check-in", "BF5ABAD7-A4DC-4DD0-B10A-B126D23C6F73" );
            // Attrib for BlockType: Attendance Self Entry:Workflow     
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"The optional workflow type to launch when a person is checked in. The primary person will be passed to the workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FamilyPersonIds, OtherPersonIds.", 14, @"", "4E099838-6487-4E7A-9592-F5C0A6B81CFA" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 1 Title", "UnknownIndividualPanel1Title", "Unknown Individual Panel 1 Title", @"The title to display on the primary watcher panel.", 15, @"Tell Us a Little About You...", "418D0AF2-EA3B-4EC2-BAC6-CB378E7354FE" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Intro Text     
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 1 Intro Text", "UnknownIndividualPanel1IntroText", "Unknown Individual Panel 1 Intro Text", @"The intro text to display on the primary watcher panel.", 16, @" We love to learn a little about you so that we can best serve you and your family.", "E196E82E-51B7-41BA-B2E9-8BAFD153ED0A" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 2 Title", "UnknownIndividualPanel2Title", "Unknown Individual Panel 2 Title", @"The title to display on the other watcher panel.", 17, @"Who Else Is Joining You?", "33E12BF7-8765-43CB-BF4F-402DACA43F22" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 2 Intro Text", "UnknownIndividualPanel2IntroText", "Unknown Individual Panel 2 Intro Text", @"The intro text to display on the other watcher panel.", 18, @"We'd love to know more about others watching with you.", "5719012E-5818-47B1-A382-F7637058720F" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Title  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 3 Title", "UnknownIndividualPanel3Title", "Unknown Individual Panel 3 Title", @"The title to display on the account panel.", 19, @"Would You Like to Create An Account?", "1DA99754-75C3-4A75-9BBA-FFD841A1DB63" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 3 Intro Text", "UnknownIndividualPanel3IntroText", "Unknown Individual Panel 3 Intro Text", @"The intro text to display on the account panel.", 20, @"Creating an account will help you to connect on our website.", "3DA4F419-1999-477D-A420-AF8A3367D1E6" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Title   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 1 Title", "KnownIndividualPanel1Title", "Known Individual Panel 1 Title", @"The title to display on the known individual panel.", 21, @"Great to see you {{ CurrentPerson.NickName }}!", "E4CF976C-BE6B-4CB4-A781-578ACE8B7621" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Intro Text  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 1 Intro Text", "KnownIndividualPanel1IntroText", "Known Individual Panel 1 Intro Text", @"The intro text to display on the known individual panel.", 22, @"We'd love to know who is watching with you today.", "664F339B-C142-40A7-BD1B-EE609B01637A" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 2 Title", "KnownIndividualPanel2Title", "Known Individual Panel 2 Title", @"The title to display on the success panel.", 23, @"Thanks for Connecting!", "60B08B90-0F55-4DF8-A2A2-525EE964CD6F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Shown", "PrimaryPersonMobilePhoneShown", "Primary Person Mobile Phone Shown", @"Should mobile phone be displayed for primary person?", 5, @"True", "5FB63D9A-D9ED-45C5-9520-6258EFF49B8F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Required", "PrimaryPersonMobilePhoneRequired", "Primary Person Mobile Phone Required", @"Determine if mobile phone for primary person is required.", 6, @"False", "36305F0D-A8D4-480C-87F5-6F3FD064B79C" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Shown        
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Shown", "OtherPersonMobilePhoneShown", "Other Person Mobile Phone Shown", @"Should mobile phone be displayed for other person?", 9, @"True", "636540DB-AFA2-4D36-A867-2CC2A33B2C2F" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Required", "OtherPersonMobilePhoneRequired", "Other Person Mobile Phone Required", @"Determine if mobile phone for other person is required.", 10, @"False", "49957315-CE82-4BB0-AFCD-743E3FC56708" );  
        }

        /// <summary>
        /// JE: Update CSS Classes in Mobile Templates
        /// </summary>
        private void UpdateCSSClassesInMobileTemplates()
        {
            Sql( @"
                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading1""', 'StyleClass=""h1""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')
    
                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading2""', 'StyleClass=""h2""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading3""', 'StyleClass=""h3""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading4""', 'StyleClass=""h4""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading5""', 'StyleClass=""h5""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading6""', 'StyleClass=""h6""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')" );
        }

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// JE/DH: Mobile Event Detail Block
        /// </summary>
        public void MobileEventDetailBlock()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Calendar Event Item Occurrence View",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6593D4EB-2B7A-4C24-8D30-A02991D26BC0",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW,
                "Default",
                @"<StackLayout>
    <Label Text=""{{ Event.Name | Escape }}"" />
    <Button Text=""Register"" Command=""{Binding OpenExternalBrowser}"">
        <Button.CommandParameter>
            <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                <Rock:Parameter Name=""RegistrationInstanceId"" Value=""0"" />
            </Rock:OpenExternalBrowserParameters>
        </Button.CommandParameter>
    </Button>
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.UpdateNoteType(
                "Structured Content User Value",
                "Rock.Model.ContentChannelItem",
                false,
                Rock.SystemGuid.NoteType.CONTENT_CHANNEL_ITEM_STRUCTURED_CONTENT_USER_VALUE );
        }

        /// <summary>
        /// SK: Update Card Lava Template in Step Type
        /// </summary>
        public void UpdateCardLavaTemplateInStepType()
        {
            string lavaTemplate = @"{% if LatestStepStatus %}
            <span class=""label"" style=""background-color: {{ LatestStepStatus.StatusColor }};"">{{ LatestStepStatus.Name }}</span>
        {% endif %}";

            string newLavaTemplate = @"{% if LatestStepStatus %}
            <span class=""label"" style=""background-color: {{ LatestStepStatus.StatusColor }};"">{{ LatestStepStatus.Name }}</span>
        {% endif %}
        {% if ShowCampus and LatestStep and LatestStep.Campus != '' %}
            <span class=""label label-campus"">{{ LatestStep.Campus.Name }}</span>
        {% endif %}";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "CardLavaTemplate" );

            Sql( $@"
                UPDATE
	                [dbo].[StepType]
                SET [CardLavaTemplate] = REPLACE({targetColumn}, '{lavaTemplate}', '{newLavaTemplate}')
                WHERE [CardLavaTemplate] IS NOT NULL and {targetColumn} NOT LIKE '%{newLavaTemplate}%'"
                );
        }
    }
}
