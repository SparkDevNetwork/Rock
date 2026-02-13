// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class AddIconLibrariesDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Icon Libraries defined type
            RockMigrationHelper.AddDefinedType( "Global", "Icon Libraries", "The list of icon libraries and their icon definitions.", "EEFC6AA8-3946-47AB-8DAF-149EEF347DF3", @"" );
            // Add Table Icon Library
            RockMigrationHelper.UpdateDefinedValue( "EEFC6AA8-3946-47AB-8DAF-149EEF347DF3", "Tabler", "/Assets/Icons/Libraries/tabler-icons.json", "59511AA1-EC0E-48BB-9E44-CC993B87ED40", false );
            // Add Font Awesome Icon Library
            RockMigrationHelper.UpdateDefinedValue( "EEFC6AA8-3946-47AB-8DAF-149EEF347DF3", "FontAwesome", "/Assets/Icons/Libraries/fontawesome-icons.json", "D05F7287-77F1-40B8-B0D0-5383E8C205E2", false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the Tabler Icon Library
            RockMigrationHelper.DeleteDefinedValue( "59511AA1-EC0E-48BB-9E44-CC993B87ED40" ); // Tabler
            // Delete the Font Awesome Icon Library
            RockMigrationHelper.DeleteDefinedValue( "D05F7287-77F1-40B8-B0D0-5383E8C205E2" ); // FontAwesome
            // Delete the Icon Libraries defined type
            RockMigrationHelper.DeleteDefinedType( "EEFC6AA8-3946-47AB-8DAF-149EEF347DF3" ); // Icon Libraries
        }
    }
}
