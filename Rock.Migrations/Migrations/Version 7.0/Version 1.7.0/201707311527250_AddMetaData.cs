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
    using System.Linq;

    /// <summary>
    ///
    /// </summary>
    public partial class AddMetaData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "MetaPersonicxLifestageClusterId", c => c.Int());
            AddColumn("dbo.Person", "MetaPersonicxLifestageGroupId", c => c.Int());
            AlterColumn("dbo.MetaLastNameLookup", "LastName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.MetaPersonicxLifestageCluster", "Summary", c => c.String(maxLength: 1000));
            CreateIndex("dbo.Person", "MetaPersonicxLifestageClusterId");
            CreateIndex("dbo.Person", "MetaPersonicxLifestageGroupId");
            AddForeignKey("dbo.Person", "MetaPersonicxLifestageClusterId", "dbo.MetaPersonicxLifestageCluster", "Id");
            AddForeignKey("dbo.Person", "MetaPersonicxLifestageGroupId", "dbo.MetaPersonicxLifestageGroup", "Id");

            Sql( "DELETE [MetaFirstNameGenderLookup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaFirstNameGenderLookup] ([FirstName],[MaleCount],[FemaleCount],[Country],[Language],[TotalCount],[FemalePercent],[MalePercent],[Guid])",
                MigrationSQL._201707311527250_FirstNameGender );

            Sql( "DELETE [MetaLastNameLookup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaLastNameLookup] ([LastName],[Count],[Rank],[CountIn100k],[Guid])",
                MigrationSQL._201707311527250_LastName );

            Sql( "DELETE [MetaNickNameLookup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaNickNameLookup] ([FirstName],[NickName],[Gender],[Count],[Guid])",
                MigrationSQL._201707311527250_NickName );

            Sql( "DELETE [MetaPersonicxLifestageGroup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaPersonicxLifestageGroup] ([LifestyleGroupCode],[LifestyleGroupName],[Description],[Summary],[PercentUS],[LifeStage],[MaritalStatus],[HomeOwnership],[Children],[Income],[IncomeRank],[Urbanicity], [UrbanicityRank], [NetWorth], [NetworthRank],[DetailsUrl],[Guid])",
                MigrationSQL._201707311527250_PersonicxGroup );

            Sql( "DELETE [MetaPersonicxLifestageCluster]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaPersonicxLifestageCluster] ([LifestyleClusterCode],[LifestyleClusterName],[Description],[Summary],[PercentUS],[LifeStage],[MaritalStatus],[HomeOwnership],[Children],[Income],[IncomeRank],[Urbanicity], [UrbanicityRank], [NetWorth], [NetworthRank],[MetaPersonicxLifestyleGroupId],[DetailsUrl],[Guid])",
                MigrationSQL._201707311527250_PersonicxCluster );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Person", "MetaPersonicxLifestageGroupId", "dbo.MetaPersonicxLifestageGroup");
            DropForeignKey("dbo.Person", "MetaPersonicxLifestageClusterId", "dbo.MetaPersonicxLifestageCluster");
            DropIndex("dbo.Person", new[] { "MetaPersonicxLifestageGroupId" });
            DropIndex("dbo.Person", new[] { "MetaPersonicxLifestageClusterId" });
            AlterColumn("dbo.MetaPersonicxLifestageCluster", "Summary", c => c.String(maxLength: 600));
            AlterColumn("dbo.MetaLastNameLookup", "LastName", c => c.String(nullable: false, maxLength: 10));
            DropColumn("dbo.Person", "MetaPersonicxLifestageGroupId");
            DropColumn("dbo.Person", "MetaPersonicxLifestageClusterId");
        }

        private void ExecuteSqlInsert( string insertIntoStatement, string columnValues )
        {
            // Convert line-feed delimited values into string list
            var valueLines = columnValues.Split( new[] { "\n" }, StringSplitOptions.None ).ToList();

            // Build and execute an insert statement that inserts a group of values.
            int pos = 0;
            int take = 500;
            var subset = valueLines.Skip( pos ).Take( take ).ToList();

            while ( subset.Any() )
            {
                string values = subset.Select( v => $"({v},NEWID())" ).ToList().AsDelimited( "," );
                string sqlStatement = $"{insertIntoStatement} VALUES {values}";

                Sql( sqlStatement );

                pos += take;
                subset = valueLines.Skip( pos ).Take( take ).ToList();
            }
        }
    }
}
