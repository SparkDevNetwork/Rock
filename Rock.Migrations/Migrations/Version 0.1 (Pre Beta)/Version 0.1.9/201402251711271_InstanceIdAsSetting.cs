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
    public partial class InstanceIdAsSetting : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Update InstanceId to be a System Setting rather than a Global Attribute
    UPDATE [Attribute] SET [EntityTypeQualifierColumn] = 'SystemSetting' WHERE [EntityTypeId] IS NULL AND [Key] = 'RockInstanceId'

    -- Update Family group type to allow multiple locations (not really used by family edit blocks, but this is more consistent with behavior)
    UPDATE [GroupType] SET [AllowMultipleLocations] = 1 WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
