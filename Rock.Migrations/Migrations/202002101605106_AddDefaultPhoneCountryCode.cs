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
    public partial class AddDefaultPhoneCountryCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                string.Empty,
                string.Empty,
                "Default Phone Country Code",
                "This is the default country code you would like to use for your phone numbers.",
                10,
                "1",
                "2EA6AE8E-CF09-43A8-A83B-A70F05F282DF",
                "DefaultPhoneCountryCode" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "2EA6AE8E-CF09-43A8-A83B-A70F05F282DF" );
        }
    }
}
