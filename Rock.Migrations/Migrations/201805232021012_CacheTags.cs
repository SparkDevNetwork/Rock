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
    public partial class CacheTags : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "CMS Settings", string.Empty, string.Empty, "262313F6-5D66-41CE-9B6F-D36567D9AB9D", 1 );

            string cacheTagDefineTypeDesc = @"Cached tags are used to link cached content so that it can be expired as a group. The Defined Value''s value is the key for the cache tag and should not be updated without refreshing the entire Rock cache. Changing tag keys can also break custom Lava that is using the key values.";
            RockMigrationHelper.AddDefinedType( "CMS Settings", "Cache Tags", cacheTagDefineTypeDesc, "BDF73089-9154-40C1-90E4-74518E9937DC", cacheTagDefineTypeDesc );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedType( "BDF73089-9154-40C1-90E4-74518E9937DC" );
            RockMigrationHelper.DeleteCategory( "262313F6-5D66-41CE-9B6F-D36567D9AB9D" );
        }
    }
}
