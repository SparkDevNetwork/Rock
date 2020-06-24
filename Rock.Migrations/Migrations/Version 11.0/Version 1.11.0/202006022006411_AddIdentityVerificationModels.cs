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

namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    using Rock.SystemGuid;

    /// <summary>
    /// Adds the Identification Verification Models
    /// </summary>
    public partial class AddIdentityVerificationModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.IdentityVerificationCode",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Code = c.String( nullable: false, maxLength: 6 ),
                    LastIssueDateTime = c.DateTime(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.Code, unique: true )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.IdentityVerification",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    ReferenceNumber = c.String( maxLength: 150 ),
                    IssueDateTime = c.DateTime( nullable: false ),
                    RequestIpAddress = c.String( maxLength: 45 ),
                    IdentityVerificationCodeId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.IdentityVerificationCode", t => t.IdentityVerificationCodeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.IdentityVerificationCodeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddIdentityVerificationCodes();
            AddPhoneLookupPageBlockAndAttributes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.IdentityVerification", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.IdentityVerification", "IdentityVerificationCodeId", "dbo.IdentityVerificationCode" );
            DropForeignKey( "dbo.IdentityVerification", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.IdentityVerificationCode", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.IdentityVerificationCode", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.IdentityVerification", new[] { "Guid" } );
            DropIndex( "dbo.IdentityVerification", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.IdentityVerification", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.IdentityVerification", new[] { "IdentityVerificationCodeId" } );
            DropIndex( "dbo.IdentityVerificationCode", new[] { "Guid" } );
            DropIndex( "dbo.IdentityVerificationCode", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.IdentityVerificationCode", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.IdentityVerificationCode", new[] { "Code" } );
            DropTable( "dbo.IdentityVerification" );
            DropTable( "dbo.IdentityVerificationCode" );

            RemovePhoneLookupPageBlockAndAttributes();
        }

        private void AddIdentityVerificationCodes()
        {
            Sql( @"WITH x AS (SELECT n FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) v(n))
                    SELECT RIGHT('000000' + CAST([ones].n + 10*[tens].n + 100*[hundreds].n + 1000*[thousands].n + 10000*[tenthousand].n  + 100000*[hundredthousand].n AS NVARCHAR(10)), 6) AS [Code]
                    INTO #Codes
                    FROM x [ones]
	                    , x [tens]
	                    , x [hundreds]
	                    , x [thousands]
	                    , x [tenthousand]
	                    , x [hundredthousand]
                    ORDER BY 1

                    INSERT INTO [IdentityVerificationCode] ([Code], [GUID], [CreatedDateTime], [ModifiedDateTime])
                    SELECT DISTINCT [Code], NEWID(), GETDATE(), GETDATE()
                    FROM #Codes
                    WHERE [Code] NOT LIKE '%000%'
		                    AND [Code] NOT LIKE '%111%'
		                    AND [Code] NOT LIKE '%222%'
		                    AND [Code] NOT LIKE '%333%'
		                    AND [Code] NOT LIKE '%444%'
		                    AND [Code] NOT LIKE '%555%'
		                    AND [Code] NOT LIKE '%666%'
		                    AND [Code] NOT LIKE '%777%'
		                    AND [Code] NOT LIKE '%888%'
		                    AND [Code] NOT LIKE '%999%'

                    DROP TABLE #Codes
            " );
        }

        private void AddPhoneLookupPageBlockAndAttributes()
        {
            RockMigrationHelper.UpdateBlockType( "Phone Number Lookup",
                "Login via phone number",
                "~/Blocks/Security/PhoneNumberIdentification.ascx",
                "Security",
                BlockType.PHONE_NUMBER_LOOKUP );

            // Site:External Website
            RockMigrationHelper.AddPage( true,
                Page.SUPPORT_PAGES_EXTERNAL_SITE,
                Layout.FULL_WIDTH,
                "Phone Number Lookup",
                "",
                Page.PHONE_NUMBER_LOOKUP,
                "" );

            // for Page: Phone number lookup
            RockMigrationHelper.AddPageRoute( Page.PHONE_NUMBER_LOOKUP,
                "phoneidentification",
                PageRoute.PHONE_NUMBER_LOOKUP
            );

            // Add Block to Page: Phone Number Lookup Site: External Website
            RockMigrationHelper.AddBlock( true,
                Page.PHONE_NUMBER_LOOKUP.AsGuid(),
                null,
                Site.EXTERNAL_SITE.AsGuid(),
                BlockType.PHONE_NUMBER_LOOKUP.AsGuid(),
                "Phone Number Lookup",
                "Main",
                "",
                "",
                0,
                Block.PHONE_NUMBER_LOOKUP );

            // Attrib for BlockType: Phone Number Lookup:Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.TEXT,
                "Title",
                "Title",
                "Title",
                "The title for the block text.",
                3,
                "Individual Lookup",
                Attribute.PHONE_NUMBER_LOOKUP_TITLE );

            // Attrib for BlockType: Phone Number Lookup:Initial Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.TEXT,
                "Initial Instructions",
                "InitialInstructions",
                "Initial Instructions",
                "The instructions to show on the initial screen.",
                4,
                "Please enter your mobile phone number below. We’ll use this number for verification.",
                Attribute.PHONE_NUMBER_INITIAL_INSTRUCTIONS );

            // Attrib for BlockType: Phone Number Lookup:Verification Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.TEXT,
                "Verification Instructions",
                "VerificationInstructions",
                "Verification Instructions",
                "The instructions to show on the Verification screen.",
                5,
                "Please enter the six digit confirmation code below.",
                Attribute.PHONE_NUMBER_VERIFICATION_INSTRUCTIONS );

            // Attrib for BlockType: Phone Number Lookup:Individual Selection Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.TEXT,
                "Individual Selection Instructions",
                "IndividualSelectionInstructions",
                "Individual Selection Instructions",
                "The instructions to show on the Individual Selection screen.",
                6,
                "The phone number provided matches several individuals in our records. Please select yourself from the list.",
                Attribute.PHONE_NUMBER_INDIVIDUAL_SELECTION_INSTRUCTIONS );

            // Attrib for BlockType: Phone Number Lookup:Phone Number Not Found Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.TEXT,
                "Phone Number Not Found Message",
                "PhoneNumberNotFoundMessage",
                "Phone Number Not Found Message",
                "The instructions to show when the phone number is not found in Rock after the phone number has been verified.",
                7,
                "We did not find the phone number you provided in our records.",
                Attribute.PHONE_NUMBER_NOT_FOUND_MESSAGE );

            // Attrib for BlockType: Phone Number Lookup:Authentication Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.SINGLE_SELECT,
                "Authentication Level",
                "AuthenticationLevel",
                "Authentication Level",
                "This determines what level of authentication that the lookup would do.",
                4,
                "30",
                Attribute.PHONE_NUMBER_AUTHENTICATION_LEVEL );

            // Attrib for BlockType: Phone Number Lookup:Verification Time Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.INTEGER,
                "Verification Time Limit",
                "VerificationTimeLimit",
                "Verification Time Limit",
                "The number of minutes that the user has to verify their phone number.",
                8,
                "5",
                Attribute.PHONE_NUMBER_VERIFICATION_TIME_LIMIT );

            // Attrib for BlockType: Phone Number Lookup:IP Throttle Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.INTEGER,
                "IP Throttle Limit",
                "IPThrottleLimit",
                "IP Throttle Limit",
                "The number of times a single IP address can submit phone numbers for verification per day.",
                9,
                "5000",
                Attribute.PHONE_NUMBER_IP_THROTTLE_LIMIT );

            // Attrib for BlockType: Phone Number Lookup:SMS Number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.DEFINED_VALUE,
                "SMS Number",
                "SMSNumber",
                "SMS Number",
                "The phone number SMS messages should be sent from",
                10,
                "",
                Attribute.PHONE_NUMBER_SMS_NUMBER );

            // Attrib for BlockType: Phone Number Lookup:Text Message Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BlockType.PHONE_NUMBER_LOOKUP,
                FieldType.CODE_EDITOR,
                "Text Message Template",
                "TextMessageTemplate",
                "Text Message Template",
                "The template to use for the SMS message.",
                2,
                "Your {{ 'Global' | Attribute:'OrganizationName' }} verification code is {{ ConfirmationCode }}",
                Attribute.PHONE_NUMBER_TEXT_MESSAGE_TEMPLATE );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Authentication Level Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_AUTHENTICATION_LEVEL,
                @"30" );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Text Message Template Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_TEXT_MESSAGE_TEMPLATE,
                "Your {{ 'Global' | Attribute:'OrganizationName' }} verification code is {{ ConfirmationCode }}" );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Title Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_LOOKUP_TITLE,
                "Individual Lookup" );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Initial Instructions Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_INITIAL_INSTRUCTIONS,
                "Please enter your mobile phone number below. We’ll use this number for verification." );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Verification Instructions Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_VERIFICATION_INSTRUCTIONS,
                "Please enter the six digit confirmation code below." );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Individual Selection Instructions Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_INDIVIDUAL_SELECTION_INSTRUCTIONS,
                "The phone number provided matches several individuals in our records. Please select yourself from the list." );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Phone Number Not Found Message Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_NOT_FOUND_MESSAGE,
                "We did not find the phone number you provided in our records." );

            // Attrib Value for Block:Phone Number Lookup, Attribute:Verification Time Limit Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_VERIFICATION_TIME_LIMIT,
                "5" );

            // Attrib Value for Block:Phone Number Lookup, Attribute:IP Throttle Limit Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_IP_THROTTLE_LIMIT,
                "5000" );

            // Attrib Value for Block:Phone Number Lookup, Attribute:SMS Number Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( Block.PHONE_NUMBER_LOOKUP,
                Attribute.PHONE_NUMBER_SMS_NUMBER,
                "" );
        }

        private void RemovePhoneLookupPageBlockAndAttributes()
        {
            // Attrib for BlockType: Phone Number Lookup:SMS Number
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_SMS_NUMBER );
            // Attrib for BlockType: Phone Number Lookup:IP Throttle Limit
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_IP_THROTTLE_LIMIT );
            // Attrib for BlockType: Phone Number Lookup:Verification Time Limit
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_VERIFICATION_TIME_LIMIT );
            // Attrib for BlockType: Phone Number Lookup:Phone Number Not Found Message
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_NOT_FOUND_MESSAGE );
            // Attrib for BlockType: Phone Number Lookup:Individual Selection Instructions
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_INDIVIDUAL_SELECTION_INSTRUCTIONS );
            // Attrib for BlockType: Phone Number Lookup:Verification Instructions
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_VERIFICATION_INSTRUCTIONS );
            // Attrib for BlockType: Phone Number Lookup:Initial Instructions
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_INITIAL_INSTRUCTIONS );
            // Attrib for BlockType: Phone Number Lookup:Title
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_LOOKUP_TITLE );
            // Attrib for BlockType: Phone Number Lookup:Text Message Template
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_TEXT_MESSAGE_TEMPLATE );
            // Attrib for BlockType: Phone Number Lookup:Authentication Level
            RockMigrationHelper.DeleteAttribute( Attribute.PHONE_NUMBER_AUTHENTICATION_LEVEL );
            // Remove Block: Phone Number Lookup, from Page: Phone Number Lookup, Site: External Website
            RockMigrationHelper.DeleteBlock( Block.PHONE_NUMBER_LOOKUP );
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.PHONE_NUMBER_LOOKUP );
            //  Page: Phone Number Lookup, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( Page.PHONE_NUMBER_LOOKUP );
            // Phone Number Lookup
            RockMigrationHelper.DeleteBlockType( BlockType.PHONE_NUMBER_LOOKUP );
        }
    }
}
