// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class TuneDuplicateFinder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.PersonDuplicate", new[] { "ConfidenceScore" });
            Sql( @"
    ALTER TABLE [PersonDuplicate] DROP COLUMN [ConfidenceScore]
    ALTER TABLE [PersonDuplicate] ADD [ConfidenceScore] float null
" );
            CreateIndex("dbo.PersonDuplicate", "ConfidenceScore");

            DropIndex( "dbo.PersonDuplicate", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonDuplicate", new[] { "ForeignGuid" } );
            DropIndex( "dbo.PersonDuplicate", new[] { "ForeignId" } );
            DropIndex( "dbo.PersonDuplicate", new[] { "ForeignKey" } );
            DropIndex( "dbo.PersonDuplicate", new[] { "Guid" } );
            DropIndex( "dbo.PersonDuplicate", new[] { "ModifiedByPersonAliasId" } );

            Sql( MigrationSQL._201602041745307_TuneDuplicateFinder );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateIndex( "dbo.PersonDuplicate", "CreatedByPersonAliasId" );
            CreateIndex( "dbo.PersonDuplicate", "ForeignGuid" );
            CreateIndex( "dbo.PersonDuplicate", "ForeignId" );
            CreateIndex( "dbo.PersonDuplicate", "ForeignKey" );
            CreateIndex( "dbo.PersonDuplicate", "Guid" );
            CreateIndex( "dbo.PersonDuplicate", "ModifiedByPersonAliasId" );

            DropIndex( "dbo.PersonDuplicate", new[] { "ConfidenceScore" } );
            Sql( @"
    ALTER TABLE [PersonDuplicate] DROP COLUMN [ConfidenceScore]
    ALTER TABLE [PersonDuplicate] ADD [ConfidenceScore] AS (
	    sqrt (
		    ( CASE WHEN [TotalCapacity] > 0 
			    THEN [Capacity] / ( [TotalCapacity] * 0.01 ) 
			    ELSE 0 END )
		    *
		    ( CASE WHEN [Capacity] > 0 
			    THEN [Score] / ( [Capacity] * 0.01 ) 
			    ELSE 0 END )
		    )
        ) PERSISTED
" );
            CreateIndex("dbo.PersonDuplicate", "ConfidenceScore");
        }
    }
}
