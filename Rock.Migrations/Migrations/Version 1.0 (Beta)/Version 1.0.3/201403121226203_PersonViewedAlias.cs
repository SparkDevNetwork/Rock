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
    public partial class PersonViewedAlias : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.PersonViewed", "ViewerPersonAliasId", c => c.Int() );
            AddColumn( "dbo.PersonViewed", "TargetPersonAliasId", c => c.Int() );

            Sql( @"
    UPDATE [PersonViewed] SET 
        [ViewerPersonAliasId] = (SELECT TOP 1 [Id] FROM [PersonAlias] WHERE [AliasPersonId] = [ViewerPersonId]),
        [TargetPersonAliasId] = (SELECT TOP 1 [Id] FROM [PersonAlias] WHERE [AliasPersonId] = [TargetPersonId])
" );

            DropForeignKey( "dbo.PersonViewed", "TargetPersonId", "dbo.Person" );
            DropForeignKey("dbo.PersonViewed", "ViewerPersonId", "dbo.Person");
            DropIndex("dbo.PersonViewed", new[] { "TargetPersonId" });
            DropIndex("dbo.PersonViewed", new[] { "ViewerPersonId" });
            DropColumn( "dbo.PersonViewed", "ViewerPersonId" );
            DropColumn( "dbo.PersonViewed", "TargetPersonId" );
           
            CreateIndex("dbo.PersonViewed", "TargetPersonAliasId");
            CreateIndex("dbo.PersonViewed", "ViewerPersonAliasId");
            AddForeignKey("dbo.PersonViewed", "TargetPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.PersonViewed", "ViewerPersonAliasId", "dbo.PersonAlias", "Id");
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.PersonViewed", "TargetPersonId", c => c.Int());
            AddColumn("dbo.PersonViewed", "ViewerPersonId", c => c.Int());

            Sql( @"
    UPDATE [PersonViewed] SET 
        [ViewerPersonId] = (SELECT TOP 1 [PersonId] FROM [PersonAlias] WHERE [Id] = [ViewerPersonAliasId]),
        [TargetPersonId] = (SELECT TOP 1 [PersonId] FROM [PersonAlias] WHERE [Id] = [TargetPersonAliasId])
" );

            DropForeignKey("dbo.PersonViewed", "ViewerPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonViewed", "TargetPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonViewed", new[] { "ViewerPersonAliasId" });
            DropIndex("dbo.PersonViewed", new[] { "TargetPersonAliasId" });
            DropColumn("dbo.PersonViewed", "TargetPersonAliasId");
            DropColumn("dbo.PersonViewed", "ViewerPersonAliasId");

            CreateIndex("dbo.PersonViewed", "ViewerPersonId");
            CreateIndex("dbo.PersonViewed", "TargetPersonId");
            AddForeignKey("dbo.PersonViewed", "ViewerPersonId", "dbo.Person", "Id");
            AddForeignKey("dbo.PersonViewed", "TargetPersonId", "dbo.Person", "Id");
        }
    }
}
