using Rock;
using Rock.Plugin;

namespace com.bemaservices.ClientTools
{
    [MigrationNumber( 1, "1.9.4" )]
    public class AddBemaPackageInstaller : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            int clientAttributeId = 0;
            var clientAttributeIdObject = SqlScalar( string.Format( "Select Id From Attribute Where Guid = '{0}'", com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID ) );
            if ( clientAttributeIdObject != null )
            {
                clientAttributeId = clientAttributeIdObject.ToString().AsInteger();
            }

            if ( clientAttributeId > 0 )
            {
                Sql( string.Format( @"
                        Update Attribute
                        Set [Name] = 'BEMA Client Package Version',
                            [AbbreviatedName] = 'BEMA Client Package Version',
                            [Key] = 'BEMAClientPackageVersion',
                            [Description] = 'Currently installed BEMA Client Package version number'
                        Where Guid = '{0}'"
               , com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID ) );
            }
            else
            {
                RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Client Package Version", "Currently installed BEMA Client Package version number", 0, "0.0.0.0", com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_CLIENT_PACKAGE_VERSION_ATTRIBUTE_GUID );
            }

            int standardAttributeId = 0;

            var standardAttributeIdObject = SqlScalar( string.Format( "Select Id From Attribute Where Guid = '{0}'", com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID ) );
            if ( standardAttributeIdObject != null )
            {
                standardAttributeId = standardAttributeIdObject.ToString().AsInteger();
            }

            if ( standardAttributeId > 0 )
            {
                Sql( string.Format( @"
                        Update Attribute
                        Set [Name] = 'BEMA Standard Package Version',
                            [AbbreviatedName] = 'BEMA Standard Package Version',
                            [Key] = 'BEMAStandardPackageVersion',
                            [Description] = 'Currently installed BEMA Standard Package version number'
                        Where Guid = '{0}'"
               , com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID ) );
            }
            else
            {
                RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, "", "", "BEMA Standard Package Version", "Currently installed BEMA Standard Package version number", 0, "0.0.0.0", com.bemaservices.ClientTools.SystemGuid.Attribute.BEMA_STANDARD_PACKAGE_VERSION_ATTRIBUTE_GUID );
            }

            RockMigrationHelper.UpdateBlockTypeByGuid( "BEMA Package Installer", "Allows a client to download the latest BEMA Packages.", "~/Plugins/com_bemaservices/ClientTools/BemaPackageInstaller.ascx", "BEMA Services > Client Tools", "82360F6E-7D0B-4EAC-9371-A55ACE0F512C" );
            RockMigrationHelper.AddBlock( true, "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "82360F6E-7D0B-4EAC-9371-A55ACE0F512C", "BEMA Package Installer", "Sidebar1", "", "", 0, "18A61600-6147-47DE-B543-472E7953905E" );
            Sql( @"  Update Block
                    Set [Name] = 'BEMA Package Installer'
                    Where Guid = '18A61600-6147-47DE-B543-472E7953905E'" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

