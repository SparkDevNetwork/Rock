using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;

namespace com.centralaz.Utility.Migrations
{
    [MigrationNumber( 2, "1.0.8" )]
    public class AddContributionStatementPrefs : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Contribution Statement Preference", "Displays and or sets the user's statement preference such as quarterly, yearly, none.", "~/Plugins/com_centralaz/Utility/ContributionStatementPreference.ascx", "com_centralaz > Utility", "E27853BD-9231-4E21-990B-E3B2B7762898" );
            RockMigrationHelper.AddBlockTypeAttribute( "E27853BD-9231-4E21-990B-E3B2B7762898", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Frequency Preference Attribute Guid", "FrequencyPreferenceAttributeGuid", null, "Guid of the person attribute that holds each person's frequency choice.  Note: The attribute must be of type DefinedType.", 0, "546F10C6-58E5-4E0B-99A9-1E7B85E1C121", "6EA9A88D-D052-4AD3-AA64-5D7E85909C3D" );

            RockMigrationHelper.AddDefinedType( "Giving", "Contribution Statement Frequency", "Frequency that paper (USPS mailed) contribution statements can be mailed.", "4F7062A8-3A12-4A83-8C91-2632203653A9" );
            RockMigrationHelper.AddDefinedValue( "4F7062A8-3A12-4A83-8C91-2632203653A9", "yearly", "I only need a year-end paper statement mailed <b>annually</b>", "4C085452-B85F-47EA-BDA3-C3068EDB6F4C", false );
            RockMigrationHelper.AddDefinedValue( "4F7062A8-3A12-4A83-8C91-2632203653A9", "quarterly", "I need a paper statement mailed <b>quarterly</b>", "7951691D-F65D-415F-9013-A854965C275C", false );
            RockMigrationHelper.AddDefinedValue( "4F7062A8-3A12-4A83-8C91-2632203653A9", "never", "I <b>never</b> need a paper statement mailed", "C3F882A2-4261-4708-9354-AE78FA20389E", false );

            // add Statement Frequency Preference attribute and attribute qualifier
            Sql( string.Format( @"
                DECLARE @CSPAttributeId INT
                INSERT INTO [Attribute]
                    ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid])
                VALUES
                    (0, 16, 15, N'', N'', N'StatementFrequencyPreference', N'Statement Frequency Preference', N'The frequency a person wishes to receive paper (USPS mailed) contribution statements.', 0, 0, N'', 0, 0, N'546F10C6-58E5-4E0B-99A9-1E7B85E1C121')
                SET @CSPAttributeId = SCOPE_IDENTITY()

                -- qualifier for the Statement Frequency Preference attribute
                IF NOT EXISTS(SELECT * FROM [AttributeQualifier] WHERE [Guid] = 'C15FB534-2298-4D37-871C-8152BAC1A3A7')
                BEGIN
                    DECLARE @CSFDefinedTypeId INT
                    SET @CSFDefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '4F7062A8-3A12-4A83-8C91-2632203653A9')

                    INSERT INTO [AttributeQualifier]
                        ([IsSystem],[AttributeId],[Key],[Value],[Guid])
                    VALUES
                        (0,@CSPAttributeId,'definedtype',@CSFDefinedTypeId,'C15FB534-2298-4D37-871C-8152BAC1A3A7')
                END
" ) );

            // add sample page to the Power Tools page with the new block on it
            RockMigrationHelper.AddPage( "7f1f4130-cb98-473b-9de1-7a886d2283ed", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Sample Contribution Statement Preference Page", "", "703F4902-1D69-4B08-A827-E0EA30BCD9F6", "fa fa-recycle" );
            RockMigrationHelper.AddBlock( "703F4902-1D69-4B08-A827-E0EA30BCD9F6", "", "E27853BD-9231-4E21-990B-E3B2B7762898", "Contribution Statement Preference", "Main", "", "", 0, "B7037EF4-32BC-42B2-A75C-82D9E302DDE8" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeletePage( "703F4902-1D69-4B08-A827-E0EA30BCD9F6" );
            RockMigrationHelper.DeleteBlock( "B7037EF4-32BC-42B2-A75C-82D9E302DDE8" );
            RockMigrationHelper.DeleteBlockType( "E27853BD-9231-4E21-990B-E3B2B7762898" ); // Contribution Statement Preference
            RockMigrationHelper.DeleteDefinedType( "4F7062A8-3A12-4A83-8C91-2632203653A9" ); // Contribution Statement Frequency
            RockMigrationHelper.DeleteAttribute( "546F10C6-58E5-4E0B-99A9-1E7B85E1C121" ); // StatementFrequencyPreference attribute
        }
    }
}
