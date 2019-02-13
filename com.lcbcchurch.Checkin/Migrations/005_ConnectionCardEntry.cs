// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class ConnectionCardEntry : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Weekend Gathering", "Holds all the campus weekend gathering groups for attendance purposes.", "Group", "Member", false, true, true, null, 0, null, 0, "4A406CB0-495B-4795-B788-52BDFDE00B01", "85FAEE00-42F3-415E-B921-86712E855B85" );
            var groupTypeId = SqlScalar( @"Select Top 1 Id from GroupType Where [Guid] = '85FAEE00-42F3-415E-B921-86712E855B85'" ).ToString().AsInteger();

            // Page: Volunteer Tools
            RockMigrationHelper.AddPage( "98163C8B-5C91-4A68-BB79-6AD948A604CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Volunteer Tools", "", "DA9F36C9-0577-42A6-9A52-B72A0995FB67", "" ); // Site:Rock RMS
                                                                                                                                                                                              // Page: Connection Card Entry
            RockMigrationHelper.AddPage( "DA9F36C9-0577-42A6-9A52-B72A0995FB67", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Card Entry", "", "9F6D0A84-6FF7-44C8-B650-DCCC0D8C2D19", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Connection Card Entry", "Provides a way to manually enter attendance for a large group of people in an efficient manner.", "~/Plugins/com_bemadev/Checkin/ConnectionCardEntry.ascx", "com_bemadev > Check-in", "449C6C4A-112F-401D-8903-19D6D0C68962" );
            // Add Block to Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9F6D0A84-6FF7-44C8-B650-DCCC0D8C2D19", "", "449C6C4A-112F-401D-8903-19D6D0C68962", "Connection Card Entry", "Main", "", "", 0, "56057841-D64E-477E-90E4-073EF819BDFC" );
            // Attrib for BlockType: Connection Card Entry:Expires After (Days)
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpireDays", "", "Default number of days until the request will expire.", 0, @"14", "6F0A5A34-FD0D-4C04-867A-537FA6D0EBD5" );
            // Attrib for BlockType: Connection Card Entry:Checkin Config Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Checkin Config Id", "CheckinConfigId", "", "Select the parent group whose immediate children will be displayed as options to take attendance for.", 0, @"", "2D6D78C5-11C3-4F79-A9CD-5B7CB93435B3" );
            // Attrib for BlockType: Connection Card Entry:Default Show Current Attendees
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Show Current Attendees", "DefaultShowCurrentAttendees", "", "Should the Current Attendees grid be visible by default. When the grid is enabled performance will be reduced.", 0, @"False", "607165F0-5B63-472B-94D4-0FF0083E277B" );
            // Attrib for BlockType: Connection Card Entry:Address Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "AddressType", "", "The type of address to be displayed / edited.", 0, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "2E23B538-D952-4922-8378-5E25381D3CB5" );
            // Attrib for BlockType: Connection Card Entry:Default Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "", "The connection status that should be set by default", 0, @"", "E9D919AD-2E50-4029-BB36-646AEBFBA9C8" );
            // Attrib for BlockType: Connection Card Entry:Record Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status that should be used when adding new people.", 0, @"618F906C-C33D-4FA3-8AEF-E58CB7B63F1E", "E4A27982-36B0-45F9-9F9B-ABAD595EC951" );
            // Attrib for BlockType: Connection Card Entry:Default Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Default Campus", "DefaultCampus", "", "An optional campus to use by default when adding a new family.", 0, @"", "F8F554CE-A7D3-48A2-9F02-E22DCE4F8087" );
            // Attrib for BlockType: Connection Card Entry:General Comment Attribute Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "9C204CD0-1233-41C5-818A-C5DA439445AA", "General Comment Attribute Key", "GeneralCommentAttributeKey", "", "The Key of the Workflow Attribute that the comment's text will be passed to", 0, @"Comment", "3E932F79-07CE-44E1-82A7-9F4A8DD1220F" );
            // Attrib for BlockType: Connection Card Entry:Commitment Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Commitment Workflow Type", "CommitmentWorkflowType", "", "The type of workflow to be fired if any of the commitments are selected", 0, @"", "68AC3DAB-6468-4C62-83DF-F89B6C5CD442" );
            // Attrib for BlockType: Connection Card Entry:Interest Attribute Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Interest Attribute Key", "InterestAttributeKey", "", "The Key of the Workflow Attribute that the selected interests will be passed to", 0, @"Interests", "9D652364-D130-4E97-BDA7-6380C35CD995" );
            // Attrib for BlockType: Connection Card Entry:Interest Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Interest Workflow Type", "InterestWorkflowType", "", "The type of workflow to be fired if any of the interests are selected", 0, @"", "C3428369-5E20-4511-9A88-EE7D1948BA75" );
            // Attrib for BlockType: Connection Card Entry:General Comment Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "General Comment Workflow Type", "GeneralCommentWorkflowType", "", "The type of workflow to be fired", 0, @"", "BD2902FD-B2F4-4799-95A8-C0D6EBA3EE8A" );
            // Attrib for BlockType: Connection Card Entry:Commitment Attribute Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Commitment Attribute Key", "CommitmentAttributeKey", "", "The Key of the Workflow Attribute that the selected commitments will be passed to", 0, @"Commitment", "A9EDA0F1-814E-45AA-887C-C9EDA5A77D97" );
            // Attrib for BlockType: Connection Card Entry:Contact Info Attribute Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contact Info Attribute Key", "ContactInfoAttributeKey", "", "The Key of the Workflow Attribute that the contact info will be passed to", 0, @"ContactInfo", "3CE7D634-F6A0-41A6-896E-34D6EC56A453" );
            // Attrib for BlockType: Connection Card Entry:Default Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "", "If a category is not selected, choose a default category to use for all new prayer requests.", 1, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "3A4E868B-9A0F-4C85-8E1F-356B6679FF10" );
            // Attrib for BlockType: Connection Card Entry:Default To Public
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "", "If enabled, all prayers will be set to public by default", 4, @"False", "FDDEA8C8-17C1-41EA-B657-C7EE7713A960" );
            // Attrib for BlockType: Connection Card Entry:Default Allow Comments Checked
            RockMigrationHelper.UpdateBlockTypeAttribute( "449C6C4A-112F-401D-8903-19D6D0C68962", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments Checked", "DefaultAllowCommentsChecked", "", "If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", 5, @"True", "8B82FE66-F173-4900-AB1B-47A640D65AC7" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Expires After (Days) Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "6F0A5A34-FD0D-4C04-867A-537FA6D0EBD5", @"14" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Checkin Config Id Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "2D6D78C5-11C3-4F79-A9CD-5B7CB93435B3", groupTypeId.ToString() );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default Show Current Attendees Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "607165F0-5B63-472B-94D4-0FF0083E277B", @"False" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Address Type Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "2E23B538-D952-4922-8378-5E25381D3CB5", @"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default Connection Status Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "E9D919AD-2E50-4029-BB36-646AEBFBA9C8", @"b91ba046-bc1e-400c-b85d-638c1f4e0ce2" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Record Status Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "E4A27982-36B0-45F9-9F9B-ABAD595EC951", @"618f906c-c33d-4fa3-8aef-e58cb7b63f1e" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default Campus Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "F8F554CE-A7D3-48A2-9F02-E22DCE4F8087", @"9d44a3da-5b86-45da-aa52-2be359c72d0d" );
            // Attrib Value for Block:Connection Card Entry, Attribute:General Comment Attribute Key Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "3E932F79-07CE-44E1-82A7-9F4A8DD1220F", @"Comment" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Commitment Workflow Type Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "68AC3DAB-6468-4C62-83DF-F89B6C5CD442", @"54fe40b1-3885-4e65-b15b-6e5c64aa59b9" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Interest Attribute Key Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "9D652364-D130-4E97-BDA7-6380C35CD995", @"Interests" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Interest Workflow Type Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "C3428369-5E20-4511-9A88-EE7D1948BA75", @"63b0641b-346b-4774-ae4a-b3f49fbd7708" );
            // Attrib Value for Block:Connection Card Entry, Attribute:General Comment Workflow Type Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "BD2902FD-B2F4-4799-95A8-C0D6EBA3EE8A", @"0cf2ca1c-60cf-46e3-b2c9-6437e7ba0fee" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Commitment Attribute Key Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "A9EDA0F1-814E-45AA-887C-C9EDA5A77D97", @"Commitment" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Contact Info Attribute Key Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "3CE7D634-F6A0-41A6-896E-34D6EC56A453", @"ContactInfo" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default Category Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "3A4E868B-9A0F-4C85-8E1F-356B6679FF10", @"4b2d88f5-6e45-4b4b-8776-11118c8e8269" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default To Public Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "FDDEA8C8-17C1-41EA-B657-C7EE7713A960", @"False" );
            // Attrib Value for Block:Connection Card Entry, Attribute:Default Allow Comments Checked Page: Connection Card Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "56057841-D64E-477E-90E4-073EF819BDFC", "8B82FE66-F173-4900-AB1B-47A640D65AC7", @"True" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "3CE7D634-F6A0-41A6-896E-34D6EC56A453" );
            RockMigrationHelper.DeleteAttribute( "A9EDA0F1-814E-45AA-887C-C9EDA5A77D97" );
            RockMigrationHelper.DeleteAttribute( "BD2902FD-B2F4-4799-95A8-C0D6EBA3EE8A" );
            RockMigrationHelper.DeleteAttribute( "C3428369-5E20-4511-9A88-EE7D1948BA75" );
            RockMigrationHelper.DeleteAttribute( "9D652364-D130-4E97-BDA7-6380C35CD995" );
            RockMigrationHelper.DeleteAttribute( "68AC3DAB-6468-4C62-83DF-F89B6C5CD442" );
            RockMigrationHelper.DeleteAttribute( "3E932F79-07CE-44E1-82A7-9F4A8DD1220F" );
            RockMigrationHelper.DeleteAttribute( "F8F554CE-A7D3-48A2-9F02-E22DCE4F8087" );
            RockMigrationHelper.DeleteAttribute( "E4A27982-36B0-45F9-9F9B-ABAD595EC951" );
            RockMigrationHelper.DeleteAttribute( "E9D919AD-2E50-4029-BB36-646AEBFBA9C8" );
            RockMigrationHelper.DeleteAttribute( "2E23B538-D952-4922-8378-5E25381D3CB5" );
            RockMigrationHelper.DeleteAttribute( "607165F0-5B63-472B-94D4-0FF0083E277B" );
            RockMigrationHelper.DeleteAttribute( "2D6D78C5-11C3-4F79-A9CD-5B7CB93435B3" );
            RockMigrationHelper.DeleteAttribute( "6F0A5A34-FD0D-4C04-867A-537FA6D0EBD5" );
            RockMigrationHelper.DeleteAttribute( "8B82FE66-F173-4900-AB1B-47A640D65AC7" );
            RockMigrationHelper.DeleteAttribute( "FDDEA8C8-17C1-41EA-B657-C7EE7713A960" );
            RockMigrationHelper.DeleteAttribute( "3A4E868B-9A0F-4C85-8E1F-356B6679FF10" );
            RockMigrationHelper.DeleteBlock( "56057841-D64E-477E-90E4-073EF819BDFC" );
            RockMigrationHelper.DeleteBlockType( "449C6C4A-112F-401D-8903-19D6D0C68962" );
            RockMigrationHelper.DeletePage( "9F6D0A84-6FF7-44C8-B650-DCCC0D8C2D19" ); //  Page: Connection Card Entry
            RockMigrationHelper.DeletePage( "DA9F36C9-0577-42A6-9A52-B72A0995FB67" ); //  Page: Volunteer Tools
        }
    }
}
