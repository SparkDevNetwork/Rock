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
    public partial class FundraisingDonateButtonText : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D",
                SystemGuid.FieldType.TEXT, "Donate Button Text", "core_DonateButtonText",
                "If set this text will be used on the Donate button when viewing a fundraising opportunity. If not set then the default 'Donate to a Participant' will be used instead.",
                0, string.Empty, SystemGuid.Attribute.DEFINED_VALUE_FUNDRAISING_DONATE_BUTTON_TEXT );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
