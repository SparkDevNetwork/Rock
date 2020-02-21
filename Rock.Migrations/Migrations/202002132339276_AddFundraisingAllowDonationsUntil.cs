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
    public partial class AddFundraisingAllowDonationsUntil : Rock.Migrations.RockMigration
    {
        private const string AllowDonationsUntilGuid = "73AA96AC-BD64-4655-947D-AF6144F143CC";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute( 
                SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY,
                SystemGuid.FieldType.DATE,
                "Allow Donations Until",
                "Donations to members of this group will be allowed up to and including the date specified.",
                0,
                string.Empty,
                AllowDonationsUntilGuid,
                false );

            Sql( $@"
                DECLARE @Order int = (SELECT [Order] + 1 FROM [Attribute] WHERE [Guid] = '7C6FF01B-F68E-4A83-A96D-85071A92AAF1')
                
                -- Update Order on the new attribute
                UPDATE [Attribute]
                    SET [Order] = @Order
                WHERE [Guid] = '{AllowDonationsUntilGuid}'
                
                -- Update Order on the Show Public and Registration Notes attributes
                UPDATE [Attribute]
                    SET [Order] = [Order] + 1
                WHERE [Guid] IN ('BBD6C818-765C-43FB-AA72-5AF66F91B499', '7360CF56-7DF5-42E9-AD2B-AD839E0D4EDB') AND [Order] >= @Order" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( AllowDonationsUntilGuid );
        }
    }
}
