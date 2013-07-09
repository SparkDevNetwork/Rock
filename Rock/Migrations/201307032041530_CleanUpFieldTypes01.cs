//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CleanUpFieldTypes01 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete dead fieldtypes that don't exist anymore
            Sql( 
@"delete from [FieldType] where [Class] in 
    ('Rock.Field.Types.AudiencePrimarySecondaryFieldType',
    'Rock.Field.Types.AudiencesFieldType',
    'Rock.Field.Types.MarketingCampaignAdTypesFieldType',
    'Rock.Field.Types.MarketingCampaignAdImageAttributeNameFieldType'
    )" );

            // fix typo in Class Name
            Sql( "update [FieldType] set [Class] = 'Rock.Field.Types.AccountsFieldType' where [Class] = 'Rock.Field.Type.AccountsFieldType'" );

            // update name and/or description to be more accurate
            UpdateFieldType( "Group", "A single Group (or none)", "Rock", "Rock.Field.Types.GroupFieldType", "F4399CEF-827B-48B2-A735-F7806FCFE8E8" );
            UpdateFieldType( "Group Role Field Type", "A Group Role", "Rock", "Rock.Field.Types.GroupRoleFieldType", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8" );
            UpdateFieldType( "Group Type", "A single Group Type (or none)", "Rock", "Rock.Field.Types.GroupTypeFieldType", "18E29E23-B43B-4CF7-AE41-C85672C09F50" );
            UpdateFieldType( "Group Types", "A list of zero or more Group Types", "Rock", "Rock.Field.Types.GroupTypesFieldType", "F725B854-A15E-46AE-9D4C-0608D4154F1E" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // un-fix typo in Class Name
            Sql( "update [FieldType] set [Class] = 'Rock.Field.Type.AccountsFieldType' where [Class] = 'Rock.Field.Types.AccountsFieldType'" );

            // un-update name and/or description
            UpdateFieldType( "Group", "Group", "Rock", "Rock.Field.Types.GroupFieldType", "F4399CEF-827B-48B2-A735-F7806FCFE8E8" );
            UpdateFieldType( "Group Role Field Type", "", "Rock", "Rock.Field.Types.GroupRoleFieldType", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8" );
            UpdateFieldType( "Group Type", "List of Group Types", "Rock", "Rock.Field.Types.GroupTypeFieldType", "18E29E23-B43B-4CF7-AE41-C85672C09F50" );
            UpdateFieldType( "Group Type", "List of Group Types", "Rock", "Rock.Field.Types.GroupTypesFieldType", "F725B854-A15E-46AE-9D4C-0608D4154F1E" );
        }
    }
}
