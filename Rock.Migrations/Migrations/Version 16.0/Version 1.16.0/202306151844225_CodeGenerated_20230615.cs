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
    public partial class CodeGenerated_20230615 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Core.AttributeValues
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Core.AttributeValues", "Attribute Values", "Rock.Blocks.Types.Mobile.Core.AttributeValues, Rock, Version=1.16.0.5, Culture=neutral, PublicKeyToken=null", false, false, "6751AC1E-C467-4416-9F02-0B9A0D1FAC2D");
            
            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Crm.GroupMembers              
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Crm.GroupMembers", "Group Members", "Rock.Blocks.Types.Mobile.Crm.GroupMembers, Rock, Version=1.16.0.5, Culture=neutral, PublicKeyToken=null", false, false, "592242ED-7536-49EA-94DE-7B4EBA7E87A6");
            
            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Types.Mobile.Crm.PersonProfile              
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Crm.PersonProfile", "Person Profile", "Rock.Blocks.Types.Mobile.Crm.PersonProfile, Rock, Version=1.16.0.5, Culture=neutral, PublicKeyToken=null", false, false, "A1EEA3BD-7B40-47A9-82D4-7187290C917C");
            
            // Add/Update Mobile Block Type              
            //   Name:Attribute Values              
            //   Category:Mobile > Core              
            //   EntityType:Rock.Blocks.Types.Mobile.Core.AttributeValues              
            RockMigrationHelper.UpdateMobileBlockType("Attribute Values", "Used to display attribute values based on the category.", "Rock.Blocks.Types.Mobile.Core.AttributeValues", "Mobile > Core", "DF110543-C295-4DD9-B06E-82640AC63610");
            
            // Add/Update Mobile Block Type              
            //   Name:Group Members              
            //   Category:Mobile > Crm              
            //   EntityType:Rock.Blocks.Types.Mobile.Crm.GroupMembers              
            RockMigrationHelper.UpdateMobileBlockType("Group Members", "Allows you to view the other members of a group person belongs to (e.g. Family groups).", "Rock.Blocks.Types.Mobile.Crm.GroupMembers", "Mobile > Crm", "1F1E7598-8D51-4750-8D61-E5791A226FDB");
            
            // Add/Update Mobile Block Type              
            //   Name:Person Profile              
            //   Category:Mobile > Crm              
            //   EntityType:Rock.Blocks.Types.Mobile.Crm.PersonProfile              
            RockMigrationHelper.UpdateMobileBlockType("Person Profile", "The person profile block.", "Rock.Blocks.Types.Mobile.Crm.PersonProfile", "Mobile > Crm", "F97E2359-BB2D-4534-821D-870F853CA5CC");
            
            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "5FCC6C1C-EF91-40C7-B619-4DA1C4AE9EFC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"81A89C1A-4B55-4224-9BD3-5923C6165BC3"); 
            
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "390ADF11-23EC-4511-8C8E-6DBF4D230F80" );
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Use Abbreviated Name              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF110543-C295-4DD9-B06E-82640AC63610", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Abbreviated Name", "UseAbbreviatedName", "Use Abbreviated Name", @"Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.", 2, @"False", "1AA445A3-C89A-4737-B549-CBA2E8C09403" );
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Auto Create Group              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F1E7598-8D51-4750-8D61-E5791A226FDB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Create Group", "AutoCreateGroup", "Auto Create Group", @"If person doesn't belong to a group of this type, should one be created for them (default is Yes).", 2, @"True", "4FED48D9-1ED6-4ACF-A5FE-F100CCD504A0" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Show Demographics Panel              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics Panel", "ShowDemographicsPanel", "Show Demographics Panel", @"When enabled, the demographics panel will be shown.", 4, @"True", "B4985F9E-556D-40F1-BF23-069ADF45CD26" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Show Contact Information Panel              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Contact Information Panel", "ShowContactInformationPanel", "Show Contact Information Panel", @"When enabled, the contact information panel will be shown.", 5, @"True", "0AC1AA75-1921-443A-9D2C-F30BE5485827" );
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Group Edit Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F1E7598-8D51-4750-8D61-E5791A226FDB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"Page used to edit the members of the selected group.", 3, @"", "9328C591-B457-4979-AEA0-E3C651B84EB3" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Phone Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types", "PhoneTypes", "Phone Types", @"The phone numbers to display for editing.", 0, @"", "02491D3A-D8B0-49CA-B8A3-13E6FC8A583A" );
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF110543-C295-4DD9-B06E-82640AC63610", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity to display attribute values of.", 3, @"", "55F92937-ADBC-4E59-B18E-73AC6483C5F6" );
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Group Type              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F1E7598-8D51-4750-8D61-E5791A226FDB", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "Group Type", @"The group type to display groups for (default is Family).", 1, @"790E3215-3B10-442B-AF69-616C0DCB998E", "5141DFA7-DA70-4B3E-B7E2-3A5FB23B525B" );
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF110543-C295-4DD9-B06E-82640AC63610", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "Category", @"The Attribute Categories to display attributes from", 0, @"", "31708644-B9D0-4281-94A7-FE8A0FAB0E4D" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Header Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"Lava template used to render the header above the reminder edit fields.", 1, @"<StackLayout Spacing=""4"">            {% if CanEdit %}        <Button Text=""Edit""          Command=""{Binding ShowEdit}""          HorizontalOptions=""End""          StyleClass=""btn, btn-link"" />      {% endif %}            <!-- The main layout of the block. -->      <StackLayout HorizontalOptions=""Center""          Spacing=""4"">          <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:Person.PhotoUrl | Escape }}""               WidthRequest=""128""               HeightRequest=""128"">              <Rock:CircleTransformation />          </Rock:Image>                    {% if Person.IsDeceased %}              <Label Text=""Deceased""                  StyleClass=""text-danger""                  FontAttributes=""Bold""                  HorizontalTextAlignment=""Center"" />          {% endif %}                    <Label Text=""{{ Person.FullName }}""              StyleClass=""h2""              HorizontalTextAlignment=""Center"" />                        <!-- Our campus, connection status and record status -->          <StackLayout Orientation=""Horizontal""              HorizontalOptions=""Center"">                            <!-- Campus -->              <Frame Padding=""4""                  CornerRadius=""4""                  HasShadow=""False""                  BackgroundColor=""#d9f2fe"">                  <Label Text=""{{ Person | Campus | Property:'Name' }}""                      TextColor=""#0079b0""                      VerticalTextAlignment=""Center""                      FontSize=""14"" />              </Frame>                            <!-- Connection Status -->              <Frame Padding=""4""                  CornerRadius=""4""                  HasShadow=""False""                  BackgroundColor=""#dcf6ed"">                  <Label Text=""{{ Person.ConnectionStatusValue.Value }}""                      FontSize=""14""                      TextColor=""#065f46"" />              </Frame>                            <!-- Record Status -->              {% assign recordStatus = Person.RecordStatusValue.Value %}              {% if recordStatus == 'Inactive' %}                  <Frame Padding=""8""                      CornerRadius=""4""                      HasShadow=""False""                      BackgroundColor=""#f9e5e2"">                      <Label Text=""{{ Person.RecordStatusValue.Value }}""                          TextColor=""#ac3523"" />                  </Frame>              {% endif %}                        </StackLayout>      </StackLayout>  </StackLayout>", "C8227D52-696D-4A7A-9944-30B2670C77D6" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Custom Actions Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Custom Actions Template", "CustomActionsTemplate", "Custom Actions Template", @"Lava template used to render custom actions (such as navigation) below the action buttons.", 2, @"", "680CE145-F10C-4B09-AC33-77FDA449A3D9" );
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Badge Bar Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F97E2359-BB2D-4534-821D-870F853CA5CC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Badge Bar Template", "BadgeBarTemplate", "Badge Bar Template", @"Lava template used to render the header above the reminder edit fields.", 3, @"", "2A122C02-83C2-4E3E-B492-0FD6B5D9F890" );
            
            // Attribute for BlockType              
            //   BlockType: Structured Content View              
            //   Category: Mobile > Cms              
            //   Attribute: Document Not Found Content              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BBE3F8-F3CC-4C0A-AB2F-5085F5BF59E7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Document Not Found Content", "DocumentNotFoundContent", "Document Not Found Content", @"Template used to render when a document isn't found. Lava is not enabled. Leave blank to display nothing.", 0, @"", "E44FB372-E8E9-4398-BC93-55591D0EA427" );
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Members Template              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F1E7598-8D51-4750-8D61-E5791A226FDB", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Members Template", "MembersTemplate", "Members Template", @"The template to use when rendering the members. Provided with a merge field containing a list of groups and a value depicting whether the user is authorized to edit the group (for cases where there are multiple).", 0, @"13470DDB-5F8C-4EA2-93FD-B738F37C9AFC", "791C091E-CAF5-4B98-9683-D3CB0B2FD4DA" );
            
            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue("81A89C1A-4B55-4224-9BD3-5923C6165BC3","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType              
            //   BlockType: Structured Content View              
            //   Category: Mobile > Cms              
            //   Attribute: Document Not Found Content              
            RockMigrationHelper.DeleteAttribute("E44FB372-E8E9-4398-BC93-55591D0EA427");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Show Contact Information Panel              
            RockMigrationHelper.DeleteAttribute("0AC1AA75-1921-443A-9D2C-F30BE5485827");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Show Demographics Panel              
            RockMigrationHelper.DeleteAttribute("B4985F9E-556D-40F1-BF23-069ADF45CD26");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Badge Bar Template              
            RockMigrationHelper.DeleteAttribute("2A122C02-83C2-4E3E-B492-0FD6B5D9F890");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Custom Actions Template              
            RockMigrationHelper.DeleteAttribute("680CE145-F10C-4B09-AC33-77FDA449A3D9");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Header Template              
            RockMigrationHelper.DeleteAttribute("C8227D52-696D-4A7A-9944-30B2670C77D6");
            
            // Attribute for BlockType              
            //   BlockType: Person Profile              
            //   Category: Mobile > Crm              
            //   Attribute: Phone Types              
            RockMigrationHelper.DeleteAttribute("02491D3A-D8B0-49CA-B8A3-13E6FC8A583A");
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Group Edit Page              
            RockMigrationHelper.DeleteAttribute("9328C591-B457-4979-AEA0-E3C651B84EB3");
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Auto Create Group              
            RockMigrationHelper.DeleteAttribute("4FED48D9-1ED6-4ACF-A5FE-F100CCD504A0");
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Group Type              
            RockMigrationHelper.DeleteAttribute("5141DFA7-DA70-4B3E-B7E2-3A5FB23B525B");
            
            // Attribute for BlockType              
            //   BlockType: Group Members              
            //   Category: Mobile > Crm              
            //   Attribute: Members Template              
            RockMigrationHelper.DeleteAttribute("791C091E-CAF5-4B98-9683-D3CB0B2FD4DA");
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Use Abbreviated Name              
            RockMigrationHelper.DeleteAttribute("1AA445A3-C89A-4737-B549-CBA2E8C09403");
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Category              
            RockMigrationHelper.DeleteAttribute("31708644-B9D0-4281-94A7-FE8A0FAB0E4D");
            
            // Attribute for BlockType              
            //   BlockType: Attribute Values              
            //   Category: Mobile > Core              
            //   Attribute: Entity Type              
            RockMigrationHelper.DeleteAttribute("55F92937-ADBC-4E59-B18E-73AC6483C5F6");
            
            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute("390ADF11-23EC-4511-8C8E-6DBF4D230F80");
            
            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("81A89C1A-4B55-4224-9BD3-5923C6165BC3");
            
            // Delete BlockType               
            //   Name: Person Profile              
            //   Category: Mobile > Crm              
            //   Path: -              
            //   EntityType: Person Profile              
            RockMigrationHelper.DeleteBlockType("F97E2359-BB2D-4534-821D-870F853CA5CC");
            
            // Delete BlockType               
            //   Name: Group Members              
            //   Category: Mobile > Crm              
            //   Path: -              
            //   EntityType: Group Members              
            RockMigrationHelper.DeleteBlockType("1F1E7598-8D51-4750-8D61-E5791A226FDB");
            
            // Delete BlockType               
            //   Name: Attribute Values              
            //   Category: Mobile > Core              
            //   Path: -              
            //   EntityType: Attribute Values              
            RockMigrationHelper.DeleteBlockType("DF110543-C295-4DD9-B06E-82640AC63610");
        }
    }
}
