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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Text;

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
            Sql( "DELETE [MetaFirstNameGenderLookup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaFirstNameGenderLookup] ([FirstName],[MaleCount],[FemaleCount],[Country],[Language],[TotalCount],[FemalePercent],[MalePercent],[Guid])",
                MigrationSQL._201707311527250_FirstNameGender );

            // use BulkInsert for LastName since there are 150,000+ rows
            List<Rock.Model.MetaLastNameLookup> metaLastNameLookupsToInsert = new List<Model.MetaLastNameLookup>();
            List<string> lastNameLines;
            using ( var lastNameZip = new System.IO.Compression.GZipStream( new MemoryStream( MigrationSQL._201707311527250_LastName ), System.IO.Compression.CompressionMode.Decompress, false ) )
            {
                var lastNameText = Encoding.ASCII.GetString( lastNameZip.ReadBytesToEnd() );
                lastNameLines = lastNameText.Split( new[] { "\n" }, StringSplitOptions.None ).ToList();
            }

            foreach ( var lastNameLine in lastNameLines )
            {
                var lastNameColValues = lastNameLine.Split( new[] { ',' }, StringSplitOptions.None ).ToList();
                if ( lastNameColValues.Count == 4 )
                {
                    var metaLastNameLookup = new Rock.Model.MetaLastNameLookup();
                    metaLastNameLookup.LastName = lastNameColValues[0].TrimStart( new[] { '\'' } ).TrimEnd( new[] { '\'' } );
                    metaLastNameLookup.Count = lastNameColValues[1].AsInteger();
                    metaLastNameLookup.Rank = lastNameColValues[2].AsInteger();
                    metaLastNameLookup.CountIn100k = lastNameColValues[3].AsDecimalOrNull();
                    metaLastNameLookup.Guid = Guid.NewGuid();
                    metaLastNameLookupsToInsert.Add( metaLastNameLookup );
                }
            }

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "DELETE [MetaLastNameLookup]" );
                rockContext.Database.ExecuteSqlCommand( "ALTER TABLE MetaLastNameLookup ALTER COLUMN LastName NVARCHAR(100)" );
                rockContext.BulkInsertWithConditionalCacheUse( metaLastNameLookupsToInsert, false );
            }

            Sql( "DELETE [MetaNickNameLookup]" );
            ExecuteSqlInsert(
                "INSERT INTO [MetaNickNameLookup] ([FirstName],[NickName],[Gender],[Count],[Guid])",
                MigrationSQL._201707311527250_NickName );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void ExecuteSqlInsert( string insertIntoStatement, string columnValues )
        {
            // Convert line-feed delimited values into string list, and remove trailing newline
            var valueLines = columnValues.Split( new[] { "\n" }, StringSplitOptions.None ).Select( s => s.Trim() ).ToList();

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
