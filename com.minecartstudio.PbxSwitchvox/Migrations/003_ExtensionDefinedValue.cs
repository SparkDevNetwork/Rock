using System;
using Rock.Plugin;

namespace com.minecartstudio.PbxSwitchvox.Migrations
{
    [MigrationNumber( 3, "1.7.0" )]
    public class ExtensionDefinedValue : Rock.Plugin.Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Switchvox Extensions", "Listing of all Switchvox extensions.", SystemGuid.DefinedType.SWITCHVOX_EXTENSIONS );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.SWITCHVOX_EXTENSIONS, Rock.SystemGuid.FieldType.INTEGER, "Account Id", "AccountId", "The Switchvox Account Id for the extension.", 0, "", "155B3F6E-70F6-77A1-4B02-3E2E38431D8C" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.SWITCHVOX_EXTENSIONS, Rock.SystemGuid.FieldType.PERSON, "Owner", "Owner", "The person linked to this extension in Rock. This is this first person matched if multiple people share this extension.", 1, "", "822EF56E-2DF5-FBA3-46AB-F9EB8611D1CD" );

            Sql( @"UPDATE [Attribute]
                    SET [IsGridColumn] = 1
                    WHERE [Guid] IN ('155B3F6E-70F6-77A1-4B02-3E2E38431D8C', '822EF56E-2DF5-FBA3-46AB-F9EB8611D1CD')" );
        }
    
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.SWITCHVOX_EXTENSIONS );
        }
    }
}
