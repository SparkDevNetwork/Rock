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
    public partial class AddCollectionFastLoadAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // These types are used in the LoadAttributes() code to be able to
            // pass table-type data to a raw SQL query.
            Sql( @"
CREATE TYPE [dbo].[EntityIdList] AS TABLE(
	[Id] [int] NULL
)

CREATE TYPE [dbo].[LoadAttributesKeyList] AS TABLE(
	[EntityTypeId] [int] NULL,
	[EntityId] [int] NULL,
	[RealEntityId] [int] NULL
)
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DROP TYPE [dbo].[LoadAttributesKeyList]
DROP TYPE [dbo].[EntityIdList]
" );
        }
    }
}
