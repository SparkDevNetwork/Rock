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
    public partial class Rollup_20221004 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateCacheTypeDescription();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// PA: Fix the typo of double quotes in the Description
        /// </summary>
        private void UpdateCacheTypeDescription()
        {
            Sql( @"UPDATE DefinedType
                  SET Description = 'Cached tags are used to link cached content so that it can be expired as a group. The Defined Value''s value is the key for the cache tag and should not be updated without refreshing the entire Rock cache. Changing tag keys can also break custom Lava that is using the key values.'
                  WHERE Guid = 'bdf73089-9154-40c1-90e4-74518e9937dc'" );
        }
    }
}
