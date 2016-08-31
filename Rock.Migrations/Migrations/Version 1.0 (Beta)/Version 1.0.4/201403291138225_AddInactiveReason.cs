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
    public partial class AddInactiveReason : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "InactiveReasonNote", c => c.String(maxLength: 1000));
            DropColumn("dbo.Person", "DoNotEmail");

            // Other Migration Rollups
            Sql( @"
    -- Move Authentication Services to security page
    DECLARE @ParentPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F')
    UPDATE [Page] SET 
        [ParentPageId] = @ParentPageId
        , [Order] = 7
    WHERE [Guid] = 'CE2170A9-2C8E-40B1-A42E-DFA73762D01D'

    -- Serving Team Leader
    UPDATE [GroupTypeRole] set [IsLeader] = 1, [Order] = 0 WHERE [Guid] = 'F6CECB48-52C1-4D25-9411-F1465755EB70' 

    -- Serving Team Member
    UPDATE [GroupTypeRole] set [IsLeader] = 0, [Order] = 1 WHERE [Guid] = '8F63AB81-A2F7-4D69-82E9-158FDD92DB3D' 

    -- Small Group Leader
    UPDATE [GroupTypeRole] set [IsLeader] = 1, [Order] = 0 WHERE [Guid] = '6D798EFA-0110-41D5-BCE4-30ACEFE4317E' 

    -- Small Group Member
    UPDATE [GroupTypeRole] set [IsLeader] = 0, [Order] = 1 WHERE [Guid] = 'F0806058-7E5D-4CA9-9C04-3BDF92739462' 
" );

            AddPage("EBAA5140-4B8F-44B8-B1E8-C73B654E4B22","5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD","Email Preference","","74317DFD-F10D-4347-8E6A-CCD0FAFE31D7",""); // Site:Rock Solid Church
            AddPageRoute( "74317DFD-F10D-4347-8E6A-CCD0FAFE31D7", "Unsubscribe/{Person}" );

            UpdateBlockType( "Edit Email Preference", "Allows user to set their email preference.", "~/Blocks/Communication/EditEmailPreference.ascx", "Communication", "B3C076C7-1325-4453-9549-456C23702069" );

            // Add Block to Page: Email Preference, Site: Rock Solid Church
            AddBlock("74317DFD-F10D-4347-8E6A-CCD0FAFE31D7","","B3C076C7-1325-4453-9549-456C23702069","Edit Email Preference","Main",@"
<h5>Which option best describes how you would like us to update your email preference?</h5>
","",1,"1F73AE04-1CD3-49E0-900C-0D19371EEEC0");

            // Attrib for BlockType: Communication:Channels
            AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "039E2E97-3682-4B29-8748-7132287A2059", "Channels", "Channels", "", "The Channels that should be available to user to send through (If none are selected, all active channels will be available).", 0, @"", "2005FF70-7422-43C6-B06B-1CEE9B863559" );

            // Attrib for BlockType: Email Preference:Not Involved Text
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","Not Involved Text","NotInvolvedText","","Text to display for the 'Not Involved' option.",3,@" I am no longer involved with {{ GlobalAttribute.OrganizationName }}.","2A01DED5-E06A-470C-A695-9B6A032F087C");
            // Attrib for BlockType: Email Preference:Success Text
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","Success Text","SuccessText","","Text to display after user submits selection.",4,@"<h4>Thank-You</h4>We have saved your email preference.","46309218-6CDF-427D-BE45-B3DE6FAC1FE1");
            // Attrib for BlockType: Email Preference:No Emails Text
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","No Emails Text","NoEmailsText","","Text to display for the 'No Emails' option.",2,@"I am still involved with {{ GlobalAttribute.OrganizationName }}, but do not want to receive emails of ANY kind.","3A2E8CB6-B8DD-4C86-ACF9-BD8BE21ABAF4");
            // Attrib for BlockType: Email Preference:No Mass Emails Text
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","No Mass Emails Text","NoMassEmailsText","","Text to display for the 'No Mass Emails' option.",1,@"I am still involved with {{ GlobalAttribute.OrganizationName }}, but do not wish to receive mass emails (personal emails are fine).","F97CB073-2808-4AD0-B1F3-0D3986F64AEC");
            // Attrib for BlockType: Email Preference:Reasons to Exclude
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","Reasons to Exclude","ReasonstoExclude","","A delimited list of the Inactive Reasons to exclude from Reason list",5,@"No Activity,Deceased","2FB90DF2-BB7F-4AEA-985E-CB56FB3173F4");
            // Attrib for BlockType: Email Preference:Emails Allowed Text
            AddBlockTypeAttribute("B3C076C7-1325-4453-9549-456C23702069","9C204CD0-1233-41C5-818A-C5DA439445AA","Emails Allowed Text","EmailsAllowedText","","Text to display for the 'Emails Allowed' option.",0,@"I am still involved with {{ GlobalAttribute.OrganizationName }}, and wish to receive all emails.","755A0402-E971-4DBD-ACED-BFA02EC597EC");

            // Attrib Value for Block:Communication, Attribute:Channels Page: New Communication, Site: Rock RMS
            AddBlockAttributeValue("BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC","2005FF70-7422-43C6-B06B-1CEE9B863559",@"");

            // Attrib Value for Block:Email Preference, Attribute:Emails Allowed Text Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","755A0402-E971-4DBD-ACED-BFA02EC597EC",@"I am still involved with {{ GlobalAttribute.OrganizationName }}, and wish to receive all emails.");
            // Attrib Value for Block:Email Preference, Attribute:No Emails Text Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","3A2E8CB6-B8DD-4C86-ACF9-BD8BE21ABAF4",@"I am still involved with {{ GlobalAttribute.OrganizationName }}, but do not want to receive emails of ANY kind.");
            // Attrib Value for Block:Email Preference, Attribute:No Mass Emails Text Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","F97CB073-2808-4AD0-B1F3-0D3986F64AEC",@"I am still involved with {{ GlobalAttribute.OrganizationName }}, but do not wish to receive mass emails (personal emails are fine).");
            // Attrib Value for Block:Email Preference, Attribute:Not Involved Text Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","2A01DED5-E06A-470C-A695-9B6A032F087C",@"I am no longer involved with {{ GlobalAttribute.OrganizationName }}.");
            // Attrib Value for Block:Email Preference, Attribute:Reasons to Exclude Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","2FB90DF2-BB7F-4AEA-985E-CB56FB3173F4",@"No Activity,Deceased");
            // Attrib Value for Block:Email Preference, Attribute:Success Text Page: Email Preference, Site: Rock Solid Church
            AddBlockAttributeValue("1F73AE04-1CD3-49E0-900C-0D19371EEEC0","46309218-6CDF-427D-BE45-B3DE6FAC1FE1",@"<h4>Thank-You</h4>We have saved your email preference.");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Email Preference:Emails Allowed Text
            DeleteAttribute( "755A0402-E971-4DBD-ACED-BFA02EC597EC" );
            // Attrib for BlockType: Email Preference:Reasons to Exclude
            DeleteAttribute( "2FB90DF2-BB7F-4AEA-985E-CB56FB3173F4" );
            // Attrib for BlockType: Email Preference:No Mass Emails Text
            DeleteAttribute( "F97CB073-2808-4AD0-B1F3-0D3986F64AEC" );
            // Attrib for BlockType: Email Preference:No Emails Text
            DeleteAttribute( "3A2E8CB6-B8DD-4C86-ACF9-BD8BE21ABAF4" );
            // Attrib for BlockType: Email Preference:Success Text
            DeleteAttribute( "46309218-6CDF-427D-BE45-B3DE6FAC1FE1" );
            // Attrib for BlockType: Email Preference:Not Involved Text
            DeleteAttribute( "2A01DED5-E06A-470C-A695-9B6A032F087C" );

            // Attrib for BlockType: Communication:Channels
            DeleteAttribute( "2005FF70-7422-43C6-B06B-1CEE9B863559" );

            // Remove Block: Email Preference, from Page: Email Preference, Site: Rock Solid Church
            DeleteBlock( "1F73AE04-1CD3-49E0-900C-0D19371EEEC0" );

            DeleteBlockType( "B3C076C7-1325-4453-9549-456C23702069" ); // Email Preference

            DeletePage( "74317DFD-F10D-4347-8E6A-CCD0FAFE31D7" ); // Page: Email PreferenceLayout: FullWidth, Site: Rock Solid Church


            AddColumn("dbo.Person", "DoNotEmail", c => c.Boolean(nullable: false));
            DropColumn("dbo.Person", "InactiveReasonNote");
        }
    }
}
