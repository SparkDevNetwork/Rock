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
            var definedTypeId = SqlScalar( String.Format( "Select Top 1 Id from DefinedType Where Guid = '{0}'", com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS ) ).ToString();

            RockMigrationHelper.AddDefinedTypeAttribute( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS, "9C204CD0-1233-41C5-818A-C5DA439445AA", "API Key", "APIKey", "", 1032, true, "", false, true, com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE );
            RockMigrationHelper.AddDefinedTypeAttribute( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "MailChimp Account", "MailChimpAccount", "", 1033, true, "", false, true, com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "ispassword", "False", "DC555C21-32B4-4BC2-8BAF-D304895130BC" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "maxcharacters", "", "21E5C15C-6443-4BDE-BD18-73D12E8A41FD" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE, "showcountdown", "False", "09B0E597-C3BD-4387-96B0-D839DF3636AF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "allowmultiple", "False", "3944F235-6109-4401-A453-A34EF9F3C77E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "definedtype", definedTypeId, "9FF49383-ACCA-4548-9B54-A826EC1AA4DE" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "displaydescription", "False", "93E13E42-6FD4-4483-8650-184CD593DDA3" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "enhancedselection", "False", "2C77ADCC-7071-44E7-9030-AA67D6698FAD" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE, "includeInactive", "False", "8051B44C-2600-4AC6-9D09-9160A11080C1" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_ACCOUNT_APIKEY_ATTRIBUTE ); // APIKey
            RockMigrationHelper.DeleteAttribute( com.bemaservices.MailChimp.SystemGuid.Attribute.MAIL_CHIMP_LIST_ACCOUNT_ATTRIBUTE ); // MailChimpAccount
            RockMigrationHelper.DeleteDefinedType( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS ); // MailChimp Accounts
            RockMigrationHelper.DeleteDefinedType( com.bemaservices.MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_LISTS ); // MailChimp Lists
        }
    }
}
