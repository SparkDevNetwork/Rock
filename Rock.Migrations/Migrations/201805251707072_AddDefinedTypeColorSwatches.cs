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
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddDefinedTypeColorSwatches : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Color Picker Swatches", "", "CC1400B3-E161-45E3-BF49-49825D3D6467", @"" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#f44336", "Red", "69E71798-DF68-4ED7-93AC-CFE9915D664D" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#9c27b0", "Purple", "EDFF48D7-DE7C-455D-9B89-86614D3FA14D" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#3f51b5", "Indigo", "0DA03527-0DBB-41D2-A3D1-DE60D10E4193" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#2196f3", "Blue", "D64BD9F7-5A0E-4709-9A0E-B8F88ED360F9" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#4caf50", "Green", "B18A4333-A5BA-418A-9559-0CEB59770D33" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#ffeb3b", "Yellow", "80051D45-6D75-4316-BD12-2EBCA226F0BC" );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#ee7625", "Rock Orange", "161AB06A-2B03-4380-977F-B8A88E4169E0" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue( "69E71798-DF68-4ED7-93AC-CFE9915D664D" ); // #f44336	1
            RockMigrationHelper.DeleteDefinedValue( "0DA03527-0DBB-41D2-A3D1-DE60D10E4193" ); // #3f51b5	1
            RockMigrationHelper.DeleteDefinedValue( "EDFF48D7-DE7C-455D-9B89-86614D3FA14D" ); // #9c27b0	1
            RockMigrationHelper.DeleteDefinedValue( "D64BD9F7-5A0E-4709-9A0E-B8F88ED360F9" ); // #2196f3	1
            RockMigrationHelper.DeleteDefinedValue( "B18A4333-A5BA-418A-9559-0CEB59770D33" ); // #4caf50	1
            RockMigrationHelper.DeleteDefinedValue( "80051D45-6D75-4316-BD12-2EBCA226F0BC" ); // #ffeb3b	1
            RockMigrationHelper.DeleteDefinedValue( "161AB06A-2B03-4380-977F-B8A88E4169E0" ); // #ee7625	1
            RockMigrationHelper.DeleteDefinedType( "CC1400B3-E161-45E3-BF49-49825D3D6467" ); // Color Picker Swatches	2
        }
    }
}
