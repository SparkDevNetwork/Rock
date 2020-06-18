using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 2, "1.9.4" )]
    public class Attributes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var categoryId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '8BF1BECD-9B0C-4689-8204-8405327DCBCF'" ).ToString();
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Category", @"", 1, "", "F1B412F2-B6DD-4B11-8651-2DD8324EF955" );
            RockMigrationHelper.AddAttributeQualifier( "F1B412F2-B6DD-4B11-8651-2DD8324EF955", "allowmultiple", "True", "7E3B2592-9A10-47BD-8791-6502D4F58903" );
            RockMigrationHelper.AddAttributeQualifier( "F1B412F2-B6DD-4B11-8651-2DD8324EF955", "definedtype", categoryId, "B22689D0-BAD8-408A-AC3E-F23780C70716" );

            var ageRangeId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '64C9BFA7-EE35-4829-802E-004CD2F78971'" ).ToString();
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Life Stage", @"", 2, "", "ADFD8705-8143-4DDA-95ED-9B3C9D75556D" );
            RockMigrationHelper.AddAttributeQualifier( "ADFD8705-8143-4DDA-95ED-9B3C9D75556D", "allowmultiple", "True", "47F1A353-BAB0-4C1F-8384-258E9E79BEA0" );
            RockMigrationHelper.AddAttributeQualifier( "ADFD8705-8143-4DDA-95ED-9B3C9D75556D", "definedtype", ageRangeId, "1EC3C845-7422-4915-8143-EE6671B9CB09" );

            var preferredContactMethodId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '982CC7BD-451B-420F-9F54-FDBAE32672E0'" ).ToString();
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Preferred Contact Method", @"", 0, "", "6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC" );
            RockMigrationHelper.AddAttributeQualifier( "6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC", "allowmultiple", "True", "D40EAEE4-6C7A-419E-945C-D5DAA9BA48C9" );
            RockMigrationHelper.AddAttributeQualifier( "6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC", "definedtype", preferredContactMethodId, "555F6729-C699-44DF-B07D-B7A6176C6E48" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "F1B412F2-B6DD-4B11-8651-2DD8324EF955" );    // GroupType - Group Attribute, Small Group: Category  
            RockMigrationHelper.DeleteAttribute( "ADFD8705-8143-4DDA-95ED-9B3C9D75556D" );    // GroupType - Group Attribute, Small Group: Age Range  
            RockMigrationHelper.DeleteAttribute( "6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC" );    // GroupType - Group Member Attribute, Small Group: Preferred Contact Method
        }
    }
}

