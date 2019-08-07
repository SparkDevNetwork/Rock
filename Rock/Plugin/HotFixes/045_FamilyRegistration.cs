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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 45, "1.7.0" )]
    public class FamilyRegistration : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Added to migration rollup for V8

//            RockMigrationHelper.AddPage( true, "7625A63E-6650-4886-B605-53C2234FA5E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Family Registration", "", "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "" ); // Site:External Website
//            RockMigrationHelper.AddPageRoute( "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "FamilyRegistration", "E518B93B-26AB-42A8-989A-6A2DA864EF25" );// for Page:Family Registration

//            RockMigrationHelper.AddPage( true, "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Famiy Registration", "", "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44", "" ); // Site:External Website
//            RockMigrationHelper.AddPageRoute( "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44", "FamilyRegistrationSuccess", "7C47D437-E576-48EE-B308-FDD81C4F9386" );// for Page:Family Registration Success
//            Sql( @"
//    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44'
//" );
//            RockMigrationHelper.UpdateBlockType( "Family Pre Registration", "Provides a way to allow people to pre-register their families for weekend check-in.", "~/Blocks/Crm/FamilyPreRegistration.ascx", "Check-in", "463A454A-6370-4B4A-BCA1-415F2D9B0CB7" );

//            // Add Block to Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlock( true, "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", @"", @"", 0, "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99" );
//            // Add Block to Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlock( true, "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "", "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "Family Pre Registration", "Main", @"", @"", 0, "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06" );
//            // Add Block to Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlock( true, "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "HTML Content", "Main", @"", @"", 0, "DC006503-C69E-49CC-B384-EB199AFED5BD" );
//            // Add Block to Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlock( true, "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", @"", @"", 0, "91742CE0-FC1B-4E1B-B046-24748DD5F6CC" );
            
//            // Add/Update HtmlContent for Block: HTML Content
//            RockMigrationHelper.UpdateHtmlContentBlock( "DC006503-C69E-49CC-B384-EB199AFED5BD", @"{% assign when = PageParameter.When %}
//<h2>Thank-you for Registering!</h2>
//<h4>We're excited to see you on {{ when | Date:'dddd' }}!</h4>
//<br/><br/>
//<h4>Now What?</h4>
//<p>When you arrive, just head to the Children's Ministry Check-in Desk to check-in your children.</p>
//<p>If you have any questions when you are trying to check in children, please see a volunteer to help you.</p>
//<p>You will receive a tag to place on each child, as well as a tag for you to use to pick up your children after the service.</p>
//<p>Then, just take your children to the room listed on their tag.</p>
//<p>When the service is over, return to the same room where you dropped off your children and present your other tag to check them out.</p>", "D99BFE10-72A8-4349-A838-860EC34516D8" );
            
//            // Attrib for BlockType: Family Pre Registration:Known Relationship Types
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Known Relationship Types", "Relationships", "", @"The known relationship types that should be displayed as the possible ways that a child can be related to the adult(s).", 0, @"0", "B0126571-2A6E-42AF-8206-EA7E1AA6E2E6" );
//            // Attrib for BlockType: Family Pre Registration:Same Family Known Relationship Types
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Same Family Known Relationship Types", "FamilyRelationships", "", @"The known relationship types that if selected for a child should just add child to same family as the adult(s) rather than actually creating the know relationship.", 1, @"0", "75379DB6-11B2-4948-AF56-85589BE569AD" );
//            // Attrib for BlockType: Family Pre Registration:Can Check-in Known Relationship Types
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Can Check-in Known Relationship Types", "CanCheckinRelationships", "", @"The known relationship types that if selected for a child should also create the 'Can Check-in' known relationship type.", 2, @"", "C095F851-D713-4666-B437-D28AD8547A8D" );
//            // Attrib for BlockType: Family Pre Registration:Show Campus
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "", @"Should the campus field be displayed?", 0, @"True", "D1E84153-89DA-49F1-8C3B-A1071225978B" );
//            // Attrib for BlockType: Family Pre Registration:Auto Match
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Match", "AutoMatch", "", @"Should this block attempt to match people to to current records in the database.", 5, @"True", "F7CFEAAE-C295-4B84-941E-72054F0AD1F7" );
//            // Attrib for BlockType: Family Pre Registration:Allow Updates
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Updates", "AllowUpdates", "", @"If the person visiting this block is logged in, should the block be used to update their family? If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.", 4, @"False", "E5AD4FFE-CF89-4768-A43D-C8B8200F14C2" );
//            // Attrib for BlockType: Family Pre Registration:Workflow Types
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "", @"
//The workflow type(s) to launch when a family is added. The primary family will be passed to each workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: ParentIds, ChildIds, PlannedVisitDate.
//", 8, @"", "A94136DB-8BAD-4B19-9D21-686B75FFEB92" );
//            // Attrib for BlockType: Family Pre Registration:Family Attributes
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Family Attributes", "FamilyAttributes", "", @"The Family attributes that should be displayed", 3, @"", "2CD19B7E-0831-40AC-A05D-4E690079B11A" );
//            // Attrib for BlockType: Family Pre Registration:Suffix
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Suffix", "ChildSuffix", "", @"How should Suffix be displayed for children?", 0, @"Hide", "54E5EE57-3133-42E1-A6E6-1B1781A7FF30" );
//            // Attrib for BlockType: Family Pre Registration:Birth Date
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birth Date", "AdultBirthdate", "", @"How should Gender be displayed for adults?", 2, @"Optional", "1BA9D9FD-B3B7-4121-8541-1E2736250C1B" );
//            // Attrib for BlockType: Family Pre Registration:Planned Visit Date
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Planned Visit Date", "PlannedVisitDate", "", @"How should the Planned Visit Date field be displayed (this value is only used when starting a workflow)?", 2, @"Optional", "09FDE848-1D37-4E56-88A7-26CEE91ED9B0" );
//            // Attrib for BlockType: Family Pre Registration:Suffix
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Suffix", "AdultSuffix", "", @"How should Suffix be displayed for adults?", 0, @"Hide", "90F96BB4-BC83-4290-96A8-95074646736F" );
//            // Attrib for BlockType: Family Pre Registration:Gender
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "AdultGender", "", @"How should Gender be displayed for adults?", 1, @"Optional", "2F57914E-0EC9-478D-9012-A1F349444C95" );
//            // Attrib for BlockType: Family Pre Registration:Email
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "AdultEmail", "", @"How should Email be displayed for adults?", 4, @"Required", "9B4B62FC-0BC3-416B-A506-5A1693F06287" );
//            // Attrib for BlockType: Family Pre Registration:Mobile Phone
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "AdultMobilePhone", "", @"How should Mobile Phone be displayed for adults?", 5, @"Required", "6E943B74-F141-4F1E-AE31-B9DC5CA5DF62" );
//            // Attrib for BlockType: Family Pre Registration:Gender
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "ChildGender", "", @"How should Gender be displayed for children?", 1, @"Optional", "66F6BE0D-5440-42F8-BB3D-41FEAEC81632" );
//            // Attrib for BlockType: Family Pre Registration:Birth Date
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Birth Date", "ChildBirthdate", "", @"How should Gender be displayed for children?", 2, @"Required", "DC4861D2-6A8A-4466-8772-C07E59D5BD8E" );
//            // Attrib for BlockType: Family Pre Registration:Grade
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Grade", "ChildGrade", "", @"How should Grade be displayed for children?", 3, @"Optional", "DACFD270-F239-440C-8906-4E01A3B60863" );
//            // Attrib for BlockType: Family Pre Registration:Mobile Phone
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mobile Phone", "ChildMobilePhone", "", @"How should Mobile Phone be displayed for children?", 4, @"Hide", "30CC4D33-EB07-4A24-A95D-F81882FE621F" );
//            // Attrib for BlockType: Family Pre Registration:Marital Status
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Marital Status", "AdultMaritalStatus", "", @"How should Marital Status be displayed for adults?", 3, @"Required", "2CD1A897-651E-45DA-8CB4-7D7F0DF73B2A" );
//            // Attrib for BlockType: Family Pre Registration:Connection Status
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", @"The connection status that should be used when adding new people.", 6, @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "8F372CAB-AC31-4074-AD82-756333FD7778" );
//            // Attrib for BlockType: Family Pre Registration:Record Status
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", @"The record status that should be used when adding new people.", 7, @"618F906C-C33D-4FA3-8AEF-E58CB7B63F1E", "EFF3D9C6-C467-4969-AC28-A26EEB8E28B8" );
//            // Attrib for BlockType: Family Pre Registration:Default Campus
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Default Campus", "DefaultCampus", "", @"An optional campus to use by default when adding a new family.", 1, @"", "092A9700-DCDA-4BB1-9FAD-5C6089E77E1D" );
//            // Attrib for BlockType: Family Pre Registration:Attribute Categories
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "AdultAttributeCategories", "", @"The adult Attribute Categories to display attributes from", 6, @"", "BB6E36A5-4B44-4B55-9980-995E3D4B2024" );
//            // Attrib for BlockType: Family Pre Registration:Attribute Categories
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "ChildAttributeCategories", "", @"The children Attribute Categories to display attributes from.", 5, @"", "2A65C2FF-686D-4FA6-B7B8-D364657F150C" );
//            // Attrib for BlockType: Family Pre Registration:Redirect URL
//            RockMigrationHelper.UpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Redirect URL", "RedirectURL", "", @"
//The URL to redirect user to when they have completed the registration. The merge fields that are available includes 'Family', which is an object for the primary family 
//that is created/updated, and 'RelatedChildren', which is a list of the children who have a relationship with the family, but are not in the family.
//", 9, @"", "579F1F5E-1393-4476-895D-FED8CC2343CA" );

//            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:Template Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" );
//            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" );
//            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
//            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
//            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:Include Page List Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );

//            // Attrib Value for Block:Family Pre Registration, Attribute:Suffix Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "54E5EE57-3133-42E1-A6E6-1B1781A7FF30", @"Hide" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Birth Date Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "1BA9D9FD-B3B7-4121-8541-1E2736250C1B", @"Optional" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Show Campus Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "D1E84153-89DA-49F1-8C3B-A1071225978B", @"True" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Default Campus Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "092A9700-DCDA-4BB1-9FAD-5C6089E77E1D", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Planned Visit Date Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "09FDE848-1D37-4E56-88A7-26CEE91ED9B0", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Family Attributes Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "2CD19B7E-0831-40AC-A05D-4E690079B11A", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Auto Match Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "F7CFEAAE-C295-4B84-941E-72054F0AD1F7", @"True" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Connection Status Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "8F372CAB-AC31-4074-AD82-756333FD7778", @"b91ba046-bc1e-400c-b85d-638c1f4e0ce2" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Record Status Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "EFF3D9C6-C467-4969-AC28-A26EEB8E28B8", @"618f906c-c33d-4fa3-8aef-e58cb7b63f1e" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Suffix Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "90F96BB4-BC83-4290-96A8-95074646736F", @"Hide" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Gender Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "2F57914E-0EC9-478D-9012-A1F349444C95", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Email Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "9B4B62FC-0BC3-416B-A506-5A1693F06287", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Mobile Phone Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "6E943B74-F141-4F1E-AE31-B9DC5CA5DF62", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Attribute Categories Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "BB6E36A5-4B44-4B55-9980-995E3D4B2024", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Gender Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "66F6BE0D-5440-42F8-BB3D-41FEAEC81632", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Birth Date Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "DC4861D2-6A8A-4466-8772-C07E59D5BD8E", @"Required" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Grade Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "DACFD270-F239-440C-8906-4E01A3B60863", @"Optional" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Mobile Phone Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "30CC4D33-EB07-4A24-A95D-F81882FE621F", @"Hide" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Attribute Categories Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "2A65C2FF-686D-4FA6-B7B8-D364657F150C", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Known Relationship Types Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "B0126571-2A6E-42AF-8206-EA7E1AA6E2E6", @"0" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Same Family Known Relationship Types Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "75379DB6-11B2-4948-AF56-85589BE569AD", @"0" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Can Check-in Known Relationship Types Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "C095F851-D713-4666-B437-D28AD8547A8D", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Workflow Types Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "A94136DB-8BAD-4B19-9D21-686B75FFEB92", @"" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Redirect URL Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "579F1F5E-1393-4476-895D-FED8CC2343CA", @"~/FamilyRegistrationSuccess?FamilyId={{ Family.Id }}&Parents={{ ParentIds }}&Children={{ ChildIds }}&When={{ PlannedVisitDate }}" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Allow Updates Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "E5AD4FFE-CF89-4768-A43D-C8B8200F14C2", @"False" );
//            // Attrib Value for Block:Family Pre Registration, Attribute:Marital Status Page: Family Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06", "2CD1A897-651E-45DA-8CB4-7D7F0DF73B2A", @"Optional" );

//            // Attrib Value for Block:Page Menu, Attribute:Include Page List Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
//            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
//            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
//            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
//            // Attrib Value for Block:Page Menu, Attribute:Template Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" );
//            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Famiy Registration, Site: External Website
//            RockMigrationHelper.AddBlockAttributeValue( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //// Attrib for BlockType: Family Pre Registration:Marital Status
            //RockMigrationHelper.DeleteAttribute( "2CD1A897-651E-45DA-8CB4-7D7F0DF73B2A" );
            //// Attrib for BlockType: Family Pre Registration:Allow Updates
            //RockMigrationHelper.DeleteAttribute( "E5AD4FFE-CF89-4768-A43D-C8B8200F14C2" );
            //// Attrib for BlockType: Family Pre Registration:Redirect URL
            //RockMigrationHelper.DeleteAttribute( "579F1F5E-1393-4476-895D-FED8CC2343CA" );
            //// Attrib for BlockType: Family Pre Registration:Workflow Types
            //RockMigrationHelper.DeleteAttribute( "A94136DB-8BAD-4B19-9D21-686B75FFEB92" );
            //// Attrib for BlockType: Family Pre Registration:Can Check-in Known Relationship Types
            //RockMigrationHelper.DeleteAttribute( "C095F851-D713-4666-B437-D28AD8547A8D" );
            //// Attrib for BlockType: Family Pre Registration:Same Family Known Relationship Types
            //RockMigrationHelper.DeleteAttribute( "75379DB6-11B2-4948-AF56-85589BE569AD" );
            //// Attrib for BlockType: Family Pre Registration:Known Relationship Types
            //RockMigrationHelper.DeleteAttribute( "B0126571-2A6E-42AF-8206-EA7E1AA6E2E6" );
            //// Attrib for BlockType: Family Pre Registration:Attribute Categories
            //RockMigrationHelper.DeleteAttribute( "2A65C2FF-686D-4FA6-B7B8-D364657F150C" );
            //// Attrib for BlockType: Family Pre Registration:Mobile Phone
            //RockMigrationHelper.DeleteAttribute( "30CC4D33-EB07-4A24-A95D-F81882FE621F" );
            //// Attrib for BlockType: Family Pre Registration:Grade
            //RockMigrationHelper.DeleteAttribute( "DACFD270-F239-440C-8906-4E01A3B60863" );
            //// Attrib for BlockType: Family Pre Registration:Birth Date
            //RockMigrationHelper.DeleteAttribute( "DC4861D2-6A8A-4466-8772-C07E59D5BD8E" );
            //// Attrib for BlockType: Family Pre Registration:Gender
            //RockMigrationHelper.DeleteAttribute( "66F6BE0D-5440-42F8-BB3D-41FEAEC81632" );
            //// Attrib for BlockType: Family Pre Registration:Attribute Categories
            //RockMigrationHelper.DeleteAttribute( "BB6E36A5-4B44-4B55-9980-995E3D4B2024" );
            //// Attrib for BlockType: Family Pre Registration:Mobile Phone
            //RockMigrationHelper.DeleteAttribute( "6E943B74-F141-4F1E-AE31-B9DC5CA5DF62" );
            //// Attrib for BlockType: Family Pre Registration:Email
            //RockMigrationHelper.DeleteAttribute( "9B4B62FC-0BC3-416B-A506-5A1693F06287" );
            //// Attrib for BlockType: Family Pre Registration:Gender
            //RockMigrationHelper.DeleteAttribute( "2F57914E-0EC9-478D-9012-A1F349444C95" );
            //// Attrib for BlockType: Family Pre Registration:Suffix
            //RockMigrationHelper.DeleteAttribute( "90F96BB4-BC83-4290-96A8-95074646736F" );
            //// Attrib for BlockType: Family Pre Registration:Record Status
            //RockMigrationHelper.DeleteAttribute( "EFF3D9C6-C467-4969-AC28-A26EEB8E28B8" );
            //// Attrib for BlockType: Family Pre Registration:Connection Status
            //RockMigrationHelper.DeleteAttribute( "8F372CAB-AC31-4074-AD82-756333FD7778" );
            //// Attrib for BlockType: Family Pre Registration:Auto Match
            //RockMigrationHelper.DeleteAttribute( "F7CFEAAE-C295-4B84-941E-72054F0AD1F7" );
            //// Attrib for BlockType: Family Pre Registration:Family Attributes
            //RockMigrationHelper.DeleteAttribute( "2CD19B7E-0831-40AC-A05D-4E690079B11A" );
            //// Attrib for BlockType: Family Pre Registration:Planned Visit Date
            //RockMigrationHelper.DeleteAttribute( "09FDE848-1D37-4E56-88A7-26CEE91ED9B0" );
            //// Attrib for BlockType: Family Pre Registration:Default Campus
            //RockMigrationHelper.DeleteAttribute( "092A9700-DCDA-4BB1-9FAD-5C6089E77E1D" );
            //// Attrib for BlockType: Family Pre Registration:Show Campus
            //RockMigrationHelper.DeleteAttribute( "D1E84153-89DA-49F1-8C3B-A1071225978B" );
            //// Attrib for BlockType: Family Pre Registration:Birth Date
            //RockMigrationHelper.DeleteAttribute( "1BA9D9FD-B3B7-4121-8541-1E2736250C1B" );
            //// Attrib for BlockType: Family Pre Registration:Suffix
            //RockMigrationHelper.DeleteAttribute( "54E5EE57-3133-42E1-A6E6-1B1781A7FF30" );

            //// Remove Block: Page Menu, from Page: Famiy Registration, Site: External Website
            //RockMigrationHelper.DeleteBlock( "91742CE0-FC1B-4E1B-B046-24748DD5F6CC" );
            //// Remove Block: HTML Content, from Page: Famiy Registration, Site: External Website
            //RockMigrationHelper.DeleteBlock( "DC006503-C69E-49CC-B384-EB199AFED5BD" );
            //// Remove Block: Family Pre Registration, from Page: Family Registration, Site: External Website
            //RockMigrationHelper.DeleteBlock( "4EB7A683-F28D-4708-BF7E-BC9A80FA1A06" );
            //// Remove Block: Page Menu, from Page: Family Registration, Site: External Website
            //RockMigrationHelper.DeleteBlock( "A2388785-4FCD-4FBC-9EAF-F007FA7ADA99" );

            //RockMigrationHelper.DeleteBlockType( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7" ); // Family Pre Registration
            //RockMigrationHelper.DeletePage( "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44" ); //  Page: Famiy Registration, Layout: LeftSidebar, Site: External Website
            //RockMigrationHelper.DeletePage( "3B31B9A2-DE35-4407-8E7D-3633F93906CD" ); //  Page: Family Registration, Layout: LeftSidebar, Site: External Website

        }
    }
}
