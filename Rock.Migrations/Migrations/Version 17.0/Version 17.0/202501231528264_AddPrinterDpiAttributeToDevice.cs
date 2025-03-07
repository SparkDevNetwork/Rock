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
    public partial class AddPrinterDpiAttributeToDevice : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device",
                "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", // integer
                "DeviceTypeValueId",
                "43", // printer
                "DPI",
                "Used by the check-in label designer to render the labels at the correct size for the printer.",
                1042,
                "203",
                "D2339F8F-71C0-4A43-B7EF-5F89BDE1DA72",
                "core_device_PrinterDpi",
                false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "D2339F8F-71C0-4A43-B7EF-5F89BDE1DA72" );
        }
    }
}
