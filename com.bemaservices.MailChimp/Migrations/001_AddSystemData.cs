using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.MailChimp.SystemGuid;

namespace com.bemaservices.MailChimp.Migrations
{
    [MigrationNumber( 1, "1.9.4" )]
    public class AddSystemData : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Communication", "MailChimp Accounts", "", com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, @"" );
            RockMigrationHelper.AddDefinedType( "Communication", "MailChimp Lists", "", com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS, @"" );
            var mailChimpAccountDefinedTypeId = SqlScalar( String.Format( "Select Top 1 Id from DefinedType Where Guid = '{0}'", com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS ) ).ToString();
            var mailChimpListDefinedTypeId = SqlScalar( String.Format( "Select Top 1 Id from DefinedType Where Guid = '{0}'", com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS ) ).ToString();
            RockMigrationHelper.AddDefinedTypeAttribute( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, "9C204CD0-1233-41C5-818A-C5DA439445AA", "API Key", "APIKey", "", 1032, true, "", false, true, com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE );
            RockMigrationHelper.AddDefinedTypeAttribute( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "MailChimp Account", "MailChimpAccount", "", 1033, true, "", false, true, com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE );
            RockMigrationHelper.AddDefinedTypeAttribute( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS, "9C204CD0-1233-41C5-818A-C5DA439445AA", "MailChimp List Id", "ListId", "", 1034, true, "", false, true, com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ID_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "ispassword", "False", "DC555C21-32B4-4BC2-8BAF-D304895130BC" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "maxcharacters", "", "21E5C15C-6443-4BDE-BD18-73D12E8A41FD" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "showcountdown", "False", "09B0E597-C3BD-4387-96B0-D839DF3636AF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "allowmultiple", "False", "3944F235-6109-4401-A453-A34EF9F3C77E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "definedtype", mailChimpAccountDefinedTypeId, "9FF49383-ACCA-4548-9B54-A826EC1AA4DE" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "displaydescription", "False", "93E13E42-6FD4-4483-8650-184CD593DDA3" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "enhancedselection", "False", "2C77ADCC-7071-44E7-9030-AA67D6698FAD" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "includeInactive", "False", "8051B44C-2600-4AC6-9D09-9160A11080C1" );
           
            // Page: MailChimp Accounts
            RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "MailChimp Accounts", "", "3A0F2C4C-CE31-4F17-9869-86D2476976F9", "fa fa-envelope" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Mail Chimp Account List", "Template block for developers to use to start a new list block.", "~/Plugins/com_bemaservices/MailChimp/MailChimpAccountList.ascx", "BEMA Services > Utility", "C83CBCF2-6109-4DAC-99AD-F5E7511854B6" );
            // Add Block to Page: MailChimp Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3A0F2C4C-CE31-4F17-9869-86D2476976F9", "", "C83CBCF2-6109-4DAC-99AD-F5E7511854B6", "Mail Chimp Account List", "Main", "", "", 0, "F5A9DCB4-521B-409E-9A2F-3D36BDE0AAA9" );
            // Attrib for BlockType: Mail Chimp Account List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C83CBCF2-6109-4DAC-99AD-F5E7511854B6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "6F2B96AD-F7F0-4ECC-B50C-9D2310C6B5D6" );
            // Attrib for BlockType: Mail Chimp Account List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "C83CBCF2-6109-4DAC-99AD-F5E7511854B6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "722C9D19-4C74-430E-8D55-2F0EA5BBB926" );
            // Attrib for BlockType: Mail Chimp Account List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "C83CBCF2-6109-4DAC-99AD-F5E7511854B6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "2C3FC0A5-5CD8-49E9-B7B3-D7EC0B2972FD" );
            // Attrib Value for Block:Mail Chimp Account List, Attribute:core.CustomGridEnableStickyHeaders Page: MailChimp Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F5A9DCB4-521B-409E-9A2F-3D36BDE0AAA9", "722C9D19-4C74-430E-8D55-2F0EA5BBB926", @"False" );
            // Attrib Value for Block:Mail Chimp Account List, Attribute:Detail Page Page: MailChimp Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F5A9DCB4-521B-409E-9A2F-3D36BDE0AAA9", "2C3FC0A5-5CD8-49E9-B7B3-D7EC0B2972FD", @"3e73dc3b-caa9-4593-a1af-3852dee55d98" );

            // Page: Account Details
            RockMigrationHelper.AddPage( "3A0F2C4C-CE31-4F17-9869-86D2476976F9", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Account Details", "", "3E73DC3B-CAA9-4593-A1AF-3852DEE55D98", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Mail Chimp Account Detail", "Template block for developers to use to start a new detail block.", "~/Plugins/com_bemaservices/MailChimp/MailChimpAccountDetail.ascx", "BEMA Services > Utility", "1747AA33-E25A-4A9A-88E9-88E480B53F8C" );
            RockMigrationHelper.UpdateBlockType( "Mail Chimp List List", "A Block to display the list of MailChimp lists", "~/Plugins/com_bemaservices/MailChimp/MailChimpListList.ascx", "BEMA Services > MailChimp", "91B80F57-A19D-4BFE-AD30-8C197E12C26D" );
            // Add Block to Page: Account Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3E73DC3B-CAA9-4593-A1AF-3852DEE55D98", "", "1747AA33-E25A-4A9A-88E9-88E480B53F8C", "Mail Chimp Account Detail", "Main", "", "", 0, "FBD0208F-422F-4D47-9433-DFAF5E106791" );
            // Add Block to Page: Account Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3E73DC3B-CAA9-4593-A1AF-3852DEE55D98", "", "91B80F57-A19D-4BFE-AD30-8C197E12C26D", "Mail Chimp List List", "Main", "", "", 1, "B5AFF6BC-3B5C-4339-B42C-D7DF59A7B7F4" );
            // Attrib for BlockType: Mail Chimp List List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "91B80F57-A19D-4BFE-AD30-8C197E12C26D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "BB1CE116-8199-4A32-A274-FA7F99D0A5B5" );
            // Attrib for BlockType: Mail Chimp List List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "91B80F57-A19D-4BFE-AD30-8C197E12C26D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "F2F45742-20B7-48F0-BDBC-A929918C27A1" );
            // Attrib for BlockType: Mail Chimp List List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "91B80F57-A19D-4BFE-AD30-8C197E12C26D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "6CD4E8D8-5576-4D52-A453-05CFFC711844" );
            // Attrib Value for Block:Mail Chimp List List, Attribute:Detail Page Page: Account Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B5AFF6BC-3B5C-4339-B42C-D7DF59A7B7F4", "BB1CE116-8199-4A32-A274-FA7F99D0A5B5", @"a784e99f-f27f-4acc-87e9-f4b15fb70479" );
            // Attrib Value for Block:Mail Chimp List List, Attribute:core.CustomGridEnableStickyHeaders Page: Account Details, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B5AFF6BC-3B5C-4339-B42C-D7DF59A7B7F4", "6CD4E8D8-5576-4D52-A453-05CFFC711844", @"False" );

            // Page: List Detail
            RockMigrationHelper.AddPage( "3E73DC3B-CAA9-4593-A1AF-3852DEE55D98", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "List Detail", "", "A784E99F-F27F-4ACC-87E9-F4B15FB70479", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Mail Chimp Group List", "A block to display the list of groups on a MailChimp List.", "~/Plugins/com_bemaservices/MailChimp/MailChimpGroupList.ascx", "BEMA Services > MailChimp", "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B" );
            RockMigrationHelper.UpdateBlockType( "Mail Chimp List Detail", "A block for people to edit the details of a MailChimp list.", "~/Plugins/com_bemaservices/MailChimp/MailChimpListDetail.ascx", "BEMA Services > MailChimp", "BCE7205C-FC2C-4A58-A301-F20BC6B2799B" );
            // Add Block to Page: List Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A784E99F-F27F-4ACC-87E9-F4B15FB70479", "", "BCE7205C-FC2C-4A58-A301-F20BC6B2799B", "Mail Chimp List Detail", "Main", "", "", 0, "9F7649FF-88FB-410E-A957-96A0F42C706F" );
            // Add Block to Page: List Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A784E99F-F27F-4ACC-87E9-F4B15FB70479", "", "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B", "Mail Chimp Group List", "Main", "", "", 1, "24E70313-DE15-4545-A92B-B9BF38495C5E" );
            // Attrib for BlockType: Mail Chimp Group List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "A96E2051-E518-4DFD-B888-4EC4F7AA6D1B" );
            // Attrib for BlockType: Mail Chimp Group List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "72908630-0B95-400E-96A7-68CD25D187E8" );
            // Attrib for BlockType: Mail Chimp Group List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "62046D21-7546-4C5E-A06A-9D6FBAFE4A01" );
            // Attrib Value for Block:Mail Chimp Group List, Attribute:Detail Page Page: List Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "24E70313-DE15-4545-A92B-B9BF38495C5E", "A96E2051-E518-4DFD-B888-4EC4F7AA6D1B", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Mail Chimp Group List, Attribute:core.CustomGridEnableStickyHeaders Page: List Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "24E70313-DE15-4545-A92B-B9BF38495C5E", "62046D21-7546-4C5E-A06A-9D6FBAFE4A01", @"False" );

            // Attribute: Mail Chimp List on Communication Lists
            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, "Mail Chimp List", "The mail Chimp List to sync with.", 0, @"", com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, false );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, "definedtype", mailChimpListDefinedTypeId, "8BE5E7D4-B648-4708-89D1-90B661E3BF67" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, "allowmultiple", "False", "BE51C8AA-2604-4D2A-B0FE-10D417A28205" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, "displaydescription", "False", "77486C51-5E56-42F7-9129-C5F4E1AE333B" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, "enhancedselection", "False", "DA092EEC-CA54-472E-83E3-4B317869C055" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.COMMUNICATION_LIST_MAIL_CHIMP_LIST_ATTRIBUTE, "includeInactive", "False", "0BAD4CD9-F949-4BA4-8919-D02E75B102F7" );

            //Add Job
            Sql( string.Format( @"
			    INSERT INTO ServiceJob
			    (
				    IsSystem
				    , IsActive
				    , Name
				    , Description
				    , Class
				    , CronExpression
				    , NotificationStatus
				    , Guid
			    )
			    SELECT
				    0
				    ,0
				    ,'Mail Chimp Sync'
				    ,'Syncs Group Members with a Mail Chimp List'
				    ,'com.bemaservices.MailChimp.Jobs.MailChimpSync'
				    ,'0 0/20 * 1/1 * ? *'
				    ,3
				    ,'A7EF4133-6616-4CF1-AD44-6F8DBE4EA46C'
            " ) );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "62046D21-7546-4C5E-A06A-9D6FBAFE4A01" );
            RockMigrationHelper.DeleteAttribute( "72908630-0B95-400E-96A7-68CD25D187E8" );
            RockMigrationHelper.DeleteAttribute( "A96E2051-E518-4DFD-B888-4EC4F7AA6D1B" );
            RockMigrationHelper.DeleteBlock( "24E70313-DE15-4545-A92B-B9BF38495C5E" );
            RockMigrationHelper.DeleteBlock( "9F7649FF-88FB-410E-A957-96A0F42C706F" );
            RockMigrationHelper.DeleteBlockType( "BCE7205C-FC2C-4A58-A301-F20BC6B2799B" );
            RockMigrationHelper.DeleteBlockType( "66C7F65A-EC97-4A84-AA9B-DC89C5DCA78B" );
            RockMigrationHelper.DeletePage( "A784E99F-F27F-4ACC-87E9-F4B15FB70479" ); //  Page: List Detail

            RockMigrationHelper.DeleteAttribute( "6CD4E8D8-5576-4D52-A453-05CFFC711844" );
            RockMigrationHelper.DeleteAttribute( "F2F45742-20B7-48F0-BDBC-A929918C27A1" );
            RockMigrationHelper.DeleteAttribute( "BB1CE116-8199-4A32-A274-FA7F99D0A5B5" );
            RockMigrationHelper.DeleteBlock( "B5AFF6BC-3B5C-4339-B42C-D7DF59A7B7F4" );
            RockMigrationHelper.DeleteBlock( "FBD0208F-422F-4D47-9433-DFAF5E106791" );
            RockMigrationHelper.DeleteBlockType( "91B80F57-A19D-4BFE-AD30-8C197E12C26D" );
            RockMigrationHelper.DeleteBlockType( "1747AA33-E25A-4A9A-88E9-88E480B53F8C" );
            RockMigrationHelper.DeletePage( "3E73DC3B-CAA9-4593-A1AF-3852DEE55D98" ); //  Page: Account Details

            RockMigrationHelper.DeleteAttribute( "2C3FC0A5-5CD8-49E9-B7B3-D7EC0B2972FD" );
            RockMigrationHelper.DeleteAttribute( "722C9D19-4C74-430E-8D55-2F0EA5BBB926" );
            RockMigrationHelper.DeleteAttribute( "6F2B96AD-F7F0-4ECC-B50C-9D2310C6B5D6" );
            RockMigrationHelper.DeleteBlock( "F5A9DCB4-521B-409E-9A2F-3D36BDE0AAA9" );
            RockMigrationHelper.DeleteBlockType( "C83CBCF2-6109-4DAC-99AD-F5E7511854B6" );
            RockMigrationHelper.DeletePage( "3A0F2C4C-CE31-4F17-9869-86D2476976F9" ); //  Page: MailChimp Accounts

            RockMigrationHelper.DeleteAttribute( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE ); // APIKey
            RockMigrationHelper.DeleteAttribute( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE ); // MailChimpAccount
            RockMigrationHelper.DeleteDefinedType( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS ); // MailChimp Accounts
            RockMigrationHelper.DeleteDefinedType( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS ); // MailChimp Lists
        }
    }
}
