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
            var groupTypeGuid = "50FCFB30-F51A-49DF-86F4-2B176EA1820B";
            var groupTypeId = SqlScalar( "Select Top 1 [Id] From GroupType Where Guid = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" ).ToString();
            var lifeGroupTypeGuidObject = SqlScalar( "Select Top 1 [Guid] From GroupType Where Guid = 'a4f16049-2525-426e-a6e8-cdfb7b198664'" );
            if ( lifeGroupTypeGuidObject != null )
            {
                groupTypeGuid = lifeGroupTypeGuidObject.ToString();
                groupTypeId = SqlScalar( "Select Top 1 [Id] From GroupType Where Guid = 'a4f16049-2525-426e-a6e8-cdfb7b198664'" ).ToString();
            }

            RockMigrationHelper.AddNewEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.TEXT, "Id", groupTypeId, "Public Name", "Public Name", "", 0, "", "29C49EF6-CF9F-4898-86F2-11DBD3782171", "PublicName" );

            var categoryId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '8BF1BECD-9B0C-4689-8204-8405327DCBCF'" ).ToString();
            var preferredContactMethodId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '982CC7BD-451B-420F-9F54-FDBAE32672E0'" ).ToString();

            RockMigrationHelper.AddGroupTypeGroupAttribute( groupTypeGuid, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Category", @"", 1, "", "F1B412F2-B6DD-4B11-8651-2DD8324EF955" );
            RockMigrationHelper.AddAttributeQualifier( "F1B412F2-B6DD-4B11-8651-2DD8324EF955", "allowmultiple", "True", "7E3B2592-9A10-47BD-8791-6502D4F58903" );
            RockMigrationHelper.AddAttributeQualifier( "F1B412F2-B6DD-4B11-8651-2DD8324EF955", "definedtype", categoryId, "B22689D0-BAD8-408A-AC3E-F23780C70716" );

            RockMigrationHelper.AddGroupTypeGroupAttribute( groupTypeGuid, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Age", @"", 6, "", "49E2CA73-BD1F-4BDC-B022-7E6C58D0A268", "MinimumAge" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( groupTypeGuid, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Age", @"", 7, "", "328886F1-33BA-4F3D-BA69-4E3FFB1DF117", "MaximumAge" );

            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( groupTypeGuid, "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Preferred Contact Method", @"", 0, "", "6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC" );
            Sql( @"
                Update Attribute
                Set IsGridColumn = 1
                Where Guid = '6F18B7FD-5EC4-48D4-AD3D-48AE328CB0EC'" );

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

