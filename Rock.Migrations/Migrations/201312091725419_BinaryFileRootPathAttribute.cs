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
    public partial class BinaryFileRootPathAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Set BinaryFileType of ExternalFile's storage root path attribute
            AddEntityAttribute( "Rock.Model.BinaryFileType", "9C204CD0-1233-41C5-818A-C5DA439445AA", "StorageEntityTypeId", "52", "Root Path", "", "Root path where the files will be stored on disk.  For example, ~/App_Data/SomeFolderName", 0, "", "434E89D8-8BF5-4480-9D58-9D0713EDAEAF" );
            AddAttributeValue( "434E89D8-8BF5-4480-9D58-9D0713EDAEAF", 4, "~/App_Data/ExternalFiles", "98C6BC91-0795-4D63-9C62-9E804A237545" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete BinaryFileType of ExternalFile's storage root path attribute
            DeleteAttribute( "434E89D8-8BF5-4480-9D58-9D0713EDAEAF" );
        }
    }
}
