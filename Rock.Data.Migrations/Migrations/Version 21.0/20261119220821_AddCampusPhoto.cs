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
namespace Rock.Data.Migrations;

/// <summary>
/// Adds the PhotoId column to the Campus table, with an index and a foreign key to the BinaryFile table.
/// </summary>
[SqlMigration( 20261119220821, "21.0" )]
internal class AddCampusPhoto : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        Sql( @"
                ALTER TABLE [dbo].[Campus] ADD [PhotoId] [int]" );
        Sql( @"
                CREATE INDEX [IX_PhotoId] ON [dbo].[Campus]([PhotoId])" );
        Sql( @"
                ALTER TABLE [dbo].[Campus] ADD CONSTRAINT [FK_dbo.Campus_dbo.BinaryFile_PhotoId] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[BinaryFile] ([Id])" );
    }
}
