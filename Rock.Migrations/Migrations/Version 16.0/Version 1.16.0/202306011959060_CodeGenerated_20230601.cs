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

    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20230601 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {


            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard", "Reminder Dashboard", "Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard, Rock, Version=1.16.0.4, Culture=neutral, PublicKeyToken=null", false, false, "AD29BE7E-00B2-4AE3-8DA4-756C348E7AFA" );


            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderEdit              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Reminders.ReminderEdit", "Reminder Edit", "Rock.Blocks.Types.Mobile.Reminders.ReminderEdit, Rock, Version=1.16.0.4, Culture=neutral, PublicKeyToken=null", false, false, "A07DA3CE-4598-4177-AD47-B0D1EBFB1E7A" );

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderList              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Reminders.ReminderList", "Reminder List", "Rock.Blocks.Types.Mobile.Reminders.ReminderList, Rock, Version=1.16.0.4, Culture=neutral, PublicKeyToken=null", false, false, "35B1CA9F-DCD9-453F-892E-33E0E09E7CB3" );

            // Add/Update Obsidian Block Type              
            //   Name:Reminder Dashboard              
            //   Category:Reminders              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard              
            RockMigrationHelper.UpdateMobileBlockType( "Reminder Dashboard", "Allows management of the current person's reminders.", "Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard", "Reminders", "223F5122-C93A-44CD-BFB7-AF990A2B6B65" );

            // Add/Update Obsidian Block Type              
            //   Name:Reminder Edit              
            //   Category:Reminders              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderEdit              
            RockMigrationHelper.UpdateMobileBlockType( "Reminder Edit", "Allows adding/editing of reminders.", "Rock.Blocks.Types.Mobile.Reminders.ReminderEdit", "Reminders", "BA26C29E-660C-470D-9FEA-5830DB15E935" );

            // Add/Update Obsidian Block Type              
            //   Name:Reminder List              
            //   Category:Reminders              
            //   EntityType:Rock.Blocks.Types.Mobile.Reminders.ReminderList              
            RockMigrationHelper.UpdateMobileBlockType( "Reminder List", "Allows management of the current person's reminders.", "Rock.Blocks.Types.Mobile.Reminders.ReminderList", "Reminders", "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B" );

            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "1F7E8D1F-5B52-40A2-A9F0-E3998246F2C7".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "86B86214-EE7D-40C0-9A50-44E1D11ED986" );

            // Attribute for BlockType              
            //   BlockType: Reminder Dashboard              
            //   Category: Reminders              
            //   Attribute: Enable Color Pair Calculation              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "223F5122-C93A-44CD-BFB7-AF990A2B6B65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Color Pair Calculation", "EnableColorPairCalculation", "Enable Color Pair Calculation", @"If enabled, the associated foreground and background color of the reminder type will undergo calculations to ensure readability. This is initially based on the 'HighlightColor' of the reminder type.", 4, @"True", "C6EDFE11-3075-49CE-82BE-7671E39069F4" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "5F081206-47D3-4E06-8999-D358B31BCBE9" );

            // Attribute for BlockType              
            //   BlockType: Reminder List              
            //   Category: Reminders              
            //   Attribute: Completion Display Delay              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Completion Display Delay", "CompletionDisplayDelay", "Completion Display Delay", @"The amount of time after a reminder is marked complete to delay before removing it from the UI (in MS).", 3, @"5000", "37A12983-E489-491D-99A8-6F487A908E34" );

            // Attribute for BlockType              
            //   BlockType: Reminder Dashboard              
            //   Category: Reminders              
            //   Attribute: Reminder List Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "223F5122-C93A-44CD-BFB7-AF990A2B6B65", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder List Page", "ReminderListPage", "Reminder List Page", @"Page to link to when user taps on a reminder type or reminder filter card. PersonGuid is passed in the query string, as well as corresponding filter parameters.", 0, @"", "19017C50-CB8F-4094-ABEA-5E43E2FF4D51" );

            // Attribute for BlockType              
            //   BlockType: Reminder Dashboard              
            //   Category: Reminders              
            //   Attribute: Reminder Edit Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "223F5122-C93A-44CD-BFB7-AF990A2B6B65", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Edit Page", "EditReminderPage", "Reminder Edit Page", @"The page where a person can edit a reminder.", 1, @"", "43FF18E9-5B9C-410B-8296-21A7D5E1673C" );

            // Attribute for BlockType              
            //   BlockType: Reminder List              
            //   Category: Reminders              
            //   Attribute: Reminder Edit Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Reminder Edit Page", "ReminderEditPage", "Reminder Edit Page", @"The page where a person can edit a reminder.", 0, @"", "18AF8C6E-A289-46C4-83E7-B50F66519B2E" );

            // Attribute for BlockType              
            //   BlockType: Reminder Edit              
            //   Category: Reminders              
            //   Attribute: Header Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BA26C29E-660C-470D-9FEA-5830DB15E935", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the reminder edit fields.", 0, @"<StackLayout>      <StackLayout.Resources>          <Rock:AllTrueMultiValueConverter x:Key=""AllTrueConverter"" />       </StackLayout.Resources>        <!-- If this is a person entity and no entity is selected -->      <Grid RowDefinitions=""*""          ColumnDefinitions=""64, *"">          <Grid.IsVisible>              <MultiBinding Converter=""{StaticResource AllTrueConverter}"">                  <Binding Path=""Reminder.IsPersonEntityType"" />                  <Binding Path=""Reminder.IsEntitySelected""                      Converter=""{Rock:InverseBooleanConverter}"" />              </MultiBinding>          </Grid.IsVisible>            <!-- The add icon -->          <Frame WidthRequest=""64""              HeightRequest=""64""              CornerRadius=""32""              HasShadow=""false""              StyleClass=""bg-light, p-0""              Grid.Column=""0"">              <Rock:Icon IconClass=""fa-plus""                  IconFamily=""FontAwesomeSolid""                  FontSize=""24""                  VerticalTextAlignment=""Center""                  HorizontalTextAlignment=""Center"" />          </Frame>            <!-- Our select individual label -->          <Label Text=""Select Individual""              Grid.Column=""1""              VerticalOptions=""Center""              StyleClass=""title"" />            <Grid.GestureRecognizers>              <TapGestureRecognizer Command=""{Binding ShowPersonSearch}"" />          </Grid.GestureRecognizers>      </Grid>        <!-- If an entity is selected -->      <Grid RowDefinitions=""*""          ColumnDefinitions=""64, *""          IsVisible=""{Binding Reminder.IsEntitySelected}"">            <!-- The bell icon -->          <Frame WidthRequest=""64""              HeightRequest=""64""              CornerRadius=""32""              HasShadow=""false""              StyleClass=""bg-light, p-0""              Grid.Column=""0"">              <Rock:Icon IconClass=""fa-bell""                  IconFamily=""FontAwesomeSolid""                  FontSize=""24""                  VerticalTextAlignment=""Center""                  HorizontalTextAlignment=""Center"" />          </Frame>            <!-- The person information -->          <StackLayout Grid.Column=""1""               Spacing=""0""              VerticalOptions=""Center"">              <Label Text=""New Reminder""                  StyleClass=""text-gray-600, text-sm"" />              <Label Text=""{Binding Reminder.Name}""                  StyleClass=""title"" />          </StackLayout>      </Grid>  </StackLayout>", "AD27AAAC-7086-4A42-B84E-DAFD4D22E828" );

            // Attribute for BlockType              
            //   BlockType: Reminder Edit              
            //   Category: Reminders              
            //   Attribute: Save Navigation Action              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BA26C29E-660C-470D-9FEA-5830DB15E935", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Save Navigation Action", "SaveNavigationAction", "Save Navigation Action", @"The action to perform after the reminder is saved.", 1, @"{""Type"": 1, ""PopCount"": 1}", "83981FBF-C8BE-480B-97C6-0B8B03EB7E73" );

            // Attribute for BlockType              
            //   BlockType: Reminder Dashboard              
            //   Category: Reminders              
            //   Attribute: Reminder Types Include              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "223F5122-C93A-44CD-BFB7-AF990A2B6B65", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Include", "ReminderTypesInclude", "Reminder Types Include", @"Select any specific reminder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).", 2, @"", "15CC0DA9-5065-4158-9104-1F6ECFC7FE61" );

            // Attribute for BlockType              
            //   BlockType: Reminder Dashboard              
            //   Category: Reminders              
            //   Attribute: Reminder Types Exclude              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "223F5122-C93A-44CD-BFB7-AF990A2B6B65", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Exclude", "ReminderTypesExclude", "Reminder Types Exclude", @"Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.", 3, @"", "D701803C-E158-4929-8DA7-2354A435ECBF" );

            // Attribute for BlockType              
            //   BlockType: Reminder List              
            //   Category: Reminders              
            //   Attribute: Reminder Types Include              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Include", "ReminderTypesInclude", "Reminder Types Include", @"Select any specific reminder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).", 1, @"", "909FAF5D-4BD5-49A3-B675-7B5DF2D5BF77" );

            // Attribute for BlockType              
            //   BlockType: Reminder List              
            //   Category: Reminders              
            //   Attribute: Reminder Types Exclude              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B", "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7", "Reminder Types Exclude", "ReminderTypesExclude", "Reminder Types Exclude", @"Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.", 2, @"", "9483286E-EFE2-4843-9352-9C09819412D1" );

            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "86B86214-EE7D-40C0-9A50-44E1D11ED986", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType             
            //   BlockType: Group Tree View             
            //   Category: Groups             
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "5F081206-47D3-4E06-8999-D358B31BCBE9" );

            // Attribute for BlockType             
            //   BlockType: Reminder List             
            //   Category: Reminders             
            //   Attribute: Completion Display Delay              
            RockMigrationHelper.DeleteAttribute( "37A12983-E489-491D-99A8-6F487A908E34" );

            // Attribute for BlockType             
            //   BlockType: Reminder List             
            //   Category: Reminders             
            //   Attribute: Reminder Types Exclude              
            RockMigrationHelper.DeleteAttribute( "9483286E-EFE2-4843-9352-9C09819412D1" );

            // Attribute for BlockType             
            //   BlockType: Reminder List             
            //   Category: Reminders             
            //   Attribute: Reminder Types Include              
            RockMigrationHelper.DeleteAttribute( "909FAF5D-4BD5-49A3-B675-7B5DF2D5BF77" );

            // Attribute for BlockType             
            //   BlockType: Reminder List             
            //   Category: Reminders             
            //   Attribute: Reminder Edit Page              
            RockMigrationHelper.DeleteAttribute( "18AF8C6E-A289-46C4-83E7-B50F66519B2E" );

            // Attribute for BlockType             
            //   BlockType: Reminder Edit             
            //   Category: Reminders             
            //   Attribute: Save Navigation Action              
            RockMigrationHelper.DeleteAttribute( "83981FBF-C8BE-480B-97C6-0B8B03EB7E73" );

            // Attribute for BlockType             
            //   BlockType: Reminder Edit             
            //   Category: Reminders             
            //   Attribute: Header Template              
            RockMigrationHelper.DeleteAttribute( "AD27AAAC-7086-4A42-B84E-DAFD4D22E828" );

            // Attribute for BlockType             
            //   BlockType: Reminder Dashboard             
            //   Category: Reminders             
            //   Attribute: Enable Color Pair Calculation              
            RockMigrationHelper.DeleteAttribute( "C6EDFE11-3075-49CE-82BE-7671E39069F4" );

            // Attribute for BlockType             
            //   BlockType: Reminder Dashboard             
            //   Category: Reminders             
            //   Attribute: Reminder Types Exclude              
            RockMigrationHelper.DeleteAttribute( "D701803C-E158-4929-8DA7-2354A435ECBF" );

            // Attribute for BlockType             
            //   BlockType: Reminder Dashboard             
            //   Category: Reminders             
            //   Attribute: Reminder Types Include              
            RockMigrationHelper.DeleteAttribute( "15CC0DA9-5065-4158-9104-1F6ECFC7FE61" );

            // Attribute for BlockType             
            //   BlockType: Reminder Dashboard             
            //   Category: Reminders             
            //   Attribute: Reminder Edit Page              
            RockMigrationHelper.DeleteAttribute( "43FF18E9-5B9C-410B-8296-21A7D5E1673C" );

            // Attribute for BlockType             
            //   BlockType: Reminder Dashboard             
            //   Category: Reminders             
            //   Attribute: Reminder List Page              
            RockMigrationHelper.DeleteAttribute( "19017C50-CB8F-4094-ABEA-5E43E2FF4D51" );

            // Remove Block             
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS             
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "86B86214-EE7D-40C0-9A50-44E1D11ED986" );

            // Delete BlockType              
            //   Name: Reminder List             
            //   Category: Reminders             
            //   Path: -             
            //   EntityType: Reminder List              
            RockMigrationHelper.DeleteBlockType( "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B" );

            // Delete BlockType              
            //   Name: Reminder Edit             
            //   Category: Reminders             
            //   Path: -             
            //   EntityType: Reminder Edit              
            RockMigrationHelper.DeleteBlockType( "BA26C29E-660C-470D-9FEA-5830DB15E935" );

            // Delete BlockType              
            //   Name: Reminder Dashboard             
            //   Category: Reminders             
            //   Path: -             
            //   EntityType: Reminder Dashboard              
            RockMigrationHelper.DeleteBlockType( "223F5122-C93A-44CD-BFB7-AF990A2B6B65" );
        }
    }
}
