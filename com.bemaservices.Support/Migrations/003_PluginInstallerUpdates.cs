using Rock.Plugin;

namespace com.bemaservices.Support
{
    [MigrationNumber( 3, "1.9.4" )]
    public class PluginInstallerUpdates : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Base Client Package Version", "Currently installed BEMA Base Client Package version number", 0, "0.0.0.0", com.bemaservices.Support.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID, "BEMABaseClientPackageVersion" );
            Sql( string.Format( @"
                        Update Attribute
                        Set [Name] = 'BEMA Custom Client Package Version',
                            [AbbreviatedName] = 'BEMA Custom Client Package Version',
                            [Key] = 'BEMACustomClientPackageVersion',
                            [Description] = 'Currently installed BEMA Custom Client Package version number'
                        Where Guid = '{0}'"
                ,com.bemaservices.Support.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID ) );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
           
        }
    }
}

